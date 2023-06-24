using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using embedCONTROL.Services;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteSimulator;
using TcMenu.CoreSdk.Serialisation;
using TcMenu.CoreSdk.SocketRemote;
using TcMenuCoreMaui.BaseSerial;

namespace embedControl.Services
{
    public interface IConnectionConfiguration
    {
        string Name { get; }
        string Describe { get; }

        IRemoteController Build();

        bool Pair(PairingUpdateEventHandler handler);
    }

    public class SimulatorConfiguration : IConnectionConfiguration
    {
        public const string SimulatorName = "Simulator";
        public const string DefaultMenuTree = "{\"items\":[{\"parentId\":0,\"type\":\"analogItem\",\"item\":{\"name\":\"Voltage\",\"eepromAddress\":2,\"id\":1,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onVoltageChange\",\"maxValue\":255,\"offset\":-128,\"divisor\":2,\"unitName\":\"V\"}},{\"parentId\":0,\"type\":\"analogItem\",\"item\":{\"name\":\"Current\",\"eepromAddress\":4,\"id\":2,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onCurrentChange\",\"maxValue\":255,\"offset\":0,\"divisor\":100,\"unitName\":\"A\"}},{\"parentId\":0,\"type\":\"enumItem\",\"item\":{\"name\":\"Limit\",\"eepromAddress\":6,\"id\":3,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onLimitMode\",\"enumEntries\":[\"Current\",\"Voltage\"]}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Settings\",\"eepromAddress\":-1,\"id\":4,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":4,\"type\":\"boolItem\",\"item\":{\"name\":\"Pwr Delay\",\"eepromAddress\":-1,\"id\":5,\"readOnly\":false,\"localOnly\":false,\"naming\":\"YES_NO\"}},{\"parentId\":4,\"type\":\"actionMenu\",\"item\":{\"name\":\"Save all\",\"eepromAddress\":-1,\"id\":10,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onSaveRom\"}},{\"parentId\":4,\"type\":\"subMenu\",\"item\":{\"name\":\"Advanced\",\"eepromAddress\":-1,\"id\":11,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":11,\"type\":\"boolItem\",\"item\":{\"name\":\"S-Circuit Protect\",\"eepromAddress\":8,\"id\":12,\"readOnly\":false,\"localOnly\":false,\"naming\":\"ON_OFF\"}},{\"parentId\":11,\"type\":\"boolItem\",\"item\":{\"name\":\"Temp Check\",\"eepromAddress\":9,\"id\":13,\"readOnly\":false,\"localOnly\":false,\"naming\":\"ON_OFF\"}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Status\",\"eepromAddress\":-1,\"id\":7,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":7,\"type\":\"floatItem\",\"item\":{\"name\":\"Volt A0\",\"eepromAddress\":-1,\"id\":8,\"readOnly\":true,\"localOnly\":false,\"numDecimalPlaces\":2}},{\"parentId\":7,\"type\":\"floatItem\",\"item\":{\"name\":\"Volt A1\",\"eepromAddress\":-1,\"id\":9,\"readOnly\":true,\"localOnly\":false,\"numDecimalPlaces\":2}},{\"parentId\":7,\"type\":\"largeNumItem\",\"item\":{\"name\":\"RotationCounter\",\"eepromAddress\":-1,\"id\":16,\"readOnly\":true,\"localOnly\":false,\"decimalPlaces\":4,\"digitsAllowed\":8}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Connectivity\",\"eepromAddress\":-1,\"id\":14,\"readOnly\":false,\"localOnly\":true,\"secured\":true}},{\"parentId\":14,\"type\":\"textItem\",\"item\":{\"name\":\"Ip Address\",\"eepromAddress\":10,\"id\":15,\"readOnly\":false,\"visible\":\"false\",\"localOnly\":false,\"itemType\":\"IP_ADDRESS\",\"textLength\":20}}]}";

        public string Name { get; set; }
        public string Describe => "Simulator based";
        public string JsonObjects { get; set; }

