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
    }
}
