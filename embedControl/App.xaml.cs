using embedCONTROL.Services;
using TcMenu.CoreSdk.StoreWrapper;
using TcMenu.CoreSdk.Util;

namespace embedControl
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}