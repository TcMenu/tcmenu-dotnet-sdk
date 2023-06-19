using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TcMenu.CoreSdk.Commands;

namespace TcMenu.CoreSdk.Protocol
{
    /// <summary>
    /// An exception that indicates that there has been a protocol error during communication.
    /// Usually thrown during either the processing of a command to a message or alternatively
    /// during message to command processing.
    /// </summary>
    public class TcProtocolException : Exception
    {
        public TcProtocolException(string why) : base(why)
        {
        }

        public TcProtocolException(string why, Exception cause) : base(why, cause)
        {
        }
    }

    /// <summary>
    /// This interface is able to convert between protocol messages and command objects. An instance of this interface
    /// </summary>
    public interface IProtocolCommandConverter
    {
        MenuCommand ConvertMessageToCommand(Stream inStream);
        void ConvertCommandToMessage(MenuCommand cmd, ProtocolId protocol, Stream outStream);
        int PositionOfEOMInBuffer(byte[] data, int len, ProtocolId protocol);
        string LogDataToCommandLine(ProtocolId protocol, byte[] readBuffer, int offset, int len);
    }

    public class DefaultProtocolCommandConverter : IProtocolCommandConverter
    {
        private Serilog.ILogger logger = Log.Logger.ForContext<DefaultProtocolCommandConverter>();

        public const byte START_OF_MESSAGE = 0x01;
        public const byte END_OF_MESSAGE = 0x02;

        private IDictionary<(ushort msgType, ProtocolId protocol), Func<Stream, MenuCommand>> MsgToCommand = new ConcurrentDictionary<(ushort msgType, ProtocolId protocol), Func<Stream, MenuCommand>>();
        private IDictionary<(ushort msgType, ProtocolId protocol), Action<MenuCommand, Stream>> CommandToMsg = new ConcurrentDictionary<(ushort msgType, ProtocolId protocol), Action<MenuCommand, Stream>>();

        public void RegisterToMessageConverter(ushort msgType, ProtocolId protocol, Action<MenuCommand, Stream> convertFn)
        {
            logger.Debug($"Registered cmd to msg for {msgType} on protocol {protocol}");
            CommandToMsg.Add((msgType, protocol), convertFn);
        }

        public void RegisterFromMsgToCmdConverter(ushort msgType, ProtocolId protocol, Func<Stream, MenuCommand> convertFn)
        {
            logger.Debug($"Registered msg to cmd for {msgType} on protocol {protocol}");
            MsgToCommand.Add((msgType, protocol), convertFn);
        }

        public void ConvertCommandToMessage(MenuCommand cmd, ProtocolId protocol, Stream outStream)
        {
            var key = (cmd.CommandType, protocol);
            if (!CommandToMsg.ContainsKey(key)) throw new TcProtocolException($"MsgType/Protocol {key} not found");

            logger.Information($"Converting command message {cmd} to stream");
            outStream.WriteByte(START_OF_MESSAGE);
            outStream.WriteByte((byte) protocol);
            outStream.WriteByte((byte)(cmd.CommandType >> 8));
            outStream.WriteByte((byte)(cmd.CommandType & 0xff));
            CommandToMsg[key].Invoke(cmd, outStream);
            outStream.WriteByte(END_OF_MESSAGE);
        }

        public MenuCommand ConvertMessageToCommand(Stream inStream)
        {
            // find the start of the message..
            int i = 0;
            byte by = inStream.ReadByteException();
            while (i < 1000 && by != START_OF_MESSAGE) {
                by = inStream.ReadByteException();
                i++;
            }
            if (by != START_OF_MESSAGE) throw new TcProtocolException("No start of message found within 1000 chars");

            // get the protocol and make sure it's valid
            ProtocolId protocol = (ProtocolId)inStream.ReadByteException();
            if (!Enum.IsDefined(typeof(ProtocolId), protocol)) throw new TcProtocolException($"Unexpected protocol {protocol}");

            // get the message type
            var cmdType = MenuCommand.MakeCmdPair((char)(inStream.ReadByteException()), (char)inStream.ReadByteException());

            // find in the converters and execute.
            var key = (cmdType, protocol);
            if (!MsgToCommand.ContainsKey(key)) throw new TcProtocolException($"Unexpected MsgType/Protocol {key}");

            var cmd = MsgToCommand[key].Invoke(inStream);
            logger.Information($"Converting incoming message  {(char)(cmdType >> 8)}{(char)(cmdType&0xff)} protocol {protocol} to {cmd}");
            return cmd;
        }

        public int PositionOfEOMInBuffer(byte[] data, int len, ProtocolId protocol)
        {
            if (protocol == ProtocolId.TAG_VAL_PROTOCOL)
            {
                bool foundStart = false;
                for(int i=0; i< len; i++)
                {
                    byte b = data[i];
                    if (!foundStart && b == 0x01) foundStart = true;
                    if (foundStart && b == 0x02)
                    {
                        return i;
                    }
                }
                return -1;
            }
            else throw new IOException($"Unexpected protocol {protocol}");
        }

        public string LogDataToCommandLine(ProtocolId protocol, byte[] readBuffer, int offset, int len)
        {
            if (protocol == ProtocolId.TAG_VAL_PROTOCOL)
            {
                var sb = new StringBuilder(255);
                for(int i = offset; i < (offset + len); i++)
                {
                    if (readBuffer[i] < 32)
                    {
                        sb.AppendFormat("<0x{0:X2}>", readBuffer[i]);
                    }
                    else
                    {
                        sb.Append((char)readBuffer[i]);
                    }
                }
                return sb.ToString();
            }
            else
                return "Unknown protocol, cannot log"; 
                   
        }
    }

    public static class StreamHelper
    {
        public static byte ReadByteException(this Stream stream)
        {
            int r = stream.ReadByte();
            if (r == -1) throw new TcProtocolException("Stream returned -1");
            return (byte)r;
        }
    }
}
