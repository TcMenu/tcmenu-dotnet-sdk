using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdk.Commands
{
    /// <summary>
    /// the base type of all commands.
    /// </summary>
    /// <summary>
    /// In this communication API, everything that can be sent / received over the wire is represented
    /// by a command. Every "command" is converted into a particular protocol and transmitted over the
    /// wire. Likewise, every protocol message that is received is converted into a command.
    /// </summary>
    public class MenuCommand
    {
        /// <summary>
        /// Every command has a type, usually these are two character codes generated using MakeCmdPair
        /// </summary>
        /// <see cref="MakeCmdPair(char, char)"/>
        public ushort CommandType { get; }

        public MenuCommand(ushort cmdType)
        {
            CommandType = cmdType;
        }

        /// <summary>
        /// Converts a two character message or field ID into a ushort, the internal format for
        /// field names and message types.
        /// </summary>
        /// <param name="hi">the high byte (or first of the two characters)</param>
        /// <param name="lo">the low byte (or second of the two characters)</param>
        /// <returns></returns>
        public static ushort MakeCmdPair(char hi, char lo)
        {
            return (ushort)( (((byte)hi) << 8) | (lo & 0xff) );
        }
    }

    /// <summary>
    /// Representation of a heartbeat message that is sent / received when the connection is
    /// otherwise idle, so the higher layers can detect loss of connection.
    /// </summary>
    public class HeartbeatCommand : MenuCommand
    {
        public static readonly ushort HEARTBEAT_CMD_ID = MenuCommand.MakeCmdPair('H', 'B');
        /// <summary>
        /// The server (usually embedded device) sets the interval at which we send heartbeats
        /// </summary>
        public int Interval { get; }
        /// <summary>
        /// Optionally a timestamp of the API / devices choosing can be sent for diagnostics.
        /// </summary>
        public long Timestamp { get; }

        /// <summary>
        /// Signify the type of heartbeat to send, usually NORMAL.
        /// </summary>
        public HeartbeatMode Mode { get; }

        /// <summary>
        /// Construct a heartbeat message
        /// </summary>
        /// <param name="interval">the interval at which heartbeats are being sent</param>
        /// <param name="timestamp">an optional timestamp, can be 0</param>
        public HeartbeatCommand(HeartbeatMode mode, int interval, long timestamp = 0) : base(HEARTBEAT_CMD_ID)
        {
            Mode = mode;
            Interval = interval;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"Heartbeat[Mode={Mode}, Interval={Interval}, Timestamp={Timestamp}]";
        }
    }

    /// <summary>
    /// Representation of a new joiner message in the API.
    /// </summary>
    public class NewJoinerCommand : MenuCommand
    {
        public static readonly ushort NEW_JOINER_CMD_ID = MenuCommand.MakeCmdPair('N', 'J');
        /// <summary>
        /// Name of the connection
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// UUID associated with this connection
        /// </summary>
        public string Uuid { get; }
        /// <summary>
        /// The serial number of the board
        /// </summary>
        public int SerialNumber { get; }
        /// <summary>
        /// API version for this connection
        /// </summary>
        public ushort ApiVersion { get; }
        /// <summary>
        /// API platform for this connection
        /// </summary>
        public ApiPlatform ApiPlatform { get; }

        /// <summary>
        /// Constructs a new joiner command message setting all the fields
        /// </summary>
        /// <param name="name">name of the connection</param>
        /// <param name="uuid">uuid for the connection</param>
        /// <param name="version">version of the connection</param>
        /// <param name="platform">platform for the connection</param>
        /// <param name="serialNumber">the serial number for the connection</param>
        public NewJoinerCommand(string name, string uuid, ushort version, ApiPlatform platform, int serialNumber) : base(NEW_JOINER_CMD_ID)
        {
            Name = name;
            Uuid = uuid;
            ApiVersion = version;
            ApiPlatform = platform;
            SerialNumber = serialNumber;
        }

        public override string ToString()
        {
            return $"NewJoiner[Name={Name}, Uuid={Uuid} (S/N={SerialNumber}), ApiVer={ApiVersion}, ApiPlatform={ApiPlatform}]";
        }
    }

    /// <summary>
    /// A command that indicates either a bootstrap operation is starting or finishing
    /// </summary>
    public class BootstrapCommand : MenuCommand
    {
        public static readonly ushort BOOTSTRAP_CMD_ID = MenuCommand.MakeCmdPair('B', 'S');

        /// <summary>
        /// Indicate the mode, either START or END
        /// </summary>
        public BootstrapType BootType { get; }

        /// <summary>
        /// Create a bootstrap message indicating the type
        /// </summary>
        /// <param name="bootType">the mode of bootstrap</param>
        public BootstrapCommand(BootstrapType bootType) : base(BOOTSTRAP_CMD_ID)
        {
            BootType = bootType;
        }

        public override string ToString()
        {
            return $"Bootstrap[Type={BootType}]";
        }
    }

    /// <summary>
    /// The command that is used to pair with another device. It contains the name and UUID
    /// of the client that is attempting to pair.
    /// </summary>
    public class PairingCommand : MenuCommand
    {
        public static readonly ushort PAIRING_CMD_ID = MenuCommand.MakeCmdPair('P', 'R');

        /// <summary>
        /// The UUID of the application that is requesting to pair.
        /// </summary>
        public string Uuid { get; }

        /// <summary>
        /// The name of the application that is requesting to pair
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Create a pairing request providing the name and uuid.
        /// </summary>
        /// <param name="uuid">the UUID of the requestor</param>
        /// <param name="name">the name of the requestor</param>
        public PairingCommand(string uuid, string name) : base(PAIRING_CMD_ID)
        {
            Uuid = uuid;
            Name = name;
        }

        public override string ToString()
        {
            return $"Pairing[Uuid={Uuid}, Name={Name}]";
        }
    }

    /// <summary>
    /// A command that represents an acknowledgement of a request, it generally is associated
    /// with a correlation id, some kind of operation and a status.
    /// </summary>
    public class AcknowledgementCommand : MenuCommand
    {
        public static readonly ushort ACKNOWLEDGEMENT_CMD_ID = MenuCommand.MakeCmdPair('A', 'K');

        /// <summary>
        /// The correlation id associated with the ack
        /// </summary>
        public CorrelationId Correlation;

        /// <summary>
        /// the status of the ack.
        /// </summary>
        public AckStatus Status;

        /// <summary>
        /// Create an acknowledgement command based on a correlation and status
        /// </summary>
        /// <param name="correlation">the correlation id</param>
        /// <param name="status">the status of the ACK</param>
        public AcknowledgementCommand(CorrelationId correlation, AckStatus status) : base(ACKNOWLEDGEMENT_CMD_ID)
        {
            Correlation = correlation;
            Status = status;
        }

        public override string ToString()
        {
            return $"Ack[Correlation={Correlation}, Status={Status}]";
        }
    }

    /// <summary>
    /// Represents a message that updates the dialog state. It can be sent in either direction
    /// </summary>
    public class DialogCommand : MenuCommand
    {
        public static readonly ushort DIALOG_CMD_ID = MenuCommand.MakeCmdPair('D', 'M');

        /// <summary>
        /// The current mode of the dialog, when sending to the embeded device only action is supported
        /// </summary>
        public DialogMode Mode { get; }

        /// <summary>
        /// The text of the header section
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// The text of the body.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The button for the left of the dialog
        /// </summary>
        public MenuButtonType Button1 { get; }

        /// <summary>
        /// The button for the right of the dialog
        /// </summary>
        public MenuButtonType Button2 { get; }

        /// <summary>
        /// The correlation id that will be returned in an acknowledgment
        /// </summary>
        public CorrelationId Correlation { get; }

        /// <summary>
        /// Create a dialog command.
        /// </summary>
        /// <param name="mode">The mode that should be used</param>
        /// <param name="header">Header text</param>
        /// <param name="msg">The message body</param>
        /// <param name="button1">The left button</param>
        /// <param name="button2">The right button</param>
        /// <param name="correlation">The correlation id to be returned by acknowledgement</param>
        public DialogCommand(DialogMode mode, string header, string msg, MenuButtonType button1, MenuButtonType button2, CorrelationId correlation) : base(DIALOG_CMD_ID)
        {
            Mode = mode;
            Header = header;
            Message = msg;
            Button1 = button1;
            Button2 = button2;
            Correlation = correlation;
        }

        public override string ToString()
        {
            return $"DlgMsg[mode={Mode}, header={Header}, msg={Message}, b1={Button1}, b2={Button2}]";
        }
    }

    /// <summary>
    /// Represents a change in a value of a menu item. Either delta or absolute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MenuChangeCommand : MenuCommand
    {
        public static readonly ushort CHANGE_CMD_ID = MenuCommand.MakeCmdPair('V', 'C');

        /// <summary>
        /// the id of the changed item 
        /// </summary>
        public int MenuId { get; }

        /// <summary>
        /// the correlation ID of the change
        /// </summary>
        public CorrelationId Correlation { get; }
        
        /// <summary>
        /// The type of change, delta, absolute or list.
        /// </summary>
        public ChangeType ChangeType { get; }
        
        /// <summary>
        /// Unless it is a list update, this contains the value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// On list updates this has the new values
        /// </summary>
        public List<string> ListValues { get; }

        public MenuChangeCommand(int id, CorrelationId correlation, ChangeType type, string value) : base(CHANGE_CMD_ID)
        {
            MenuId = id;
            Correlation = correlation;
            ChangeType = type;
            Value = value;
            ListValues = null;
        }

        public MenuChangeCommand(int id, CorrelationId correlation, ChangeType type, List<string> values) : base(CHANGE_CMD_ID)
        {
            MenuId = id;
            Correlation = correlation;
            ChangeType = type;
            Value = null;
            ListValues = values;
        }
    }
}
