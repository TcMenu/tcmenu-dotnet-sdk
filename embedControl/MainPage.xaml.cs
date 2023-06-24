using embedCONTROL.Services;
using System.Runtime.ConstrainedExecution;
using embedControl.Models;
using TcMenu.CoreSdk.Util;
using TcMenuCoreMaui.FormUi;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace embedControl;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();

        BindingContext = EmbedControlViewModel.Instance;
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
