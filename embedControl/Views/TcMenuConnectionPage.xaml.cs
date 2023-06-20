using embedControl.Models;
using embedCONTROL.Services;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteSimulator;
using TcMenu.CoreSdk.Serialisation;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace embedControl.Views;

public partial class TcMenuConnectionPage : ContentPage
{
    private readonly RemoteController _remoteController;
    public TcMenuConnectionModel ConnectionModel { get; }

    public TcMenuConnectionPage() : this(MenuTree.ROOT)
    { }

    public TcMenuConnectionPage(MenuItem where)
	{
		InitializeComponent();

        var defMenuTree = "{\"items\":[{\"parentId\":0,\"type\":\"analogItem\",\"item\":{\"name\":\"Voltage\",\"eepromAddress\":2,\"id\":1,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onVoltageChange\",\"maxValue\":255,\"offset\":-128,\"divisor\":2,\"unitName\":\"V\"}},{\"parentId\":0,\"type\":\"analogItem\",\"item\":{\"name\":\"Current\",\"eepromAddress\":4,\"id\":2,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onCurrentChange\",\"maxValue\":255,\"offset\":0,\"divisor\":100,\"unitName\":\"A\"}},{\"parentId\":0,\"type\":\"enumItem\",\"item\":{\"name\":\"Limit\",\"eepromAddress\":6,\"id\":3,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onLimitMode\",\"enumEntries\":[\"Current\",\"Voltage\"]}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Settings\",\"eepromAddress\":-1,\"id\":4,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":4,\"type\":\"boolItem\",\"item\":{\"name\":\"Pwr Delay\",\"eepromAddress\":-1,\"id\":5,\"readOnly\":false,\"localOnly\":false,\"naming\":\"YES_NO\"}},{\"parentId\":4,\"type\":\"actionMenu\",\"item\":{\"name\":\"Save all\",\"eepromAddress\":-1,\"id\":10,\"readOnly\":false,\"localOnly\":false,\"functionName\":\"onSaveRom\"}},{\"parentId\":4,\"type\":\"subMenu\",\"item\":{\"name\":\"Advanced\",\"eepromAddress\":-1,\"id\":11,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":11,\"type\":\"boolItem\",\"item\":{\"name\":\"S-Circuit Protect\",\"eepromAddress\":8,\"id\":12,\"readOnly\":false,\"localOnly\":false,\"naming\":\"ON_OFF\"}},{\"parentId\":11,\"type\":\"boolItem\",\"item\":{\"name\":\"Temp Check\",\"eepromAddress\":9,\"id\":13,\"readOnly\":false,\"localOnly\":false,\"naming\":\"ON_OFF\"}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Status\",\"eepromAddress\":-1,\"id\":7,\"readOnly\":false,\"localOnly\":false,\"secured\":false}},{\"parentId\":7,\"type\":\"floatItem\",\"item\":{\"name\":\"Volt A0\",\"eepromAddress\":-1,\"id\":8,\"readOnly\":true,\"localOnly\":false,\"numDecimalPlaces\":2}},{\"parentId\":7,\"type\":\"floatItem\",\"item\":{\"name\":\"Volt A1\",\"eepromAddress\":-1,\"id\":9,\"readOnly\":true,\"localOnly\":false,\"numDecimalPlaces\":2}},{\"parentId\":7,\"type\":\"largeNumItem\",\"item\":{\"name\":\"RotationCounter\",\"eepromAddress\":-1,\"id\":16,\"readOnly\":true,\"localOnly\":false,\"decimalPlaces\":4,\"digitsAllowed\":8}},{\"parentId\":0,\"type\":\"subMenu\",\"item\":{\"name\":\"Connectivity\",\"eepromAddress\":-1,\"id\":14,\"readOnly\":false,\"localOnly\":true,\"secured\":true}},{\"parentId\":14,\"type\":\"textItem\",\"item\":{\"name\":\"Ip Address\",\"eepromAddress\":10,\"id\":15,\"readOnly\":false,\"visible\":\"false\",\"localOnly\":false,\"itemType\":\"IP_ADDRESS\",\"textLength\":20}}]}";
        var persistor = new JsonMenuItemPersistor();
        var menuTree = new MenuTree();
        foreach (var itemAndParent in persistor.DeSerialiseItemsFromJson(defMenuTree))
        {
            menuTree.AddMenuItem(menuTree.GetMenuById(itemAndParent.ParentId) as SubMenuItem, itemAndParent.Item);
        }
        var connector = new SimulatedRemoteConnection(menuTree, "Simulator");
        _remoteController = new RemoteController(connector, menuTree, new SystemClock());
        _remoteController.Start();


        ConnectionModel = new TcMenuConnectionModel(_remoteController, ApplicationContext.Instance.AppSettings, ControlsGrid, where,
            item => Navigation.PushAsync(new TcMenuConnectionPage(item)));
        BindingContext = ConnectionModel;
    }

    private void OnDialogChanged(object sender, EventArgs e)
    {
            ConnectionModel.SendDialogEvent(sender.Equals(DlgBtn1) ? 0 : 1);
    }
}