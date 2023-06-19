using Serilog;
using System;
using System.Threading;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    public class StreamNotConnectedState : IRemoteConnectorState
    {
        private readonly ILogger logger = Log.Logger.ForContext(typeof(StreamNotConnectedState));

        private readonly IRemoteConnectorContext _context;
        private volatile int _delay = 1500;
        private readonly object _monintorObj = new object();
        private volatile bool _exit;

        public StreamNotConnectedState(IRemoteConnectorContext context)
        {
            _context = context;
            _exit = false;
        }

        public bool NeedsRead => false;

        public AuthenticationStatus AuthStatus => AuthenticationStatus.AWAITING_CONNECTION;

        public void ThreadProc()
        {
            logger.Debug("NotConnectedState - connection loop start");
            while (!_exit)
            {
                // if we are already connected, then no need to go through this again. Just move to the next state and don't
                // try connecting again, as that will just make matters worse.
                if (_context.DeviceConnected)
                {
                    _context.ChangeState(AuthenticationStatus.ESTABLISHED_CONNECTION);
                    return;
                }
                try
                {
                    if (DateTime.Now.Subtract(_context.LastConnectionAttempt).TotalSeconds < 2)
                    {
                        Thread.Sleep(2000);
                    }

                    if (_context.PerformConnection())
                    {
                        logger.Information($"Device is connected for {_context.ConnectionName}");
                        _context.ChangeState(AuthenticationStatus.ESTABLISHED_CONNECTION);
                    }

                    if (!_context.DeviceConnected && !_exit)
                    {
                        logger.Information($"Waiting before reconnection to {_context.ConnectionName} for {_delay}ms");
                        lock (_monintorObj)
                        {
                            Monitor.Wait(_monintorObj, _delay);
                        }
                        _delay = Math.Min(60000, _delay + _delay); // semi exponential backoff to 60 seconds
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Not connected to {_context.ConnectionName}");
                }
            }
            logger.Debug("NotConnectedState - connection loop end");
        }

        public void EnterState()
        {
            
        }

        public void ExitState(IRemoteConnectorState prevState)
        {
            _exit = true;
            lock(_monintorObj)
            {
                Monitor.PulseAll(_monintorObj);
            }
        }

        public void ProcessCommand(MenuCommand command)
        {
            // no messages should be expected.
        }

        public void Tick()
        {
            // in this state we don't care how long it takes, we are waiting
            // for a hardware connection.
        }

        public bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return false; // no commands should be sent.
        }
    }
}
