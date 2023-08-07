using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using embedControl.Services;
using embedCONTROL.Services;
using TcMenuCoreMaui.BaseSerial;

namespace embedControl.Views
{
    public delegate void ConnectionChangeDelegate(IConnectionConfiguration newConfiguration);

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewConnectionDetail : ContentPage
    {
        private readonly IConnectionConfiguration _existingConfiguration;
        private readonly ConnectionChangeDelegate _changeDelegate;
        private ObservableCollection<SerialPortInformation> _serialItemSource;
        public string SaveMode => _existingConfiguration == null ? "Create Connection" : "Save Changes";
        public string PageTitle => _existingConfiguration == null ? "Create Connection" : "Edit Connection " + _existingConfiguration.Name;
        public bool DiscardButtonNeeded => _existingConfiguration != null;
        public NewConnectionDetail()
        {
            InitializeComponent();
            BindingContext = this;

            if (!ApplicationContext.Instance.IsSerialAvailable)
            {
                SerialPortCheckbox.IsVisible = false;
                SerialPortLabel.IsVisible = false;
            }
        }

        public NewConnectionDetail(IConnectionConfiguration existingConfiguration, ConnectionChangeDelegate changeDelegate)
        {
            _existingConfiguration = existingConfiguration;
            _changeDelegate = changeDelegate;

            InitializeComponent();
            BindingContext = this;

            if (!ApplicationContext.Instance.IsSerialAvailable)
            {
                SerialPortCheckbox.IsVisible = false;
                SerialPortLabel.IsVisible = false;
                if (existingConfiguration is SerialCommsConfiguration) return; // we can't process it.
            }

            ConnectionNameEntry.Text = existingConfiguration.Name;

            if (existingConfiguration is RawSocketConfiguration socketComms)
            {
                ManualSocketCheckbox.IsChecked = true;
                IpAddressHostEntry.Text = socketComms.Host;
                IpAddressPortEntry.Text = socketComms.Port.ToString();
            }
            else if (existingConfiguration is SerialCommsConfiguration serialComms)
            {
                SerialPortCheckbox.IsChecked = true;
                SerialPortList.SelectedItem = serialComms.SerialInfo;
                BaudRatePicker.SelectedItem = serialComms.BaudRate;
            }
            else if (existingConfiguration is SimulatorConfiguration simComms)
            {
                SimulatorCheckbox.IsChecked = true;
                JsonDataEditor.Text = simComms.JsonObjects;
            }
        }

        private async void CancelButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void CreateButton_OnClicked(object sender, EventArgs e)
        {
            if ((ConnectionNameEntry.Text?.Length ?? 0)==0)
            {
                await DisplayAlert("Name is missing", "Please provide a connection name", "Close");
                return;
            }

            IConnectionConfiguration connectionData = null;
            if (ManualSocketCheckbox.IsChecked)
            {
                var ip = IpAddressHostEntry.Text;
                var port = IpAddressPortEntry.Text;
                if (ip?.Length != 0 && port?.Length != 0 && ushort.TryParse(port, NumberStyles.Integer, CultureInfo.InvariantCulture, out var actPort))
                {
                    connectionData = new RawSocketConfiguration(ip, actPort);
                }
                else
                {
                    await DisplayAlert("Invalid IP or port", "Please ensure you provide a host or IP address and a numeric port", "OK");
                    return;
                }
            }
            else if (SimulatorCheckbox.IsChecked)
            {
                connectionData = new SimulatorConfiguration()
                {
                    JsonObjects = JsonDataEditor.Text?.Trim() ?? ""
                };
                
            }
            else if (SerialPortCheckbox.IsChecked)
            {
                if (BaudRatePicker.SelectedIndex != -1 && SerialPortList.SelectedItem is SerialPortInformation spi)
                {
                    connectionData = new SerialCommsConfiguration(spi, BaudRatePicker.SelectedItem as int? ?? 115200);
                }
                else
                {
                    await DisplayAlert("Serial settings incorrect", "Please choose a serial port and baud rate", "OK");
                    return;
                }
            }

            if(connectionData == null)
            {
                await DisplayAlert("Internal error", "Unable to create a connection with the selected parameters", "OK");
                return;
            }

            var connection = new TcMenuPanelSettings(-1, ConnectionNameEntry.Text, connectionData, DateTime.Now);
            if (_existingConfiguration == null)
            {
                ApplicationContext.Instance.MenuPersitence.Insert(connection);
                await Shell.Current.GoToAsync(nameof(MyConnectionsPage));
            }
            else
            {
                ApplicationContext.Instance.MenuPersitence.Update(connection);
                _changeDelegate?.Invoke(connectionData);
                await Navigation.PopModalAsync();
            }
        }

        private void SerialPortCheckbox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var sf = ApplicationContext.Instance.SerialPortFactory;

            if (SerialPortCheckbox.IsChecked)
            {
                _serialItemSource = new ObservableCollection<SerialPortInformation>();
                SerialPortList.ItemsSource = _serialItemSource;
                var scanStarted = sf.StartScanningPorts(SerialPortType.ALL, (portInfo, serailPortMode) =>
                {
                    ApplicationContext.Instance.ThreadMarshaller.OnUiThread(() =>
                    {
                        if (serailPortMode == SerialPortDelegateMode.AddOrUpdate)
                        {
                            _serialItemSource.Add(portInfo);
                        }
                        else
                        {
                            foreach (var sp in _serialItemSource)
                            {
                                if (!sp.Id.Equals(portInfo.Id)) continue;
                                _serialItemSource.Remove(sp);
                                break;
                            }
                        }
                    });
                });

                if (scanStarted)
                {
                    BaudRatePicker.ItemsSource = SerialPortInformation.ALL_BAUD_RATES.ToList();
                    SerialPortList.IsEnabled = true;
                    BaudRatePicker.IsEnabled = true;
                    BaudRatePicker.SelectedItem = 115200;
                }
                else
                {
                    DisplayAlert("Bluetooth not supported", "Cannot scan for bluetooth devices", "OK");
                }
            }
            else
            {
                sf.StopScanningPorts();
                _serialItemSource?.Clear();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ApplicationContext.Instance.SerialPortFactory?.StopScanningPorts();
        }
    }
}