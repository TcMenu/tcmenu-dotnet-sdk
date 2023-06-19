using Microsoft.Maui.Controls.Xaml.Internals;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{
    public enum PortableAlignment
    {
        Left,
        Right,
        Center
    }

    public enum RenderStatus { Normal, RecentlyUpdated, EditInProgress, CorrelationError }

    [Flags]
    public enum RedrawingMode
    {
        ShowName = 1,
        ShowValue = 2,
        ShowNameInLabel = 4,
        Hidden = 8,
        ShowNameAndValue = ShowName | ShowValue,
        ShowLabelNameAndValue = ShowNameInLabel | ShowValue
    }

    public class ComponentPositioning
    {
        public int Row { get; }
        public int Col { get; }
        public int RowSpan { get; }
        public int ColSpan { get; }

        public ComponentPositioning(int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            Row = row;
            Col = col;
            RowSpan = rowSpan;
            ColSpan = colSpan;
        }
    }

    public enum ControlType
    {
        /// <summary>
        /// a horizontal slider or progress style control that can present text in the middle
        /// </summary>
        HorizontalSlider,
        /// <summary>
        /// use up down buttons to control items have a range that can be moved through
        /// </summary>
        UpDownControl,
        /// <summary>
        /// Use a text control for more or less any item, it shows the value in a label with an optional edit button
        /// </summary>
        TextControl,
        /// <summary>
        /// Show the control as a button, in the case of boolean items, it will toggle
        /// </summary>
        ButtonControl,
        /// <summary>
        /// Show the control as a VU meter style control for analog items only
        /// </summary>
        VuMeter,
        /// <summary>
        /// Show the control as a date that can be picked if editable
        /// </summary>
        DateControl,
        /// <summary>
        /// Show the control as time, that can be edited if allowed
        /// </summary>
        TimeControl,
        /// <summary>
        /// Show the control as RGB, that can be edited if allowed using an RGB picker
        /// </summary>
        RgbControl,
        /// <summary>
        /// Show as a list of items
        /// </summary>
        ListControl,
        /// <summary>
        /// Show as the authorization and IoT control for that display technology
        /// </summary>
        IoTControl,
        /// <summary>
        /// Indicates the component is not to be rendered
        /// </summary>
        CantRender
    }

    public enum FontSizeMeasurement
    {
        ABS_SIZE, PERCENT
    }

    public class FontInformation
    {
        public static readonly FontInformation Font100Percent = new(100, FontSizeMeasurement.PERCENT);
        public int Size;
        public FontSizeMeasurement Measurement;

        public FontInformation(int size, FontSizeMeasurement measurement)
        {
            Size = size;
            Measurement = measurement;
        }

        public string ToWire() { return Size + ((Measurement == FontSizeMeasurement.PERCENT) ? "%" : ""); }

        public static FontInformation FromWire(string wireFormat)
        {
            if (wireFormat.EndsWith("%"))
            {
                return new FontInformation(int.Parse(wireFormat[..^1]), FontSizeMeasurement.PERCENT);
            }
            else
            {
                return new FontInformation(int.Parse(wireFormat), FontSizeMeasurement.ABS_SIZE);
            }
        }
    }


    public class ComponentSettings
    {
        public static readonly ComponentSettings NoComponent = new ComponentSettings(new NullConditionalColoring(),
                    FontInformation.Font100Percent, new ComponentPositioning(0,0), PortableAlignment.Left, RedrawingMode.ShowName, ControlType.CantRender);

        public FontInformation FontInfo { get; }
        public IConditionalColoring Colors { get; }
        public PortableAlignment Justification { get; }
        public RedrawingMode DrawMode { get; }
        public ComponentPositioning Positioning { get; }
        public ControlType ControlType { get; }
        public bool Customised { get; }

        public ComponentSettings(IConditionalColoring colors, FontInformation fontInfo, ComponentPositioning positioning, PortableAlignment justification, RedrawingMode mode, ControlType controlType, bool custom = false)
        {
            FontInfo = fontInfo;
            Colors = colors;
            Justification = justification;
            Positioning = positioning;
            DrawMode = mode;
            Customised = custom;
            ControlType = controlType;
        }

        public static bool IsComponentSupportedFor(ControlType controlType, MenuItem item)
        {
            switch (controlType)
            {
                case ControlType.HorizontalSlider: return item is AnalogMenuItem;
                case ControlType.UpDownControl: return item is AnalogMenuItem or EnumMenuItem or ScrollChoiceMenuItem;
                case ControlType.TextControl: return true;
                case ControlType.VuMeter: return item is AnalogMenuItem;
                case ControlType.ButtonControl: return item is BooleanMenuItem or ActionMenuItem or SubMenuItem;
                case ControlType.DateControl: return item is EditableTextMenuItem { EditType: EditItemType.GREGORIAN_DATE };
                case ControlType.CantRender: return true;
                case ControlType.RgbControl: return item is Rgb32MenuItem;
                case ControlType.ListControl: return item is RuntimeListMenuItem;
                case ControlType.IoTControl: return true;
                case ControlType.TimeControl:
                    return item is EditableTextMenuItem
                    {
                        EditType: EditItemType.TIME_12H or EditItemType.TIME_12H_HHMM or EditItemType.TIME_24H or EditItemType.TIME_24H_HHMM or 
                        EditItemType.TIME_24_HUNDREDS or EditItemType.TIME_DURATION_HUNDREDS or EditItemType.TIME_DURATION_SECONDS
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(controlType), controlType, null);
            }
        }

        public static ControlType DefaultComponentTypeFor(MenuItem item)
        {
            switch (item)
            {
                case AnalogMenuItem: return ControlType.HorizontalSlider;
                case EnumMenuItem or ScrollChoiceMenuItem: return ControlType.UpDownControl;
                case BooleanMenuItem or ActionMenuItem: return ControlType.ButtonControl;
                case EditableTextMenuItem et:
                {
                    return et.EditType switch
                    {
                        EditItemType.GREGORIAN_DATE => ControlType.DateControl,
                        EditItemType.PLAIN_TEXT => ControlType.TextControl,
                        EditItemType.IP_ADDRESS => ControlType.TextControl,
                        EditItemType.TIME_24H => ControlType.TimeControl,
                        EditItemType.TIME_12H => ControlType.TimeControl,
                        EditItemType.TIME_24_HUNDREDS => ControlType.TimeControl,
                        EditItemType.TIME_DURATION_SECONDS => ControlType.TimeControl,
                        EditItemType.TIME_DURATION_HUNDREDS => ControlType.TimeControl,
                        EditItemType.TIME_24H_HHMM => ControlType.TimeControl,
                        EditItemType.TIME_12H_HHMM => ControlType.TimeControl,
                        _ => ControlType.TimeControl
                    };
                }
                case LargeNumberMenuItem: return ControlType.TextControl;
                case Rgb32MenuItem: return ControlType.RgbControl;
                case SubMenuItem: return ControlType.TextControl;
                case RuntimeListMenuItem: return ControlType.ListControl;
                default: return ControlType.TextControl;
            }
        }
    }
}
