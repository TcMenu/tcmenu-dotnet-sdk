using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenu.CoreSdk.SocketRemote
{
    public class SocketRemoteConnector : RemoteConnectorBase
    {
        private const int WORST_CASE_TIMEOUT = 10000;

        private volatile Socket _socket = null;
        public string HostOrIp { get; }
        public int Port { get; }
        public override string ConnectionName { get => $"{HostOrIp}:{Port}"; }

        public override bool DeviceConnected => _socket?.Connected ?? false;

        public SocketRemoteConnector(LocalIdentification localId, string hostOrIp, int port, IProtocolCommandConverter converter, 
                                     ProtocolId protocol, SystemClock clock, bool pairing) : base(localId, converter, protocol, clock) 
        {
            HostOrIp = hostOrIp;
            Port = port;

            _stateMappings[AuthenticationStatus.NOT_STARTED] = typeof(NoOperationRemoteConnectorState);
            _stateMappings[AuthenticationStatus.AWAITING_CONNECTION] = typeof(StreamNotConnectedState);
            _stateMappings[AuthenticationStatus.ESTABLISHED_CONNECTION] = typeof(SocketWaitingForJoinState);
            _stateMappings[AuthenticationStatus.SEND_JOIN] = pairing ? typeof(SendPairingMessageState) : typeof(SendAndProcessJoinState);
            _stateMappings[AuthenticationStatus.AUTHENTICATED] = typeof(InitiateBootstrapState);
            _stateMappings[AuthenticationStatus.FAILED_AUTH] = typeof(FailedAuthenticationState);
            _stateMappings[AuthenticationStatus.BOOTSTRAPPING] = typeof(BootstrappingState);
            _stateMappings[AuthenticationStatus.CONNECTION_READY] = typeof(ConnectionReadyState);
        }

        public override bool PerformConnection()
        {
            TickConnectionAttemptTime();
            try
            {
                _socket = null;
                logger.Information($"Attempt connection to {HostOrIp}:{Port}");
                IPAddress ipAddress;
                if (Regex.IsMatch(HostOrIp, @"\d+\.\d+\.\d+\.\d+"))
                {
                    ipAddress = IPAddress.Parse(HostOrIp);
                }
                else
                {
                    IPHostEntry host = Dns.GetHostEntry(HostOrIp);
                    ipAddress = host.AddressList[0];
                }
                IPEndPoint remoteEndpoint = new IPEndPoint(ipAddress, Port);
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEndpoint);
                sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, WORST_CASE_TIMEOUT);
                sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, WORST_CASE_TIMEOUT);
                //sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _socket = sender;
                logger.Information("Socket connection has been established to " + _socket.RemoteEndPoint.ToString());
                return true;
            }
            catch (Exception err)
            {
                logger.Error(err, $"Connection to {HostOrIp}:{Port} failed");
                return false;
            }
        }

        public override void Close()
        {
            logger.Information($"Socket closing for {ConnectionName}");
            base.Close();
            Socket sock = _socket;
            sock?.Close(2);
            _socket = null;
            logger.Debug($"Socket now closed for {ConnectionName}");
        }

        public override int ReadFromDevice(byte[] data, int offset)
        {
            var sock = _socket;
            if(sock != null) 
            {
                return sock.Receive(data, offset, data.Length - offset, SocketFlags.None);
            }
            else
            {
                throw new IOException("Socket already closed");
            }
        }

        public override int InternalSendData(byte[] data, int offset, int len)
        {
            var sock = _socket;
            if(sock != null)
            {
                return sock.Send(data, offset, len, SocketFlags.None);
            }
            else
            {
                throw new IOException("Socket already closed");
            }
        }

    }
}
