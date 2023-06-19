using System;
using System.Threading;
using Serilog;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{

    public abstract class BaseRemoteConnectorState : IRemoteConnectorState
    {
        private ILogger logger = Log.Logger.ForContext<BaseRemoteConnectorState>();
        protected IRemoteConnectorContext Context { get; }
        protected volatile bool _stateRunning;
        protected int _ticksSinceStart; // thread safe via interlocked.
        protected readonly int _allowableTicksToWait;

        protected BaseRemoteConnectorState(IRemoteConnectorContext context, int allowableMillis = 10000)
        {
            Context = context;
            _allowableTicksToWait = allowableMillis / RemoteConnectorBase.TICK_INTERVAL_MILLIS;
        }

        public abstract bool NeedsRead { get; }

        public abstract AuthenticationStatus AuthStatus { get; }

        public virtual bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return command.CommandType == HeartbeatCommand.HEARTBEAT_CMD_ID;
        }

        public virtual void EnterState()
        {
            _stateRunning = true;
            logger.Information($"Enter state {GetType().Name}");
        }

        public virtual void ExitState(IRemoteConnectorState newState)
        {
            _stateRunning = false;
            logger.Debug($"Exit state {GetType().Name}");
        }

        public abstract void ProcessCommand(MenuCommand command);

        public virtual void Tick()
        {
            if (Interlocked.Increment(ref _ticksSinceStart) > _allowableTicksToWait)
            {
                Context.BackToStart(this.GetType().Name + " took too long");
            }
        }

        protected void StandardHeartbeatProcessing(HeartbeatCommand hb)
        {
            if (hb.Mode == HeartbeatMode.START)
            {
                Context.SendHeartbeatCommand(5000, HeartbeatMode.START);
            }
            else if (hb.Mode == HeartbeatMode.END)
            {
                Context.BackToStart("HB end message received");
            }
        }

        public virtual void ThreadProc()
        {
            logger.Debug($"Read Thread for {GetType().Name} starting");
            while (_stateRunning)
            {
                try
                {
                    if (!NeedsRead && _stateRunning)
                    {
                        Thread.Sleep(250);
                    }
                    else if (NeedsRead && _stateRunning)
                    {
                        Context.ReadAndProcess();
                    }
                }
                catch (Exception err)
                {
                    logger.Error(err, $"Error on connection {Context.ConnectionName}");
                    Context.BackToStart("Exception in Threaded Reader");
                }
            }

            logger.Debug($"Thread {GetType().Name} exiting");
        }
    }
}
