using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdkTests.ProtocolTests
{
    [TestClass]
    public class TagValTextParserTests
    {
        [TestMethod]
        public void TestReadingValidTokens()
        {
            var msgBytes = Encoding.ASCII.GetBytes($"ID=4|DC=2937849\\=|ES=\\|404|\U00000002");
            TagValTextParser textParser = new TagValTextParser(new MemoryStream(msgBytes, false));
            Assert.AreEqual("4", textParser.GetValueForKey(MenuCommand.MakeCmdPair('I', 'D')));
            Assert.AreEqual("2937849=", textParser.GetValueForKey(MenuCommand.MakeCmdPair('D', 'C')));
            Assert.AreEqual("|404", textParser.GetValueForKeyWithDefault(MenuCommand.MakeCmdPair('E', 'S'), "def"));
            Assert.AreEqual("default", textParser.GetValueForKeyWithDefault(MenuCommand.MakeCmdPair('A', 'A'), "default"));
        }
        
        [TestMethod]
        public void TestReadingIntegerValues()
        {
            var msgBytes = Encoding.ASCII.GetBytes($"ID=4|DC=293784|\u0002");
            TagValTextParser textParser = new TagValTextParser(new MemoryStream(msgBytes, false));
            Assert.AreEqual(4, textParser.GetValueForKeyAsInt(MenuCommand.MakeCmdPair('I', 'D')));
            Assert.AreEqual(4, textParser.GetValueForKeyAsIntWithDefault(MenuCommand.MakeCmdPair('I', 'D'), 88));
            Assert.AreEqual(10, textParser.GetValueForKeyAsIntWithDefault(MenuCommand.MakeCmdPair('D', 'X'), 10));
        }

        [TestMethod]
        public void TestGetAllKeys()
        {
            var msgBytes = Encoding.ASCII.GetBytes($"ID=4|QA=item1|QB=item2|\U00000002");
            var textParser = new TagValTextParser(new MemoryStream(msgBytes, false));
            var list = textParser.GetAllKeysAsString('Q');
            CollectionAssert.AreEquivalent(new List<string>() { "item1", "item2" }, list);
        }

        [TestMethod]
        public void TestGetAllKeysWith2Prefix()
        {
            var msgBytes = Encoding.ASCII.GetBytes($"ID=4|QA=item1|QB=item2|QC=item3|qA=val1|qB=val2|\U00000002");
            var textParser = new TagValTextParser(new MemoryStream(msgBytes, false));
            var list = textParser.GetAllKeysAsString('Q', 'q');
            CollectionAssert.AreEquivalent(new List<string>() { "item1\tval1", "item2\tval2", "item3\t" }, list);
        }

        [TestMethod]
        public void TestReadingMissingMsgEnding()
        {
            var msgBytes = Encoding.ASCII.GetBytes($"DC=2937849|ES=\\|404|BD=20\u0002");
            TagValTextParser textParser = new TagValTextParser(new MemoryStream(msgBytes, false));
            Assert.AreEqual("2937849", textParser.GetValueForKey(MenuCommand.MakeCmdPair('D', 'C')));
            Assert.AreEqual("|404", textParser.GetValueForKey(MenuCommand.MakeCmdPair('E', 'S')));
        }

        [TestMethod]
        [ExpectedException(typeof(TcProtocolException))]
        public void TestReadingMissingMalforedKey()
        {
            var msgBytes = Encoding.ASCII.GetBytes($"MTRA=NJ|\u00000002");
            TagValTextParser textParser = new TagValTextParser(new MemoryStream(msgBytes, false));
        }
    }
}
