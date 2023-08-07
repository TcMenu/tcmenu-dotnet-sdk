using embedControl.Services;
using embedCONTROL.Services;
using embedControl.Views;
using embedCONTROL.Views;
using System;

namespace embedControl
{
    public partial class AppShell : Shell
    {
        public FlyoutBehavior FlyoutMode { get; set; }

        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;
            var currentMainDisplayInfo = Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo;
            FlyoutMode = currentMainDisplayInfo.Width >= currentMainDisplayInfo.Height ? FlyoutBehavior.Locked : FlyoutBehavior.Flyout;


            Routing.RegisterRoute("GlobalSettingsPage", typeof(GlobalSettingsPage));
            Routing.RegisterRoute("MyConnectionsPage/detail", typeof(TcMenuConnectionPage));
            Routing.RegisterRoute("NewConnectionDetail", typeof(NewConnectionDetail));
            Routing.RegisterRoute("MyConnectionsPage", typeof(MyConnectionsPage));
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await Browser.Default.OpenAsync("https://www.thecoderscorner.com/products/apps/embed-control/", BrowserLaunchMode.SystemPreferred);

        }
    }
}