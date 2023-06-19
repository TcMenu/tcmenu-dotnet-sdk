using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using Moq;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Commands;
using System.Threading;
using System.Threading.Tasks;
using TcMenu.CoreSdk.RemoteStates;
using TcMenu.CoreSdk.RemoteSimulator;
using TcMenu.CoreSdkTests.MenuTests;
using System.Linq;

namespace TcMenu.CoreSdkTests.RemoteCore
{
    [TestClass]
    public class RemoteControllerTests
    {
        private RemoteController _controller;
        private MenuTree _myTree;
        private MenuTree _simTree;
        private SimulatedRemoteConnection _remoteConnector;
        private Mock<SystemClock> _clock;
        private readonly List<AuthenticationStatus> _authReceived = new List<AuthenticationStatus>();
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly CountdownEvent _countDown = new CountdownEvent(2);
        private volatile CorrelationId _correlation;

        [TestInitialize]
        public void InitialiseController()
        {
            _clock = new Mock<SystemClock>();

            _simTree = MenuItemFixtures.LoadMenuTree(MenuItemFixtures.LARGE_MENU_TREE);

            var initialValues = new Dictionary<int, object>
            {
                [1] = 100,
                [3] = 1,
                [5] = true,
                [8] = 10.4F,
                [16] = (decimal)1039.434,
                [15] = "Text"
            };

            _remoteConnector = new SimulatedRemoteConnection(_simTree, "simulator", 1, initialValues);

            _myTree = new MenuTree();

            _controller = new RemoteController(_remoteConnector, _myTree, _clock.Object);
            _controller.Connector.ConnectionChanged += OnAuthenticationChangeEvent;

            Assert.AreEqual(AuthenticationStatus.NOT_STARTED, _controller.Connector.AuthStatus);

            _controller.Start();

            _waitHandle.Reset();
        }

        private void OnAuthenticationChangeEvent(AuthenticationStatus status)
        {
            _authReceived.Add(status);
            if (status == AuthenticationStatus.CONNECTION_READY) _waitHandle.Set();
        }

        [TestCleanup]
        public void TidyUpConnections()
        {
            _controller.Stop();
        }

        [TestMethod]
        public void TestDialogUpdate()
        {
            _controller.DialogUpdatedEvent += OnDialogUpdated;
            _remoteConnector.SendDialogAction(DialogMode.SHOW, "header", "msg", MenuButtonType.CLOSE, MenuButtonType.NONE, CorrelationId.EMPTY_CORRELATION);
            bool triggered = _waitHandle.WaitOne(1000);
            Assert.IsTrue(triggered);
        }

        private void OnDialogUpdated(CorrelationId cor, DialogMode mode, string hdr, string msg, MenuButtonType b1, MenuButtonType b2)
        {
            if (hdr.Equals("header") && msg.Equals("msg") && b1 == MenuButtonType.CLOSE && b2 == MenuButtonType.NONE)
            {
                _waitHandle.Set();
            }
        }

        [TestMethod]
        public async Task TestJoiningAndBootstrap()
        {
            Assert.IsTrue(_waitHandle.WaitOne(5000));

            CollectionAssert.AreEquivalent(_simTree.GetAllMenuItems().ToList(), _myTree.GetAllMenuItems().ToList());

            Assert.AreEqual(100, _myTree.GetState(_myTree.GetMenuById(1)).ValueAsObject() as int? ?? 0);
            Assert.AreEqual(1, _myTree.GetState(_myTree.GetMenuById(3)).ValueAsObject() as int? ?? 0);
            Assert.AreEqual(true, _myTree.GetState(_myTree.GetMenuById(5)).ValueAsObject() as bool? ?? false);
            Assert.AreEqual(10.4F, _myTree.GetState(_myTree.GetMenuById(8)).ValueAsObject() as float? ?? 9999.9F, 0.0001F);
            Assert.AreEqual("Text", _myTree.GetState(_myTree.GetMenuById(15)).ValueAsObject() as string ?? "");
            Assert.AreEqual(1039.434M, _myTree.GetState(_myTree.GetMenuById(16)).ValueAsObject() as decimal? ?? 0.0M);

            _controller.MenuChangedEvent += OnMenuChange;
            _controller.AcknowledgementsReceived += OnAcknowledgementRx;

            _countDown.Reset();
            _correlation = await _controller.SendDeltaChange(_myTree.GetMenuById(2), 1);
            Assert.IsTrue(_countDown.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(1, _myTree.GetState(_myTree.GetMenuById(2)).ValueAsObject() as int? ?? 0);

            _countDown.Reset();
            _correlation = await _controller.SendDeltaChange(_myTree.GetMenuById(3), -1);
            Assert.IsTrue(_countDown.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(0, _myTree.GetState(_myTree.GetMenuById(3)).ValueAsObject() as int? ?? 9038);

            _countDown.Reset();
            _correlation = await _controller.SendAbsoluteChange(_myTree.GetMenuById(5), true);
            Assert.IsTrue(_countDown.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(true, _myTree.GetState(_myTree.GetMenuById(5)).ValueAsObject() as bool? ?? false);

            _countDown.Reset();
            _correlation = await _controller.SendAbsoluteChange(_myTree.GetMenuById(16), 1234.5678M);
            Assert.IsTrue(_countDown.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(1234.5678M, _myTree.GetState(_myTree.GetMenuById(16)).ValueAsObject() as decimal? ?? 0.0M);

            _countDown.Reset();
            _correlation = await _controller.SendAbsoluteChange(_myTree.GetMenuById(15), "hello");
            Assert.IsTrue(_countDown.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual("hello", _myTree.GetState(_myTree.GetMenuById(15)).ValueAsObject() as string);
        }

        private void OnAcknowledgementRx(CorrelationId correlation, AckStatus status)
        {
            if(Equals(correlation, _correlation))
            {
                if(_countDown.CurrentCount != 0) _countDown.Signal();
            }
        }

        private void OnMenuChange(MenuItem changed, bool valueOnly)
        {
            if(_countDown.CurrentCount != 0) _countDown.Signal();
        }
    }
}
