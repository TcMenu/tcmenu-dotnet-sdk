using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteStates;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdk.RemoteCore
{
 
    public class RemoteController : IRemoteController
    {
        private ILogger logger = Log.Logger.ForContext<RemoteController>();

        public event AcknolwedgementsReceivedHandler AcknowledgementsReceived;
        public event MenuChangedEventHandler MenuChangedEvent;
        public event DialogUpdatedEventHandler DialogUpdatedEvent;
        private readonly SystemClock _clock;

        public IRemoteConnector Connector { get; }
        public MenuTree ManagedMenu { get; }

        public RemoteController(IRemoteConnector connector, MenuTree managedMenu, SystemClock clock)
        {
            ManagedMenu = managedMenu;
            Connector = connector;
            _clock = clock;
        }

        public void Start()
        {
            logger.Information($"Starting controller for {Connector.ConnectionName}");
            Connector.MessageReceived += ConnectorMessageReceived;
            Connector.Start();
        }

        public void Stop(bool waitForCompleteStop = false)
        {
             logger.Information($"Stopping controller for {Connector.ConnectionName} instance, wait for complete is {waitForCompleteStop}");
            Connector.MessageReceived -= ConnectorMessageReceived;
            Connector.Stop(waitForCompleteStop);
            logger.Information($"Stopped {Connector.ConnectionName}");
        }

        private void ConnectorMessageReceived(MenuCommand command)
        {
            switch (command)
            {
                case AcknowledgementCommand ack:
                    ProcessAck(ack);
                    break;
                case MenuChangeCommand mc:
                    ProcessChangeCommand(mc);
                    break;
                case DialogCommand dc:
                    ProcessDialogUpdate(dc);
                    break;
                case AnalogBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case LargeNumberBootstrapCommand ln:
                    ProcessBootCommand(ln);
                    break;
                case EnumBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case BooleanBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case FloatBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case TextBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case RuntimeListBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case ActionBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case SubMenuBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case Rgb32BootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                case ScrollChoiceBootstrapCommand b:
                    ProcessBootCommand(b);
                    break;
                default:
                    logger.Warning($"Controller on {Connector.ConnectionName} has unexpected message: {command}");
                    break;
            }
        }

        private void ProcessDialogUpdate(DialogCommand dc)
        {
            DialogUpdatedEvent?.Invoke(dc.Correlation, dc.Mode, dc.Header, dc.Message, dc.Button1, dc.Button2);
        }

        private void ProcessChangeCommand(MenuChangeCommand command)
        {
            if (Connector.AuthStatus != AuthenticationStatus.CONNECTION_READY) return;

            var item = ManagedMenu.GetMenuById(command.MenuId);
            switch(item)
            {
                case AnalogMenuItem an:
                    ChangeIntegerState(an, command);
                    break;
                case EnumMenuItem en:
                    ChangeIntegerState(en, command);
                    break;
                case BooleanMenuItem bl:
                    ChangeBooleanState(bl, command);
                    break;
                case EditableTextMenuItem txt:
                    ChangeStringState(txt, command);
                    break;
                case FloatMenuItem flt:
                    ChangeFloatState(flt, command);
                    break;
                case SubMenuItem sub:
                    ChangeBooleanState(sub, command);
                    break;
                case ActionMenuItem act:
                    ChangeBooleanState(act, command);
                    break;
                case RuntimeListMenuItem list:
                    ChangeListState(list, command);
                    break;
                case LargeNumberMenuItem largeNum:
                    ChangeDecimalState(largeNum, command);
                    break;
                case Rgb32MenuItem rgb:
                    ChangeRgbState(rgb, command);
                    break;
                case ScrollChoiceMenuItem sc:
                    ChangeScrollState(sc, command);
                    break;
                default:
                    // default is to ignore and not invoke the delegate.
                    return;
            }
            MenuChangedEvent?.Invoke(item, true);
        }

        private void ChangeRgbState(Rgb32MenuItem rgb, MenuChangeCommand command)
        {
            logger.Debug($"Processing rgb change {command} on {rgb}");
            var old = ManagedMenu.GetState(rgb) as MenuState<PortableColor>;
            var newVal = new PortableColor(command.Value);
            ManagedMenu.ChangeItemState(rgb, new MenuState<PortableColor>(rgb, true, old?.Active ?? false, newVal));
        }

        private void ChangeScrollState(ScrollChoiceMenuItem sc, MenuChangeCommand command)
        {
            logger.Debug($"Processing scroll choice change {command} on {sc}");
            var old = ManagedMenu.GetState(sc) as MenuState<CurrentScrollPosition>;
            var newVal = new CurrentScrollPosition(command.Value);
            ManagedMenu.ChangeItemState(sc, new MenuState<CurrentScrollPosition>(sc, true, old?.Active ?? false, newVal));
        }

        private void ChangeListState(MenuItem an, MenuChangeCommand command)
        {
            logger.Debug($"Processing list change {command} on {an}");
            var old = ManagedMenu.GetState(an) as MenuState<List<string>>;
            ManagedMenu.ChangeItemState(an, new MenuState<List<String>>(an, true, old?.Active ?? false, command.ListValues));
        }

        private void ChangeFloatState(MenuItem an, MenuChangeCommand command)
        {
            logger.Debug($"Processing float change {command} on {an}");
            var old = ManagedMenu.GetState(an) as MenuState<float>;
            float val = float.Parse(command.Value);
            ManagedMenu.ChangeItemState(an, new MenuState<float>(an, Math.Abs((old?.Value ?? 0) - val) > 0.000001, old?.Active ?? false, val));
        }

        private void ChangeIntegerState(MenuItem an, MenuChangeCommand command)
        {
            logger.Debug($"Processing integer change {command} on {an}");
            var old = ManagedMenu.GetState(an) as MenuState<int>;
            int val = int.Parse(command.Value);
            if (command.ChangeType == ChangeType.DELTA && old != null) val = old.Value + val;
            ManagedMenu.ChangeItemState(an, new MenuState<int>(an, (old?.Value ?? 0) != val, old?.Active ?? false, val));
        }

        private void ChangeDecimalState(MenuItem an, MenuChangeCommand command)
        {
            logger.Debug($"Processing decimal change {command} on {an}");
            var old = ManagedMenu.GetState(an) as MenuState<decimal>;
            var strRaw = command.Value;
            strRaw = strRaw.Replace("[", ""); // sometimes the device will send edit strings including [ and ], remove!
            strRaw = strRaw.Replace("]", "");
            decimal val = decimal.Parse(strRaw);
            ManagedMenu.ChangeItemState(an, new MenuState<decimal>(an, (old?.Value ?? 0) != val, old?.Active ?? false, val));
        }

        private void ChangeBooleanState(MenuItem en, MenuChangeCommand command)
        {
            logger.Debug($"Processing bool change {command} on {en}");
            var old = ManagedMenu.GetState(en) as MenuState<bool>;
            bool val = int.Parse(command.Value) != 0;
            ManagedMenu.ChangeItemState(en, new MenuState<bool>(en, (old?.Value ?? false) != val, old?.Active ?? false, val));
        }

        private void ChangeStringState(MenuItem ss, MenuChangeCommand command)
        {
            logger.Debug($"Processing text change {command} on {ss}");
            var old = ManagedMenu.GetState(ss) as MenuState<string>;
            var same = old?.Value?.Equals(command.Value) ?? false;
            ManagedMenu.ChangeItemState(ss, new MenuState<string>(ss, !same, old?.Active ?? false, command.Value));
        }

        private void ProcessBootCommand<T,V>(BootstrapMenuCommand<T, V> command) where T: MenuItem
        {
            var oldState = ManagedMenu.GetState(command.Item);
            ManagedMenu.AddOrUpdateItem(command.SubMenuId, command.Item);
            ManagedMenu.ChangeItemState(command.Item, command.NewMenuState(oldState));
            MenuChangedEvent?.Invoke(command.Item, false);
        }

        private void ProcessAck(AcknowledgementCommand ack)
        {
            AcknowledgementsReceived?.Invoke(ack.Correlation, ack.Status);
        }

        public async Task<CorrelationId> SendDeltaChange(MenuItem item, int difference)
        {
            CorrelationId corr = new CorrelationId(_clock);
            await SendToConnectorAsTask(new MenuChangeCommand(item.Id, corr, ChangeType.DELTA, difference.ToString()));
            return corr;
        }

        public async Task<CorrelationId> SendAbsoluteChange(MenuItem item, object newValue)
        {
            CorrelationId corr = new CorrelationId(_clock);
            if (newValue is List<string> strList)
            {
                await SendToConnectorAsTask(new MenuChangeCommand(item.Id, corr, ChangeType.CHANGE_LIST, strList));
            }
            else
            {
                if(newValue is bool b)
                {
                    newValue = b ? 1 : 0;
                }
                await SendToConnectorAsTask(new MenuChangeCommand(item.Id, corr, ChangeType.ABSOLUTE, newValue?.ToString() ?? ""));
            }
            return corr;
        }

        public async Task<CorrelationId> SendDialogAction(MenuButtonType buttonType)
        {
            CorrelationId corr = new CorrelationId(_clock);
            await SendToConnectorAsTask(new DialogCommand(DialogMode.ACTION,  "", "", buttonType, buttonType, corr));

            return corr;
        }

        Task SendToConnectorAsTask(MenuCommand command)
        {
            return Task.Run(() =>Connector.SendMenuCommand(command));
        }
    }
}
