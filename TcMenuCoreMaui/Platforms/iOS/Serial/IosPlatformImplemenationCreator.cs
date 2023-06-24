using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcMenuCoreMaui.BaseSerial;
using TcMenuCoreMaui.iOS.SerialImpl;

namespace TcMenuCoreMaui
{
    public class PlatformImplementationCreator
    {
        public ISerialPortFactory CreateSerialPort()
        {
            return new AppleBluetoothSerialFactory();
        }
    }
}