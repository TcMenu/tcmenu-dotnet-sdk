using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.Commands
{
    /// <summary>
    /// A heartbeat message can be specialised to signify the start or end of a connection.
    /// </summary>
    public enum HeartbeatMode
    {
        /// <summary>
        /// This represents the normal heartbeat that occurs during a connection
        /// </summary>
        NORMAL,
        /// <summary>
        /// This is sent at the very start of a connection, to indicate the start of a new connection
        /// </summary>
        START,
        /// <summary>
        /// This is sent at the very end of a connection, causing the other side to close the connection.
        /// </summary>
        END
    }

    /// <summary>
    /// This enum describes the various types of acknowledgement from the server.
    /// </summary>
    public enum AckStatus
    {
        /// <summary>
        /// This is a warning that the value was out of range
        /// </summary>
        VALUE_RANGE_WARNING = -1,

        /// <summary>
        /// The operation was successful
        /// </summary>
        SUCCESS = 0,

        /// <summary>
        /// The requested ID was not found
        /// </summary>
        ID_NOT_FOUND = 1,

        /// <summary>
        /// The credentials provided were incorrect
        /// </summary>
        INVALID_CREDENTIALS = 2,

        /// <summary>
        /// There was an error that is not categorised. 
        /// </summary>
        UNKNOWN_ERROR = 10000
    }

    /// <summary>
    /// DialogMode represents the meaning of a dialog mesage between the client and server.
    /// </summary>
    public enum DialogMode
    {
        /// <summary>
        /// A dialog needs to be shown
        /// </summary>
        SHOW = 'S',

        /// <summary>
        /// A dialog needs to be hidden
        /// </summary>
        HIDE = 'H',

        /// <summary>
        /// An action on a dialog is to be performed.
        /// </summary>
        ACTION = 'A'
    }

    /// <summary>
    /// Represents the type of button in a dialog
    /// </summary>
    public enum MenuButtonType
    {
        NONE = 0,
        OK = 1,
        ACCEPT = 2,
        CANCEL = 3,
        CLOSE = 4
    }

    /// <summary>
    /// This enum is used during joining to indicate which platform is on the other side of the connection
    /// </summary>
    public enum ApiPlatform
    {
        ARDUINO = 0,
        ARDUINO32 = 2,
        JAVA_API = 1,
        DNET_API = 3
    }

    /// <summary>
    /// Indications of the values that can be in a bootstrap message
    /// </summary>
    public enum BootstrapType
    {
        START, END
    }

    /// <summary>
    /// The type of change that is being sent or received.
    /// </summary>
    public enum ChangeType
    {
        DELTA = 0, ABSOLUTE = 1, CHANGE_LIST = 2
    }
}
