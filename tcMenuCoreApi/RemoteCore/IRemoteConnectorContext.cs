using System;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteStates;

namespace TcMenu.CoreSdk.RemoteCore
{
    /// <summary>
    /// In order to isolate the functions of the remote connector and the state we
    /// provide an interface that gives the state control of matters without the
    /// existing interfaces becoming blurred by this functionality.
    /// </summary>
    public interface IRemoteConnectorContext
    {
        /// <summary>
        /// the name of the connection in a printable form
        /// </summary>
        string ConnectionName { get; }

        DateTime LastConnectionAttempt { get; }

        /// <summary>
        /// Gets hold of the global clock instance.
        /// </summary>
        SystemClock Clock { get; }

        /// <summary>
        /// Sends a heartbeat command of a given mode
        /// </summary>
        /// <param name="mode">the mode to use</param>
        /// <param name="frequency">the heartbeat frequency</param>
        /// <returns>async boolean, true for success</returns>
        void SendHeartbeatCommand(int frequency, HeartbeatMode mode);

        /// <summary>
        /// Sends a join command to the remote
        /// </summary>
        /// <returns>async return boolean, true for success</returns>
        void SendJoinCommand();

        /// <summary>
        /// Indicates if the underlying device is connected. EG: Socket, Serial port.
        /// </summary>
        bool DeviceConnected { get; }

        /// <summary>
        /// Tells the connector to try and initiate a connection and let us know when
        /// the job is done.
        /// </summary>
        /// <returns>boolean: true for success, otherwise false</returns>
        bool PerformConnection();

        /// <summary>
        /// Tell the connector to go back to the starting condition due to a connection problem
        /// </summary>
        void BackToStart(string reason);

        /// <summary>
        /// Change into the new connection state, calling end state on the prior state
        /// and enter state on the new one. This also notifies the connection listeners
        /// of the new state.
        /// </summary>
        /// <param name="newState">the state specified as a required final status</param>
        /// <param name="force">only the core connection is allowed to use this flag</param>
        void ChangeState(AuthenticationStatus stateMapping, bool force = false);

        /// <summary>
        /// Set the remote party information associated with this connection
        /// </summary>
        /// <param name="ri">remote party information</param>
        void SetRemoteParty(RemoteInformation ri);

        /// <summary>
        /// Publish the command to the controller object so it can be processed.
        /// </summary>
        /// <param name="command">the command to process.</param>
        void PublishToController(MenuCommand command);

        /// <summary>
        /// Publish a pairing command to the remote
        /// </summary>
        void SendPairingCommand();

        /// <summary>
        /// Reads and processes a message from the underlying stream
        /// </summary>
        void ReadAndProcess();
    }
}
