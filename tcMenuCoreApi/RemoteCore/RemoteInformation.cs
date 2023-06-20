using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Util;

namespace TcMenu.CoreSdk.RemoteCore
{
    /// <summary>
    /// Holds remote connectivity information about the remote connection
    /// </summary>
    public class RemoteInformation
    {
        public static RemoteInformation EMPTY_REMOTE_INFO = new RemoteInformation("Unknown", 0, ApiPlatform.ARDUINO, Guid.NewGuid().ToString(), 0);

        public string Name { get; }
        public ApiPlatform Platform { get; }
        public int Major { get; }
        public int Minor { get; }
        public string Uuid { get; }
        public int SerialNumber { get; }
        public LibraryVersion Version { get; }

        public RemoteInformation(string name, int version, ApiPlatform platform, string uuid, int serialNumber)
        {
            Major = version / 100;
            Minor = version % 100;
            Name = name;
            Platform = platform;
            Uuid = uuid;
            Version = new LibraryVersion(Major, Minor, 0, ReleaseType.STABLE);
            SerialNumber = serialNumber;
        }

        public override string ToString()
        {
            return $"{Name} V{Major}.{Minor} {Platform}";
        }
    }
}
