using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    public class SendPairingMessageState : BaseRemoteConnectorState
    {
        public SendPairingMessageState(IRemoteConnectorContext context) : base(context)
        {
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.SEND_JOIN;

        public override void EnterState()
        {
            base.EnterState();
            Context.SendPairingCommand();
        }

        public override void ProcessCommand(MenuCommand command)
        {
            if (command is HeartbeatCommand hb)
            {
                StandardHeartbeatProcessing(hb);
            }
            else if (command is AcknowledgementCommand ack)
            {
                Context.ChangeState(ack.Status == AckStatus.SUCCESS ? AuthenticationStatus.AUTHENTICATED : AuthenticationStatus.FAILED_AUTH);
            }
        }

        public override bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return command.CommandType == HeartbeatCommand.HEARTBEAT_CMD_ID ||
                   command.CommandType == NewJoinerCommand.NEW_JOINER_CMD_ID ||
                   command.CommandType == PairingCommand.PAIRING_CMD_ID;

        }

    }
}
