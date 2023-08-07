using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using embedCONTROL.Services;
using Microsoft.Maui.Media;
using Serilog;
using Serilog.Core;
using SQLite;
using TcMenu.CoreSdk.Serialisation;
using TcMenuCoreMaui.BaseSerial;
using TcMenuCoreMaui.Services;
using static embedControl.Services.TcMenuPersistedConnectionType;
using static SQLite.SQLite3;

namespace embedControl.Services
{
    public class TcMenuPanelSettings
    {
        public int LocalId { get; }
        public string ConnectionName { get; }
        public string ConnectionDescription => $"{ConnectionConfiguration.Describe} - ID={LocalId}";
        public IConnectionConfiguration ConnectionConfiguration { get; }
        public DateTime LastOpened { get; set; }

        public TcMenuPanelSettings(int localId, string connName, IConnectionConfiguration connectionConfiguration, DateTime lastOpened)
        {
            LocalId = localId;
            ConnectionName = connName;
            ConnectionConfiguration = connectionConfiguration;
            LastOpened = lastOpened;
        }
    }

    public interface IMenuConnectionPersister
    {
        TcMenuPanelSettings this[int localId] { get; }
        IReadOnlyCollection<TcMenuPanelSettings> AllItems { get; }
        bool Delete(int localId);
        void Insert(TcMenuPanelSettings connection);
        void Update(TcMenuPanelSettings connection);
        event MenuPanelsChangeDelegate ChangeNotification;
    }

    public enum PanelSettingChangeMode
    {
        Added, Deleted, Modified
    }

    public delegate void MenuPanelsChangeDelegate(TcMenuPanelSettings panel, PanelSettingChangeMode mode);

    internal enum TcMenuPersistedConnectionType { SerialConnection, RawSocketConnection, SimulatedConnection }
 
    internal class TcMenuConnectionPersistenceObject
    {
        public TcMenuConnectionPersistenceObject()
        {
        }

        public TcMenuConnectionPersistenceObject(TcMenuPanelSettings settings)
        {
            LocalId = settings.LocalId;
            ConnectionName = settings.ConnectionName;
            LastModified = settings.LastOpened;
            switch (settings.ConnectionConfiguration)
            {
                case SimulatorConfiguration sc:
                    ConnectionType = SimulatedConnection;
                    RawExtraData = Encoding.UTF8.GetBytes(sc.JsonObjects);
                    HostOrSerialInfo = "";
                    IpPortOrBaud = "";
                    break;
                case RawSocketConfiguration rsc:
                    ConnectionType = RawSocketConnection;
                    RawExtraData = null;
                    HostOrSerialInfo = rsc.Host;
                    IpPortOrBaud = rsc.Port.ToString();
                    break;
                case SerialCommsConfiguration ser:
                    ConnectionType = SerialConnection;
                    RawExtraData = null;
                    HostOrSerialInfo = ser.SerialInfo.ToWire();
                    IpPortOrBaud = ser.BaudRate.ToString();
                    break;
            }
        }

        [PrimaryKey, AutoIncrement] 
        public int LocalId { get; set; }

        [MaxLength(64)]
        public string ConnectionName { get; set; }

        public TcMenuPersistedConnectionType ConnectionType { get; set; }
        [MaxLength(64)]
        public string HostOrSerialInfo { get; set; }
        public string IpPortOrBaud { get; set; }
        public byte[] RawExtraData { get; set; }
        public string FormName { get; set; }
        public DateTime LastModified { get; set; }
    }

    internal class TcMenuFormPersistenceObject
    {
        [PrimaryKey]
        public int LocalId { get; set; }
        [Indexed]
        public string FormName { get; set; }
        
        public byte[] XmlRawBytes { get; set; }
    }

