using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using embedControl.Windows.Serial;
using Serilog;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;
using TcMenuCoreMaui.BaseSerial;

namespace embedControl.Windows.Serial
{
    public class WinBleRemoteConnection : RemoteConnectorBase
    {
        private static readonly ILogger _logger = Log.ForContext<WinBleRemoteConnection>();

        public readonly Guid ExpectedServiceUuid = Guid.Parse("8589F957-1916-4B27-BA6D-B0AF36F317EF");
        public readonly Guid CharacteristicApiToDevice = Guid.Parse("7E7CA4D5-CF52-4918-BD60-6E98A810F0EB");
        public readonly Guid CharacteristicDeviceToApi = Guid.Parse("D361F4F9-B13D-4118-A6C4-B54CF12EED3C");
        public readonly Guid CharacteristicApiSeqCounter = Guid.Parse("AB2ED3BE-C8C4-4E0D-8BF9-E7D6717F761C");
        public readonly Guid CharacteristicDeviceSeqCounter = Guid.Parse("79A24791-EEDA-4DA5-9E8C-04B8C9060B1F");
        public const int MaxMsgLenBle = 256;

        private readonly SerialPortInformation _serialPort;
        private GattCharacteristic _apiToDeviceCharacteristic;
        private GattCharacteristic _deviceToApiCharacteristic;
        private GattCharacteristic _apiSeqCharacteristic;
        private GattCharacteristic _deviceSeqCharacteristic;
        private BluetoothLEDevice _leDevice;
        private uint _deviceSequenceRx = 0;
        private uint _deviceSequenceTx = 0;
        private uint _apiSequenceLast = 0;
        private readonly Queue<byte[]> _dataBlocks = new();
        private bool _initialised = false;

        public WinBleRemoteConnection(LocalIdentification localId, IProtocolCommandConverter converter, SerialPortInformation portInformation,
            ProtocolId protocol, SystemClock clock, bool pairing) : base(localId, converter, protocol, clock)
        {
            _serialPort = portInformation;

            _stateMappings[AuthenticationStatus.NOT_STARTED] = typeof(NoOperationRemoteConnectorState);
            _stateMappings[AuthenticationStatus.AWAITING_CONNECTION] = typeof(StreamNotConnectedState);
            _stateMappings[AuthenticationStatus.ESTABLISHED_CONNECTION] = typeof(SerialWaitingForInitialMsg);
            _stateMappings[AuthenticationStatus.SEND_JOIN] = pairing ? typeof(SendPairingMessageState) : typeof(SendAndProcessJoinState);
            _stateMappings[AuthenticationStatus.AUTHENTICATED] = typeof(InitiateBootstrapState);
            _stateMappings[AuthenticationStatus.FAILED_AUTH] = typeof(StreamNotConnectedState);
            _stateMappings[AuthenticationStatus.BOOTSTRAPPING] = typeof(BootstrappingState);
            _stateMappings[AuthenticationStatus.CONNECTION_READY] = typeof(ConnectionReadyState);
        }

        public override string ConnectionName => _serialPort.Name;

        public override int ReadFromDevice(byte[] data, int offset)
        {
            InitialiseDevice().GetAwaiter().GetResult();

            int attempts = 0;
            while (DeviceConnected && attempts++ < 200)
            {
                if (_dataBlocks.Count != 0)
                {
                    var localArray = _dataBlocks.Dequeue();

                    int i = 0;
                    while (i < localArray.Length && localArray[i] != 0)
                    {
                        data[offset + i] = localArray[i];
                        i++;
                    }

                    return i;
                }

                Thread.Sleep(25);
            }

            return 0;
        }

