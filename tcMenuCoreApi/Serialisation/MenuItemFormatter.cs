using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TcMenu.CoreSdk.MenuItems;

namespace TcMenu.CoreSdk.Serialisation
{
    public class MenuItemFormatException : Exception
    {
        public MenuItemFormatException()
        {
        }

        public MenuItemFormatException(string message) : base(message)
        {
        }

        public MenuItemFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public static class MenuItemFormatter
    {
        public static string FormatToWire(MenuItem item, string text)
        {
            switch (item)
            {
                case FloatMenuItem _:
                case ActionMenuItem _:
                case SubMenuItem _:
                case RuntimeListMenuItem _:
                    throw new MenuItemFormatException("Not wire editable");
                case AnalogMenuItem an:
                    return FormatAnalogWire(an, text);
                case EnumMenuItem en:
                    return FormatEnumWire(en, text);
                case BooleanMenuItem bi:
                    return FormatBoolWire(bi, text);
                case LargeNumberMenuItem ln:
                    return FormatLargeNumWire(ln, text);
                case Rgb32MenuItem rgb:
                    return FormatRgbItemWire(rgb, text);
                case ScrollChoiceMenuItem sc:
                    return FormatScrollItemWire(sc, text);
                case EditableTextMenuItem et:
                    return FormatEditableTextWire(et, text);
                default: return text;
            }
        }
        
        private static string FormatEditableTextWire(EditableTextMenuItem et, string text)
        {
            if(et.EditType == EditItemType.PLAIN_TEXT && text.Length < et.TextLength)
            {
                return text;
            }
            else if(et.EditType == EditItemType.IP_ADDRESS)
            {
                if (!Regex.IsMatch(text, @"\d+\.\d+\.\d+\.\d+")) throw new MenuItemFormatException($"IP Address {text} invalid");
                return text;
            }
            else if(et.EditType == EditItemType.TIME_24H || et.EditType == EditItemType.TIME_24_HUNDREDS || et.EditType == EditItemType.TIME_12H)
            {
                // time is always sent back to the server in 24 hour format, it is always possible (but optional) to provide hundreds/sec.
                if (!Regex.IsMatch(text, @"\d+\:\d+\:\d+(.\d*)*")) throw new MenuItemFormatException($"Time {text} invalid");
                return text;
            }
            else if (et.EditType == EditItemType.GREGORIAN_DATE)
            {
                if (!Regex.IsMatch(text, @"\d+\/\d+\/\d+")) throw new MenuItemFormatException($"Date {text} invalid");
                return text;                
            }

            throw new MenuItemFormatException($"{et.EditType} validation failed for {text}");
        }

        private static string FormatLargeNumWire(LargeNumberMenuItem ln, string text)
        {
            if(decimal.TryParse(text, out var val))
            {
                double largestWhole = Math.Pow(10, (ln.TotalDigits - ln.DecimalPlaces));

                if (val >= (decimal)largestWhole)
                {
                    throw new MenuItemFormatException($"{text} is too big for {ln.Name}, largest number is {largestWhole}");
                }

                if ((val < 0 && !ln.AllowNegative))
                {
                    throw new MenuItemFormatException($"{text} is negative but {ln.Name} does not allow negative values");
                }
                return text;
            }
            throw new MenuItemFormatException($"Large number {text} invalid");
        }

        private static string FormatRgbItemWire(Rgb32MenuItem rgb, string text)
        {
            return new PortableColor(text).ToString();            
        }

        private static string FormatScrollItemWire(ScrollChoiceMenuItem scroll, string text)
        {
            if(int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sel))
            {
                return new CurrentScrollPosition(sel, "").ToString();
            }
            throw new MenuItemFormatException("Scroll item wire type is int");
        }

        private static string FormatBoolWire(BooleanMenuItem bi, string text)
        {
            text = text.ToUpperInvariant();
            if (text == "ON" || text == "YES" || text == "TRUE") return "1";
            else if (text == "OFF" || text == "NO" || text == "FALSE") return "0";
            throw new MenuItemFormatException("Not a valid boolean value");
        }

        private static string FormatEnumWire(EnumMenuItem en, string text)
        {
            if (uint.TryParse(text, out var val))
            {
                if (val < en.EnumEntries.Count)
                {
                    return text;
                }
            }
            else if (en.EnumEntries.Contains(text))
            {
                return en.EnumEntries.IndexOf(text).ToString(CultureInfo.InvariantCulture);
            }
            throw new MenuItemFormatException($"Invalid enum value: {text}");
        }


