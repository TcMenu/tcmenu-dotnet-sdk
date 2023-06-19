using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    public class ConnectionReadyState : BaseRemoteConnectorState
    {
        private Serilog.ILogger logger = Serilog.Log.Logger.ForContext<ConnectionReadyState>();
        private volatile int _heartbeatInterval = 5000;
        private volatile object _timingLock = new object();
        private long _lastRx;
        private long _lastTx;

        public ConnectionReadyState(IRemoteConnectorContext context) : base(context)
        {
            lock(_timingLock)
            {
                _lastRx = Context.Clock.SystemMillis();
                _lastTx = Context.Clock.SystemMillis();
            }
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.CONNECTION_READY;

        public override bool BeforeSendCommandToRemote(MenuCommand command)
        {
            lock (_timingLock) _lastTx = Context.Clock.SystemMillis(); 
            return true;
        }

        public override void ProcessCommand(MenuCommand command)
        {
            lock (_timingLock) _lastRx = Context.Clock.SystemMillis();

            if (command is HeartbeatCommand hb)
            {
                if(hb.Mode == HeartbeatMode.NORMAL)
                {
                    if (hb.Interval != _heartbeatInterval)
                    {
                        logger.Information($"Changed HB frequency to {hb.Interval} was {_heartbeatInterval} for {Context.ConnectionName}");
                        _heartbeatInterval = hb.Interval;
                    }
                }
                else 
                {
                    StandardHeartbeatProcessing(hb);
                }                
            } 
            else
            {
                Context.PublishToController(command);
            }
        }

        public override void Tick()
        {
            if(!Context.DeviceConnected)
            {
                Context.BackToStart("Device became disconnected");
                return;
            }

            bool noMessageRxForTooLong;
            bool needToSendHB;
            lock (_timingLock)
            {
                noMessageRxForTooLong = (Context.Clock.SystemMillis() - _lastRx) > (3 * _heartbeatInterval);
                needToSendHB = (Context.Clock.SystemMillis() - _lastTx) > _heartbeatInterval;
            }

            if (noMessageRxForTooLong)
            {
                Context.BackToStart("RX Heartbeat timeout");
                return;
            }

            if (needToSendHB)
            {
                logger.Information($"Sending heartbeat to {Context.ConnectionName}");
                Context.SendHeartbeatCommand(_heartbeatInterval, HeartbeatMode.NORMAL);
            }
        }
    }
}
