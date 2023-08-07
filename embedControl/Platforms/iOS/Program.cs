using embedControl.Platforms.iOS.Serial;
using embedCONTROL.Services;
using embedControl.Views;
using embedControl.Windows.Serial;
using ObjCRuntime;
using TcMenu.CoreSdk.Util;
using UIKit;

namespace embedControl
{
    public class Program
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            var v = AppInfo.Version;
            var ver = new LibraryVersion(v.Major, v.Minor, v.Build, ReleaseType.BETA);

            var appCtx = new ApplicationContext(new MauiUiThreadMarshaller(), ver);
            ApplicationContext.Instance.SetSerialFactory(new AppleBluetoothSerialFactory());

            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}