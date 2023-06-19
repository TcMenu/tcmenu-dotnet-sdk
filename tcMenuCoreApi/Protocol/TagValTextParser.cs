using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TcMenu.CoreSdk.Commands;

namespace TcMenu.CoreSdk.Protocol
{
    /// <summary>
    /// This is the parser implementation that understands tag value format and can convert the tags back into
    /// a series of tags and values suitable for the protocol to decode messages.
    /// </summary>
    public class TagValTextParser
    {
        public const int MAX_MESSAGE_VALUE_LEN = 1024;

        private Dictionary<ushort, string> _keyValuePairs = new Dictionary<ushort, string>();

        public TagValTextParser(Stream stream)
        {
            bool foundEnd = false;
            while (!foundEnd)
            {
                string key = ReadString(stream);
                if (key.Length > 0 && key[0] == DefaultProtocolCommandConverter.END_OF_MESSAGE)
                {
                    foundEnd = true;
                }
                else
                {
                    if (String.IsNullOrEmpty(key) || key.Length != 2)
                    {
                        throw new TcProtocolException($"Incorrect Key: {key}");
                    }
                    string value = ReadString(stream);
                    if (value.Length > 0 && value[0] == DefaultProtocolCommandConverter.END_OF_MESSAGE)
                    {
                        foundEnd = true;
                    }
                    _keyValuePairs.Add(MenuCommand.MakeCmdPair(key[0], key[1]), value);
                }
            }
        }

        private string ReadString(Stream stream)
        {
            StringBuilder sb = new StringBuilder(40);
            bool foundEnd = false;
            while (!foundEnd || sb.Length > MAX_MESSAGE_VALUE_LEN)
            {
                int ch = stream.ReadByte();
                if (ch == DefaultProtocolCommandConverter.END_OF_MESSAGE)
                {
                    // no matter where we find an end of message, normalise it and end the message.
                    sb.Clear();
                    return "\u0002";
                }
                else if (ch == '\\')
                {
                    // special escape case allows | or ~ to be sent
                    sb.Append((char)stream.ReadByte());
                }
                else if (ch == -1 || ch == TagValProtocolMessageProcessors.KEY_VAL_SEPARATOR || ch == TagValProtocolMessageProcessors.END_OF_FIELD)
                {
                    foundEnd = true;
                }
                else
                {
                    // within current token
                    sb.Append((char)ch);
                }
            }
            return sb.ToString();
        }

        public string GetValueForKey(ushort keyPair)
        {
            if (!_keyValuePairs.ContainsKey(keyPair)) throw new TcProtocolException($"Missing Key in Msg: {keyPair}");
            return _keyValuePairs[keyPair];
        }

        public string GetValueForKeyWithDefault(ushort pair, string defaultStr)
        {
            if (!_keyValuePairs.ContainsKey(pair)) return defaultStr;
            return _keyValuePairs[pair];
        }

        public int GetValueForKeyAsInt(ushort keyPair)
        {
            if (!_keyValuePairs.ContainsKey(keyPair)) throw new TcProtocolException($"Missing Key in Msg: {keyPair}");
            return int.Parse(_keyValuePairs[keyPair]);
        }

        public int GetValueForKeyAsIntWithDefault(ushort keyPair, int defaultInt)
        {
            if (!_keyValuePairs.ContainsKey(keyPair)) return defaultInt;
            return int.Parse(_keyValuePairs[keyPair]);
        }

        public bool HasKey(ushort keyPair)
        {
            return _keyValuePairs.ContainsKey(keyPair);
        }

        public List<string> GetAllKeysAsString(char prefix1, char prefix2 = (char)0)
        {
            List<string> toReturn = new List<string>();
            ushort key = MenuCommand.MakeCmdPair(prefix1, 'A');
            int i = 0;
            while (HasKey(key))
            {
                var str = GetValueForKeyWithDefault(key, "");
                if (prefix2 != 0)
                {
                    ushort key2 = MenuCommand.MakeCmdPair(prefix2, (char)('A' + i));
                    str += "\t";
                    str += GetValueForKeyWithDefault(key2, "");
                }
                toReturn.Add(str);
                i++;
                key = MenuCommand.MakeCmdPair(prefix1, (char)('A' + i));
            }
            return toReturn;
        }
    }
}
