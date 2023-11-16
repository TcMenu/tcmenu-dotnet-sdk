using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace TcMenu.CoreSdk.BaseSerial
{
    /// <summary>
    /// Represents the types of serial ports that can exist. Not all platforms will support all types.
    /// </summary>
    [Flags]
    public enum SerialPortType
    {
        /// <summary>
        /// A named serial port that can be acquired from the OS. EG COM1, /dev/usb1234
        /// </summary>
        NAMED_SERIAL = 1,

        /// <summary>
        /// A bluetooth connection, for devices that identify bluetooth ports differently, some bluetooth ports
        /// may show up as named serial ports.
        /// </summary>
        BLUETOOTH = 2,

        /// <summary>
        /// A low power bluetooth connection, again for devices that identify such devices differently, sometimes these
        /// could show up as named ports.
        /// </summary>
        BLE_BLUETOOTH = 4,

        /// <summary>
        /// A shortcut for all above ports.
        /// </summary>
        ALL = NAMED_SERIAL | BLUETOOTH | BLE_BLUETOOTH
    }

    /// <summary>
    /// Represents a serial port that can be connected to on a given device.
    /// </summary>
    public class SerialPortInformation
    {
        public static readonly int[] ALL_BAUD_RATES = new int[] { 110, 150, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };

        /// <summary>This represents the empty serial port, can be used where no other port is yet configured.</summary>
        public static readonly SerialPortInformation EMPTY = new SerialPortInformation("", SerialPortType.NAMED_SERIAL, "");

        /// <summary>
        /// The name of the serial port, can be a user friendly name. Not used to identify the port
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The Type of serial port, EG bluetooth or named serial port.
        /// </summary>
        public SerialPortType PortType { get; }

        /// <summary>
        /// The identifier of the port. This will be used to locate the port. EG COM1, /dev/usb0102
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The signal strength of the last transmission, if appropriate, or NaN
        /// </summary>
        public double LatestRadioStrength { get; }

        /// <summary>
        /// Construct a serial port info giving all the parameters
        /// </summary>
        /// <param name="name">name of the port</param>
        /// <param name="portType">type of the port</param>
        /// <param name="id">identifier of the port</param>
        public SerialPortInformation(string name, SerialPortType portType, string id, double rssi = Double.NaN)
        {
            Name = name;
            PortType = portType;
            Id = id;
            LatestRadioStrength = rssi;
        }

        public override string ToString()
        {
            var radioLevel = "";
            if (!Double.IsNaN(LatestRadioStrength)) radioLevel = " " + LatestRadioStrength.ToString("#.##dB"); 
            return $"{Name} - {NicePortType(PortType)}{radioLevel}";
        }

        private sealed class NamePortTypeIdEqualityComparer : IEqualityComparer<SerialPortInformation>
        {
            public bool Equals(SerialPortInformation x, SerialPortInformation y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name == y.Name && x.PortType == y.PortType && x.Id == y.Id;
            }

            public int GetHashCode(SerialPortInformation obj)
            {
                unchecked
                {
                    var hashCode = (obj.Name != null ? obj.Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int) obj.PortType;
                    hashCode = (hashCode * 397) ^ (obj.Id != null ? obj.Id.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<SerialPortInformation> NamePortTypeIdComparer { get; } = new NamePortTypeIdEqualityComparer();

        private object NicePortType(SerialPortType portType)
        {
            switch (portType)
            {
                case SerialPortType.BLUETOOTH: return " Bluetooth";
                case SerialPortType.BLE_BLUETOOTH: return " Bluetooth LE";
                case SerialPortType.NAMED_SERIAL:
                default:
                    return "Serial Port";
            }
        }

        public static SerialPortInformation FromWire(string text)
        {
            var parts = text.Split('|');
            if(parts.Length != 3) return EMPTY;

            if (!SerialPortType.TryParse(parts[0], out SerialPortType ty)) return EMPTY;

            var portName = parts[1];
            var portId = parts[2];
            return new SerialPortInformation(portName, ty, portId);
        }

        public string ToWire()
        {
            return $"{PortType}|{Name}|{Id}";
        }
    }
}
