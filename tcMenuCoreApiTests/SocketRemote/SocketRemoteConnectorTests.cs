using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;
using TcMenu.CoreSdk.SocketRemote;
using TcMenu.CoreSdkTests.MenuTests;

namespace TcMenu.CoreSdkTests.SocketRemote
{
    [TestClass]
    public class SocketRemoteConnectorTests
    {
        private const int TEST_ITERATIONS = 1000;

        private readonly List<MenuCommand> _writtenToController = new List<MenuCommand>();
        private readonly List<MenuCommand> _readFromController = new List<MenuCommand>();
        private readonly DefaultProtocolCommandConverter _protocolConverter = new DefaultProtocolCommandConverter();
        private readonly BackgroundWorker _readWorker = new BackgroundWorker();
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly SystemClock _clock = new SystemClock();
        private readonly object _sockLock = new object();
        private volatile SocketRemoteConnector _socketRemote;
        private volatile Socket _socket;
        private volatile Socket _serverSocket;
        private volatile AuthenticationStatus _currentConnectivity = AuthenticationStatus.NOT_STARTED;

        [TestInitialize]
        public void StartSocket()
        {
            var logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();
            Log.Logger = logger;

            var tagVal = new TagValProtocolMessageProcessors();
            tagVal.RegisterConverters(_protocolConverter);
            var localId = new LocalIdentification(Guid.NewGuid(), "unitTestLocal");
            _socketRemote = new SocketRemoteConnector(localId, "127.0.0.1", 54321, _protocolConverter, ProtocolId.TAG_VAL_PROTOCOL, _clock, false);
            _socketRemote.MessageReceived += SocketConnector_MessageRx;
            _socketRemote.ConnectionChanged += SocketConnector_ConnectionChange;
            _socketRemote.Start();
            WaitForConnectorState(AuthenticationStatus.AWAITING_CONNECTION);

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 54321);

            _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(localEndPoint);
            _serverSocket.Listen(2);
            _socket = _serverSocket.Accept();
            _readWorker.DoWork += ReadThreadWorker;
            _readWorker.RunWorkerAsync(_socket);
        }

        [TestCleanup]
        public void StopSocket()
        {
            _serverSocket.Close();
            _waitHandle.Set();

            _socketRemote.Close();
            WaitForConnectorState(AuthenticationStatus.AWAITING_CONNECTION);

            _socketRemote.Stop();
            WaitForConnectorState(AuthenticationStatus.NOT_STARTED);
        }

        [TestMethod]
        [Ignore]
        public void TestSocketSendReceive()
        {
            WaitForConnectorState(AuthenticationStatus.ESTABLISHED_CONNECTION);

            SendOnSocket(new HeartbeatCommand(HeartbeatMode.START, 1500));
            SendOnSocket(new NewJoinerCommand("unit-test", Guid.NewGuid().ToString(), 101, ApiPlatform.DNET_API, "0"));
            WaitForConnectorState(AuthenticationStatus.SEND_JOIN);
            WaitForMessageOfType(typeof(NewJoinerCommand));

            SendOnSocket(new AcknowledgementCommand(CorrelationId.EMPTY_CORRELATION, AckStatus.SUCCESS));
            WaitForConnectorState(AuthenticationStatus.AUTHENTICATED);

            SendOnSocket(new BootstrapCommand(BootstrapType.START));
            WaitForConnectorState(AuthenticationStatus.BOOTSTRAPPING);
            SendOnSocket(new AnalogBootstrapCommand(0, MenuItemFixtures.AnAnalogItem(100, "abc"), 100));
            SendOnSocket(new BootstrapCommand(BootstrapType.END));
            WaitForConnectorState(AuthenticationStatus.CONNECTION_READY);

            var allCorrelations = new HashSet<CorrelationId>();

            for(int i = 0; i < TEST_ITERATIONS; i++)
            {
                var corr = new CorrelationId(_clock);
                allCorrelations.Add(corr);
                _socketRemote.SendMenuCommand(new MenuChangeCommand(1, corr, ChangeType.ABSOLUTE, i.ToString()));
                
                corr = new CorrelationId(_clock);
                allCorrelations.Add(corr);
                SendOnSocket(new MenuChangeCommand(i, corr, ChangeType.ABSOLUTE, "123"));
            }
            _socketRemote.SendMenuCommand(new DialogCommand(DialogMode.ACTION, "", "", MenuButtonType.ACCEPT, MenuButtonType.ACCEPT, CorrelationId.EMPTY_CORRELATION));
            WaitForMessageOfType(typeof(DialogCommand), TEST_ITERATIONS);

            int count = 0;
            while (++count < 25 && _readFromController.Count < (TEST_ITERATIONS + 4))
            {
                Thread.Sleep(250);
            }

            lock (_sockLock)
            {
                Assert.AreEqual(TEST_ITERATIONS + 4, _readFromController.Count);
                Assert.AreEqual(TEST_ITERATIONS + 2, _writtenToController.Count);
                Assert.IsInstanceOfType(_writtenToController[0], typeof(AnalogBootstrapCommand));
                Assert.AreEqual(((BootstrapCommand)_writtenToController[1]).BootType, BootstrapType.END);
                for(int i = 0; i < TEST_ITERATIONS; i++)
                {
                    var fromEmbedded = _writtenToController[i + 2] as MenuChangeCommand;
                    Assert.IsNotNull(fromEmbedded);
                    Assert.AreEqual(i, fromEmbedded.MenuId);
                    Assert.IsTrue(allCorrelations.Contains(fromEmbedded.Correlation));                    
                }

                Assert.AreEqual(TEST_ITERATIONS, _readFromController
                    .Where(toEmbedded => toEmbedded is MenuChangeCommand chg && allCorrelations.Contains(chg.Correlation))
                    .Count()
                );
            }
        }

