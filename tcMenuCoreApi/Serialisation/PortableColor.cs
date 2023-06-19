using System.Globalization;

namespace TcMenu.CoreSdk.Serialisation
{
    public readonly struct PortableColor
    {
        public readonly short red;
        public readonly short green;
        public readonly short blue;
        public readonly short alpha;

        public PortableColor(short r, short g, short b, short a = 255)
        {
            red = r;
            green = g;
            blue = b;
            alpha = a;
        }

        private static short ParseHex(char val) 
        {
            if(val >= '0' && val <= '9') return (short)(val - '0');
            val = char.ToUpper(val);
            if(val >= 'A' && val <= 'F') return (short)(val - ('A' - 10));
            return 0;
        }
        
        public PortableColor(string htmlCode)
        {
            alpha = 255;

            if (htmlCode.StartsWith("#") && htmlCode.Length == 4)
            {
                red = (short)(ParseHex(htmlCode[1]) << 4);
                green = (short)(ParseHex(htmlCode[2]) << 4);
                blue = (short)(ParseHex(htmlCode[3]) << 4);
                return;
            }
            if (htmlCode.StartsWith("#") && htmlCode.Length >= 7)
            {
                red = (short)((ParseHex(htmlCode[1]) << 4) + ParseHex(htmlCode[2]));
                green = (short)((ParseHex(htmlCode[3]) << 4) + ParseHex(htmlCode[4]));
                blue = (short)((ParseHex(htmlCode[5]) << 4) + ParseHex(htmlCode[6]));
                if (htmlCode.Length == 9)
                {
                    alpha = (short)((ParseHex(htmlCode[7]) << 4) + ParseHex(htmlCode[8]));
                }
                return;
            }

            red = green = blue = 0;
        }

        public override string ToString()
        {
            var rFmt = red.ToString("X2", CultureInfo.InvariantCulture);
            var gFmt = green.ToString("X2", CultureInfo.InvariantCulture);
            var bFmt = blue.ToString("X2", CultureInfo.InvariantCulture);
            var aFmt = alpha.ToString("X2", CultureInfo.InvariantCulture);
            return $"#{rFmt}{gFmt}{bFmt}{aFmt}";
        }

        public bool Equals(PortableColor other)
        {
            return red == other.red && green == other.green && blue == other.blue && alpha == other.alpha;
        }

        public override bool Equals(object obj)
        {
            return obj is PortableColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = red.GetHashCode();
                hashCode = (hashCode * 397) ^ green.GetHashCode();
                hashCode = (hashCode * 397) ^ blue.GetHashCode();
                hashCode = (hashCode * 397) ^ alpha.GetHashCode();
                return hashCode;
            }
        }
    }
}