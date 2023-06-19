using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    public class SendAndProcessJoinState : BaseRemoteConnectorState
    {
        public SendAndProcessJoinState(IRemoteConnectorContext context) : base(context)
        {            
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.SEND_JOIN;

        public override bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return command.CommandType == HeartbeatCommand.HEARTBEAT_CMD_ID || command.CommandType == NewJoinerCommand.NEW_JOINER_CMD_ID;
        } 

        public override void EnterState()
        {
            base.EnterState();
            Context.SendJoinCommand();
        }

        public override void ProcessCommand(MenuCommand command)
        {
            if(command is HeartbeatCommand hb)
            {
                StandardHeartbeatProcessing(hb);
            }
            else if(command is AcknowledgementCommand ack)
            {
                if (ack.Status == AckStatus.SUCCESS)
                {
                    Context.ChangeState(AuthenticationStatus.AUTHENTICATED);
                }
                else
                {
                    Context.ChangeState(AuthenticationStatus.FAILED_AUTH);
                }
            }
        }
    }
}
