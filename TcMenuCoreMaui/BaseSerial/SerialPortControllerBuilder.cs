using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenuCoreMaui.BaseSerial;

namespace TcMenuCoreMaui.BaseSerial
{
    public enum SerialPortDelegateMode
    {
        AddOrUpdate,
        Remove
    };

    /// <summary>
    /// When registering with serial port factories the serial information is provided back using this callback. You should compare using the ID on the info to ensure you're
    /// not creating duplicates if that is important.
    /// </summary>
    /// <param name="info">the information about a discovered port</param>
    /// <param name="mode">the mode in which it was found (updated/new) or deleted</param>
    public delegate void SerialPortDelegate(SerialPortInformation info,  SerialPortDelegateMode mode);

    /// <summary>
    /// This factory is used by the below builder to generate serial connector classes, and also for getting a list
    /// of available ports.
    /// </summary>
    public interface ISerialPortFactory
    {
        /// <summary>
        /// Returns a task result that will provide a list of serial ports available on the device. You can await the task
        /// to get the results. The results can be either for all ports SerialPortType.ALL or a specific type.
        /// </summary>
        /// <param name="type">The type of ports to acquire or ALL</param>
        /// <returns>true if port scanning has started, otherwise false</returns>
        bool StartScanningPorts(SerialPortType type, SerialPortDelegate portDelegate);

        /// <summary>
        /// Stops any port scanning that is associated with the above StartScanningPorts method, if any scan is still running.
        /// Safe to call even when a scan is not running.
        /// </summary>
        void StopScanningPorts();

        /// <summary>
        /// Creates an IRemoteConnector that can be used with the serial port builder.
        /// </summary>
        /// <param name="info">an instance of serial port info, usually acquired from GetAllSerialPorts</param>
        /// <param name="baud">the baud rate at which to connect</param>
        /// <param name="converter">the protocol converter to use</param>
        /// <param name="protocol">the protocol to use for transmission</param>
        /// <param name="clock">the system clock</param>
        /// <param name="pairing">indicates if the connection is for pairing or usual activity</param>
        /// <returns></returns>
        IRemoteConnector CreateSerialConnector(LocalIdentification localId, SerialPortInformation info, int baud, 
                                               IProtocolCommandConverter converter, ProtocolId protocol, SystemClock clock, bool pairing);
    }

    /// <summary>
    /// Constructs a remote controller instance that is backed by a serial port. The port will be created using the
    /// current serial factory for the platform.
    /// </summary>
    public class SerialPortControllerBuilder : RemoteControllerBuilderBase<SerialPortControllerBuilder>
    {
        private readonly ISerialPortFactory _serialPortFactory;
        private SerialPortInformation _serialInfo;
        private int _baudRate;

        public SerialPortControllerBuilder(ISerialPortFactory serialPortFactory)
        {
            _serialPortFactory = serialPortFactory;
        }

        public SerialPortControllerBuilder WithSerialPortAndBaud(SerialPortInformation info, int baud)
        {
            _serialInfo = info;
            _baudRate = baud;
            return this;
        }

        public override IRemoteConnector BuildConnector(bool pairing)
        {
            var localId = new LocalIdentification(_localGuid, _localName);
            return _serialPortFactory.CreateSerialConnector(localId,  _serialInfo, _baudRate, GetDefaultConverters(), _protocol, _clock, pairing);
        }

        public override SerialPortControllerBuilder GetThis()
        {
            return this;
        }
    }
}
