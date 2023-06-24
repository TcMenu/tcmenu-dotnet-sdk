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

[QueryProperty(nameof(LocalID), "LocalID")]

public partial class TcMenuConnectionPage : ContentPage
{
    private ILogger _logger = Log.Logger.ForContext<TcMenuConnectionPage>();
    private IRemoteController _remoteController;
    private readonly object _remoteLock = new();
    private TcMenuPanelSettings _panelSettings;

    public int LocalID
    {
        get => _panelSettings?.LocalId ?? 0;
        set
        {
            _panelSettings = ApplicationContext.Instance.MenuPersitence[value];
            _remoteController = _panelSettings.ConnectionConfiguration.Build();
            ConnectionModel = new TcMenuConnectionModel(_remoteController, ApplicationContext.Instance.AppSettings, ControlsGrid, MenuTree.ROOT,
                item => HandleNavigation(item as SubMenuItem));
            BindingContext = ConnectionModel;
        }
    }

    public TcMenuConnectionModel ConnectionModel { get; set; }

    public TcMenuConnectionPage()
	{
		InitializeComponent();
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
            await Shell.Current.GoToAsync("//MainPage");
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
}