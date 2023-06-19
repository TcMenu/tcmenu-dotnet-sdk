using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdkTests.MenuTests
{
    public static class MenuItemFixtures
    {
        public static AnalogMenuItem AnAnalogItem(int id, string name, int eeprom = 100)
        {
            return new AnalogMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithEepromLocation(eeprom)
                .WithOffset(0)
                .WithDivisor(2)
                .WithMaxValue(20)
                .WithUnitName("dB")
                .Build();
        }

        public static EnumMenuItem AnEnumItem(int id, string name, int eeprom = -1)
        {
            return new EnumMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithEepromLocation(eeprom)
                .WithNewEntry("hello")
                .WithNewEntry("world")
                .Build();
        }

        public static SubMenuItem ASubItem(int id, string name, int eeprom = -1)
        {
            return new SubMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithEepromLocation(eeprom)
                .Build();
        }

        public static BooleanMenuItem ABoolItem(int id, string name, int eeprom = 104)
        {
            return new BooleanMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithNaming(BooleanNaming.ON_OFF)
                .WithEepromLocation(eeprom)
                .Build();
        }

        public static EditableTextMenuItem ATextItem(int id, string name, int eeprom = -1, EditItemType type = EditItemType.PLAIN_TEXT)
        {
            return new EditableTextMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithEepromLocation(eeprom)
                .WithTextLength(10)
                .WithEditType(type)
                .Build();
        }

        public static FloatMenuItem AFloatItem(int id, string name, int eeprom = -1, int dp = 4)
        {
            return new FloatMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithEepromLocation(eeprom)
                .WithDecimalPlaces(dp)
                .Build();
        }

        public static ActionMenuItem AnActionItem(int id, string name, string function = "")
        {
            return new ActionMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithFunctionName(function)
                .Build();
        }

        public static LargeNumberMenuItem ALargeNumberItem(int id, string name, int eeprom = -1, int dp = 4, int maxDigits = 8)
        {
            return new LargeNumberMenuItemBuilder()
                .WithId(id)
                .WithName(name)
                .WithEepromLocation(eeprom)
                .WithDecimalPlaces(dp)
                .WithTotalDigits(maxDigits)
                .WithAllowNegative(true)
                .Build();
        }

        /// <summary>
        /// A menu tree in JSON that is based on the AdaEthernet32 example
        /// </summary>
        public const string LARGE_MENU_TREE = "{\"items\": [    {      \"parentId\": 0,      \"type\": \"analogItem\",      \"item\": {        \"name\": \"Voltage\",        \"eepromAddress\": 2,        \"id\": 1,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onVoltageChange\",        \"maxValue\": 255,        \"offset\": -128,        \"divisor\": 2,        \"unitName\": \"V\"      }    },    {      \"parentId\": 0,      \"type\": \"analogItem\",      \"item\": {        \"name\": \"Current\",        \"eepromAddress\": 4,        \"id\": 2,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onCurrentChange\",        \"maxValue\": 255,        \"offset\": 0,        \"divisor\": 100,        \"unitName\": \"A\"      }    },    {      \"parentId\": 0,      \"type\": \"enumItem\",      \"item\": {        \"name\": \"Limit\",        \"eepromAddress\": 6,        \"id\": 3,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onLimitMode\",        \"enumEntries\": [          \"Current\",          \"Voltage\"        ]      }    },    {      \"parentId\": 0,      \"type\": \"subMenu\",      \"item\": {        \"name\": \"Settings\",        \"eepromAddress\": -1,        \"id\": 4,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"secured\": false      }    },    {      \"parentId\": 4,      \"type\": \"boolItem\",      \"item\": {        \"name\": \"Pwr Delay\",        \"eepromAddress\": -1,        \"id\": 5,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"naming\": \"YES_NO\"      }    },    {      \"parentId\": 4,      \"type\": \"actionMenu\",      \"item\": {        \"name\": \"Save all\",        \"eepromAddress\": -1,        \"id\": 10,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onSaveRom\"      }    },    {      \"parentId\": 4,      \"type\": \"subMenu\",      \"item\": {        \"name\": \"Advanced\",        \"eepromAddress\": -1,        \"id\": 11,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"secured\": false      }    },    {      \"parentId\": 11,      \"type\": \"boolItem\",      \"item\": {        \"name\": \"S-Circuit Protect\",        \"eepromAddress\": 8,        \"id\": 12,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"naming\": \"ON_OFF\"      }    },    {      \"parentId\": 11,      \"type\": \"boolItem\",      \"item\": {        \"name\": \"Temp Check\",        \"eepromAddress\": 9,        \"id\": 13,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"naming\": \"ON_OFF\"      }    },    {      \"parentId\": 4,      \"type\": \"scrollItem\",      \"item\": {        \"name\": \"Rom Choice\",        \"eepromAddress\": -1,        \"id\": 18,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onRomChoice\",        \"itemWidth\": 10,        \"numEntries\": 40,        \"variable\": \"eeprom\",        \"eepromOffset\": 25,        \"choiceMode\": \"ARRAY_IN_EEPROM\"      }    },    {      \"parentId\": 4,      \"type\": \"scrollItem\",      \"item\": {        \"name\": \"Custom Choice\",        \"eepromAddress\": -1,        \"id\": 19,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onCustomChoice\",        \"itemWidth\": 10,        \"numEntries\": 0,        \"variable\": \"eeprom\",        \"eepromOffset\": 0,        \"choiceMode\": \"CUSTOM_RENDERFN\"      }    },    {      \"parentId\": 0,      \"type\": \"subMenu\",      \"item\": {        \"name\": \"Status\",        \"eepromAddress\": -1,        \"id\": 7,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"secured\": false      }    },    {      \"parentId\": 7,      \"type\": \"floatItem\",      \"item\": {        \"name\": \"Volt A0\",        \"eepromAddress\": -1,        \"id\": 8,        \"readOnly\": true,        \"localOnly\": false,        \"visible\": true,        \"numDecimalPlaces\": 2      }    },    {      \"parentId\": 7,      \"type\": \"floatItem\",      \"item\": {        \"name\": \"Volt A1\",        \"eepromAddress\": -1,        \"id\": 9,        \"readOnly\": true,        \"localOnly\": false,        \"visible\": true,        \"numDecimalPlaces\": 2      }    },    {      \"parentId\": 7,      \"type\": \"largeNumItem\",      \"item\": {        \"name\": \"RotationCounter\",        \"eepromAddress\": -1,        \"id\": 16,        \"readOnly\": true,        \"localOnly\": false,        \"visible\": true,        \"decimalPlaces\": 4,        \"digitsAllowed\": 8,        \"negativeAllowed\": true      }    },    {      \"parentId\": 7,      \"type\": \"rgbItem\",      \"item\": {        \"name\": \"LED RGB\",        \"eepromAddress\": -1,        \"id\": 17,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": true,        \"functionName\": \"onLedRgb\",        \"includeAlphaChannel\": true      }    },    {      \"parentId\": 0,      \"type\": \"subMenu\",      \"item\": {        \"name\": \"Connectivity\",        \"eepromAddress\": -1,        \"id\": 14,        \"readOnly\": false,        \"localOnly\": true,        \"visible\": true,        \"secured\": true      }    },    {      \"parentId\": 14,      \"type\": \"textItem\",      \"item\": {        \"name\": \"Ip Address\",        \"eepromAddress\": 10,        \"id\": 15,        \"readOnly\": false,        \"localOnly\": false,        \"visible\": false,        \"itemType\": \"IP_ADDRESS\",        \"textLength\": 20      }    }  ]}";
        /// <summary>
        /// A small block of a tree containing a single item for reparenting tests.
        /// </summary>
        public const string SMALL_BLOCK_TREE = "{\"items\":[{\"parentId\":15,\"type\":\"textItem\",\"item\":{\"name\":\"IP123\",\"eepromAddress\":13,\"id\":16,\"readOnly\":false,\"localOnly\":false,\"itemType\":\"IP_ADDRESS\",\"textLength\":20}}]}";

        /// <summary>
        /// Loads any menu tree provided in json format
        /// </summary>
        /// <returns></returns>
        public static MenuTree LoadMenuTree(string json)
        {
            JsonMenuItemPersistor persistor = new JsonMenuItemPersistor();
            List<MenuItemWithParent> items = persistor.DeSerialiseItemsFromJson(json);

            MenuTree menuTree = new MenuTree();

            foreach(var item in items)
            {
                var par = menuTree.GetMenuById(item.ParentId) as SubMenuItem;
                menuTree.AddMenuItem(par, item.Item);
            }

            return menuTree;
        }
    }
}
