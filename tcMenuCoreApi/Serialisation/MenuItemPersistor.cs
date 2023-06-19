using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using TcMenu.CoreSdk.MenuItems;

namespace TcMenu.CoreSdk.Serialisation
{
    public interface IMenuItemPersistor
    {
        /// <summary>
        /// DeSerialises a json blob that contains menu items that was serialised using the
        /// SerialiseItemsRecursively.
        /// </summary>
        /// <param name="json">The JSON blob to be converted to menu items</param>
        /// <returns>a list of items with their parents</returns>
        List<MenuItemWithParent> DeSerialiseItemsFromJson(string json);

        /// <summary>
        /// Serialises a set of menu items recursively into an item structure so that it can
        /// be de-serialised with DeSerialiseItemsFromJson
        /// </summary>
        /// <param name="startingPoint">the item to start serialisation at in the tree</param>
        /// <param name="tree">the tree to recurse to find additional child items</param>
        /// <param name="includeRoot">if the item itself should be included</param>
        /// <returns>a string of serialised items</returns>
        string SerialiseItemsRecursively(MenuItem startingPoint, MenuTree tree, bool includeRoot = false);

        /// <summary>
        /// Recursively saves a menu into the json array provided.
        /// </summary>
        /// <param name="startingPoint">the place to start converting</param>
        /// <param name="jsonArray">the array to store in</param>
        /// <param name="tree">the menu tree</param>
        void RecurseMenuSave(MenuItem startingPoint, JArray jsonArray, MenuTree tree);

        /// <summary>
        /// Generates a list of menu items with their parent ID from a json array.
        /// </summary>
        /// <param name="json">the json containing menu items</param>
        /// <returns></returns>
        List<MenuItemWithParent> GenerateListFromJson(JObject json);
    }

    public class JsonMenuItemPersistor : IMenuItemPersistor
    {
        private ILogger logger = Log.Logger.ForContext<JsonMenuItemPersistor>();

        public List<MenuItemWithParent> DeSerialiseItemsFromJson(string json)
        {
            try
            {
                var items = JObject.Parse(json);
                return GenerateListFromJson(items);
            }
            catch (Exception e)
            {
                logger.Error(e, "Could not deserialise item, check formmating etc.");
                return new List<MenuItemWithParent>();
            }
        }

        public List<MenuItemWithParent> GenerateListFromJson(JObject json)
        {
            var list = new List<MenuItemWithParent>();

            var items = json["items"].Children().AsEnumerable();

            foreach (var item in items)
            {
                var parent = item.Value<int>("parentId");
                var childItem = item["item"];
                switch (item.Value<string>("type"))
                {
                    case "subMenu":
                        list.Add(new MenuItemWithParent(parent, ReadSubMenuItem(childItem)));
                        break;
                    case "analogItem":
                        list.Add(new MenuItemWithParent(parent, ReadAnalogItem(childItem)));
                        break;
                    case "boolItem":
                        list.Add(new MenuItemWithParent(parent, ReadBooleanItem(childItem)));
                        break;
                    case "enumItem":
                        list.Add(new MenuItemWithParent(parent, ReadEnumItem(childItem)));
                        break;
                    case "actionMenu":
                        list.Add(new MenuItemWithParent(parent, ReadActionItem(childItem)));
                        break;
                    case "floatItem":
                        list.Add(new MenuItemWithParent(parent, ReadFloatItem(childItem)));
                        break;
                    case "runtimeList":
                        list.Add(new MenuItemWithParent(parent, ReadRuntimeListItem(childItem)));
                        break;
                    case "textItem":
                        list.Add(new MenuItemWithParent(parent, ReadTextItem(childItem)));
                        break;
                    case "rgbItem":
                        list.Add(new MenuItemWithParent(parent, ReadRgb32MenuItem(childItem)));
                        break;
                    case "scrollItem":
                        list.Add(new MenuItemWithParent(parent, ReadScrollMenuItem(childItem)));
                        break;
                    case "largeNumItem":
                        list.Add(new MenuItemWithParent(parent, ReadLargeNumItem(childItem)));
                        break;
                    default:
                        logger.Error("Skipping an item. No such item mapping for " + item);
                        break;
                }
            }
            return list;
        }

        private AnalogMenuItem ReadAnalogItem(JToken item)
        {
            var builder = new AnalogMenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.WithMaxValue(item.Value<int>("maxValue"))
                .WithOffset(item.Value<int>("offset"))
                .WithDivisor(item.Value<int>("divisor"))
                .WithUnitName(item.Value<string>("unitName"))
                .WithStep(item.Value<int>("step"))
                .Build();
        }

        private BooleanMenuItem ReadBooleanItem(JToken item)
        {
            var builder = new BooleanMenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.WithNaming((BooleanNaming)Enum.Parse(typeof(BooleanNaming), item.Value<string>("naming")))
                .Build();
        }
        private EnumMenuItem ReadEnumItem(JToken item)
        {
            var builder = new EnumMenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.WithEntries(item["enumEntries"].Children().Select(jsonEnum => jsonEnum.Value<string>()).ToList())
                .Build();
        }

