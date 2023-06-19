using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TcMenu.CoreSdk.Util
{
    /// <summary>
    /// Represents an error that occurs during a directory operation using IDirectory and IStorage methods.
    /// </summary>
    public class DirectoryAccessException : IOException
    {
        public DirectoryAccessException(string message) : base(message)
        {
        }

        public DirectoryAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// File operations work differently on each platform. This class allows directories to be treated
    /// in a fairly device independent manner, where they provide operations to load, save, delete and
    /// copy, inline with what's needed in our subsystem.
    /// </summary>
    public interface IDirectory
    {
        /// <summary>
        /// Finds the first EMF file that's located within the structure as text. This is a convenience
        /// method that calls EMfFileName then gets the source file.
        /// </summary>
        /// <returns>the contents of the EMF file as text</returns>
        Task<string> GetFirstEmfFile();

        /// <summary>
        /// Gets the name of the first EMF file within the project directory
        /// </summary>
        /// <returns>name of the emf source file.</returns>
        Task<string> EmfFileName();

        /// <summary>
        /// Saves the newData string into the first EMF file within the project.
        /// </summary>
        /// <param name="newData"></param>
        /// <returns>A task that can be used to track it's success</returns>
        Task SaveToFirstEmfFile(string newData);

        /// <summary>
        /// Gets a soruce file with the given name, or throws an exception if not available.
        /// </summary>
        /// <param name="name">name of the file</param>
        /// <returns>the contents of the file as text</returns>
        Task<string> GetSourceFileWithName(string name);

        /// <summary>
        /// Saves a source file of the given name with the newContent provided.
        /// </summary>
        /// <param name="name">name of the file</param>
        /// <param name="newContent">the new content</param>
        /// <returns>a task for tracking.</returns>
        Task SaveSourceFileWithName(string name, string newContent);

        /// <summary>
        /// Gets all directories within this directory, as a list of IDirectory objects.
        /// </summary>
        /// <returns>a list of directories</returns>
        Task<List<IDirectory>> GetDirectories();

        /// <summary>
        /// Gets a list of all files names within the directory. These can be opened later
        /// with GetSourceFileWithName
        /// </summary>
        /// <returns>a list of all file names</returns>
        Task<List<string>> GetFileNames();

        /// <summary>
        /// checks if a file of the given name exists.
        /// </summary>
        /// <param name="name">the file to check</param>
        /// <returns>true if it exists, otherwise false</returns>
        Task<bool> HasFileWithName(string name);

        /// <summary>
        /// Gets a folder with a given name if it exists.
        /// </summary>
        /// <param name="folderName">the name of the folder</param>
        /// <returns>the folder as an IDirectory</returns>
        Task<IDirectory> GetFolderWithName(string folderName, bool createIfNeeded = false);

        /// <summary>
        /// Gets the underlying path as a string if it is available on that platform.
        /// </summary>
        string UnderlyingPath { get; }

        /// <summary>
        /// Gets the name of the leaf directory only
        /// </summary>
        string LeafDirectoryName { get; }

        /// <summary>
        /// Deletes the file requested from the file system
        /// </summary>
        /// <param name="fileName">file to delete</param>
        /// <returns>a task that can be used await completion</returns>
        Task DeleteFile(string fileName);

        /// <summary>
        /// Unzip a zip file recusively creating directories into this folder.
        /// </summary>
        /// <param name="zipStream">the stream of the zip file</param>
        /// <returns>A task to await completion</returns>
        Task UnzipContentsToHere(Stream zipStream);

        /// <summary>
        /// Starts a new security scope block that must be ended using EndSecurityScope
        /// </summary>
        void StartSecurityScope();

        /// <summary>
        /// Ends the above security scope, releasing any resources.
        /// </summary>
        void EndSecurityScope();

        /// <summary>
        /// Writes the contents string to fileName immediately waiting for any tasks to finish
        /// before returning. This method completely resets the contents of the file to content.
        /// 
        /// NOTE: This is a convenience for cases where the full range of errors are not needed,
        /// it will return true or false. The file MUST be in the trusted home directory structure.
        /// Provide only the file name to this method within the directory.
        /// </summary>
        /// <param name="fileName">the file name to write to</param>
        /// <param name="content">the new contents of the file</param>
        /// <returns></returns>
        bool WriteImmediately(string fileName, string content);
    }
}
