using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdkTests.Serialisation
{
    [TestClass]
    public class CurrentScrollPositionTest
    {
        [TestMethod]
        public void TestCurrentScrollPosition()
        {
            CurrentScrollPosition pos1 = new CurrentScrollPosition(10, "ABC");
            CurrentScrollPosition pos2 = new CurrentScrollPosition("20-Another Super-Duper");
            CurrentScrollPosition pos3 = new CurrentScrollPosition("ABC"); // not valid but shouldnt fail
            CurrentScrollPosition pos4 = new CurrentScrollPosition("a-ABC"); // not valid but shouldnt fail
            CurrentScrollPosition pos5 = new CurrentScrollPosition("252-");
            
            Assert.AreEqual(pos1.Position, 10);
            Assert.AreEqual(pos1.Value, "ABC");

            Assert.AreEqual(pos2.Position, 20);
            Assert.AreEqual(pos2.Value, "Another Super-Duper");

            Assert.AreEqual(pos5.Position, 252);
            Assert.AreEqual(pos5.Value, "");
            
            Assert.AreEqual(pos2, new CurrentScrollPosition("20-Another Super-Duper"));
            Assert.AreNotEqual(pos2, new CurrentScrollPosition("10-Another Super-Duper"));
            Assert.AreNotEqual(pos2, new CurrentScrollPosition("20-Another Duper"));
            
            Assert.AreEqual("10-ABC", pos1.ToString());
            Assert.AreEqual("20-Another Super-Duper", pos2.ToString());
            Assert.AreEqual("0-Unknown", pos3.ToString());
            Assert.AreEqual("0-ABC", pos4.ToString());
            Assert.AreEqual("252-", pos5.ToString());
        }
    }
}