        private RuntimeListMenuItem ReadRuntimeListItem(JToken item)
        {
            var builder = new RuntimeListMenuItemBuilder();
            var creationMode = ListCreationMode.CustomRtCall;
            if (Enum.TryParse(item.Value<string>("listCreationMode"), out ListCreationMode cm))
            {
                creationMode = cm;
            }
            BuildBaseFields(builder, item);
            return builder.WithInitialRows(item.Value<int>("initialRows"))
                .WithCreationMode(creationMode)
                .Build();
        }

        private EditableTextMenuItem ReadTextItem(JToken item)
        {
            var builder = new EditableTextMenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.WithEditType((EditItemType)Enum.Parse(typeof(EditItemType), item.Value<string>("itemType")))
                .WithTextLength(item.Value<int>("textLength"))
                .Build();
        }

        private Rgb32MenuItem ReadRgb32MenuItem(JToken item)
        {
            var builder = new Rgb32MenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.WithIncludeAlphaChannel(item.Value<bool>("includeAlphaChannel"))
                .Build();
        }

        private ScrollChoiceMenuItem ReadScrollMenuItem(JToken item)
        {
            var builder = new ScrollChoiceMenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.WithItemWidth(item.Value<int>("itemWidth"))
                .WithEepromOffset(item.Value<int>("eepromOffset"))
                .WithChoiceMode((ScrollChoiceMode) Enum.Parse(typeof(ScrollChoiceMode), item.Value<string>("choiceMode")))
                .WithNumEntries(item.Value<int>("numEntries"))
                .WithRamVariable(item.Value<string>("variable"))
                .Build();
        }

        private void BuildBaseFields<TBuild, TItem>(MenuItemBuilder<TBuild, TItem> builder, JToken item)
            where TItem : MenuItem
            where TBuild : MenuItemBuilder<TBuild, TItem>
        {
            bool visible = true;
            if (!string.IsNullOrEmpty(item.Value<string>("visible")))
            {
                visible = item.Value<bool>("visible");
            }

            builder.WithId(item.Value<int>("id"))
                .WithName(item.Value<string>("name"))
                .WithEepromLocation(item.Value<int>("eepromAddress"))
                .WithFunctionName(item.Value<string>("functionName"))
                .WithVariableName(item.Value<string>("variableName"))
                .WithReadOnly(item.Value<bool>("readOnly"))
                .WithLocalOnly(item.Value<bool>("localOnly"))
                .WithStaticInRam(item.Value<bool>("staticDataInRAM"))
                .WithVisible(visible);
        }

        private SubMenuItem ReadSubMenuItem(JToken item)
        {
            var builder = new SubMenuItemBuilder();
            BuildBaseFields(builder, item);
            builder.WithSecured(item.Value<bool>("secured"));
            return builder.Build();
        }

        private ActionMenuItem ReadActionItem(JToken item)
        {
            var builder = new ActionMenuItemBuilder();
            BuildBaseFields(builder, item);
            return builder.Build();
        }

        private FloatMenuItem ReadFloatItem(JToken item)
        {
            var builder = new FloatMenuItemBuilder();
            BuildBaseFields(builder, item);
            builder.WithDecimalPlaces(item.Value<int>("numDecimalPlaces"));
            return builder.Build();
        }

        private LargeNumberMenuItem ReadLargeNumItem(JToken item)
        {
            var builder = new LargeNumberMenuItemBuilder();
            BuildBaseFields(builder, item);
            builder.WithDecimalPlaces(item.Value<int>("decimalPlaces"));
            builder.WithTotalDigits(item.Value<int>("digitsAllowed"));
            builder.WithAllowNegative(item["negativeAllowed"]?.Value<bool>() ?? true);
            return builder.Build();
        }

