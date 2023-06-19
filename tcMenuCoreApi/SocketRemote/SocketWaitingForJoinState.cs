using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenu.CoreSdk.SocketRemote
{
    /// <summary>
    /// Each connection protocol has it's own way of beginning a connection.
    /// For sockets it's much simpler than serial. Here we know we are connected
    /// to a stream and just wait for the join message.
    /// </summary>
    class SocketWaitingForJoinState : BaseRemoteConnectorState
    {
        public SocketWaitingForJoinState(IRemoteConnectorContext context) : base(context)
        {
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.ESTABLISHED_CONNECTION;

        public override bool BeforeSendCommandToRemote(MenuCommand command)
        {
            return command.CommandType == HeartbeatCommand.HEARTBEAT_CMD_ID ||
                   command.CommandType == NewJoinerCommand.NEW_JOINER_CMD_ID;
        }

        public override void EnterState()
        {
            base.EnterState();
            Context.SendHeartbeatCommand(5000, HeartbeatMode.START);
        }

        public override void ProcessCommand(MenuCommand command)
        {
            if(command is HeartbeatCommand hb)
            {
                StandardHeartbeatProcessing(hb);
            }
            else if(command is NewJoinerCommand nj)
            {
                RemoteInformation ri = new RemoteInformation(nj.Name, nj.ApiVersion, nj.ApiPlatform, nj.Uuid, nj.SerialNumber);
                Context.SetRemoteParty(ri);
                Context.ChangeState(AuthenticationStatus.SEND_JOIN);
            }
        }
    }
}