        private static int GetActualDecimalDivisor(int divisor)
        {
            if (divisor < 2) return 1;
            return (divisor > 1000) ? 10000 : (divisor > 100) ? 1000 : (divisor > 10) ? 100 : 10;
        }

        private static string FormatAnalogWire(AnalogMenuItem an, string text)
        {
            if(!string.IsNullOrEmpty(an.UnitName)) text = text.Replace(an.UnitName, "");

            if(an.Divisor < 2)
            {
                if (int.TryParse(text, out var parsed))
                {
                    var intVal = parsed - an.Offset;
                    if (intVal < 0 || intVal > an.MaximumValue) throw new MenuItemFormatException("Integer out of range");
                    return intVal.ToString(CultureInfo.InvariantCulture);
                }
                throw new MenuItemFormatException("Not an integer and divisor is 0");
            }

            if (double.TryParse(text, out var value))
            {
                var maxFract = GetActualDecimalDivisor(an.Divisor);
                int whole = (int)value;
                int fract = (int)(maxFract * (value - whole));
                int correctedFraction = fract / (maxFract / an.Divisor);
                int calcVal =  ((whole * an.Divisor) + correctedFraction) - an.Offset;
                if (calcVal < 0 || calcVal > an.MaximumValue) throw new MenuItemFormatException("Value out of range");
                return calcVal.ToString(CultureInfo.InvariantCulture);
            }
            throw new MenuItemFormatException("Not a valid numeric value");
        }

        public static string FormatForDisplay(MenuItem item, object data)
        {
            switch(item)
            {
                case FloatMenuItem fl:
                    return FormatFloatForDisplay(fl, (float) data);
                case AnalogMenuItem an:
                    return FormatAnalogForDisplay(an, (int) data);
                case BooleanMenuItem bl:
                    return FormatBoolForDisplay(bl, (bool) data);
                case EnumMenuItem en:
                    return FomatEnumForDisplay(en, (int)data);
                case LargeNumberMenuItem ln:
                    return FormatLargeNumForDisplay(ln, (decimal)data);
                case EditableTextMenuItem tm:
                    return FormatTextForDisplay(tm, (string)data);
                case Rgb32MenuItem rgb:
                    return FormatRgbItemForDisplay(rgb, (PortableColor)data);
                case ScrollChoiceMenuItem sc:
                    return FormatScrollItemForDisplay(sc, (CurrentScrollPosition) data);                
                case ActionMenuItem _:
                case SubMenuItem _:
                default:
                    return "";
            }
        }

        private static string FormatScrollItemForDisplay(ScrollChoiceMenuItem sc, CurrentScrollPosition data)
        {
            return data.Value;
        }

        private static string FormatRgbItemForDisplay(Rgb32MenuItem rgb, PortableColor col)
        {
            return col.ToString();
        }

        private static string FormatTextForDisplay(EditableTextMenuItem tm, string data)
        {
            return data;
        }

        private static string FormatLargeNumForDisplay(LargeNumberMenuItem ln, decimal data)
        {
            return data.ToString($"F{ln.DecimalPlaces}");
        }

        private static string FomatEnumForDisplay(EnumMenuItem en, int data)
        {
            if(en.EnumEntries.Count > data)
            {
                return en.EnumEntries[data];
            }
            return "";
        }

        private static string FormatBoolForDisplay(BooleanMenuItem bl, bool val)
        {
            switch (bl.Naming)
            {
                case BooleanNaming.ON_OFF:
                    return val ? "On" : "Off";
                case BooleanNaming.YES_NO:
                    return val ? "Yes" : "No";
                case BooleanNaming.TRUE_FALSE:
                default:
                    return val ? "True" : "False";
            }
        }

        private static string FormatAnalogForDisplay(AnalogMenuItem an, int val)
        {
            int calcVal = val + an.Offset;
            int divisor = an.Divisor;

            if (divisor < 2)
            {
                return calcVal.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var whole = (calcVal / divisor).ToString(CultureInfo.InvariantCulture);
                int fractMax = GetActualDecimalDivisor(an.Divisor);
                int fraction = Math.Abs((calcVal % divisor)) * (fractMax / divisor);

                return whole + "." + fraction.ToString($"D{CalculateRequiredDigits(divisor)}") + (an.UnitName ?? "");
            }
        }

        private static object CalculateRequiredDigits(int divisor)
        {
            return (divisor <= 10) ? 1 : (divisor <= 100) ? 2 : (divisor <= 1000) ? 3 : 4;
        }

        private static string FormatFloatForDisplay(FloatMenuItem fl, float val)
        {
            return val.ToString($"F{fl.DecimalPlaces}");
        }
    }
}
