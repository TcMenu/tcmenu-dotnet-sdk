using embedCONTROL.Services;
using System.Runtime.ConstrainedExecution;
using TcMenu.CoreSdk.Util;
using TcMenuCoreMaui.FormUi;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace embedControl;

public partial class MainPage : ContentPage
{
    public string AppTitle => "Embed Control " + ApplicationContext.Instance.Version;

    public MainPage()
    {
        InitializeComponent();

        var v = AppInfo.Version;

        var ver = new LibraryVersion(v.Major, v.Minor, v.Build, ReleaseType.BETA);
        var appCtx = new ApplicationContext(new MauiUiThreadMarshaller(), ver);
        BindingContext = this;
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
    }
}

class MauiUiThreadMarshaller : UiThreadMashaller
{
    public Task OnUiThread(Action work)
    {
        if (MainThread.IsMainThread)
        {
            work.Invoke();
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(work);
    }
}
