using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdk.RemoteSimulator
{
    /// <summary>
    /// An implementation of a remote connection that is actually backed locally by a MenuTree.
    /// It is possible to update most types and it acts very much like a regular embedded app.
    /// It is constructed given a menu tree and a name that will be provided as the remote info.
    /// 
    /// It always accepts connections and does not support pairing or any other form of security.
    /// </summary>
    public class SimulatedRemoteConnection : IRemoteConnector
    {
        private readonly Serilog.ILogger logger = Serilog.Log.Logger.ForContext<SimulatedRemoteConnection>();
        private readonly MenuTree _tree;
        private readonly string _simName;
        private readonly string _uuid = Guid.NewGuid().ToString();
        private readonly int _latencyMillis;
        private readonly Dictionary<int, object> _valuesById;
        private readonly RuntimeListMenuItem _simUpdateList;
        private readonly List<string> _recentUpdates = new List<string>();

        public string ConnectionName => _simName;

        public AuthenticationStatus AuthStatus { get; private set; }

        public RemoteInformation RemoteInfo { get; }

        public event ConnectionChangedHandler ConnectionChanged;
        public event MessageReceivedHandler MessageReceived;

        public SimulatedRemoteConnection(MenuTree tree, string simName, int latencyMillis = 100, 
                                         Dictionary<int, object> valuesById = null) 
        {
            _tree = tree;
            _simName = simName;
            _latencyMillis = latencyMillis;
            _valuesById = valuesById;
            RemoteInfo = new RemoteInformation(_simName, 101, ApiPlatform.DNET_API, _uuid, "123456");
            var simulatorMenu = new SubMenuItemBuilder()
                .WithId(60001)
                .WithName("Simulator Options")
                .Build();
            _tree.AddMenuItem(MenuTree.ROOT, simulatorMenu);

            _simUpdateList = new RuntimeListMenuItemBuilder()
                .WithId(60002)
                .WithName("Recent Updates")
                .WithInitialRows(10)
                .Build();
            _tree.AddMenuItem(simulatorMenu, _simUpdateList);
        }

        public async void Close()
        {
            await Task.Delay(1000);
            Start();
            
        }

        public void SendMenuCommand(MenuCommand command)
        {
            if(AuthStatus == AuthenticationStatus.NOT_STARTED)
            {
                logger.Debug("Msg before start called");
            }

            switch (command)
            {
                case MenuChangeCommand ch:
                    ProcessChange(ch);
                    break;
                case DialogCommand dc:
                    ProcessDialog(dc);
                    break;
            }
        }

        private async void ProcessDialog(DialogCommand dc)
        {
            await Task.Delay(_latencyMillis);

            if(dc.Mode == DialogMode.ACTION && dc.Correlation != null)
            {
                SendDialogAction(DialogMode.HIDE, "", "", MenuButtonType.NONE, MenuButtonType.NONE);
                MessageReceived?.Invoke(new AcknowledgementCommand(dc.Correlation, AckStatus.SUCCESS));
            }
        }

        private async void ProcessChange(MenuChangeCommand ch)
        {
            await Task.Delay(_latencyMillis);

            var item = _tree.GetMenuById(ch.MenuId);
            if(ch.ChangeType == ChangeType.DELTA)
            {
                logger.Debug($"Delta change on id {item.Id}");
                var state = _tree.GetState(item) as MenuState<int>;
                var prevVal = state?.Value ?? 0;
                int newVal = prevVal + int.Parse(ch.Value);

                if (item is AnalogMenuItem analog)
                {
                    if (newVal < 0 || newVal > analog.MaximumValue)
                    {
                        AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.VALUE_RANGE_WARNING);
                        return;
                    }
                }
                else if(item is EnumMenuItem en)
                {
                    if (newVal < 0 || newVal >= en.EnumEntries.Count)
                    {
                        AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.VALUE_RANGE_WARNING);
                        return;
                    }
                }
                _tree.ChangeItemState(item, new MenuState<int>(item, true, state?.Active ?? false, newVal));
                MessageReceived?.Invoke(new MenuChangeCommand(item.Id, CorrelationId.EMPTY_CORRELATION, ChangeType.ABSOLUTE, newVal.ToString()));
                AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.SUCCESS);
            }
            else if(ch.ChangeType == ChangeType.ABSOLUTE)
            {
                if (item is AnalogMenuItem)
                {
                    logger.Debug($"Analog absolute update on id {item.Id}");
                    var state = _tree.GetState(item) as MenuState<int>;
                    _tree.ChangeItemState(item, new MenuState<int>(item, true, state?.Active ?? false, int.Parse(ch.Value, NumberStyles.Any)));
                    MessageReceived?.Invoke(new MenuChangeCommand(item.Id, CorrelationId.EMPTY_CORRELATION, ChangeType.ABSOLUTE, ch.Value));
                    AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.SUCCESS);
                }
                else if (item is BooleanMenuItem)
                {
                    logger.Debug($"Boolean change on id {item.Id}");
                    var state = _tree.GetState(item) as MenuState<bool>;
                    _tree.ChangeItemState(item, new MenuState<bool>(item, true, state?.Active ?? false, ch.Value == "1"));
                    MessageReceived?.Invoke(new MenuChangeCommand(item.Id, CorrelationId.EMPTY_CORRELATION, ChangeType.ABSOLUTE, ch.Value));
                    AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.SUCCESS);
                }
                else if(item is EditableTextMenuItem)
                {
                    logger.Debug($"Text change on id {item.Id}");
                    var state = _tree.GetState(item) as MenuState<string>;
                    _tree.ChangeItemState(item, new MenuState<string>(item, true, state?.Active ?? false, ch.Value));
                    MessageReceived?.Invoke(new MenuChangeCommand(item.Id, CorrelationId.EMPTY_CORRELATION, ChangeType.ABSOLUTE, ch.Value));
                    AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.SUCCESS);
                }
                else if(item is LargeNumberMenuItem)
                {
                    logger.Debug($"Large number change on id {item.Id}");
                    var state = _tree.GetState(item) as MenuState<decimal>;
                    _tree.ChangeItemState(item, new MenuState<decimal>(item, true, state?.Active ?? false, decimal.Parse(ch.Value)));
                    MessageReceived?.Invoke(new MenuChangeCommand(item.Id, CorrelationId.EMPTY_CORRELATION, ChangeType.ABSOLUTE, ch.Value));
                    AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.SUCCESS);
                }
                else if(item is ActionMenuItem)
                {
                    logger.Debug($"Action event change on id {item.Id}");
                    AcknowledgeChange(item, ch.Value, ch.Correlation, AckStatus.SUCCESS);
                    SendDialogAction(DialogMode.SHOW, item.Name, "Action performed", MenuButtonType.NONE, MenuButtonType.OK);
                }
            }
        }

        private void AcknowledgeChange(MenuItem item, string value, CorrelationId correlation, AckStatus status)
        {
            if(_recentUpdates.Count > 20) _recentUpdates.RemoveAt(0);
            _recentUpdates.Add($"{item.Name} {value} - {status}");
            MessageReceived?.Invoke(new MenuChangeCommand(
                _simUpdateList.Id,
                CorrelationId.EMPTY_CORRELATION, 
                ChangeType.CHANGE_LIST,
                _recentUpdates));
            MessageReceived?.Invoke(new AcknowledgementCommand(correlation, status));
        }

        public async void SendDialogAction(DialogMode mode, string title, string desc, MenuButtonType button1, MenuButtonType button2, 
                                     CorrelationId correlation = null)
        {
            await Task.Delay(_latencyMillis);
            MessageReceived?.Invoke(new DialogCommand(mode, title, desc, button1, button2, correlation ?? CorrelationId.EMPTY_CORRELATION));
        }

        private void SendCommandFor(MenuItem item)
        {
            logger.Debug($"Send {item}");

            var parId = _tree.FindParent(item).Id;
            switch (item)
            {
                case AnalogMenuItem an:
                    MessageReceived?.Invoke(new AnalogBootstrapCommand(parId, an, GetForIdIfAvailable<int>(item)));
                    break;
                case EnumMenuItem en:
                    MessageReceived?.Invoke(new EnumBootstrapCommand(parId, en, GetForIdIfAvailable<int>(item)));
                    break;
                case BooleanMenuItem en:
                    MessageReceived?.Invoke(new BooleanBootstrapCommand(parId, en, GetForIdIfAvailable<bool>(item)));
                    break;
                case FloatMenuItem fl:
                    MessageReceived?.Invoke(new FloatBootstrapCommand(parId, fl, GetForIdIfAvailable<float>(item)));
                    break;
                case EditableTextMenuItem ed:
                    MessageReceived?.Invoke(new TextBootstrapCommand(parId, ed, GetForIdIfAvailable<string>(item)));
                    break;
                case SubMenuItem sm:
                    MessageReceived?.Invoke(new SubMenuBootstrapCommand(parId, sm, GetForIdIfAvailable<bool>(item)));
                    break;
                case LargeNumberMenuItem ln:
                    MessageReceived?.Invoke(new LargeNumberBootstrapCommand(parId, ln, GetForIdIfAvailable<decimal>(item)));
                    break;
                case ActionMenuItem am:
                    MessageReceived?.Invoke(new ActionBootstrapCommand(parId, am, GetForIdIfAvailable<bool>(item)));
                    break;
                case RuntimeListMenuItem rl:
                    MessageReceived?.Invoke(new RuntimeListBootstrapCommand(parId, rl, new List<string>()));
                    break;
                case Rgb32MenuItem rgb:
                    MessageReceived?.Invoke(new Rgb32BootstrapCommand(parId, rgb, new PortableColor("#000")));
                    break;
                case ScrollChoiceMenuItem sc:
                    MessageReceived?.Invoke(new ScrollChoiceBootstrapCommand(parId, sc, new CurrentScrollPosition("0-Item")));
                    break;
            }
        }

        private TRet GetForIdIfAvailable<TRet>(MenuItem item)
        {
            if(_valuesById != null && _valuesById.ContainsKey(item.Id))
            {
                var value = (TRet)_valuesById[item.Id];
                _tree.ChangeItemState(item, new MenuState<TRet>(item, true, false, value));
                return value;
            }
            return default;
        }

        private void StateChanged(AuthenticationStatus status)
        {
            AuthStatus = status;
            ConnectionChanged?.Invoke(status);
        }

        public async void Start()
        {
            logger.Debug("Start called");

            await Task.Delay(_latencyMillis);
            StateChanged(AuthenticationStatus.ESTABLISHED_CONNECTION);
            await Task.Delay(_latencyMillis);
            StateChanged(AuthenticationStatus.AUTHENTICATED);
            await Task.Delay(_latencyMillis);
            StateChanged(AuthenticationStatus.BOOTSTRAPPING);
            foreach (var item in _tree.GetAllMenuItems())
            {
                SendCommandFor(item);
            }
            StateChanged(AuthenticationStatus.CONNECTION_READY);
        }

        public async void Stop(bool waitForThread = false)
        {
            logger.Debug("Stop called");
            await Task.Delay(_latencyMillis);
            StateChanged(AuthenticationStatus.AWAITING_CONNECTION);
            StateChanged(AuthenticationStatus.NOT_STARTED);
            
        }
    }
}
