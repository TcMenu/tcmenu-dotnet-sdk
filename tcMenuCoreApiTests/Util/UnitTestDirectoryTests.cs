using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcMenu.CoreSdk.Util;

namespace TcMenu.CoreSdkTests.Util
{
    [TestClass]
    public class UnitTestDirectoryTests
    {
        [TestMethod]
        public void TestCreatingDirectoryStructure()
        {
            var subDirs = new List<IDirectory>
            {
                new UnitTestDirectory("Root/examples", new List<IDirectory>(), new Dictionary<string, string>()),
            };
            var fileContents = new Dictionary<string, string>
            {
                ["example.emf"] = "example file contents",
                ["super.cpp"] = "cpp file content",
                ["super.h"] = "h file content"
            };
            var dirRoot = new UnitTestDirectory("Root", subDirs, fileContents);

            var examples = dirRoot.GetFolderWithName("examples").Result;
            Assert.AreEqual("examples", examples.LeafDirectoryName);
            Assert.AreEqual("Root/examples", examples.UnderlyingPath);

            Assert.AreEqual("Root", dirRoot.LeafDirectoryName);
            Assert.AreEqual("Root", dirRoot.UnderlyingPath);

            Assert.AreEqual("example.emf", dirRoot.EmfFileName().Result);
            Assert.IsTrue(dirRoot.HasFileWithName("super.cpp").Result);
            Assert.IsFalse(dirRoot.HasFileWithName("super.xyz").Result);
            Assert.AreEqual("cpp file content", dirRoot.GetSourceFileWithName("super.cpp").Result);

            CollectionAssert.AreEquivalent(new[] {"example.emf", "super.cpp", "super.h"},
                dirRoot.GetFileNames().Result);

            var dir = dirRoot.GetFolderWithName("another", true).Result;
            Assert.AreEqual("another", dir.LeafDirectoryName);
            Assert.AreEqual(Path.Combine("Root", "another"), dir.UnderlyingPath);

            dir.SaveSourceFileWithName("newFile.txt", "blah blah blah").Wait();

            var dirNames = dirRoot.GetDirectories().Result.Select(d => d.LeafDirectoryName).ToList();
            CollectionAssert.AreEquivalent(new[] {"examples", "another"}, dirNames);

            dirRoot.SaveSourceFileWithName("super.cpp", "super duper");
            Assert.AreEqual("super duper", dirRoot.GetSourceFileWithName("super.cpp").Result);

            dirRoot.SaveToFirstEmfFile("new emf data").Wait();
            Assert.AreEqual("new emf data", dirRoot.GetFirstEmfFile().Result);
        }
    }
}