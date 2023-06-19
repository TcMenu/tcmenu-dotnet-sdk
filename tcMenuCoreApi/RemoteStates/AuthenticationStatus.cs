using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.RemoteStates
{
    /// <summary>
    /// Represents the various connectivity states that the controller can be in
    /// </summary>
    public enum AuthenticationStatus
    {
        /// <summary>
        /// The connection has not been started, or stop has been called.
        /// </summary>
        NOT_STARTED,
        /// <summary>
        /// We are trying to connect with the remote
        /// </summary>
        AWAITING_CONNECTION,
        /// <summary>
        /// We have established a device connection, but need a protocol level one
        /// </summary>
        ESTABLISHED_CONNECTION,
        /// <summary>
        /// The join has been sent, and we are waiting for authentication
        /// </summary>
        SEND_JOIN,
        /// <summary>
        /// The authentication has failed, after this the connection will drop
        /// </summary>
        FAILED_AUTH,
        /// <summary>
        /// The authentication has completed, in full connection mode we proceed to bootstrap.
        /// </summary>
        AUTHENTICATED,
        /// <summary>
        /// The connection is now bootstrapping with the remote.
        /// </summary>
        BOOTSTRAPPING,
        /// <summary>
        /// The connection is fully ready for end use.
        /// </summary>
        CONNECTION_READY
    }
}