        public SimulatorConfiguration()
        {
            this.Name = "Unknown";
            this.JsonObjects = "";
        }

        public SimulatorConfiguration(string name, string json)
        {
            this.Name = name ?? "";
            this.JsonObjects = json ?? "";
        }

        public IRemoteController Build()
        {
            var persistor = new JsonMenuItemPersistor();
            var menuTree = new MenuTree();
            var json = string.IsNullOrWhiteSpace(JsonObjects) ? DefaultMenuTree : JsonObjects;
            foreach (var itemAndParent in persistor.DeSerialiseItemsFromJson(json))
            {
                menuTree.AddMenuItem(menuTree.GetMenuById(itemAndParent.ParentId) as SubMenuItem, itemAndParent.Item);
            }
            var connector = new SimulatedRemoteConnection(menuTree, Name);
            var controller = new RemoteController(connector, menuTree, new SystemClock());
            controller.Start();
            return controller;

        }

        public bool Pair(PairingUpdateEventHandler handler)
        {
            throw new NotImplementedException();
        }
    }

    public class RawSocketConfiguration : IConnectionConfiguration
    {
        public const string ManualSocketName = "Manual Socket";

        public string Name { get; } = ManualSocketName;
        public string Describe => $"Socket {Host}:{Port}";
        public string Host { get; set; }
        public int Port { get; set; }

        public RawSocketConfiguration()
        {
            Host = "localhost";
            Port = 3333;
        }

        public RawSocketConfiguration(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public IRemoteController Build()
        {
            var appSettings = ApplicationContext.Instance.AppSettings;

            return new SocketRemoteControllerBuilder()
                .WithHostAndPort(Host, Port)
                .WithNameAndGuid(appSettings.LocalName, Guid.Parse(appSettings.UniqueId))
                .WithProtocol(ProtocolId.TAG_VAL_PROTOCOL)
                .BuildController();
        }

        public bool Pair(PairingUpdateEventHandler handler)
        {
            var appSettings = ApplicationContext.Instance.AppSettings;

            return new SocketRemoteControllerBuilder()
                .WithHostAndPort(Host, Port)
                .WithNameAndGuid(appSettings.LocalName, Guid.Parse(appSettings.UniqueId))
                .WithProtocol(ProtocolId.TAG_VAL_PROTOCOL)
                .PerformPairing(handler);
        }
    }

    public class SerialCommsConfiguration : IConnectionConfiguration
    {
        public string Name => $"{SerialInfo?.Id ?? ""}({SerialInfo?.Name ?? ""})@{BaudRate}";
        public string Describe => $"Serial {SerialInfo.Name}/{SerialInfo.Id}";

        public SerialPortInformation SerialInfo { get; set; }

        public int BaudRate { get; set; }

        public SerialCommsConfiguration() : this(SerialPortInformation.EMPTY, 115200)
        {
        }

        public SerialCommsConfiguration(SerialPortInformation portInfo, int baud)
        {
            SerialInfo = portInfo;
            BaudRate = 115200;
        }

        public IRemoteController Build()
        {
            var appSettings = ApplicationContext.Instance.AppSettings;

            return new SerialPortControllerBuilder(ApplicationContext.Instance.SerialPortFactory)
                .WithSerialPortAndBaud(SerialInfo, BaudRate)
                .WithNameAndGuid(appSettings.LocalName, Guid.Parse(appSettings.UniqueId))
                .WithProtocol(ProtocolId.TAG_VAL_PROTOCOL)
                .BuildController();
        }

        public bool Pair(PairingUpdateEventHandler handler)
        {
            var appSettings = ApplicationContext.Instance.AppSettings;

            return new SerialPortControllerBuilder(ApplicationContext.Instance.SerialPortFactory)
                .WithSerialPortAndBaud(SerialInfo, BaudRate)
                .WithNameAndGuid(appSettings.LocalName, Guid.Parse(appSettings.UniqueId))
                .WithProtocol(ProtocolId.TAG_VAL_PROTOCOL)
                .PerformPairing(handler);
        }
    }

}
