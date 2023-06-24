using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;
using TcMenuCoreMaui.BaseSerial;

namespace TcMenuCoreMaui.Windows.Serial
{
    public class WinSerialRemoteConnector : WindowsSendAndRecvBase
    {
        private readonly SerialPortInformation _port;
        private readonly int _baudRate;
        private volatile SerialDevice _serialPort = null;
        private readonly object _portLock = new object();

        public override string ConnectionName => _port.Name;

        public override bool DeviceConnected
        {
            get
            {
                var sp = _serialPort;
                return sp != null;
            }
        }

        public WinSerialRemoteConnector(LocalIdentification localId, SerialPortInformation port, int baud, IProtocolCommandConverter converter,
                                        ProtocolId protocol, SystemClock clock, bool pairing) : base(localId, converter, protocol, clock)
        {
            _port = port;
            _baudRate = baud;

            _stateMappings[AuthenticationStatus.NOT_STARTED] = typeof(NoOperationRemoteConnectorState);
            _stateMappings[AuthenticationStatus.AWAITING_CONNECTION] = typeof(StreamNotConnectedState);
            _stateMappings[AuthenticationStatus.ESTABLISHED_CONNECTION] = typeof(SerialWaitingForInitialMsg);
            _stateMappings[AuthenticationStatus.SEND_JOIN] = pairing ? typeof(SendPairingMessageState) : typeof(SendAndProcessJoinState);
            _stateMappings[AuthenticationStatus.AUTHENTICATED] = typeof(InitiateBootstrapState);
            _stateMappings[AuthenticationStatus.FAILED_AUTH] = typeof(StreamNotConnectedState);
            _stateMappings[AuthenticationStatus.BOOTSTRAPPING] = typeof(BootstrappingState);
            _stateMappings[AuthenticationStatus.CONNECTION_READY] = typeof(ConnectionReadyState);

        }

        public override bool PerformConnection()
        {
            TickConnectionAttemptTime();
            logger.Information($"Attempting to open serial port {_port}");

            if (_serialPort == null)
            {
                try
                {
                    var filter = SerialDevice.GetDeviceSelector(_port.Id);
                    var devicesContinuation = DeviceInformation.FindAllAsync(filter).AsTask();
                    devicesContinuation.Wait();
                    if (devicesContinuation.Result.Any())
                    {
                        var spContinuation = SerialDevice.FromIdAsync(devicesContinuation.Result.First().Id).AsTask();
                        spContinuation.Wait();
                        var sp = spContinuation.Result;
                        if (sp == null) return false;

                        lock (_portLock)
                        {
                            sp.BaudRate = (uint)_baudRate;
                            sp.Parity = SerialParity.None;
                            sp.Handshake = SerialHandshake.None;
                            sp.StopBits = SerialStopBitCount.One;
                            sp.DataBits = 8;
                            sp.Handshake = SerialHandshake.None;
                            sp.WriteTimeout = TimeSpan.FromMilliseconds(500);
                            sp.ReadTimeout = TimeSpan.FromMilliseconds(3000);

                            CreateBuffers(sp.InputStream, sp.OutputStream);
                            _serialPort = sp;
                        }
                        return true;
                    }
                    else
                    {
                        lock (_portLock)
                        {
                            _serialPort = null;
                            DestroyBuffers();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Not connected to serial port");
                }
            }
            return false;
        }

        public override void Close()
        {
            logger.Information($"Attempting to close serial port {_port}");
            try
            {
                base.Close();
                lock (_portLock)
                {
                    _serialPort?.Dispose();
                    _serialPort = null;
                    DestroyBuffers();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to close serial port");
            }
        }

        public override object LockObject => _portLock;
    }

}
