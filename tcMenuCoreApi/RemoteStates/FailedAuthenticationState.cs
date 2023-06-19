using System.Threading;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    public class FailedAuthenticationState : BaseRemoteConnectorState
    {
        private readonly object _monObj = new object();

        public FailedAuthenticationState(IRemoteConnectorContext context) : base(context)
        {
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.FAILED_AUTH;

        public override void ProcessCommand(MenuCommand command)
        {
            // we ignore everything that comes in when we are in this state.
        }

        public override void ExitState(IRemoteConnectorState newState)
        {
            base.ExitState(newState);
            lock (_monObj) Monitor.PulseAll(_monObj);
        }

        public override void ThreadProc()
        {
            lock (_monObj) Monitor.Wait(_monObj, 2000);
            Context.BackToStart("Authentication failed");
        }
    }
}
