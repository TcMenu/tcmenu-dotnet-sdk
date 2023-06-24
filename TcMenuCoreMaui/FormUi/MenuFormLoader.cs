using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenuCoreMaui.Controls;
using TcMenuCoreMaui.Services;
using Font = Microsoft.Maui.Font;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.FormUi
{
    public class MenuFormLoader
    {
        private static readonly Guid SpecialWildcardAutoLayout = Guid.Parse("FF3E1294-2F0A-418A-8E24-2115A6A2FA39");

        private readonly Serilog.ILogger _logger = Log.Logger.ForContext<MenuFormLoader>();
        public List<IConditionalColoring> ColorSets { get; } = new();
        public Guid Uuid { get; }
        public string LayoutName { get; }
        public Dictionary<int, SubMenuStore> SubMenuStores => new();
        public int GridSize { get; set; } = 1;

        private readonly PrefsAppSettings _settings;
        private readonly MenuTree _tree;

        public MenuFormLoader(PrefsAppSettings settings, MenuTree tree)
        {
            _tree = tree;
            _settings = settings;
            GridSize = 1;
            ColorSets.Add(new PrefsConditionalColoring(settings));
            LayoutName = "Default";
            Uuid = SpecialWildcardAutoLayout;
        }


        public MenuFormLoader(PrefsAppSettings settings, string fileName, MenuTree tree) 
        {
            try
            {
                _settings = settings;
                _tree = tree;
                var doc = XDocument.Load(fileName);
                if (doc?.Root?.Name?.LocalName.Equals("EmbedControl") ?? false)
                {
                    Uuid = Guid.Parse(doc.Root.Attribute("boardUuid")?.Value ?? "");
                    LayoutName = doc.Root.Attribute("layoutName")?.Value;
                    var colorSets = doc.Root.Element("ColorSets");
                    if (colorSets != null)
                    {
                        foreach (var colorSet in colorSets.Elements("ColorSet"))
                        {
                            ColorSets.Add(new NamedXmlConditionalColoring(settings, colorSet));
                        }

                        ColorSets.Add(new PrefsConditionalColoring(settings));
                    }

                    var menuLayouts = doc.Root.Element("MenuLayouts");
                    if (menuLayouts != null)
                    {
                        foreach (var layout in menuLayouts.Elements("MenuLayout"))
                        {
                            ProcessMenuLayout(layout);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Did not load form from {fileName}");
            }
        }

        private void ProcessMenuLayout(XElement layout)
        {
            var sub = Convert.ToInt32(layout.Attribute("rootId")?.Value);
            var fontInfo = FontInformation.FromWire(layout.Attribute("fontInfo")?.Value);
            var recursive = layout.Attribute("recursive")?.Value?.Equals("true") ?? false;
            var colSet = new PrefsConditionalColoring(_settings);
            var store = new SubMenuStore(sub, colSet, fontInfo, recursive);
            GridSize = int.Min(1, Convert.ToInt32(layout.Attribute("cols")?.Value ?? "1"));

            foreach (var layoutItem in layout.Elements())
            {
                if (layoutItem.Name.LocalName.Equals("MenuElement")) InsertMenuElement(layoutItem, store);
                else if (layoutItem.Name.LocalName.Equals("VertSpace")) InsertVertSpace(layoutItem, store);
                else if (layoutItem.Name.LocalName.Equals("StaticText")) InsertStaticText(layoutItem, store);
                else _logger.Warning("Unknown menu layout element ", layoutItem);
            }

            SubMenuStores.Add(sub, store);
        }

        private void InsertMenuElement(XElement layoutItem, SubMenuStore store)
        {
            var menuItem = new MenuItemFormItem(
                _tree.GetMenuById(Convert.ToInt32(layoutItem.Attribute("menuId")?.Value ?? "0")),
                SafeColorSet(layoutItem.Attribute("colorSet")?.Value ?? ""),
                ComponentPositioning.FromWire(layoutItem.Attribute("position")?.Value ?? "0,0"),
                ControlTypeFromWire(layoutItem.Attribute("controlType")?.Value),
                ToLayoutItem(layoutItem.Attribute("alignment")?.Value),
                RedrawingModeFromWire(layoutItem.Attribute("controlType")?.Value)
            );

            SetFormItemAt(store, menuItem.Positioning.Row, menuItem.Positioning.Col, menuItem);
        }

        private RedrawingMode RedrawingModeFromWire(string value)
        {
            if (value == null) return RedrawingMode.Hidden;

            return value switch
            {
                "SHOW_NAME" => RedrawingMode.ShowName,
                "SHOW_VALUE" => RedrawingMode.ShowValue,
                "SHOW_NAME_VALUE" => RedrawingMode.ShowNameAndValue,
                _ => RedrawingMode.Hidden
            };
        }

        private ControlType ControlTypeFromWire(string controlType)
        {
            if(controlType == null) return ControlType.CantRender;

            return controlType switch
            {
                "HORIZONTAL_SLIDER" => ControlType.HorizontalSlider,
                "UP_DOWN_CONTROL" => ControlType.UpDownControl,
                "TEXT_CONTROL" => ControlType.TextControl,
                "BUTTON_CONTROL" => ControlType.ButtonControl,
                "VU_METER" => ControlType.VuMeter,
                "DATE_CONTROL" => ControlType.DateControl,
                "TIME_CONTROL" => ControlType.TimeControl,
                "RGB_CONTROL" => ControlType.RgbControl,
                "LIST_CONTROL" => ControlType.ListControl,
                "AUTH_IOT_CONTROL" => ControlType.IoTControl,
                "CANT_RENDER" => ControlType.CantRender,
                _ => ControlType.CantRender
            };
        }

        private void InsertStaticText(XElement layoutItem, SubMenuStore store)
        {
            var textItem = new TextFormItem(
                layoutItem.Value.Trim(),
                SafeColorSet(layoutItem.Attribute("colorSet")?.Value ?? ""),
                ComponentPositioning.FromWire(layoutItem.Attribute("position")?.Value ?? "0,0"),
                ToLayoutItem(layoutItem.Attribute("alignment")?.Value)
            );
            SetFormItemAt(store, textItem.Positioning.Row, textItem.Positioning.Col, textItem);
        }

        private PortableAlignment ToLayoutItem(string attribute)
        {
            if(attribute == null) return PortableAlignment.Left;

            return attribute switch
            {
                "LEFT" => PortableAlignment.Left,
                "RIGHT" => PortableAlignment.Right,
                _ => PortableAlignment.Center
            };
        }

        private void InsertVertSpace(XElement layoutItem, SubMenuStore store)
        {
            var space = new VerticalSpaceFormItem(
                SafeColorSet(PrefsConditionalColoring.GlobalColorSetName),
                ComponentPositioning.FromWire(layoutItem.Attribute("position")?.Value ?? "0,0"),
                Convert.ToInt32(layoutItem.Attribute("height")?.Value ?? "10")
            );

            SetFormItemAt(store, space.Positioning.Row, space.Positioning.Col, space);
        }

        private void SetFormItemAt(SubMenuStore store, int row, int col, MenuFormItem item)
        {
            if (!store.RowEntries.ContainsKey(row))
            {
                store.RowEntries[row] = new RowEntry(GridSize);
            }

            store.RowEntries[row].Items[col] = item;
        }

        private IConditionalColoring SafeColorSet(string cs)
        {
            return ColorSets.FirstOrDefault(c => c.ColorName.Equals(cs)) ?? new PrefsConditionalColoring(_settings);
        }

        public bool HasLayoutFor(SubMenuItem sub)
        {
            return SubMenuStores.ContainsKey(sub.Id);
        }
    }

    public class SubMenuStore
    {
        public int SubId { get; }
        public Dictionary<int, RowEntry> RowEntries => new();
        public IConditionalColoring ColorSet { get; }
        public FontInformation FontInfo { get; }
        public bool Recursive { get; }

        public SubMenuStore(int subId, IConditionalColoring colSet, FontInformation fontInfo, bool recursive)
        {
            Recursive = recursive;
            SubId = subId;
            FontInfo = fontInfo;
            ColorSet = colSet;
        }

        public List<MenuFormItem> AllFormEntries()
        {
            var l = new List<MenuFormItem>(128);
            foreach(var ent in RowEntries.Values)
            {
                foreach(var it in ent.Items)
                {
                    if (it != null && it is not NoFormItem) {
                        l.Add(it);
                    }
                }
            }

            return l.OrderBy((item1) => item1.Positioning.Row).ToList();
        }
    }

    public class RowEntry
    {
        public MenuFormItem[] Items { get; set; }

        public RowEntry(int gridSize)
        {
            Items = new MenuFormItem[gridSize];
            Array.Fill(Items, MenuFormItem.NoFormItemDefined);
        }

        void ResizeTo(int cols)
        {
            // keep a copy of old data to copy as much as we can into new array
            var oldData = Items;
            int oldCols = Items.Length;
            int toFill = Math.Min(oldCols, cols);

            // create an empty array the right size
            Items = new MenuFormItem[cols];
            Array.Fill(Items, MenuFormItem.NoFormItemDefined);

            // and copy over as much as we can without overflowing either array
            for (int i = 0; i < toFill; i++)
            {
                Items[i] = oldData[i];
            }
        }

        public MenuFormItem this[int idx]
        {
            get => Items[idx];
            set => Items[idx] = value;
        }

        public int Count => Items.Length;
    }
}
