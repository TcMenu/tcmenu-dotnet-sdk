using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using TcMenu.CoreSdk.Serialisation;
using TcMenuCoreMaui.Controls;

namespace TcMenuCoreMaui.Services
{
    public class ControlColor
    {
        public PortableColor Fg;
        public PortableColor Bg;

        public ControlColor()
        {
            Fg = PortableColors.BLACK;
            Bg = PortableColors.WHITE;
        }

        public ControlColor(PortableColor fg, PortableColor bg)
        {
            Fg = fg;
            Bg = bg;
        }

        public override string ToString()
        {
            return $"Fg: {Fg}, Bg: {Bg}";
        }
    }

    public class PrefsAppSettings
    {
        private readonly Serilog.ILogger logger = Serilog.Log.Logger.ForContext<PrefsAppSettings>();

        private const string ConfigFileName = "tcMenuConfig.xml";
        private const string Root = "TcMenuConfig";
        private const string AttrUuid = "uuid";
        private const string AttrName = "name";
        private const string AttrRecurse = "recurse";
        private const string AttrDefaultCols = "GlobalColorSet";
        private string _baseDir;

        public string UniqueId { get; set; }
        public string LocalName { get; set; }

        public ControlColor UpdateColor { get; set; } = new ControlColor(PortableColors.WHITE, PortableColors.INDIGO);
        public ControlColor ErrorColor { get; set; } = new ControlColor(PortableColors.WHITE, PortableColors.RED);
        public ControlColor PendingColor { get; set; } = new ControlColor(PortableColors.DARK_GREY, PortableColors.LIGHT_GRAY);
        public ControlColor TextColor { get; set; } = new ControlColor(PortableColors.DARK_GREY, PortableColors.LIGHT_GRAY);
        public ControlColor ButtonColor { get; set; } = new ControlColor(PortableColors.DARK_GREY, PortableColors.LIGHT_GRAY);
        public ControlColor HighlightColor { get; set; } = new ControlColor(PortableColors.DARK_GREY, PortableColors.LIGHT_GRAY);
        public ControlColor DialogColor { get; set; } = new ControlColor(PortableColors.DARK_GREY, PortableColors.LIGHT_GRAY);
        public bool RecurseIntoSub { get; set; }

        public PrefsAppSettings()
        {
            SetColorsForMode();
        }

        public void SetColorsForMode()
        {
            if (Application.Current?.RequestedTheme == AppTheme.Dark)
            {
                UpdateColor = new ControlColor(PortableColors.WHITE, PortableColors.DARK_SLATE_BLUE);
                TextColor = new ControlColor(PortableColors.ANTIQUE_WHITE, PortableColors.BLACK);
                PendingColor = new ControlColor(PortableColors.LIGHT_GRAY, PortableColors.DARK_GREY);
                ButtonColor = new ControlColor(PortableColors.WHITE, PortableColors.DARK_BLUE);
                ErrorColor = new ControlColor(PortableColors.WHITE, PortableColors.RED);
                HighlightColor = new ControlColor(PortableColors.WHITE, PortableColors.CRIMSON);
                HighlightColor = new ControlColor(PortableColors.WHITE, PortableColors.DARK_SLATE_BLUE);
            }
            else
            {
                UpdateColor = new ControlColor(PortableColors.WHITE, PortableColors.INDIGO);
                TextColor = new ControlColor(PortableColors.BLACK, PortableColors.WHITE);
                PendingColor = new ControlColor(PortableColors.LIGHT_GRAY, PortableColors.GREY);
                ButtonColor = new ControlColor(PortableColors.BLACK, PortableColors.CORNFLOWER_BLUE);
                ErrorColor = new ControlColor(PortableColors.WHITE, PortableColors.RED);
                HighlightColor = new ControlColor(PortableColors.WHITE, PortableColors.CORAL);
                DialogColor = new ControlColor(PortableColors.WHITE, PortableColors.DARK_SLATE_BLUE);
            }
        }

