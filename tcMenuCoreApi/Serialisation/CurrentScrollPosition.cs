using System.Globalization;

namespace TcMenu.CoreSdk.Serialisation
{
    public readonly struct CurrentScrollPosition
    {
        public readonly int Position;
        public readonly string Value;

        public CurrentScrollPosition(int position, string value)
        {
            Position = position;
            Value = value;
        }

        public CurrentScrollPosition(string combined)
        {
            var splitPoint = combined?.IndexOf('-') ?? -1;
            if(splitPoint != -1)
            {
                var numStr = combined.Substring(0, splitPoint);
                Position = int.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx) ? idx : 0;
                Value = combined.Substring(splitPoint + 1);
                return;
            }
            Position = 0;
            Value = "Unknown";
        }

        public override string ToString()
        {
            return Position + "-" + Value;
        }
    }
}