using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenuCoreMaui.Utils
{
    public static class StringHelper
    {
        public static string CapsUnderscoreToReadable(this string toTidyUp)
        {
            var split = toTidyUp.Split('_');
            var ret = new StringBuilder();

            foreach (var s in split)
            {
                if (s.Length > 1)
                {
                    ret.Append(char.ToUpper(s[0]));
                    foreach (var c in s.Substring(1))
                    {
                        ret.Append(char.ToLower(c));
                    }
                }
                else ret.Append(s);
                ret.Append(' ');
            }
            return ret.ToString();
        }
    }
}
