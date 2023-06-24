using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenuCoreMaui.BaseSerial;

namespace TcMenuCoreMaui.Windows.Serial
{
    class WindowsSerialFactory : ISerialPortFactory
    {
        private Serilog.ILogger logger = Serilog.Log.Logger.ForContext<WindowsSerialFactory>();

        public bool StartScanningPorts(SerialPortType type, Action<SerialPortInformation> portDelegate)
        {
            logger.Information($"Getting a list of USB serial devices for {type}");

            Task.Run(async () =>
            {
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

        private async Task ScanBluetoothPorts(Action<SerialPortInformation> portDelegate)
        {
            var btPorts =
                await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
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
                            portDelegate.Invoke(new SerialPortInformation(
                                serialDevice.Device.Name,
                                SerialPortType.BLUETOOTH,
                                btDevice.Id));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, $"Device could not be enumerated {btDevice.Name}");
                }
            }
        }

        private async Task ScanSerialPorts(Action<SerialPortInformation> portDelegate)
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
                            portDelegate.Invoke(new SerialPortInformation(
                                serialPort.Name,
                                SerialPortType.NAMED_SERIAL,
                                serialDevice.PortName));
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
        }

        public IRemoteConnector CreateSerialConnector(LocalIdentification localId, SerialPortInformation info, int baud,
            IProtocolCommandConverter converter, ProtocolId protocol, SystemClock clock, bool pairing)
        {
            logger.Information($"Creating serial connector to {info} with baud {baud}");
            if (info.PortType == SerialPortType.NAMED_SERIAL)
            {
                return new WinSerialRemoteConnector(localId, info, baud, converter, protocol, clock, pairing);
            }
            else if (info.PortType == SerialPortType.BLUETOOTH)
            {
                return new RegularBluetoothStreamConnector(localId, info, converter, protocol, clock, pairing);
            }

            throw new ArgumentException("Unknown connection type for " + info);
        }

    }
}
