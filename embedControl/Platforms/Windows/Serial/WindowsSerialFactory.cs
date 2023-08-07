using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenuCoreMaui.BaseSerial;
using static TcMenuCoreMaui.BaseSerial.SerialPortDelegateMode;

namespace embedControl.Windows.Serial
{
    class WindowsSerialFactory : ISerialPortFactory
    {

        private Serilog.ILogger logger = Serilog.Log.Logger.ForContext<WindowsSerialFactory>();
        private DeviceWatcher _deviceWatcher = null;
        private SerialPortDelegate _portDelegate;

        public bool StartScanningPorts(SerialPortType type, SerialPortDelegate portDelegate)
        {
            logger.Information($"Getting a list of USB serial devices for {type}");
            _portDelegate = portDelegate;

            Task.Run(async () =>
            {
                if ((type & SerialPortType.BLE_BLUETOOTH) != 0)
                {
                    StartBleScanner();
                }

                if ((type & SerialPortType.NAMED_SERIAL) != 0)
                {
                    await ScanSerialPorts(portDelegate);
                }

                if ((type & SerialPortType.BLUETOOTH) != 0)
                {
                    await ScanBluetoothPorts(portDelegate);
                }
            });


            return true;
        }

        private void StartBleScanner()
        {
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
            string aqsAllBluetoothLeDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            _deviceWatcher =
                DeviceInformation.CreateWatcher(
                    aqsAllBluetoothLeDevices,
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);
            _deviceWatcher.Added += DeviceWatcherOnAdded;
            _deviceWatcher.Removed += DeviceWatcherOnRemove;
            _deviceWatcher.Start();
        }

        private void DeviceWatcherOnAdded(DeviceWatcher sender, DeviceInformation args)
        {
            if (string.IsNullOrWhiteSpace(args.Name)) return;
            _portDelegate.Invoke(new SerialPortInformation(args.Name, SerialPortType.BLE_BLUETOOTH, args.Id), AddOrUpdate);
        }

        private void DeviceWatcherOnRemove(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _portDelegate.Invoke(new SerialPortInformation("Deleted", SerialPortType.BLE_BLUETOOTH, args.Id), Remove);
        }

        private async Task ScanBluetoothPorts(SerialPortDelegate portDelegate)
        {
            var btPorts = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
            foreach (var btDevice in btPorts)
            {
                try
                {
                    var serialDevice = await RfcommDeviceService.FromIdAsync(btDevice.Id);
                    if (serialDevice != null)
                    {
                        using (serialDevice)
                        {
                            logger.Debug($"Enumerated {btDevice.Name} as {serialDevice.ConnectionServiceName}");
                            portDelegate.Invoke(new SerialPortInformation(serialDevice.Device.Name, SerialPortType.BLUETOOTH, btDevice.Id), AddOrUpdate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, $"Device could not be enumerated {btDevice.Name}");
                }
            }
        }

        private async Task ScanSerialPorts(SerialPortDelegate portDelegate)
        {
            var availableSerialPorts = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            foreach (var serialPort in availableSerialPorts)
            {
                try
                {
                    var serialDevice = await SerialDevice.FromIdAsync(serialPort.Id);
                    if (serialDevice != null)
                    {
                        using (serialDevice)
                        {
                            logger.Debug($"Enumerated {serialPort.Name} as {serialDevice.PortName}");
                            portDelegate.Invoke(new SerialPortInformation(serialPort.Name, SerialPortType.NAMED_SERIAL, serialDevice.PortName), AddOrUpdate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, $"Device could not be enumerated {serialPort.Name}");
                }
            }
        }

        public void StopScanningPorts()
        {
            if (_deviceWatcher != null)
            {
                _deviceWatcher.Stop();
                _deviceWatcher.Added -= DeviceWatcherOnAdded;
                _deviceWatcher.Removed -= DeviceWatcherOnRemove;
                _deviceWatcher = null;
            }
        }

        public IRemoteConnector CreateSerialConnector(LocalIdentification localId, SerialPortInformation info, int baud,
            IProtocolCommandConverter converter, ProtocolId protocol, SystemClock clock, bool pairing)
        {
            logger.Information($"Creating serial connector to {info} with baud {baud}");
            return info.PortType switch
            {
                SerialPortType.NAMED_SERIAL => new WinSerialRemoteConnector(localId, info, baud, converter, protocol, clock, pairing),
                SerialPortType.BLUETOOTH => new RegularBluetoothStreamConnector(localId, info, converter, protocol, clock, pairing),
                SerialPortType.BLE_BLUETOOTH => new WinBleRemoteConnection(localId, converter, info, protocol, clock, pairing),
                _ => throw new ArgumentException("Unknown connection type for " + info)
            };
        }

    }
}