        public string SerialiseItemsRecursively(MenuItem startingPoint, MenuTree tree, bool includeRoot = false)
        {
            var jarray = new JArray();

            if (startingPoint is SubMenuItem)
            {
                if (includeRoot && !Equals(startingPoint, MenuTree.ROOT))
                {
                    jarray.Add(TranslateMenuItemToJson(startingPoint, tree.FindParent(startingPoint)));
                }
                RecurseMenuSave(startingPoint, jarray, tree);
            }
            else
            {
                jarray.Add(TranslateMenuItemToJson(startingPoint, tree.FindParent(startingPoint)));
            }

            var jsonBlob = new JObject
            {
                { "items", jarray }
            };

            return Environment.NewLine + jsonBlob.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        public void RecurseMenuSave(MenuItem sub, JArray items, MenuTree tree)
        {
            foreach (var menuItem in tree.GetMenuItems(sub))
            {
                var itemWithParent = TranslateMenuItemToJson(menuItem, sub as SubMenuItem);
                items.Add(itemWithParent);
                if (menuItem is SubMenuItem subsub)
                {
                    RecurseMenuSave(subsub, items, tree);
                }
            }
        }

        private object TranslateMenuItemToJson(MenuItem menuItem, SubMenuItem sub)
        {
            JObject item = null;
            string type = "";

            switch (menuItem)
            {
                case SubMenuItem newSub:
                    item = WriteSubJson(newSub);
                    type = "subMenu";
                    break;
                case AnalogMenuItem analog:
                    item = WriteAnalogJson(analog);
                    type = "analogItem";
                    break;
                case EnumMenuItem enumItem:
                    item = WriteEnumJson(enumItem);
                    type = "enumItem";
                    break;
                case BooleanMenuItem boolean:
                    item = WriteBooleanJson(boolean);
                    type = "boolItem";
                    break;
                case ActionMenuItem action:
                    item = WriteBasicJson(action);
                    type = "actionMenu";
                    break;
                case FloatMenuItem flt:
                    item = WriteFloatJson(flt);
                    type = "floatItem";
                    break;
                case RuntimeListMenuItem rl:
                    item = WriteListJson(rl);
                    type = "runtimeList";
                    break;
                case EditableTextMenuItem ti:
                    item = WriteTextItemJson(ti);
                    type = "textItem";
                    break;
                case LargeNumberMenuItem l:
                    item = WriteLargeNumberJson(l);
                    type = "largeNumItem";
                    break;
                case Rgb32MenuItem rgb:
                    item = WriteRgbItem(rgb);
                    type = "rgbItem";
                    break;
                case ScrollChoiceMenuItem sc:
                    item = WriteScrollItem(sc);
                    type = "scrollItem";
                    break;
                default:
                    logger.Error("Failed to save properly, was missing type for " + menuItem);
                    break;
            }

            var itemWithParent = new JObject
            {
                { "parentId", sub.Id},
                { "type", type },
                { "item", item }
            };

            return itemWithParent;
        }

        private JObject WriteSubJson(SubMenuItem sub)
        {
            var json = WriteBasicJson(sub);
            json.Add("secured", sub.Secured);
            return json;
        }

        private JObject WriteEnumJson(EnumMenuItem item)
        {
            var json = WriteBasicJson(item);

            JArray enumEntries = new JArray();

            foreach(var entry in item.EnumEntries)
            {
                enumEntries.Add(entry);
            }

            json.Add("enumEntries", enumEntries);
            return json;
        }

        private JObject WriteListJson(RuntimeListMenuItem rl)
        {
            var json = WriteBasicJson(rl);
            json.Add("initialRows", rl.InitialRows);
            return json;
        }

        private JObject WriteFloatJson(FloatMenuItem flt)
        {
            var json = WriteBasicJson(flt);
            json.Add("numDecimalPlaces", flt.DecimalPlaces);
            return json;
        }

        private JObject WriteRgbItem(Rgb32MenuItem rgb)
        {
            var json = WriteBasicJson(rgb);
            json.Add("includeAlphaChannel", rgb.IncludeAlphaChannel);
            return json;
        }

        private JObject WriteScrollItem(ScrollChoiceMenuItem sc)
        {
            var json = WriteBasicJson(sc);
            json.Add("itemWidth", sc.ItemWidth);
            json.Add("numEntries", sc.NumEntries);
            json.Add("variable", sc.RamVariable);
            json.Add("eepromOffset", sc.EepromOffset);
            json.Add("choiceMode", Enum.GetName(typeof(ScrollChoiceMode), sc.ChoiceMode));
            return json;
        }

        private JObject WriteBooleanJson(BooleanMenuItem item)
        {
            var json = WriteBasicJson(item);
            json.Add("naming", Enum.GetName(typeof(BooleanNaming), item.Naming));
            return json;
        }

        private JObject WriteTextItemJson(EditableTextMenuItem item)
        {
            var json = WriteBasicJson(item);
            json.Add("itemType", Enum.GetName(typeof(EditItemType), item.EditType));
            json.Add("textLength", item.TextLength);
            return json;
        }

        private JObject WriteLargeNumberJson(LargeNumberMenuItem item)
        {
            var json = WriteBasicJson(item);
            json.Add("decimalPlaces", item.DecimalPlaces);
            json.Add("digitsAllowed", item.TotalDigits);
            json.Add("negativeAllowed", item.AllowNegative);
            return json;
        }

        private JObject WriteAnalogJson(AnalogMenuItem analog)
        {
            var json = WriteBasicJson(analog);
            json.Add("maxValue", analog.MaximumValue);
            json.Add("offset", analog.Offset);
            json.Add("divisor", analog.Divisor);
            json.Add("unitName", analog.UnitName);
            return json;
        }

        private JObject WriteBasicJson(MenuItem sub)
        {
            var obj = new JObject
            {
                { "name", sub.Name },
                { "eepromAddress", sub.EepromAddress },
                { "id", sub.Id },
                { "readOnly", sub.ReadOnly },
                { "localOnly", sub.LocalOnly },
                { "visible", sub.Visible }
            };

            if (sub.FunctionName != null)
            {
                obj.Add("functionName", sub.FunctionName);
            }

            if (sub.VariableName != null)
            {
                obj.Add("variableName", sub.VariableName);
            }

            return obj;
        }
    }
}
