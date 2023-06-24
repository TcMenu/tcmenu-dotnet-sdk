using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcMenuCoreMaui.BaseSerial;

namespace TcMenuCoreMaui
{
    public class PlatformImplementationCreator
    {
        public ISerialPortFactory CreateSerialPort()
        {
            return null; // no impl on MacOS  yet.
        }
    }
}