using embedCONTROL.Services;
using System.Runtime.ConstrainedExecution;
using embedControl.Views;
using TcMenu.CoreSdk.StoreWrapper;
using TcMenu.CoreSdk.Util;
using TcMenuCoreMaui.BaseSerial;

namespace embedControl
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var v = AppInfo.Version;
            var ver = new LibraryVersion(v.Major, v.Minor, v.Build, ReleaseType.BETA);

            var appCtx = new ApplicationContext(new MauiUiThreadMarshaller(), ver);
            ApplicationContext.Instance.SetSerialFactory(new TcMenuCoreMaui.PlatformImplementationCreator().CreateSerialPort());

            MainPage = new AppShell();
        }
    }
}