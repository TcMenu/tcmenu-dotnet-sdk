using System.Globalization;
using System.Text.RegularExpressions;

namespace TcMenu.CoreSdk.Util
{
    public enum ReleaseType
    {
        STABLE, BETA, PREVIOUS, PATCH
    }
    /// <summary>
    /// Represents a version in semantic version format. It supports both equality and is newer than functionality.
    /// </summary>
    public class LibraryVersion
    {
        /// <summary>
        /// This represents no version.
        /// </summary>
        public static readonly LibraryVersion ERROR_VERSION = new LibraryVersion(-1, -1, -1);

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public ReleaseType RelType { get; }

        /// <summary>
        /// Construct given the major, minor and patch version
        /// </summary>
        /// <param name="major">major version</param>
        /// <param name="minor">minor version</param>
        /// <param name="patch">the patch version</param>
        public LibraryVersion(int major, int minor, int patch, ReleaseType relType = ReleaseType.STABLE)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            RelType = relType;
        }

        /// <summary>
        /// Construct from a string in sematic form (for example 1.2.3)
        /// The format is Major.Minor.Patch, patch is optional
        /// </summary>
        /// <param name="version">the string in sematic form (</param>
        public LibraryVersion(string version)
        {
            var matcher = Regex.Match(version, @"(\d*)\.(\d*)\.(\d*)-([A-Za-z]*)");
            if (matcher.Success)
            {
                Major = int.Parse(matcher.Groups[1].Value, CultureInfo.InvariantCulture);
                Minor = int.Parse(matcher.Groups[2].Value, CultureInfo.InvariantCulture);
                Patch = int.Parse(matcher.Groups[3].Value, CultureInfo.InvariantCulture);
                RelType = FromReleaseSpecifier(matcher.Groups[4].Value);
                return;
            }

            matcher = Regex.Match(version, @"(\d*)\.(\d*)\.(\d*)");
            if (matcher.Success)
            {
                Major = int.Parse(matcher.Groups[1].Value, CultureInfo.InvariantCulture);
                Minor = int.Parse(matcher.Groups[2].Value, CultureInfo.InvariantCulture);
                Patch = int.Parse(matcher.Groups[3].Value, CultureInfo.InvariantCulture);
                RelType = ReleaseType.STABLE;
                return;
            }

            matcher = Regex.Match(version, @"(\d*)\.(\d*)");
            if (matcher.Success)
            {
                Major = int.Parse(matcher.Groups[1].Value, CultureInfo.InvariantCulture);
                Minor = int.Parse(matcher.Groups[2].Value, CultureInfo.InvariantCulture);
                Patch = 0;
                RelType = ReleaseType.STABLE;
            }
        }

        public static ReleaseType FromReleaseSpecifier(string s)
        {
            switch (s.ToLowerInvariant())
            {
                case "patch": return ReleaseType.PATCH;
                case "snapshot":
                case "beta":
                case "rc": return ReleaseType.BETA;
                case "previous": return ReleaseType.PREVIOUS;
                default: return ReleaseType.STABLE;
            }
        }

        /// <summary>
        /// Gets the version from a library property file string
        /// </summary>
        /// <param name="fileContent">the contents of a library properties file</param>
        /// <returns>The version object or ERROR_VERSION</returns>
        public static LibraryVersion FromPropertyFile(string fileContent)
        {
            var matcher = Regex.Match(fileContent, @".*version\s*=\s(.*)");
            if (matcher.Success)
            {
                return new LibraryVersion(matcher.Groups[1].Value);
            }
            else return ERROR_VERSION;
        }

        /// <summary>
        /// Returns this version as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Major == -1) return "Unknown";
            if (RelType == ReleaseType.STABLE)
            {
                return $"{Major}.{Minor}.{Patch}";
            }
            else
            {
                return $"{Major}.{Minor}.{Patch}-{RelType}";
            }
        }

        /// <summary>
        /// Checks if the this library is the same or new than the passed in library
        /// </summary>
        /// <param name="other">the library to compare</param>
        /// <returns>true if newer or same than other</returns>
        public bool IsSameOrNewerThan(LibraryVersion other)
        {
            if (other == null) return false;

            if (Major > other.Major) return true;
            if (Major < other.Major) return false;

            if (Minor > other.Minor) return true;
            if (Minor < other.Minor) return false;

            return Patch >= other.Patch;
        }

        /// <summary>
        /// Check for exact equality between two versions
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is LibraryVersion version &&
                   Major == version.Major &&
                   Minor == version.Minor &&
                   Patch == version.Patch &&
                   RelType == version.RelType;
        }

        public override int GetHashCode()
        {
            var hashCode = -639545495;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + Patch.GetHashCode();
            hashCode = hashCode * -1521134295 + RelType.GetHashCode();
            return hashCode;
        }
    }

}
