using Windows.Storage;
using embedCONTROL.Services;
using embedControl.Views;
using embedControl.Windows.Serial;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.UI.Xaml;
using Serilog;
using TcMenu.CoreSdk.Util;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace embedControl.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(storageFolder.Path + "\\embed.log",
                    fileSizeLimitBytes: 32768 * 1024, rollOnFileSizeLimit: true,  // 32mb (96mb max)
                    retainedFileCountLimit: 3,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Logger = logger;

            Log.Logger.Information("Application started up on Windows OS");

            var v = AppInfo.Version;
            var ver = new LibraryVersion(v.Major, v.Minor, v.Build, ReleaseType.BETA);

            var appCtx = new ApplicationContext(new MauiUiThreadMarshaller(), ver);
            ApplicationContext.Instance.SetSerialFactory(new WindowsSerialFactory());

            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}