        public async Task<int> InternalTaskSend(byte[] data, int offset, int len)
        {
            if (!_initialised) await InitialiseDevice();

            var seq = await _deviceSeqCharacteristic.ReadValueAsync();

            /*if ((seq?.Value?.Length ?? 0) >= 4)
            {
                using var seqReader = DataReader.FromBuffer(seq.Value);
                seqReader.ByteOrder = ByteOrder.BigEndian;
                _deviceSequenceRx = seqReader.ReadUInt32();
                _logger.Information($"Sequence read back is {_deviceSequenceRx} ours was {_deviceSequenceTx}");
            }*/

            if (_deviceSequenceRx != _deviceSequenceTx)
            {
                _logger.Warning($"Sequence mismatch failure, expected: {_deviceSequenceTx}, was: {_deviceSequenceRx}");
                return 0;
            }

            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteUInt32(_deviceSequenceTx + 1);
            for (var i = offset; i < len; i++)
            {
                writer.WriteByte(data[i]);
            }

            var writeResult = await _apiToDeviceCharacteristic.WriteValueAsync(writer.DetachBuffer());
            if (writeResult == GattCommunicationStatus.Success)
            {
                _deviceSequenceTx += 1;
                _logger.Information($"Wrote {len} bytes, at sequence {_deviceSequenceTx}");
                return len;
            }
            else
            {
                _logger.Error("Probable failure while writing to BLE stack");
                return 0;
            }

        }

        public override int InternalSendData(byte[] data, int offset, int len)
        {
            for (var i = 0; i < 5; i++)
            {
                var r = InternalTaskSend(data, offset, len).Result;
                if (r != 0)
                {
                    return r;
                }
                Thread.Sleep(50);
            }

            return 0;
        }

        public override bool PerformConnection()
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    await ClearCurrentConnectionDown();

                    _logger.Information($"Attempt creating new BLE connection to {_serialPort.Id}");

                    // from https://github.com/microsoft/Windows-universal-samples/blob/7d37b035c2701f189097a5503c9c8e854934479d/Samples/BluetoothLE/cs/Scenario2_Client.xaml.cs
                    _leDevice = await BluetoothLEDevice.FromIdAsync(_serialPort.Id);
                    if (_leDevice == null)
                    {
                        _logger.Error($"The BLE device was not available at {_serialPort.Id}");
                        return;
                    }

                    _logger.Debug("Got device, trying to acquire GATT services");