    internal class PrefsPersistenceObject
    {
        [PrimaryKey]
        public int prefsId { get; set; }
        public string LocalName { get; set; }
        public string LocalUUID { get; set; }
        public bool Recurse { get; set; }
        public string PendingFg { get; set; }
        public string PendingBg { get; set; }
        public string DialogFg { get; set; }
        public string DialogBg { get; set; }
        public string ErrorFg { get; set; }
        public string ErrorBg { get; set; }
        public string ButtonFg { get; set; }
        public string ButtonBg { get; set; }
        public string TextFg { get; set; }
        public string TextBg { get; set; }
        public string UpdateFg { get; set; }
        public string UpdateBg { get; set; }
        public string HighlightFg { get; set; }
        public string HighlightBg { get; set; }
    }


    public class TcMenuConnectionPersistence : IMenuConnectionPersister
    {
        private readonly ILogger _logger = Log.ForContext<TcMenuConnectionPersistence>();

        private readonly Dictionary<int, TcMenuPanelSettings> _settings = new();
        private SQLiteConnection _database;
        
        public  event MenuPanelsChangeDelegate ChangeNotification;

        public TcMenuPanelSettings this[int localId] =>  _settings[localId];
        public IReadOnlyCollection<TcMenuPanelSettings> AllItems => _settings.Values;

        public async void Initialise()
        {
            try
            {
                var storageLocation = FileSystem.AppDataDirectory;
                var sqLiteFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;
                var dbPath = Path.Combine(storageLocation, "EmbedControlData.db3");
                if (_database is not null)
                {
                    _logger.Error("Configuration database was not created");
                    return;
                }

                _database = new SQLiteConnection(dbPath, sqLiteFlags);

                _logger.Information("Database instance created: " + dbPath);

                _database.CreateTable<PrefsPersistenceObject>();
                _database.CreateTable<TcMenuConnectionPersistenceObject>();

                _logger.Information("Tables created successfully");

                ReloadAllEntriesFromDatabase();
            }
            catch(Exception ex)
            {

            }
        }

        private void ReloadAllEntriesFromDatabase()
        {
            _logger.Information("Start reload of connections");

            foreach (var tc in _database.Table<TcMenuConnectionPersistenceObject>().ToList())
            {
                try
                {
                    IConnectionConfiguration config = tc.ConnectionType switch
                    {
                        SimulatedConnection => new SimulatorConfiguration(tc.ConnectionName,
                            Encoding.UTF8.GetString(tc.RawExtraData)),
                        RawSocketConnection => new RawSocketConfiguration(tc.HostOrSerialInfo,
                            Convert.ToInt32(tc.IpPortOrBaud)),
                        SerialConnection => new SerialCommsConfiguration(SerialPortInformation.FromWire(tc.HostOrSerialInfo),
                            Convert.ToInt32(tc.IpPortOrBaud))
                    };
                    _settings[tc.LocalId] = new TcMenuPanelSettings(tc.LocalId, tc.ConnectionName, config, tc.LastModified);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Connection reload failed '{tc.ConnectionName}' with localID {tc.LocalId}");
                }
                _logger.Information($"Added connection '{tc.ConnectionName}' with localID {tc.LocalId}");
            }

            _logger.Information("Finished reload of connections");
        }

