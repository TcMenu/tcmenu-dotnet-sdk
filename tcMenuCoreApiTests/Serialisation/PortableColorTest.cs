using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdkTests.Serialisation
{
    [TestClass]
    public class PortableColorTest
    {
        [TestMethod]
        public void TestPortableColor()
        {
            PortableColor colorAlpha = new PortableColor(128, 255, 127, 32);
            PortableColor colorNoAlpha = new PortableColor(100, 200, 50);
            PortableColor colorRgb4 = new PortableColor("#f3b");
            PortableColor colorRgb7 = new PortableColor("#23f5D3");
            PortableColor colorRgb9 = new PortableColor("#365498aa");

            Assert.AreEqual(colorAlpha, new PortableColor(128, 255, 127, 32));
            Assert.AreNotEqual(colorAlpha, new PortableColor(128, 255, 127, 2));
            Assert.AreNotEqual(colorAlpha, new PortableColor(128, 255, 12, 32));
            Assert.AreNotEqual(colorAlpha, new PortableColor(128, 25, 12, 32));
            Assert.AreNotEqual(colorAlpha, new PortableColor(18, 255, 12, 32));
            
            AssertColor(colorAlpha, 128, 255, 127, 32, "#80FF7F20");
            AssertColor(colorNoAlpha, 100, 200, 50, 255, "#64C832FF");
            AssertColor(colorRgb4, 0xf0, 0x30, 0xb0, 255, "#F030B0FF");
            AssertColor(colorRgb7, 0x23, 0xF5, 0xD3, 255, "#23F5D3FF");
            AssertColor(colorRgb9, 0x36, 0x54, 0x98, 0xaa, "#365498AA");
        }

        private void AssertColor(PortableColor colorToTest, int red, int green, int blue, int alpha, string toStr)
        {
            Assert.AreEqual(red, colorToTest.red);
            Assert.AreEqual(green, colorToTest.green);
            Assert.AreEqual(blue, colorToTest.blue);
            Assert.AreEqual(alpha, colorToTest.alpha);
            Assert.AreEqual(toStr, colorToTest.ToString());
        }
    }
}