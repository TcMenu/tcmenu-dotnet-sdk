using embedControl.Views;

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
            Routing.RegisterRoute("TcMenuConnectionPage", typeof(TcMenuConnectionPage));
        }
    }
}