        public PrefsAppSettings LoadAppSettings()
        {
            var prefsObj = _database.Table<PrefsPersistenceObject>().FirstOrDefault();
            if (prefsObj == null)
            {
                _logger.Information("Creating new application, no saved config");
                var prefs = new PrefsAppSettings();
                prefs.UniqueId = Guid.NewGuid().ToString();
                prefs.LocalName = "Unnamed";
                prefs.RecurseIntoSub = true;
                prefs.SetColorsForMode();
                SaveAppSettings(prefs);
                return prefs;
            }
            else
            {
                _logger.Information($"Restoring application: Name='{prefsObj.LocalName}', Uuid={prefsObj.LocalUUID}");
                var prefs = new PrefsAppSettings();
                prefs.LocalName = prefsObj.LocalName;
                prefs.UniqueId = prefsObj.LocalUUID;
                prefs.RecurseIntoSub = prefsObj.Recurse;
                prefs.DialogColor.Bg = new PortableColor(prefsObj.DialogBg);
                prefs.DialogColor.Fg = new PortableColor(prefsObj.DialogFg);
                prefs.ErrorColor.Bg = new PortableColor(prefsObj.ErrorBg);
                prefs.ErrorColor.Fg = new PortableColor(prefsObj.ErrorFg);
                prefs.UpdateColor.Bg = new PortableColor(prefsObj.UpdateBg);
                prefs.UpdateColor.Fg = new PortableColor(prefsObj.UpdateFg);
                prefs.PendingColor.Bg = new PortableColor(prefsObj.PendingBg);
                prefs.PendingColor.Fg = new PortableColor(prefsObj.PendingFg);
                prefs.HighlightColor.Bg = new PortableColor(prefsObj.HighlightBg);
                prefs.HighlightColor.Fg = new PortableColor(prefsObj.HighlightFg);
                prefs.ButtonColor.Bg = new PortableColor(prefsObj.ButtonBg);
                prefs.ButtonColor.Fg = new PortableColor(prefsObj.ButtonFg);
                prefs.TextColor.Bg = new PortableColor(prefsObj.TextBg);
                prefs.TextColor.Fg = new PortableColor(prefsObj.TextFg);
                prefs.UpdateColor.Bg = new PortableColor(prefsObj.UpdateBg);
                prefs.UpdateColor.Fg = new PortableColor(prefsObj.UpdateFg);
                return prefs;
            }
        }

        public void SaveAppSettings(PrefsAppSettings settings)
        {
            _logger.Information("Saving application settings");
            var prefsObj = new PrefsPersistenceObject();
            prefsObj.prefsId = 0;
            prefsObj.LocalName = settings.LocalName;
            prefsObj.LocalUUID = settings.UniqueId;
            prefsObj.Recurse = settings.RecurseIntoSub;
            prefsObj.ErrorFg = settings.ErrorColor.Fg.ToString();
            prefsObj.ErrorBg = settings.ErrorColor.Bg.ToString();
            prefsObj.UpdateBg= settings.UpdateColor.Bg.ToString();
            prefsObj.UpdateFg = settings.UpdateColor.Fg.ToString();
            prefsObj.PendingBg = settings.PendingColor.Bg.ToString();
            prefsObj.PendingFg = settings.PendingColor.Fg.ToString();
            prefsObj.TextBg = settings.TextColor.Bg.ToString();
            prefsObj.TextFg= settings.TextColor.Fg.ToString();
            prefsObj.ButtonBg= settings.ButtonColor.Bg.ToString();
            prefsObj.ButtonFg = settings.ButtonColor.Fg.ToString();
            prefsObj.DialogBg = settings.DialogColor.Bg.ToString();
            prefsObj.DialogFg = settings.DialogColor.Fg.ToString();
            prefsObj.HighlightBg = settings.HighlightColor.Bg.ToString();
            prefsObj.HighlightFg = settings.HighlightColor.Fg.ToString();
            _database.InsertOrReplace(prefsObj);
        }

        public bool Delete(int localId)
        {
            if (!_settings.ContainsKey(localId)) return false;
            var data = _settings[localId];
            _settings.Remove(localId);
            var persistence = new TcMenuConnectionPersistenceObject(data);
            var result = _database.Delete(persistence);
            _logger.Information($"Delete stored configuration {localId} with status {result}");

            ChangeNotification?.Invoke(data, PanelSettingChangeMode.Deleted);
            return result != 0;
        }

        public void Update(TcMenuPanelSettings connection)
        {
            if (!_settings.ContainsKey(connection.LocalId)) return;

            _database.Update(new TcMenuConnectionPersistenceObject(connection));
            _settings[connection.LocalId] = connection;
            _logger.Information($"Updated stored configuration {connection.LocalId}");

            ChangeNotification?.Invoke(connection, PanelSettingChangeMode.Modified);
        }

        public void Insert(TcMenuPanelSettings connection)
        {
            if (connection.LocalId != -1) return;

            _database.Insert(new TcMenuConnectionPersistenceObject(connection));
            _logger.Information($"Added stored configuration {connection.ConnectionName}");
            ReloadAllEntriesFromDatabase();

            ChangeNotification?.Invoke(connection, PanelSettingChangeMode.Added);
        }
    } 

}
