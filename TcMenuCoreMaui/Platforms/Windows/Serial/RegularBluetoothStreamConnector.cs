
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;
using TcMenuCoreMaui.BaseSerial;

namespace TcMenuCoreMaui.Windows.Serial
{
    public class RegularBluetoothStreamConnector : WindowsSendAndRecvBase

    {
        private readonly SerialPortInformation _portInfo;
        private volatile RfcommDeviceService _service;
        private volatile StreamSocket _btSocket;

        private readonly object _portLock = new object();

        public RegularBluetoothStreamConnector(LocalIdentification localId, SerialPortInformation dev,
            IProtocolCommandConverter converter,
            ProtocolId protocol, SystemClock clock, bool pairing)
            : base(localId, converter, protocol, clock)
        {
            _portInfo = dev;

            _stateMappings[AuthenticationStatus.NOT_STARTED] = typeof(NoOperationRemoteConnectorState);
            _stateMappings[AuthenticationStatus.AWAITING_CONNECTION] = typeof(StreamNotConnectedState);
            _stateMappings[AuthenticationStatus.ESTABLISHED_CONNECTION] = typeof(SerialWaitingForInitialMsg);
            _stateMappings[AuthenticationStatus.SEND_JOIN] =
                pairing ? typeof(SendPairingMessageState) : typeof(SendAndProcessJoinState);
            _stateMappings[AuthenticationStatus.AUTHENTICATED] = typeof(InitiateBootstrapState);
            _stateMappings[AuthenticationStatus.FAILED_AUTH] = typeof(FailedAuthenticationState);
            _stateMappings[AuthenticationStatus.BOOTSTRAPPING] = typeof(BootstrappingState);
            _stateMappings[AuthenticationStatus.CONNECTION_READY] = typeof(ConnectionReadyState);
        }

        public override string ConnectionName => _service?.Device?.Name ?? "Unknown";

        public override object LockObject => _portLock;

        public override bool PerformConnection()
        {
            TickConnectionAttemptTime();
            return DoConnect().Result;
        }

        private async Task<bool> DoConnect()
        {
            logger.Information("Trying to connect to " + _portInfo);
            try
            {
                _service = await RfcommDeviceService.FromIdAsync(_portInfo.Id);
                if (_service != null)
                {
                    var btSocket = new StreamSocket();
                    await btSocket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
                    logger.Information("Connected to " + _portInfo);
                    lock (_portLock)
                    {
                        CreateBuffers(btSocket.InputStream, btSocket.OutputStream);
                        _btSocket = btSocket;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception during connect to BT endpoint " + _portInfo);
            }

            logger.Information("Connection attempt not successful " + _portInfo);
            return false;
        }

        public override void Close()
        {
            base.Close();
            _service?.Dispose();
            _btSocket?.Dispose();
            DestroyBuffers();
            _service = null;
            _btSocket = null;
        }

        public override bool DeviceConnected => _btSocket != null;
    }

}