                    var result = await _leDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        foreach (var service in result.Services)
                        {
                            if (!service.Uuid.Equals(ExpectedServiceUuid)) continue;
                            var characteristics = await GetCharacteristicsForService(service);
                            foreach (var characteristic in characteristics)
                            {
                                if (characteristic.Uuid.Equals(CharacteristicApiToDevice))
                                    _apiToDeviceCharacteristic = characteristic;
                                else if (characteristic.Uuid.Equals(CharacteristicDeviceToApi))
                                    _deviceToApiCharacteristic = characteristic;
                                else if (characteristic.Uuid.Equals(CharacteristicApiSeqCounter))
                                    _apiSeqCharacteristic = characteristic;
                                else if (characteristic.Uuid.Equals(CharacteristicDeviceSeqCounter))
                                    _deviceSeqCharacteristic = characteristic;
                            }
                        }
                    }
                });
                task.Wait();
                var success = _apiSeqCharacteristic != null && _deviceSeqCharacteristic != null && _deviceToApiCharacteristic != null && _apiToDeviceCharacteristic != null;
                if (!success)
                {
                    _logger.Information("Did not find Tc API characteristics, clearing down");

                    ClearCurrentConnectionDown().Wait();
                }
                else
                {
                    _logger.Information("Found all characteristics, starting connection");
                }
            }
            catch (Exception ex)
            {
                ClearCurrentConnectionDown().Wait();
                _logger.Error(ex, "Exception while attempting to connect to serial port");
            }

            return _leDevice != null;
        }

        private async Task ClearCurrentConnectionDown()
        {
            try
            {
                _logger.Information("Clearing down any current connection data");

                if (_deviceSeqCharacteristic != null && _deviceToApiCharacteristic != null &&
                    _leDevice?.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    // Need to clear the CCCD from the remote device so we stop receiving notifications
                    var result1 =
                        await _deviceSeqCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);
                    var result2 =
                        await _deviceToApiCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result1 == GattCommunicationStatus.Success && result2 == GattCommunicationStatus.Success)
                    {
                        _deviceSeqCharacteristic.ValueChanged -= DeviceSequenceChanged;
                        _deviceToApiCharacteristic.ValueChanged -= DeviceDataChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO handle exception
            }

            _leDevice?.Dispose();
            _leDevice = null;
            _deviceSeqCharacteristic = null;
            _apiSeqCharacteristic = null;
            _deviceToApiCharacteristic = null;
            _apiToDeviceCharacteristic = null;
            _apiSequenceLast = 0;
            _dataBlocks.Clear();
        }

        private async Task InitialiseDevice()
        {
            if (_deviceSeqCharacteristic == null || _deviceToApiCharacteristic == null) return;

            _logger.Information("Subscribing for BLE notifications on read channels");

            await EnableSubscribeForNotificationMode(_deviceSeqCharacteristic);
            await EnableSubscribeForNotificationMode(_deviceToApiCharacteristic);
            _deviceSeqCharacteristic.ValueChanged += DeviceSequenceChanged;
            _deviceToApiCharacteristic.ValueChanged += DeviceDataChanged;
            _initialised = true;
        }

        private async Task EnableSubscribeForNotificationMode(GattCharacteristic selectedCharacteristic)
        {
            // initialize status
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }

            else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                var status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                if (status == GattCommunicationStatus.Success)
                {
                    _logger.Information("Successfully registered for notify");
                }
                else
                {
                    _logger.Error($"Could not enable notify, status {status}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                _logger.Error(ex, "Could not enable notify, unlikely connection will work");
            }
        }

        private async void DeviceDataChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var toDevice = await _deviceToApiCharacteristic.ReadValueAsync();
            if (toDevice == null || (toDevice.Value?.Length ?? 0) < 4)
            {
                _logger.Information("Message too short or no device");
                return;
            }

            using var dataReader = DataReader.FromBuffer(toDevice.Value);
            dataReader.ByteOrder = ByteOrder.BigEndian;
            var rxSequence = dataReader.ReadUInt32();

            if (_apiSequenceLast != 0 && _apiSequenceLast <= rxSequence)
            {
                _logger.Debug($"API Rx duplicate sequence {rxSequence} ignored");
                return;
            }

            var localArray = new byte[toDevice.Value.Length - 4];
            dataReader.ReadBytes(localArray);
            _dataBlocks.Enqueue(localArray);

            _logger.Information($"Read Sequence: {rxSequence} of length {localArray.Length}");
            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteUInt32(rxSequence);
            _apiSeqCharacteristic.WriteValueAsync(writer.DetachBuffer());
        }

        private void DeviceSequenceChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            using var dataReader = DataReader.FromBuffer(args.CharacteristicValue);
            dataReader.ByteOrder = ByteOrder.BigEndian;
            _deviceSequenceRx = dataReader.ReadUInt32();
            _logger.Information($"Device Sequence is {_deviceSequenceRx} ours was {_deviceSequenceTx}");
        }

        public override bool DeviceConnected => (_leDevice?.ConnectionStatus ?? BluetoothConnectionStatus.Disconnected) == BluetoothConnectionStatus.Connected;

        private async Task<List<GattCharacteristic>> GetCharacteristicsForService(GattDeviceService service)
        {
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only
                    // and the new Async functions to get the characteristics of unpaired devices as well.
                    var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        return new List<GattCharacteristic>(result.Characteristics);
                    }
                    else
                    {
                        _logger.Warning("Did not find any BLE entries");
                        return new List<GattCharacteristic>();
                    }
                }
                else
                {
                    _logger.Error("BLE permission not granted");

                    // On error, act as if there are no characteristics.
                    return new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while attempting to find BLE devices");
            }
            return new List<GattCharacteristic>();
        }
    }

}
