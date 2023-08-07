using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using IDataReader = Windows.Storage.Streams.IDataReader;

namespace embedControl.Windows.Serial
{
    public abstract class WindowsSendAndRecvBase : RemoteConnectorBase
    {
        private DataReader _dataReader = null;
        private DataWriter _dataWriter = null;


        protected WindowsSendAndRecvBase(LocalIdentification localId, IProtocolCommandConverter converter, ProtocolId protocol, SystemClock clock) 
            : base(localId, converter, protocol, clock)
        {
        }

        public void CreateBuffers(IInputStream input, IOutputStream output)
        {
            lock (LockObject)
            {

                _dataWriter = new DataWriter(output);
                _dataReader = new DataReader(input);
            }
        }

        public void DestroyBuffers()
        {
            lock (LockObject)
            {
                _dataReader?.Dispose();
                _dataWriter?.Dispose();
                _dataWriter = null;
                _dataReader = null;
            }
        }

        public override int InternalSendData(byte[] data, int offset, int len)
        {
            IDataWriter writer;
            lock (LockObject)
            {
                writer = _dataWriter;
            }

            if (writer == null) throw new IOException("Serial port not open");
            byte[] _localData = new byte[len];
            Array.Copy(data, offset, _localData, 0, len);
            writer.WriteBytes(_localData);

            var continuation = writer.StoreAsync().AsTask();
            continuation.Wait();
            return (int)continuation.Result;
        }

        public override int ReadFromDevice(byte[] data, int offset)
        {
            IDataReader reader;
            lock (LockObject)
            {
                reader = _dataReader;
            }
            if (reader == null) throw new IOException("Serial port not open");

            reader.InputStreamOptions = InputStreamOptions.Partial;
            uint sizeToRead = Math.Min((uint)(data.Length - offset), reader.UnconsumedBufferLength);
            var continuation = reader.LoadAsync(sizeToRead != 0 ? sizeToRead : 1).AsTask();
            continuation.Wait();
            uint amtLoaded = continuation.Result;

            byte[] buf = new byte[amtLoaded];
            reader.ReadBytes(buf);
            Array.Copy(buf, 0, data, offset, amtLoaded);

            return (int)amtLoaded;
        }


        public abstract object LockObject { get; }
    }
}
