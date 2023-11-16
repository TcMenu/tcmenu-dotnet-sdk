// See https://aka.ms/new-console-template for more information

using System.IO.Enumeration;
using Serilog;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteSimulator;
using TcMenu.CoreSdk.RemoteStates;
using TcMenu.CoreSdk.Serialisation;
using TcMenu.CoreSdk.SocketRemote;

Serilog.Log.Logger = new Serilog.LoggerConfiguration().WriteTo.Console()
    .MinimumLevel.Debug().CreateLogger();


var clock = new SystemClock();
var tree = new MenuTree();

Serilog.Log.Information("TcMenu DotNet Hello World");

// for socket comms uncomment here
//LocalIdentification localId = new LocalIdentification(Guid.NewGuid(), "myName");
//IRemoteConnector connector = new SocketRemoteConnector(tree, "myHostOrIp", 3333);
// end of socket connection

// for simulation we create a simulated tree, here is an example of how to turn json into menu items.
string defaultMenuTree = "{\"items\":[{\"parentId\":0,\"type\":\"analogItem\",\"item\":{\"name\":\"Voltage\",\"eepromAddress\":2,\"id\":1,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onVoltageChange\",\"maxValue\":255,\"offset\":-128,\"divisor\":2,\"unitName\":\"V\"}},{\"parentId\":0,\"type\":\"analogItem\",\"item\":{\"name\":\"Current\",\"eepromAddress\":4,\"id\":2,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onCurrentChange\",\"maxValue\":255,\"offset\":0,\"divisor\":100,\"unitName\":\"A\"}},{\"parentId\":0,\"type\":\"enumItem\",\"item\":{\"name\":\"Limit\",\"eepromAddress\":6,\"id\":3,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onLimitMode\",\"enumEntries\":[\"Current\",\"Voltage\"]}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Settings\",\"eepromAddress\":-1,\"id\":4,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":4,\"type\":\"boolItem\",\"item\":{\"name\":\"Pwr Delay\",\"eepromAddress\":-1,\"id\":5,\"readOnly\":false,\"localOnly\":false,\"naming\":\"YES_NO\"}},{\"parentId\":4,\"type\":\"actionMenu\",\"item\":{\"name\":\"Save all\",\"eepromAddress\":-1,\"id\":10,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onSaveRom\"}},{\"parentId\":4,\"type\":\"subMenu\",\"item\":{\"name\":\"Advanced\",\"eepromAddress\":-1,\"id\":11,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":11,\"type\":\"boolItem\",\"item\":{\"name\":\"S-Circuit Protect\",\"eepromAddress\":8,\"id\":12,\"readOnly\":false,\"localOnly\":false,\"naming\":\"ON_OFF\"}},{\"parentId\":11,\"type\":\"boolItem\",\"item\":{\"name\":\"Temp Check\",\"eepromAddress\":9,\"id\":13,\"readOnly\":false,\"localOnly\":false,\"naming\":\"ON_OFF\"}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Status\",\"eepromAddress\":-1,\"id\":7,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":7,\"type\":\"floatItem\",\"item\":{\"name\":\"Volt A0\",\"eepromAddress\":-1,\"id\":8,\"readOnly\":true,\"localOnly\":false,\"numDecimalPlaces\":2}},{\"parentId\":7,\"type\":\"floatItem\",\"item\":{\"name\":\"Volt A1\",\"eepromAddress\":-1,\"id\":9,\"readOnly\":true,\"localOnly\":false,\"numDecimalPlaces\":2}},{\"parentId\":7,\"type\":\"largeNumItem\",\"item\":{\"name\":\"RotationCounter\",\"eepromAddress\":-1,\"id\":16,\"readOnly\":true,\"localOnly\":false,\"decimalPlaces\":4,\"digitsAllowed\":8}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Connectivity\",\"eepromAddress\":-1,\"id\":14,\"readOnly\":false,\"localOnly\":true,\"secured\":true}},{\"parentId\":14,\"type\":\"textItem\",\"item\":{\"name\":\"Ip Address\",\"eepromAddress\":10,\"id\":15,\"readOnly\":false,\"visible\":\"false\",\"localOnly\":false,\"itemType\":\"IP_ADDRESS\",\"textLength\":20}}]}";
MenuTree simTree = new MenuTree();
IMenuItemPersistor persistor = new JsonMenuItemPersistor();
persistor.DeSerialiseItemsFromJson(defaultMenuTree).ForEach(itemWithParentId => simTree.AddOrUpdateItem(itemWithParentId.ParentId, itemWithParentId.Item));
IRemoteConnector connector = new SimulatedRemoteConnection(simTree, "sim1", 500);
// end of simulated connection

// create a controller
RemoteController controller = new RemoteController(connector, tree, clock);

// register for change events
controller.MenuChangedEvent += (itemChanged, structural) =>
{
    Serilog.Log.Information("Item has changed " + itemChanged);
};

// register for connection status events, here we simply wait for a complete connection and print everything out.
controller.Connector.ConnectionChanged += (status) =>
{
    if (status == AuthenticationStatus.CONNECTION_READY)
    {
        foreach (var item in tree.GetAllMenuItems())
        {
            Serilog.Log.Information($"Item in tree {item} with parent {tree.FindParent(item)}");
        }

        //controller.SendAbsoluteChange(tree.GetMenuById(1), "new value");
    }
    else
    {
        Serilog.Log.Information("Status is now " + status);
    }
};

// spin up the connection
controller.Start();

Serilog.Log.Information("Started!!");

Thread.Sleep(100000);

// and stop it again
controller.Stop();

Serilog.Log.Information("Stopped, goodbye!!");
