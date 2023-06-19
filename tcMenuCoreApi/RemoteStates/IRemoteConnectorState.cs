using System.Threading;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    /// <summary>
    /// Classes that extend from RemoteConnectorState are responsible for managing a
    /// particular transition in the state of a connection. A connection is generally
    /// in one of many states, as it goes from disconnected through bootstrapping
    /// to fully connected.
    /// </summary>
    public interface IRemoteConnectorState
    {
        /// <summary>
        /// called by the connector when the state becomes active.
        /// </summary>
        void EnterState();

        /// <summary>
        /// called by the connector when the state deactivates, before the new state becomes active.
        /// </summary>
        /// <param name="newState">the state that is to become active.</param>
        void ExitState(IRemoteConnectorState newState);

        /// <summary>
        /// Return true if this connector needs to process messages
        /// </summary>
        bool NeedsRead { get; }

        /// <summary>
        /// When a command is read from the socket (when NeedsRead is true) then this function will
        /// receive the read messages one at a time. We do not indicate errors back to the connector,
        /// instead we just change state for example by calling context's BackToStart()..
        /// </summary>
        /// <param name="command">the command that has been read</param>
        void ProcessCommand(MenuCommand command);

        /// <summary>
        /// Called occaisonally by the watchdog thread to allow for heartbeating and other such
        /// occasional activities. Called about twice a second.
        /// </summary>
        void Tick();
        
        /// <summary>
        /// This method can rely on the thread that's provided by the connection to do all it's reading, connecting
        /// and other such processing. It should return as soon as possible after the state has been ended. Not doing
        /// so will prevent other processing.
        /// </summary>
        void ThreadProc();


        /// <summary>
        /// Authentication status indicates the approximate position within the connection process.
        /// </summary>
        AuthenticationStatus AuthStatus { get; }

        /// <summary>
        /// Before sending any commands, they are passed through the state machine, which can veto
        /// the send by returning false.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>true to allow the command, false to stop the command</returns>
        bool BeforeSendCommandToRemote(MenuCommand command);

    }

    /// <summary>
    /// An implementation of the state machine that is used only when the connection is not started
    /// </summary>
    public class NoOperationRemoteConnectorState : IRemoteConnectorState
    {
        private volatile bool _exit;
        private readonly object _monitorObj = new object();
        
        public NoOperationRemoteConnectorState(IRemoteConnectorContext context)
        {
        }

        public bool NeedsRead => false;

        public AuthenticationStatus AuthStatus => AuthenticationStatus.NOT_STARTED;

        public bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return false;
        }

        public void EnterState()
        {
        }

        public void ExitState(IRemoteConnectorState state)
        {
            _exit = true;
            lock(_monitorObj) Monitor.PulseAll(_monitorObj);
        }

        public void ProcessCommand(MenuCommand command)
        {
        }

        public void Tick()
        {
        }

        public void ThreadProc()
        {
            while (!_exit)
            {
                lock(_monitorObj) Monitor.Wait(this, 5000);
            }
        }
    }
}