        private void WaitForControllerToSend(int number, int count)
        {
            throw new NotImplementedException();
        }

        private void WaitForMessageOfType(Type type, int maxRetires = 10)
        {
            int count = 0;
            bool waitingForMsg = true;
            while(waitingForMsg && ++count < maxRetires)
            {
                _waitHandle.WaitOne(100);
                lock(_sockLock)
                {
                    foreach (var msg in _readFromController)
                    {
                        if (msg.GetType() == type) return;
                    }
                }
            }
            Assert.Fail($"Message type {type} not found");
        }

        private void WaitForConnectorState(AuthenticationStatus status)
        {
            int count = 0;
            while(status != _socketRemote.AuthStatus && count < 10)
            {
                count++;
                _waitHandle.WaitOne(TimeSpan.FromSeconds(1));
                _waitHandle.Reset();
            }

            Assert.AreEqual(status, _socketRemote.AuthStatus);
        }

        private void SocketConnector_ConnectionChange(AuthenticationStatus authstatus)
        {
            _currentConnectivity = authstatus;
            _waitHandle.Set();
        }

        private void SocketConnector_MessageRx(MenuCommand command)
        {
            lock(_sockLock) _writtenToController.Add(command);
            _waitHandle.Set();
        }


        private void ReadThreadWorker(object sender, DoWorkEventArgs e)
        {
            Log.Logger.Information("Started the test read worker");
            MemoryStream memStream = new MemoryStream();
            memStream.SetLength(0);
            try
            {
                Log.Logger.Information("Start reading from server socket");
                byte[] temp = new byte[500];
                while (_socket.Connected)
                {
                    // Do proper read to eom here as this is the going to be the full on integration test case...
                    // needs to be able to withstand 1000000 message going through for fault and failure case testing.
                    int len = _socket.Receive(temp, SocketFlags.None);
                    var pos = memStream.Position;
                    memStream.Write(temp, 0, len);
                    memStream.Seek(pos, SeekOrigin.Begin);
                    try
                    {
                        while(memStream.Position < memStream.Length)
                        {
                            lock (_sockLock) _readFromController.Add(_protocolConverter.ConvertMessageToCommand(memStream));
                        }
                    }
                    catch(TcProtocolException)
                    {
                        // just ignore and move on.
                    }
                    if (memStream.Position == memStream.Length) memStream.SetLength(0);
                }
            }
            catch (Exception ex)
            {
                // pretty much ignore exceptions here, just close the socket and exit.
            }
            finally
            {
                memStream.Dispose();
                _socket.Close();
                _waitHandle.Set();
            }
            Log.Logger.Information("Stopped the test read worker");
        }

        private void SendOnSocket(MenuCommand command)
        {
            if (_socket == null || !_socket.Connected) throw new IOException("Cant send on socket");
            MemoryStream memStream = new MemoryStream();
            _protocolConverter.ConvertCommandToMessage(command, ProtocolId.TAG_VAL_PROTOCOL, memStream);
            var data = new byte[2048];
            memStream.Seek(0, SeekOrigin.Begin);
            int len = (int)memStream.Length;
            memStream.Read(data, 0, len);
            int written = 0;
            while (written < len)
            {
                int thisTime = _socket.Send(data, written, len, SocketFlags.None);
                if (thisTime <= 0) throw new IOException("Closed socket");
                written += thisTime;
            }
            memStream.Dispose();
        }
    }
}
