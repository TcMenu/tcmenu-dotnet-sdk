using SkiaSharp;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenuCoreMaui.Controls
{
    public static class PortableColors
    {
        public static readonly PortableColor BLACK = new(0, 0, 0);
        public static readonly PortableColor WHITE = new(255, 255, 255);
        public static readonly PortableColor RED = new(255, 0, 0);
        public static readonly PortableColor INDIGO = new("#4B0082");
        public static readonly PortableColor DARK_GREY = new(80, 80, 80);
        public static readonly PortableColor GREY = new(150, 150, 150);
        public static readonly PortableColor LIGHT_GRAY = new(200, 200, 200);
        public static readonly PortableColor DARK_SLATE_BLUE = new(72, 61, 139);
        public static readonly PortableColor ANTIQUE_WHITE = new(250, 235, 215);
        public static readonly PortableColor DARK_BLUE = new(0, 0, 139);
        public static readonly PortableColor CRIMSON = new(220, 20, 60);
        public static readonly PortableColor CORAL = new(0xff, 0x7f, 0x50);
        public static readonly PortableColor CORNFLOWER_BLUE = new(100, 149, 237);
        public static readonly PortableColor BLUE = new(0, 0, 255);
        public static readonly PortableColor GREEN = new(0, 255, 0);

        public static PortableColor ToPortable(this Color color)
        {
            return new PortableColor((short) (color.Red * 255.0), (short) (color.Green * 255.0), (short) (color.Blue * 255.0));
        }

        public static Color AsXamarin(this PortableColor color)
        {
            return new Color(color.red, color.green, color.blue);
        }

        public static SKColor AsSkia(this PortableColor color)
        {
            return new SKColor((byte)color.red, (byte)color.green, (byte)color.blue);
        }

    }
}
