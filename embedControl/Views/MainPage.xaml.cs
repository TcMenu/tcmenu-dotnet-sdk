﻿using embedCONTROL.Services;
using System.Runtime.ConstrainedExecution;
using embedControl.Models;
using TcMenu.CoreSdk.Util;
using TcMenuCoreMaui.FormUi;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace embedControl.Views;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();

        BindingContext = EmbedControlViewModel.Instance;
    }

    private async void OnOnlineHelp(object sender, EventArgs e)
    {
        await Browser.Default.OpenAsync("https://www.thecoderscorner.com/products/apps/embed-control/", BrowserLaunchMode.SystemPreferred);
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
