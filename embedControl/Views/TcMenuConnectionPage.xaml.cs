using embedControl.Models;
using embedCONTROL.Services;
using Serilog;
using System.Xml.Linq;
using embedControl.Services;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteSimulator;
using TcMenu.CoreSdk.Serialisation;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace embedControl.Views;

public partial class TcMenuConnectionPage : ContentPage
{
    private ILogger _logger = Log.Logger.ForContext<TcMenuConnectionPage>();
    private IRemoteController _remoteController;
    private readonly object _remoteLock = new();
    private TcMenuPanelSettings _panelSettings;

    public TcMenuConnectionModel ConnectionModel { get; set; }

    public TcMenuConnectionPage(TcMenuPanelSettings panelSettings)
	{
		InitializeComponent();

        _panelSettings = panelSettings;
        _remoteController = _panelSettings.ConnectionConfiguration.Build();
        ConnectionModel = new TcMenuConnectionModel(_remoteController, ApplicationContext.Instance.AppSettings, ControlsGrid, MenuTree.ROOT,
            item => HandleNavigation(item as SubMenuItem), PresentPairingDialog);
        BindingContext = ConnectionModel;
    }

    private void HandleNavigation(SubMenuItem item)
    {
        ConnectionModel.OnNavChange(item);
        BackButton.Text = "[.. Back] " + ConnectionModel.CurrentNavItem.Name;
    }

    private void OnDialogChanged(object sender, EventArgs e)
    {
            ConnectionModel.SendDialogEvent(sender.Equals(DlgBtn1) ? 0 : 1);
    }

    protected override void OnDisappearing()
    {
        ConnectionModel.Stop();
        base.OnDisappearing();
    }

    private void OnBackButtonClick(object sender, EventArgs e)
    {
        ConnectionModel.OnNavChange(null);
    }

    private void OnForceReconnect(object sender, EventArgs e)
    {
        _remoteController.Connector.Close();
    }

    public void OnCompletelyDisconnect(object sender, EventArgs e)
    {
        lock (_remoteLock)
        {
            _logger.Information("Completely disconnecting from " + ConnectionModel.ConnectionName);
            if (_remoteController == null) return;
            _remoteController.Stop(false);

            ApplicationContext.Instance.ThreadMarshaller.OnUiThread(() =>
            {
                ConnectionModel.CompletelyDisconnected();
                Navigation.PopAsync();
            });
        }
    }

    private async void OnDeleteConnection(object sender, EventArgs e)
    {
        var result = await DisplayActionSheet("Really delete " + Environment.NewLine + ConnectionModel.ConnectionName,
            "No", "Yes");
        if (result == "Yes")
        {
            // delete connection
            ApplicationContext.Instance.MenuPersitence.Delete(_panelSettings.LocalId);
            await Shell.Current.GoToAsync("//" + nameof(MyConnectionsPage));
        }
    }

    private async void OnModifyConnection(object sender, EventArgs e)
    {
        var n = new NewConnectionDetail(new SimulatorConfiguration("My Sim", SimulatorConfiguration.DefaultMenuTree),
            configuration =>
            {
                ApplicationContext.Instance.MenuPersitence.Update(_panelSettings);
                _logger.Information("Connection configuration has changed to " + configuration);
            });

        await Navigation.PushModalAsync(n);
    }

    private async void PresentPairingDialog()
    {
        ConnectionModel.Stop();
        var pairing = new DevicePairingPage(_panelSettings.ConnectionConfiguration, OnPairingFinished);
        await Navigation.PushModalAsync(pairing);
    }

    private void OnPairingFinished(bool obj)
    {
        Navigation.PopModalAsync();

        _remoteController = _panelSettings.ConnectionConfiguration.Build();
        ConnectionModel = new TcMenuConnectionModel(_remoteController, ApplicationContext.Instance.AppSettings, ControlsGrid, MenuTree.ROOT,
            item => HandleNavigation(item as SubMenuItem), PresentPairingDialog);
    }
}