using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.Serialisation;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{
    public class MauiBoolEditorComponent : BaseBoolEditorComponent
    {
        private Button _button = null;
        private readonly MenuActionConsumer _consumer;

        public MauiBoolEditorComponent(IRemoteController controller, ComponentSettings settings, MenuItem item) : base(controller, settings, item)
        {
            _consumer = null;
        }

        public MauiBoolEditorComponent(IRemoteController controller, ComponentSettings settings, MenuItem item, MenuActionConsumer consumer) 
            : base(controller, settings, item)
        {
            _consumer = consumer;
        }

        public override View ViewItem
        {
            get
            {
                if (_button == null)
                {
                    _button = new Button
                    {
                        FontSize = ToScaledSize(DrawingSettings.FontInfo)
                    };
                    _button.Clicked += ButtonOnClicked;
                }
                return _button;
            }
        }

        private async void ButtonOnClicked(object sender, EventArgs e)
        {
            if (_consumer != null)
            {
                _consumer.Invoke(_item);
            }
            else if(!_item.ReadOnly)
            {
                await ToggleState();
            }
        }

        public override void ChangeControlSettings(RenderStatus status, string text)
        {
            _button.Text = text;
            _button.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.BUTTON).AsXamarin();
            _button.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.BUTTON).AsXamarin();
        }
    }

    public abstract class FormsEditorBase<T> : BaseTextEditorComponent<T>
    {
        protected View MakeTextComponent(View entryField, bool needSendBtn, EventHandler sendHandler = null)
        {
            var needLabel = (DrawingSettings.DrawMode & RedrawingMode.ShowNameInLabel) != 0;

            if (!needLabel && !needSendBtn) return entryField;

            var grid = new Grid
            {
                VerticalOptions = LayoutOptions.Center
            };

            var col = 0;
            if (needLabel)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                Grid.SetColumn(entryField, col++);
                var lbl = new Label
                {
                    Text = _item.Name,
                    TextColor = DrawingSettings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.TEXT_FIELD).AsXamarin(),
                    FontSize = ToScaledSize(DrawingSettings.FontInfo),
                    Padding = new Thickness(2, 5)
                };
                grid.Children.Add(lbl);

            }
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            Grid.SetColumn(entryField, col++);
            grid.Children.Add(entryField);

            if (needSendBtn)
            {
                var sendButton = new Button
                {
                    Text = "Send",
                    BackgroundColor = DrawingSettings.Colors.BackgroundFor(RenderStatus.Normal, ColorComponentType.BUTTON).AsXamarin(),
                    TextColor = DrawingSettings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.BUTTON).AsXamarin(),
                    HorizontalOptions = LayoutOptions.Center
                };

                if (sendHandler != null) sendButton.Clicked += sendHandler;

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(sendButton, col);
                grid.Children.Add(sendButton);
            }

            return grid;
        }

        protected FormsEditorBase(IRemoteController controller, ComponentSettings settings, MenuItem item)
            : base(controller, settings, item)
        {
        }
    }

    public class MauiTextEditorComponent<T> : FormsEditorBase<T>
    {
        private Entry _textEntry;

        public MauiTextEditorComponent(IRemoteController controller, ComponentSettings settings, MenuItem item) : base(controller, settings, item)
        {
        }

        public override View ViewItem {
            get
            {
                if (_textEntry == null)
                {
                    _textEntry = new Entry
                    {
                        Text = GetControlText(),
                        BackgroundColor = DrawingSettings.Colors.BackgroundFor(RenderStatus.Normal, ColorComponentType.TEXT_FIELD).AsXamarin(),
                        TextColor = DrawingSettings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.TEXT_FIELD).AsXamarin(),
                        IsEnabled = !_item.ReadOnly
                    };
                }
                return MakeTextComponent(_textEntry, IsItemEditable(_item), SendButton_Clicked);
            }
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            await ValidateAndSend(_textEntry.Text).ConfigureAwait(true);
        }

        public override void ChangeControlSettings(RenderStatus status, string text)
        {
            _textEntry.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
            _textEntry.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
            _textEntry.Text = text;
        }
    }

    public abstract class AbstractMauiUpDownControl<T> : BaseUpDownIntEditorComponent<T>
    {
        private Button _back;
        private Button _fwd;
        private Label _text;
        private Grid _grid;

        protected AbstractMauiUpDownControl(IRemoteController controller, ComponentSettings settings, MenuItem item) : base(controller, settings, item)
        {
        }

        public override View ViewItem
        {
            get
            {
                if (_grid == null)
                {
                    CreateComponents();
                }

                return _grid;
            }
        }

        public abstract override int CurrentInt { get; }


        private void FwdButton_Click(object sender, EventArgs e)
        {
            BumpCount(1);
        }

        private void BackBtn_Click(object sender, EventArgs e)
        {
            BumpCount(-1);
        }

        public void CreateComponents()
        {
            _grid = new Grid
            {
                BackgroundColor = DrawingSettings.Colors.BackgroundFor(RenderStatus.Normal, ColorComponentType.TEXT_FIELD).AsXamarin(),
                ColumnSpacing = 5,
                RowSpacing = 5,
            };

            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _back = new Button
            {
                Text = "<",
                BackgroundColor = DrawingSettings.Colors.BackgroundFor(RenderStatus.Normal, ColorComponentType.BUTTON).AsXamarin(),
                TextColor = DrawingSettings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.BUTTON).AsXamarin(),
                HorizontalOptions = LayoutOptions.Center
            };

            _fwd = new Button
            {
                Text = ">",
                BackgroundColor = DrawingSettings.Colors.BackgroundFor(RenderStatus.Normal, ColorComponentType.BUTTON).AsXamarin(),
                TextColor = DrawingSettings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.BUTTON).AsXamarin(),
                HorizontalOptions = LayoutOptions.Center
            };

            _text = new Label
            {
                Text = _item.Name,
                TextColor = DrawingSettings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.TEXT_FIELD).AsXamarin(),
                FontSize = ToScaledSize(DrawingSettings.FontInfo),
                HorizontalTextAlignment = ToMauiTextAlignment(DrawingSettings.Justification)
            };

            _grid.Children.Add(_back);
            Grid.SetRow(_back, 0);
            Grid.SetColumn(_back, 0);
            _grid.Children.Add(_text);
            Grid.SetRow(_text, 0);
            Grid.SetColumn(_text, 1);
            _grid.Children.Add(_fwd);
            Grid.SetRow(_fwd, 0);
            Grid.SetColumn(_fwd, 2);

            if (_item.ReadOnly)
            {
                _fwd.IsEnabled = false;
                _back.IsEnabled = false;
            }
            else
            {
                _fwd.Clicked += FwdButton_Click;
                _back.Clicked += BackBtn_Click;
            }
        }

        public override void ChangeControlSettings(RenderStatus status, string str)
        {
            if (_text == null) return;

            _text.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
            _text.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
            _back.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.BUTTON).AsXamarin();
            _back.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.BUTTON).AsXamarin();
            _fwd.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.BUTTON).AsXamarin();
            _fwd.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.BUTTON).AsXamarin();
            _text.Text = str;
        }
    }

    public class IntegerUpDownEditorComponent : AbstractMauiUpDownControl<int>
    {
        public IntegerUpDownEditorComponent(MenuItem item, IRemoteController remote, ComponentSettings settings)
            : base(remote, settings, item)
        {
        }

        public override int CurrentInt => _currentVal;
    }

    public class ScrollUpDownEditorComponent : AbstractMauiUpDownControl<CurrentScrollPosition>
    {
        public ScrollUpDownEditorComponent(MenuItem item, IRemoteController remote, ComponentSettings settings)
            : base(remote, settings, item)
        {
        }

        public override string GetControlText()
        {
            return _currentVal.Value;
        }


        protected override async void BumpCount(int delta)
        {
            if (_status == RenderStatus.EditInProgress) return;
            try
            {
                var posNow = _currentVal.Position;
                var csp = new CurrentScrollPosition(posNow + delta, "");
                var correlation = await _remoteController.SendAbsoluteChange(_item, csp.ToString());
                EditStarted(correlation);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to send message to {_remoteController.Connector.ConnectionName}");
            }
        }

        public override int CurrentInt => _currentVal.Position;
    }

    public class RgbColorEditorComponent : FormsEditorBase<PortableColor>
    {
        private Button _colorButton;
        private View _view = null;

        public RgbColorEditorComponent(MenuItem item, IRemoteController remote, ComponentSettings settings)
            : base(remote, settings, item)
        {
        }

        public void CreateComponents()
        {
            _colorButton = new Button()
            {
                BackgroundColor = Colors.Gray
            };

            if (IsItemEditable(_item))
            {
                _colorButton.Clicked += Button_Click;
            }

            _view = MakeTextComponent(_colorButton, false);
        }


        private void Button_Click(object sender, EventArgs e)
        {
            //var navigator = ApplContext.Instance.NavigationManager;
            //var dialog = new ColorDialog(CurrentVal.AsXamarin(), _item.Name, navigator, ColorChangeNotify);
            //navigator.PushPageOn(dialog);
        }

        private async void ColorChangeNotify(Color newColor)
        {
            await ValidateAndSend(newColor.ToPortable().ToString());
        }

        public override View ViewItem
        {
            get
            {
                if (_colorButton == null)
                {
                    CreateComponents();
                }

                return _view;
            }
        }

        public override void ChangeControlSettings(RenderStatus status, string str)
        {
            _colorButton.BackgroundColor = CurrentVal.AsXamarin();
            _colorButton.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.BUTTON).AsXamarin();
            _colorButton.Text = str;
        }
    }

    public class DateFieldEditorComponent : FormsEditorBase<string>
    {
        private DatePicker _dateField;
        private View _view = null;

        public DateFieldEditorComponent(IRemoteController remote, ComponentSettings settings, MenuItem item)
            : base(remote, settings, item)
        {
        }

        public void CreateComponents()
        {
            _dateField = new DatePicker { Date = DateTime.Now, IsEnabled = !_item.ReadOnly, };

            _view = MakeTextComponent(_dateField, true, DateSendToRemote);
        }

        private async void DateSendToRemote(object sender, EventArgs e)
        {
            var dateStr = _dateField.Date.ToString("yyyy/MM/dd");
            await ValidateAndSend(dateStr).ConfigureAwait(true);
        }

        public override View ViewItem
        {
            get
            {
                if (_view == null)
                {
                    CreateComponents();
                }
                return _view;
            }
        }

        public override void ChangeControlSettings(RenderStatus status, string str)
        {
            if (DateTime.TryParseExact(str, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var theDate))
            {
                _dateField.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
                _dateField.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
                _dateField.Date = theDate;
            }
        }
    }

    public class ListEditorComponent : BaseEditorComponent
    {
        private readonly RuntimeListStringAdapter _stringAdapter;
        private readonly ObservableCollection<string> _actualData = new ObservableCollection<string>();
        private ListView _listView;

        public ListEditorComponent(IRemoteController remote, ComponentSettings settings, MenuItem item,
            RuntimeListStringAdapter adapter = null)
            : base(remote, settings, item)
        {
            _stringAdapter = adapter;
        }

        public override void ChangeControlSettings(RenderStatus status, string text)
        {
            _listView.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.HIGHLIGHT).AsXamarin();
        }

        public void CreateComponents()
        {
            if (_item is RuntimeListMenuItem listItem)
            {
                _listView = new ListView
                {
                    ItemsSource = _actualData,
                    BackgroundColor = DrawingSettings.Colors.BackgroundFor(RenderStatus.Normal, ColorComponentType.HIGHLIGHT).AsXamarin(),
                    HeightRequest = 20 * listItem.InitialRows,
                };
            }
            else
            {
                throw new InvalidOperationException("Not a list object" + _item);
            }
        }

        public override string GetControlText()
        {
            return null;
        }

        public override View ViewItem
        {
            get
            {
                if (_listView == null)
                {
                    CreateComponents();
                }
                return _listView;
            }
        }

        public override void OnItemUpdated(AnyMenuState newValue)
        {
            if (newValue is MenuState<List<string>> listState) UpdateAll(listState.Value);
        }

        private void UpdateAll(List<string> values)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                _actualData.Clear();
                foreach (var val in values)
                {
                    var item = (_stringAdapter != null) ? _stringAdapter.Invoke(val) : val;
                    _actualData.Add(item);
                }
            });
        }
    }

    public class TimeFieldEditorComponent : FormsEditorBase<string>
    {
        private TimePicker _timeField;
        private View _view;

        public TimeFieldEditorComponent(IRemoteController remote, ComponentSettings settings, MenuItem item)
            : base(remote, settings, item)
        {
        }

        private string GetFormat()
        {
            if (_item is EditableTextMenuItem timeItem)
            {
                return (timeItem.EditType == EditItemType.TIME_12H) ? "hh:mm:sstt" :
                    (timeItem.EditType == EditItemType.TIME_24H) ? "HH:mm:ss" : "HH:mm:ss.ff";
            }

            return "T";
        }

        public void CreateComponents()
        {
            if (_item is EditableTextMenuItem timeItem)
            {
                _timeField = new TimePicker { Format = GetFormat(), IsEnabled = !_item.ReadOnly, };

                _view = MakeTextComponent(_timeField, !_item.ReadOnly, TimeComponentSend);
            }

            throw new ArgumentException($"{_item} is not a time item");
        }

        private async void TimeComponentSend(object sender, EventArgs e)
        {
            var theTime = DateTime.Now + _timeField.Time;
            await ValidateAndSend(theTime.ToString("HH:mm:ss")).ConfigureAwait(true);
        }

        public override View ViewItem
        {
            get
            {
                if (_view == null)
                {
                    CreateComponents();
                }
                return _view;
            }
        }

        public override void ChangeControlSettings(RenderStatus status, string str)
        {
            _timeField.BackgroundColor = DrawingSettings.Colors.BackgroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();
            _timeField.TextColor = DrawingSettings.Colors.ForegroundFor(status, ColorComponentType.TEXT_FIELD).AsXamarin();

            var strippedStr = str.Replace("[", "");
            strippedStr = strippedStr.Replace("]", "");

            if (DateTime.TryParseExact(strippedStr, GetFormat(), CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var theTime))
            {
                _timeField.Time = theTime.TimeOfDay;
            }
        }
    }

}
