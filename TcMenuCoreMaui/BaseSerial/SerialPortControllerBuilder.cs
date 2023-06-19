using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenuCoreMaui.BaseSerial;

namespace TcMenuCoreMaui.BaseSerial
{
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
        bool StartScanningPorts(SerialPortType type, Action<SerialPortInformation> portDelegate);

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
