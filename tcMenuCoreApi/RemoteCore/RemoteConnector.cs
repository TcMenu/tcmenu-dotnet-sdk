using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenu.CoreSdk.RemoteCore
{
    public abstract class RemoteConnectorBase : IRemoteConnector, IRemoteConnectorContext
    {
        private const string UI_DEF_SERIAL = "0";
        private const int API_VERSION = 103;
        private const int BUFFER_SIZE = 1024;
        public const int TICK_INTERVAL_MILLIS = 500;

        protected ILogger logger;

        protected readonly Dictionary<AuthenticationStatus, Type> _stateMappings = new Dictionary<AuthenticationStatus, Type>();
        private readonly CancellationTokenSource _cancellationTokenSrc = new CancellationTokenSource();
        private readonly IProtocolCommandConverter _commandConverter;
        private readonly ProtocolId _protocol;
        private readonly object _outputLock = new object();
        private readonly byte[] _readBuffer = new byte[BUFFER_SIZE];
        private int _readLen = 0;
        private readonly LocalIdentification _localId;

        public abstract string ConnectionName { get; }

        private DateTime _lastConnectionAttemptTime = DateTime.MinValue;

        public DateTime LastConnectionAttempt
        {
            get
            {
                lock(_stateLock) return _lastConnectionAttemptTime;
            }
        } 

        // the following fields make up the state machine, the _currentState is the actual state we
        // are in, _requestedState is potentially the next state we will be in or null if there is 
        // no transition to be done. Transitions always occur on the ThreadProc for safety.
        private readonly object _stateLock = new object();
        private IRemoteConnectorState _currentState = null;
        private IRemoteConnectorState _requestedState;
        protected IRemoteConnectorState CurrentState
        {
            get 
            {
                lock (_stateLock) return _currentState;
            }
        }

        public AuthenticationStatus AuthStatus => CurrentState?.AuthStatus ?? AuthenticationStatus.NOT_STARTED;

        private volatile RemoteInformation _remoteInformation = RemoteInformation.EMPTY_REMOTE_INFO;
        private Timer _timer;
        public RemoteInformation RemoteInfo => _remoteInformation;
        public SystemClock Clock { get; }

        public event ConnectionChangedHandler ConnectionChanged;
        public event MessageReceivedHandler MessageReceived;

        protected RemoteConnectorBase(LocalIdentification localId, IProtocolCommandConverter converter, ProtocolId protocol, SystemClock clock)
        {
            logger = Log.Logger.ForContext(GetType());
            Clock = clock;
            _commandConverter = converter;
            _protocol = protocol;
            _localId = localId;
            _currentState = new NoOperationRemoteConnectorState(this);
        }

        public void TickConnectionAttemptTime()
        {
            lock(_stateLock) _lastConnectionAttemptTime = DateTime.Now;
        }

        private void ThreadProc()
        {
            logger.Information($"Read Thread for {ConnectionName} starting");
            while (!_cancellationTokenSrc.Token.IsCancellationRequested)
            {
                try
                {
                    bool stateHasChanged = false;
                    lock (_stateLock)
                    {
                        if (_requestedState != null)
                        {
                            _currentState = _requestedState;
                            _currentState.EnterState();
                            _requestedState = null;
                            stateHasChanged = true;
                        }
                    }

                    if(stateHasChanged) ConnectionChanged?.Invoke(CurrentState.AuthStatus);

                    var st = _currentState;
                    if(st != null) 
                    {
                        st.ThreadProc();
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
                catch (Exception err)
                {
                    logger.Error(err, $"Error on connection {ConnectionName}");
                    BackToStart("Exception in Threaded Reader");
                }
            }
            logger.Information($"Thread {ConnectionName} exiting");
        }

        public void SetRemoteParty(RemoteInformation remoteInfo)
        {
            _remoteInformation = remoteInfo;
        }

        public void Start()
        {
            if (CurrentState.AuthStatus != AuthenticationStatus.NOT_STARTED) return; // just ignore duplicate start attempts
            _timer = new Timer(state => _currentState?.Tick(), null, TICK_INTERVAL_MILLIS, TICK_INTERVAL_MILLIS);
            logger.Information($"Start connection for {ConnectionName}");
            ChangeState(AuthenticationStatus.AWAITING_CONNECTION, true);
            Task.Factory.StartNew(ThreadProc, TaskCreationOptions.LongRunning);
        }

        public void BackToStart(string reason)
        {
            logger.Information("Back to start - " + reason);
            Close();
            ChangeState(AuthenticationStatus.AWAITING_CONNECTION);
        }

        public void Stop(bool waitForEnd = false)
        {
            if (CurrentState.AuthStatus == AuthenticationStatus.NOT_STARTED) return; // already stopped!
            logger.Information("Dispose timer ");
            _timer.Dispose();
            logger.Information($"Stop connection for {ConnectionName}");
            Close();
            logger.Information("goto not started");
            ChangeState(AuthenticationStatus.NOT_STARTED);
            _cancellationTokenSrc.Cancel();
        }

        public void ReadAndProcess()
        {
            var stream = ReadToEOM();
            CurrentState.ProcessCommand(_commandConverter.ConvertMessageToCommand(stream));
            stream.Dispose();            
        }

        private MemoryStream ReadToEOM()
        {
            int eom = _commandConverter.PositionOfEOMInBuffer(_readBuffer, _readLen, _protocol);
            while (eom == -1)
            {
                var amtRead = ReadFromDevice(_readBuffer, _readLen);
                _readLen += amtRead;

                if (amtRead <= 0) throw new IOException("Socket closed during read");
                if (_readLen < BUFFER_SIZE)
                {
                    eom = _commandConverter.PositionOfEOMInBuffer(_readBuffer, _readLen, _protocol);
                }
                else
                {
                    throw new TcProtocolException("Message in buffer is too long");
                }
            }

            // create a stream from the bytes
            var stream = new MemoryStream(192);
            stream.Write(_readBuffer, 0, eom + 1);
            stream.Seek(0, SeekOrigin.Begin);

            if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                logger.Debug("Read: " + _commandConverter.LogDataToCommandLine(_protocol, _readBuffer, 0, eom));
            }

            // then lastly we compact the buffer if it's not empty
            if (eom != _readLen)
            {
                _readLen -= (eom + 1);
                Array.Copy(_readBuffer, eom + 1, _readBuffer, 0, _readLen);
            }
            else _readLen = 0;

            return stream;
        }

        public void SendMenuCommand(MenuCommand command)
        {
            try
            {
                if (CurrentState.BeforeSendCommandToRemote(command))
                {
                    lock (_outputLock)
                    {
                        MemoryStream writerStream = new MemoryStream(BUFFER_SIZE);
                        writerStream.SetLength(0);
                        _commandConverter.ConvertCommandToMessage(command, _protocol, writerStream);
                        byte[] data = writerStream.GetBuffer();
                        int len = (int)writerStream.Length;

                        if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                        {
                            logger.Debug("Send: " + _commandConverter.LogDataToCommandLine(_protocol, data, 0, len));
                        }

                        int actual = InternalSendData(data, 0, len);
                        if (actual != len)
                        {
                            throw new IOException($"Data length wrong: expected {len} but was {actual}");
                        }
                        writerStream.Dispose();
                    }
                }
                else
                {
                    throw new IOException($"Send of {command} disallowed by the state machine {CurrentState} on {ConnectionName}");
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Exception while writing message {command} to {ConnectionName}");
                BackToStart("Command send failed");
            }
        }

        public void ChangeState(AuthenticationStatus newState, bool allowStart = false)
        {
            // we cannot transition when the connection is stopped (unless start forces it).
            if (!allowStart && CurrentState.AuthStatus == AuthenticationStatus.NOT_STARTED)
            {
                logger.Warning("We are in stopped state and received " + newState);
                return;
            }

            try
            {
                // find the constructor of the type and create a new state instance
                var type = _stateMappings[newState];
                var ctor = type.GetConstructor(new[] { typeof(IRemoteConnectorContext) });
                if (!(ctor?.Invoke(new object[] {this}) is IRemoteConnectorState state)) throw new ArgumentException("Ctor failure");
                logger.Information($"Transition request: {_currentState.AuthStatus} -> {newState}");
                lock (_stateLock)
                {
                    // here we tell the current state to exit and set the requested next state. It will be
                    // picked up once the current state releases the thread.
                    _currentState.ExitState(state);
                    _requestedState = state;
                }
            }
            catch(Exception e)
            {
                // This is a serious problem, and we are now in the wrong state, we must close the connection
                // and try and create a new one.
                logger.Error(e, $"Unable to transition from {CurrentState?.AuthStatus} to {newState}");
                Close();
                ChangeState(AuthenticationStatus.AWAITING_CONNECTION);
            }
        }

        public void PublishToController(MenuCommand command)
        {
            MessageReceived?.Invoke(command);
        }
        public void SendPairingCommand()
        {
            SendMenuCommand(new PairingCommand(_localId.Uuid.ToString(), _localId.Name));
        }
        
        public void SendHeartbeatCommand(int frequency, HeartbeatMode mode)
        {
            SendMenuCommand(new HeartbeatCommand(mode, frequency));
        }

        public void SendJoinCommand()
        {
            SendMenuCommand(new NewJoinerCommand(_localId.Name, _localId.Uuid.ToString(), API_VERSION, ApiPlatform.DNET_API, UI_DEF_SERIAL));
        }

        /// <summary>
        /// Implemented by the actual conector device to close the underlying connection
        /// </summary>
        public virtual void Close()
        {
            _readLen = 0;
        }
        
        /// <summary>
        /// Implemented by the actual connector device to read into a buffer starting at offset.
        /// </summary>
        /// <param name="data">the buffer to fill</param>
        /// <param name="offset">the starting offset</param>
        /// <returns>the number of bytes read, zero or less indicates socket closure</returns>
        public abstract int ReadFromDevice(byte[] data, int offset);

        /// <summary>
        /// Implemetned by the acutal connector device to send data in the buffer on the wire
        /// </summary>
        /// <param name="data">the data to be sent</param>
        /// <param name="offset">the offset in the array</param>
        /// <param name="len">the length of the data</param>
        /// <returns>the number of bytes read, zero or less indicates socket closure</returns>
        public abstract int InternalSendData(byte[] data, int offset, int len);

        /// <summary>
        /// Implemetned by the acutal connector device to perform a wire level connection
        /// </summary>
        /// <returns>async - task returning true if success</returns>
        public abstract bool PerformConnection();

        /// <summary>
        /// Indicates if we are presently connected at the wire level. Not  the
        /// protocol level.
        /// </summary>
        public abstract bool DeviceConnected { get; }
    }
}

