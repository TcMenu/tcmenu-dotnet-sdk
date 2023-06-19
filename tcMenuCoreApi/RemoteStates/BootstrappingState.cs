using System.Collections.Generic;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;

namespace TcMenu.CoreSdk.RemoteStates
{
    public class BootstrappingState : BaseRemoteConnectorState
    {
        private readonly HashSet<ushort> BOOT_COMMANDS = new HashSet<ushort>
        {
            AnalogBootstrapCommand.ANALOG_BOOT_CMD,
            EnumBootstrapCommand.ENUM_BOOT_CMD,
            BooleanBootstrapCommand.BOOLEAN_BOOT_CMD,
            SubMenuBootstrapCommand.SUBMENU_BOOT_CMD,
            ActionBootstrapCommand.ACTION_BOOT_CMD,
            FloatBootstrapCommand.FLOAT_BOOT_CMD,
            TextBootstrapCommand.TEXT_BOOT_CMD,
            LargeNumberBootstrapCommand.DECIMAL_BOOT_CMD,
            RuntimeListBootstrapCommand.LIST_BOOT_CMD,
            LargeNumberBootstrapCommand.DECIMAL_BOOT_CMD,
            Rgb32BootstrapCommand.RGB32_BOOT_CMD,
            ScrollChoiceBootstrapCommand.SCROLL_BOOT_CMD
        };

        public BootstrappingState(IRemoteConnectorContext context) : base(context)
        {
        }

        public override bool NeedsRead => true;

        public override AuthenticationStatus AuthStatus => AuthenticationStatus.BOOTSTRAPPING;

        public override void ProcessCommand(MenuCommand command)
        {
            if(command is HeartbeatCommand hb)
            {
                StandardHeartbeatProcessing(hb);
            }
            else if(command is BootstrapCommand boot)
            {
                if(boot.BootType == BootstrapType.END)
                {
                    Context.ChangeState(AuthenticationStatus.CONNECTION_READY);
                    Context.PublishToController(boot);
                }
                else
                {
                    Context.BackToStart("Unexpected second bootstrap START");
                }
            }
            else if(BOOT_COMMANDS.Contains(command.CommandType))
            {
                Context.PublishToController(command);
            }
            else if(command is MenuChangeCommand mc)
            {
                Context.PublishToController(mc);
            }
        }
    }
}
