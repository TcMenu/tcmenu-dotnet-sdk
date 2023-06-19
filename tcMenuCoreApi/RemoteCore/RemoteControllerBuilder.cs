using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenu.CoreSdk.RemoteCore
{
    /// <summary>
    /// Describes the various states of pairing.
    /// </summary>
    public enum PairingState
    {
        DISCONNECTED,
        PAIRING_SENT,
        NOT_ACCEPTED,
        ACCEPTED,
        TIMED_OUT
    }

    /// <summary>
    /// used for notification back to a UI thread during pairing.
    /// </summary>
    /// <param name="status"></param>
    public delegate void PairingUpdateEventHandler(PairingState status);

    /// <summary>
    /// Provides the ability to create remote controllers of varied types
    /// </summary>
    public interface IRemoteControllerBuilder
    {
        IRemoteController BuildController();
        bool PerformPairing(PairingUpdateEventHandler updateNoficiationHandler, int howLongSeconds);
    }

    /// <summary>
    /// An abstract implementation of the controller builder that provides the pairing
    /// functionality for all implementations.
    /// </summary>
    public abstract class RemoteControllerBuilderBase<T> : IRemoteControllerBuilder where T: IRemoteControllerBuilder
    {
        private Serilog.ILogger logger = Serilog.Log.Logger.ForContext(typeof(IRemoteControllerBuilder));

        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(false);
        private volatile PairingState _pairingState;
        private volatile PairingUpdateEventHandler _pairingHandler;
        private volatile IRemoteConnector _connector;
        protected readonly SystemClock _clock = new SystemClock();

        protected ProtocolId _protocol;
        protected string _localName;
        protected Guid _localGuid;

        public T WithProtocol(ProtocolId protocol)
        {
            _protocol = protocol;
            return GetThis();
        }

        public T WithNameAndGuid(string name, Guid uniqueId)
        {
            _localName = name;
            _localGuid = uniqueId;
            return GetThis();
        }

        public abstract T GetThis();

        public abstract IRemoteConnector BuildConnector(bool pairing);

        public IProtocolCommandConverter GetDefaultConverters()
        {
            // create the protocol converters
            var converter = new DefaultProtocolCommandConverter();
            var tagVal = new TagValProtocolMessageProcessors();
            tagVal.RegisterConverters(converter);
            return converter;
        }

        public virtual IRemoteController BuildController()
        {
            var connector = BuildConnector(false);

            // we need a clock and a menu tree.
            var tree = new MenuTree();

            var controller = new RemoteController(connector, tree, _clock);
            controller.Start();
            return controller;
        }

        public bool PerformPairing(PairingUpdateEventHandler handler, int howLongSeconds = 20)
        {
            var connector = BuildConnector(true);
            _connector = connector;

            if (connector == null || _localName == null || _localGuid == null) return false;

            logger.Information($"Started to perform pairing to {connector.ConnectionName}");
            _pairingHandler = handler;
            SetPairingState(PairingState.DISCONNECTED);

            connector.ConnectionChanged += Connector_ConnectionChanged;
            connector.Start();

            if(!_waitHandle.WaitOne(TimeSpan.FromSeconds(howLongSeconds)))
            {
                logger.Information($"Time out waiting to pair with {connector.ConnectionName}");
                SetPairingState(PairingState.TIMED_OUT);
            }

            connector.ConnectionChanged -= Connector_ConnectionChanged;

            connector.Stop();
            connector.Close();

            return _pairingState == PairingState.ACCEPTED;
        }

        private void Connector_ConnectionChanged(AuthenticationStatus status)
        {
            if(status == AuthenticationStatus.ESTABLISHED_CONNECTION)
            {
                logger.Information($"Pairing sent to {_connector.ConnectionName}");
                SetPairingState(PairingState.PAIRING_SENT);
            }
            else if(status == AuthenticationStatus.FAILED_AUTH)
            {
                SetPairingState(PairingState.NOT_ACCEPTED);
                _waitHandle.Set();
            }
            else if(status == AuthenticationStatus.AUTHENTICATED)
            {
                SetPairingState(PairingState.ACCEPTED);
                _waitHandle.Set();
            }
        }

        void SetPairingState(PairingState s)
        {
            _pairingState = s;
            _pairingHandler?.Invoke(s);
        }
    }

}
