using CoreBluetooth;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenuCoreMaui.iOS.SerialImpl
{

    public class AppleBluetoothSerialConnector : RemoteConnectorBase
    {
        private readonly CBPeripheral _btDevice;

        public AppleBluetoothSerialConnector(LocalIdentification localId, IProtocolCommandConverter converter, 
                                             ProtocolId protocol, SystemClock clock, CBPeripheral btDevice) 
            : base(localId, converter, protocol, clock)
        {
            _btDevice = btDevice;
            ConnectionName = _btDevice.Name;
            DeviceConnected = false;
        }

        public override string ConnectionName { get; }
        public override int ReadFromDevice(byte[] data, int offset)
        {
            throw new System.NotImplementedException();
        }

        public override int InternalSendData(byte[] data, int offset, int len)
        {
            throw new System.NotImplementedException();
        }

        public override bool PerformConnection()
        {
            TickConnectionAttemptTime();
            throw new System.NotImplementedException();
        }

        public override bool DeviceConnected { get; }
    }
}