using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace TcMenu.CoreSdk.Util
{

    /// <summary>
    /// An implementation of IDirectory for easy use in unit tests, stores everything in memory so therefore, creates no files.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class UnitTestDirectory : IDirectory
    {
        private readonly string _pathName;

        public List<IDirectory> Directories { get; }
        
        public Dictionary<string, string> FileToContent { get; }
        
        public bool SecurityScope { get; internal set; }

        public UnitTestDirectory(string pathName, List<IDirectory> directories, Dictionary<string, string> fileToContent)
        {
            _pathName = pathName;
            Directories = directories;
            FileToContent = fileToContent;
        }

        public UnitTestDirectory(string path)
        {
            _pathName = path;
            Directories = new List<IDirectory>();
            FileToContent = new Dictionary<string, string>();
        }

        public string UnderlyingPath => _pathName;

        public string LeafDirectoryName
        {
            get
            {
                if (_pathName.IndexOf('/') != -1)
                {
                    return _pathName.Substring(_pathName.LastIndexOf('/') + 1);
                }
                else if (_pathName.IndexOf('\\') != -1)
                {
                    return _pathName.Substring(_pathName.LastIndexOf('\\') + 1);
                }
                else return _pathName;
            }
        }

            public Task<string> EmfFileName()
            {
                string emf = FileToContent.Where(ent => ent.Key.EndsWith(".emf", StringComparison.InvariantCultureIgnoreCase))
                    .Select(ent => ent.Key)
                    .FirstOrDefault();

                return Task.FromResult(emf);
            }

            public Task<List<IDirectory>> GetDirectories()
            {
                return Task.FromResult(Directories);
            }

            public Task<List<string>> GetFileNames()
            {
                return Task.FromResult(FileToContent.Select(ent => ent.Key).ToList());
            }

            public async Task<string> GetFirstEmfFile()
            {
                var emfFile = await EmfFileName().ConfigureAwait(true);
                if (emfFile != null)
                {
                    return FileToContent[emfFile];
                }
                return null;
            }

            public Task<IDirectory> GetFolderWithName(string folderName, bool createIfNeeded = false)
            {
                foreach (var dir in Directories)
                {
                    if (dir.LeafDirectoryName == folderName)
                    {
                        return Task.FromResult(dir);
                    }
                }

                if (createIfNeeded)
                {
                    var newDir = new UnitTestDirectory(Path.Combine(_pathName, folderName), new List<IDirectory>(), new Dictionary<string, string>());
                    Directories.Add(newDir);
                    return Task.FromResult(newDir as IDirectory);
                }
                else throw new DirectoryAccessException($"could not find {folderName}");
            }

            public Task<string> GetSourceFileWithName(string name)
            {
                return Task.FromResult(FileToContent[name]);
            }

            public Task<bool> HasFileWithName(string name)
            {
                return Task.FromResult(FileToContent.ContainsKey(name));
            }

            public Task SaveSourceFileWithName(string name, string newContent)
            {
                if (FileToContent.ContainsKey(name))
                {
                    FileToContent[name] = newContent;
                }
                else
                {
                    FileToContent.Add(name, newContent);
                }
                return Task.CompletedTask;
            }

            public async Task SaveToFirstEmfFile(string newData)
            {
                var emfName = await EmfFileName();
                if (emfName == null)
                {
                    emfName = LeafDirectoryName + ".emf";
                }
                await SaveSourceFileWithName(emfName, newData);
            }

            public Task DeleteFile(string fileName)
            {
                FileToContent.Remove(fileName);
                return Task.CompletedTask;
            }

        public Task UnzipContentsToHere(Stream zipStream)
        {
            using(var archive = new ZipArchive(zipStream))
            {
                archive.ExtractToDirectory(_pathName);
            }
            return Task.CompletedTask;
        }

        public void StartSecurityScope()
        {
            SecurityScope = true;
        }

        public void EndSecurityScope()
        {
            SecurityScope = false;
        }

        public bool WriteImmediately(string fileName, string content)
        {
            FileToContent[fileName] = content;
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is UnitTestDirectory directory &&
                   _pathName == directory._pathName;
        }

        public override int GetHashCode()
        {
            return _pathName.GetHashCode();
        }
    }
}
