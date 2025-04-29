using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteSimulator;
using TcMenu.CoreSdk.RemoteStates;
using static TcMenu.CoreSdkTests.RemoteCore.RemoteControllerTests;

namespace TcMenu.CoreSdkTests.RemoteCore
{
    [TestClass]
    public class RemoteControllerBuilderTests
    {
        private MockedParingTestRemoteConnector _mockConnector;
        private TestRemoteControlBuilder _builder;
        private Guid _guid = Guid.NewGuid();
        private volatile PairingState _pairingState = PairingState.DISCONNECTED;
        private DefaultProtocolCommandConverter _converter;
        private Mock<SystemClock> _clock;
        private AutoResetEvent _statusEvent = new AutoResetEvent(false);
        private Task _task;

        [TestInitialize]
        public void MakeBuilder()
        {
            _clock = new Mock<SystemClock>();
            _converter = new DefaultProtocolCommandConverter();
            TagValProtocolMessageProcessors processors = new TagValProtocolMessageProcessors();
            processors.RegisterConverters(_converter);

            _mockConnector = new MockedParingTestRemoteConnector();
            _builder = new TestRemoteControlBuilder("super", _guid, _mockConnector);
        }


        [TestMethod]
        public async Task TestPairingSuccess()
        {
            _task = Task.Run(() => _builder.PerformPairing((ps) =>
            {
                _pairingState = ps;
                _statusEvent.Set();
            }));

            WaitForPairingUpdateTo(AuthenticationStatus.ESTABLISHED_CONNECTION, PairingState.PAIRING_SENT);
            WaitForPairingUpdateTo(AuthenticationStatus.AUTHENTICATED, PairingState.ACCEPTED);

            await _task;
            Assert.IsTrue(_mockConnector.Closed);
            Assert.IsFalse(_mockConnector.Started);
        }

        private void WaitForPairingUpdateTo(AuthenticationStatus status, PairingState state)
        {
            int count = 0;
            while (!_mockConnector.Started && count < 100)
            {
                Thread.Sleep(5);
            }

            count = 0;
            _statusEvent.Reset();
            _mockConnector.AuthStatus = status;
            while (_statusEvent.WaitOne(TimeSpan.FromSeconds(10)) && count < 4 && state != _pairingState)
            {
                _statusEvent.Reset();
                ++count;
            }
            Assert.AreEqual(state, _pairingState);
        }

        [TestMethod]
        public async Task TestPairingFails()
        {
            _task = Task.Run(() => _builder.PerformPairing((ps) =>
            {
                _pairingState = ps;
                _statusEvent.Set();
            }));

            WaitForPairingUpdateTo(AuthenticationStatus.ESTABLISHED_CONNECTION, PairingState.PAIRING_SENT);
            WaitForPairingUpdateTo(AuthenticationStatus.FAILED_AUTH, PairingState.NOT_ACCEPTED);

            await _task;
            Assert.IsTrue(_mockConnector.Closed);
            Assert.IsFalse(_mockConnector.Started);
        }

        [TestMethod]
        public async Task TestPairingTimeOut()
        {
            _task = Task.Run(() => _builder.PerformPairing((ps) =>
            {
                _pairingState = ps;
                _statusEvent.Set();
            }, 1));

            WaitForPairingUpdateTo(AuthenticationStatus.ESTABLISHED_CONNECTION, PairingState.PAIRING_SENT);

            await _task;
            Assert.AreEqual(PairingState.TIMED_OUT, _pairingState);
            Assert.IsTrue(_mockConnector.Closed);
            Assert.IsFalse(_mockConnector.Started);
        }
    }

    internal class MockedParingTestRemoteConnector : IRemoteConnector
    {
        public MockedParingTestRemoteConnector()
        {
        }

        public string ConnectionName => "Unit test";

        private volatile AuthenticationStatus _authStatus = AuthenticationStatus.NOT_STARTED;
        public AuthenticationStatus AuthStatus
        {
            get => _authStatus;
            set
            {
                _authStatus = value;
                ConnectionChanged?.Invoke(value);
            }
        }

        public RemoteInformation RemoteInfo => new RemoteInformation("Unit test", 101, ApiPlatform.ARDUINO, Guid.NewGuid().ToString(), "123");

        private volatile bool _closed, _started;
        public  bool Closed { get => _closed; private set => _closed = value; }
        public bool Started { get => _started; private set => _started = value; }

        public event ConnectionChangedHandler ConnectionChanged;
        public event MessageReceivedHandler MessageReceived;

        public void Close()
        {
            Closed = true;
        }

        public void SendMenuCommand(MenuCommand command)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            Started = true;
        }

        public void Stop(bool waitForThread = false)
        {
            Started = false;
        }
    }

    class TestRemoteControlBuilder : RemoteControllerBuilderBase<TestRemoteControlBuilder>
    {
        private IRemoteConnector _testConnector;
        public TestRemoteControlBuilder(string localName, Guid localGuid, IRemoteConnector connector)
        {
            _localName = localName;
            _localGuid = localGuid;
            _testConnector = connector;
        }

        public override IRemoteConnector BuildConnector(bool ignored)
        {
            return _testConnector;
        }

        public override IRemoteController BuildController()
        {
            throw new NotImplementedException();
        }

        public override TestRemoteControlBuilder GetThis()
        {
            return this;
        }
    }
}
