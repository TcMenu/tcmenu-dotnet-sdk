using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Commands;

namespace TcMenu.CoreSdk.Protocol
{
    public class FieldKeyPairs
    {
        public static readonly ushort FIELD_MSG_NAME    = MenuCommand.MakeCmdPair('N', 'M');
        public static readonly ushort FIELD_VERSION     = MenuCommand.MakeCmdPair('V', 'E');
        public static readonly ushort FIELD_PLATFORM    = MenuCommand.MakeCmdPair('P', 'F');
        public static readonly ushort FIELD_BOOT_TYPE   = MenuCommand.MakeCmdPair('B', 'T');
        public static readonly ushort FIELD_HB_INTERVAL = MenuCommand.MakeCmdPair('H', 'I');
        public static readonly ushort FIELD_HB_MILLISEC = MenuCommand.MakeCmdPair('H', 'M');
        public static readonly ushort FIELD_HB_MODE     = MenuCommand.MakeCmdPair('H', 'R');
        public static readonly ushort FIELD_ID          = MenuCommand.MakeCmdPair('I', 'D');
        public static readonly ushort FIELD_EEPROM      = MenuCommand.MakeCmdPair('I', 'E');
        public static readonly ushort FIELD_READONLY    = MenuCommand.MakeCmdPair('R', 'O');
        public static readonly ushort FIELD_VISIBLE     = MenuCommand.MakeCmdPair('V', 'I');
        public static readonly ushort FIELD_PARENT      = MenuCommand.MakeCmdPair('P', 'I');
        public static readonly ushort FIELD_ANALOG_MAX  = MenuCommand.MakeCmdPair('A', 'M');
        public static readonly ushort FIELD_ANALOG_OFF  = MenuCommand.MakeCmdPair('A', 'O');
        public static readonly ushort FIELD_ANALOG_DIV  = MenuCommand.MakeCmdPair('A', 'D');
        public static readonly ushort FIELD_ANALOG_UNIT = MenuCommand.MakeCmdPair('A', 'U');
        public static readonly ushort FIELD_ANALOG_STEP = MenuCommand.MakeCmdPair('A', 'S');
        public static readonly ushort FIELD_CURRENT_VAL = MenuCommand.MakeCmdPair('V', 'C');
        public static readonly ushort FIELD_WIDTH       = MenuCommand.MakeCmdPair('W', 'I');
        public static readonly ushort FIELD_BOOL_NAMING = MenuCommand.MakeCmdPair('B', 'N');
        public static readonly ushort FIELD_NO_CHOICES  = MenuCommand.MakeCmdPair('N', 'C');
        public static readonly ushort FIELD_CHANGE_TYPE = MenuCommand.MakeCmdPair('T', 'C');
        public static readonly ushort FIELD_MAX_LEN     = MenuCommand.MakeCmdPair('M', 'L');
        public static readonly ushort FIELD_REMOTE_NO   = MenuCommand.MakeCmdPair('R', 'N');
        public static readonly ushort FIELD_FLOAT_DP    = MenuCommand.MakeCmdPair('F', 'D');
        public static readonly ushort FIELD_ALLOW_NEG   = MenuCommand.MakeCmdPair('N', 'A');
        public static readonly ushort FIELD_UUID        = MenuCommand.MakeCmdPair('U', 'U');
        public static readonly ushort FIELD_SERIAL_NO   = MenuCommand.MakeCmdPair('U', 'S');
        public static readonly ushort FIELD_CORRELATION = MenuCommand.MakeCmdPair('I', 'C');
        public static readonly ushort FIELD_ACK_STATUS  = MenuCommand.MakeCmdPair('S', 'T');
        public static readonly ushort FIELD_HEADER      = MenuCommand.MakeCmdPair('H', 'F');
        public static readonly ushort FIELD_BUTTON1     = MenuCommand.MakeCmdPair('B', '1');
        public static readonly ushort FIELD_BUTTON2     = MenuCommand.MakeCmdPair('B', '2');
        public static readonly ushort FIELD_BUFFER      = MenuCommand.MakeCmdPair('B', 'U');
        public static readonly ushort FIELD_MODE        = MenuCommand.MakeCmdPair('M', 'O');
        public static readonly ushort FIELD_EDIT_MODE   = MenuCommand.MakeCmdPair('E', 'M');
        public static readonly ushort FIELD_ALPHA       = MenuCommand.MakeCmdPair('R', 'A');
    }
}