        public void Load(string basedir)
        {
            _baseDir = basedir;
            var defaultGuid = Guid.NewGuid().ToString();

            try
            {
                var conf = XDocument.Load(Path.Combine(_baseDir, ConfigFileName));
                if (conf.Root == null) throw new ArgumentException($"Document not loaded from {_baseDir}");
                if (!conf.Root.Name.LocalName.Equals(Root)) throw new ArgumentException($"Root element wrong {_baseDir}");
                UniqueId = conf.Root.Attribute(AttrUuid)?.Value ?? defaultGuid;
                LocalName = conf.Root.Attribute(AttrName)?.Value ?? "Unnamed";
                RecurseIntoSub = (bool)conf.Root.Attribute(AttrRecurse);

                var cols = conf.Root.Element(AttrDefaultCols);
                if (cols != null)
                {
                    UpdateColor = FromElement(cols, nameof(UpdateColor)) ?? UpdateColor;
                    ErrorColor = FromElement(cols, nameof(ErrorColor)) ?? ErrorColor;
                    PendingColor = FromElement(cols, nameof(PendingColor)) ?? PendingColor;
                    TextColor = FromElement(cols, nameof(TextColor)) ?? TextColor;
                    ButtonColor = FromElement(cols, nameof(ButtonColor)) ?? ButtonColor;
                    HighlightColor = FromElement(cols, nameof(HighlightColor)) ?? HighlightColor;
                    DialogColor = FromElement(cols, nameof(DialogColor)) ?? DialogColor;
                }
            }
            catch (Exception ex)
            {
                LocalName = "Unnamed";
                UniqueId = defaultGuid;
                logger.Warning(ex, $"Did not load Xml document in {_baseDir}");
            }

            if (defaultGuid.Equals(UniqueId))
            {
                Save();
            }
        }

        private ControlColor FromElement(XElement cols, string elementName)
        {
            var ce = cols.Element(elementName);
            if (ce == null) return null;

            return new ControlColor(new PortableColor((string) ce.Attribute("fg")), new PortableColor((string) ce.Attribute("bg")));
        }

        private Color ColorFromHtml(string strCol)
        {
            if (strCol != null && strCol.Length == 7 && strCol[0] == '#')
            {
                var red = int.Parse(strCol.Substring(1, 2), NumberStyles.HexNumber);
                var green = int.Parse(strCol.Substring(3, 2), NumberStyles.HexNumber);
                var blue = int.Parse(strCol.Substring(5, 2), NumberStyles.HexNumber);

                return new Color((byte)red, (byte)green, (byte)blue);
            }
            
            return Colors.Black;
        }

        public string ToHexColor(Color col)
        {
            var red = (int)(col.Red * 255.0);
            var green = (int)(col.Green * 255.0);
            var blue = (int)(col.Blue * 255.0);
            return "#" + red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");
        }

        public void CloneSettingsFrom(PrefsAppSettings other)
        {
            UniqueId = other.UniqueId;
            LocalName = other.LocalName;
            RecurseIntoSub = other.RecurseIntoSub;
            UpdateColor = other.UpdateColor;
            TextColor = other.TextColor;
            ButtonColor = other.ButtonColor;
            PendingColor = other.PendingColor;
            ErrorColor = other.ErrorColor;
            HighlightColor = other.HighlightColor;
            DialogColor = other.DialogColor;
        }

        public void Save()
        {
            var defaultColElem = new XElement(AttrDefaultCols);
            AppendColor(defaultColElem, UpdateColor, nameof(UpdateColor));
            AppendColor(defaultColElem, TextColor, nameof(TextColor));
            AppendColor(defaultColElem, ButtonColor, nameof(ButtonColor));
            AppendColor(defaultColElem, ErrorColor, nameof(ErrorColor));
            AppendColor(defaultColElem, HighlightColor, nameof(HighlightColor));
            AppendColor(defaultColElem, PendingColor, nameof(PendingColor));
            AppendColor(defaultColElem, DialogColor, nameof(DialogColor));
            XDocument doc = new XDocument(
                new XElement(Root,
                    new XAttribute(AttrName, LocalName),
                    new XAttribute(AttrUuid, UniqueId),
                    new XAttribute(AttrRecurse, RecurseIntoSub),
                    defaultColElem
                )
            );
            doc.Save(Path.Combine(_baseDir, ConfigFileName));
        }

        private void AppendColor(XElement defaultColElem, ControlColor updateColor, string elemName)
        {
            defaultColElem.Add(new XElement(elemName,
                new XAttribute("fg", updateColor.Fg.ToString()),
                new XAttribute("bg", updateColor.Bg.ToString())
            ));
        }
    }
}
