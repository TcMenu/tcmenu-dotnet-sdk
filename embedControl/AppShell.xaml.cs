using embedControl.Views;

namespace embedControl
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("GlobalSettingsPage", typeof(GlobalSettingsPage));
        }
    }
}