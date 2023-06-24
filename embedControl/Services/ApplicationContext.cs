using System;
using System.Threading.Tasks;
using embedControl.Services;
using TcMenu.CoreSdk.Protocol;
using TcMenuCoreMaui.BaseSerial;
using TcMenu.CoreSdk.Util;
using TcMenuCoreMaui.FormUi;
using TcMenuCoreMaui.Services;

namespace embedCONTROL.Services
{
    /// <summary>
    /// A platform independent way of getting something done on the UI thread
    /// </summary>
    public interface UiThreadMashaller
    {
        Task OnUiThread(Action work);
    }

    public class ApplicationContext
    {
        private static object _contextLock = new object();
        private static ApplicationContext _theInstance;

        private volatile ISerialPortFactory _serialPortFactory;
        private readonly TcMenuConnectionPersistence _persistence;
        public UiThreadMashaller ThreadMarshaller {get;}

        public PrefsAppSettings AppSettings { get; }

        public IMenuConnectionPersister MenuPersitence => _persistence;

        public ISerialPortFactory SerialPortFactory => _serialPortFactory;
        public bool IsSerialAvailable => _serialPortFactory != null;

        public SystemClock Clock => new();

        public static ApplicationContext Instance
        {
            get
            {
                lock (_contextLock)
                {
                    return _theInstance;
                }
            }
        }

        public LibraryVersion Version { get; }

        public ApplicationContext(UiThreadMashaller marshaller, LibraryVersion version)
        {
            var configDir = Path.Combine(FileSystem.AppDataDirectory, "embedControl");
            System.IO.Directory.CreateDirectory(configDir);
            Version = version;

            ThreadMarshaller = marshaller;

            _persistence = new TcMenuConnectionPersistence();
            _persistence.Initialise();
            AppSettings = _persistence.LoadAppSettings();

            /*var persistor = new XmlMenuConnectionPersister(configDir);
            DataStore = new ConnectionDataStore(persistor);*/

            _theInstance = this;
        }


        public void SetSerialFactory(ISerialPortFactory factory)
        {
            _serialPortFactory = factory;
        }

        public void SaveAllSettings()
        {
            _persistence.SaveAppSettings(AppSettings);
        }
    }
}
