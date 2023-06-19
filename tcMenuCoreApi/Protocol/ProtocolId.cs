using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.Protocol
{
    /// <summary>
    /// An enumeration of all the supported protocols by the API.
    /// </summary>
    public enum ProtocolId
    {
        /// <summary>
        /// The way to specify that a protocol is undefined.
        /// </summary>
        NO_PROTOCOL = 0,

        /// <summary>
        /// This represents the tag value protocol without encryption.
        /// </summary>
        TAG_VAL_PROTOCOL = 1
    }
}
