using System;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenu.CoreSdk.RemoteCore
{
    public delegate void ConnectionChangedHandler(AuthenticationStatus connected);
    public delegate void MessageReceivedHandler(MenuCommand command);

    public class LocalIdentification
    {
        public Guid Uuid { get; }
        public string Name { get; }

        public LocalIdentification(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }

    public interface IRemoteConnector
    {
        /// <summary>
        /// Register interest in connection change events, sent when we either gain
        /// or lose connection with the remote.
        /// </summary>
        event ConnectionChangedHandler ConnectionChanged;

        /// <summary>
        /// Register interest in message being received, each message received will be
        /// sent as a raw command to listeners.
        /// </summary>
        event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// Get the connection name associated with the connection
        /// </summary>
        string ConnectionName { get; }

        /// <summary>
        /// Check the connectivity state.
        /// </summary>
        AuthenticationStatus AuthStatus { get; }

        /// <summary>
        /// Get the name and other details of the current connected device.
        /// </summary>
        RemoteInformation RemoteInfo { get; }

        /// <summary>
        /// start a remote connector so it attempt to connect with a remote.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop a remote connector so that it stops attempting to connect
        /// </summary>
        void Stop(bool waitForThread = false);

        /// <summary>
        /// Send the command to the remote if the socket is presently connected.
        /// </summary>
        /// <param name="command">the command to send</param>
        void SendMenuCommand(MenuCommand command);

        /// <summary>
        /// Force close a connection when it's known to be bad. The connector will probably
        /// still try and allocate another connection
        /// </summary>
        void Close();
    }
}
