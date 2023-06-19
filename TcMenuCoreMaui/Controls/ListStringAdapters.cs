using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Commands;

namespace TcMenuCoreMaui.Controls
{
    public delegate string RuntimeListStringAdapter(string source);

    public static class DefaultListStringAdapters
    {
        public static string RemoteFormattingAdapter(string source)
        {
            try
            {
                string[] parts = source.Split(':');
                if (parts.Length < 4) return source;
                var ch = parts[1][0];
                var sts = ch == 'A' ? "Authenticated" : ch == 'D' ? "Disconnected" : "Connected";
                ApiPlatform platform = (ApiPlatform)int.Parse(parts[3]);
                return $"{parts[0]} V{parts[2]} {sts} {platform}";
            }
            catch (Exception)
            {
                return source;
            }
        }

    }
}
