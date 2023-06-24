using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenuCoreMaui.Windows.Serial
{
    public class SerialWaitingForInitialMsg : BaseRemoteConnectorState
    {
        private ILogger logger = Log.Logger.ForContext<SerialWaitingForInitialMsg>();

        private volatile bool _connectedNow = false;

        public SerialWaitingForInitialMsg(IRemoteConnectorContext context) : base(context, 20000)
        {
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.AWAITING_CONNECTION;

        public override bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return command is HeartbeatCommand || command is NewJoinerCommand;
        }

        public override void ProcessCommand(MenuCommand command)
        {
            if(command is HeartbeatCommand hb)
            {
                if (hb.Mode == HeartbeatMode.START)
                {
                    // we send a start back to initiate the connection logic on the remote
                    // this forces the remote 
                    Context.SendHeartbeatCommand(5000, HeartbeatMode.START);
                    logger.Information("Both sides now sent starting message");
                }
            }
            else if (command is NewJoinerCommand nj)
            {
                RemoteInformation ri = new RemoteInformation(nj.Name, nj.ApiVersion, nj.ApiPlatform, nj.Uuid, nj.SerialNumber);
                Context.SetRemoteParty(ri);
                Context.ChangeState(AuthenticationStatus.SEND_JOIN); Context.SendJoinCommand();
            }
        }
    }
}
