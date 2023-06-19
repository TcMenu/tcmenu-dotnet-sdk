using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Serialisation;
using TcMenu.CoreSdkTests.MenuTests;

namespace TcMenu.CoreSdkTests.Serialisation
{
    [TestClass]
    public class MenuItemFormatterTests
    {
        [TestMethod]
        public void TestFormattingAnalogHalves()
        {
            var volDbItem = new AnalogMenuItemBuilder()
                .WithName("Analog Item")
                .WithId(5)
                .WithOffset(-180)
                .WithDivisor(2)
                .WithUnitName("dB")
                .WithMaxValue(255)
                .Build();

            Assert.AreEqual("37.5dB", MenuItemFormatter.FormatForDisplay(volDbItem, 255));
            Assert.AreEqual("10.5dB", MenuItemFormatter.FormatForDisplay(volDbItem, 201));
            Assert.AreEqual("-90.0dB", MenuItemFormatter.FormatForDisplay(volDbItem, 0));

            Assert.AreEqual("255", MenuItemFormatter.FormatToWire(volDbItem, "37.5dB"));
            Assert.AreEqual("0", MenuItemFormatter.FormatToWire(volDbItem, "-90"));
            Assert.AreEqual("201", MenuItemFormatter.FormatToWire(volDbItem, "10.5dB"));

            Assert.ThrowsException<MenuItemFormatException>(()=>MenuItemFormatter.FormatToWire(volDbItem, "38.0dB"));
            Assert.ThrowsException<MenuItemFormatException>(()=>MenuItemFormatter.FormatToWire(volDbItem, "-100.0dB"));
        }

        [TestMethod]
        public void TestFormattingAnalogDecimal()
        {
            var ampItem = new AnalogMenuItemBuilder()
                .WithName("Analog Item")
                .WithId(5)
                .WithOffset(0)
                .WithDivisor(100)
                .WithUnitName("A")
                .WithMaxValue(1300)
                .Build();

            Assert.AreEqual("99.12A", MenuItemFormatter.FormatForDisplay(ampItem, 9912));
            Assert.AreEqual("2.03A", MenuItemFormatter.FormatForDisplay(ampItem, 203));
            Assert.AreEqual("0.00A", MenuItemFormatter.FormatForDisplay(ampItem, 0));

            Assert.AreEqual("1265", MenuItemFormatter.FormatToWire(ampItem, "12.65A"));
            Assert.AreEqual("110", MenuItemFormatter.FormatToWire(ampItem, "1.1"));
            Assert.AreEqual("1050", MenuItemFormatter.FormatToWire(ampItem, "10.50A"));

            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(ampItem, "1301"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(ampItem, "-100.0"));
        }

        [TestMethod]
        public void TestFormattingAnalogWhole()
        {
            var ampItem = new AnalogMenuItemBuilder()
                .WithName("Analog Item")
                .WithId(5)
                .WithOffset(-5)
                .WithDivisor(0)
                .WithUnitName("")
                .WithMaxValue(10)
                .Build();

            Assert.AreEqual("0", MenuItemFormatter.FormatForDisplay(ampItem, 5));
            Assert.AreEqual("1", MenuItemFormatter.FormatForDisplay(ampItem, 6));
            Assert.AreEqual("-3", MenuItemFormatter.FormatForDisplay(ampItem, 2));

            Assert.AreEqual("8", MenuItemFormatter.FormatToWire(ampItem, "3"));
            Assert.AreEqual("10", MenuItemFormatter.FormatToWire(ampItem, "5"));
            Assert.AreEqual("3", MenuItemFormatter.FormatToWire(ampItem, "-2"));

            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(ampItem, "99"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(ampItem, "-10"));

        }

        [TestMethod]
        public void TestFormattingEnum()
        {
            var enumItem = new EnumMenuItemBuilder()
                .WithName("Enum")
                .WithId(3)
                .WithEntries(new List<string> { "Entry1", "Entry2", "Entry3" })
                .Build();

            Assert.AreEqual("Entry1", MenuItemFormatter.FormatForDisplay(enumItem, 0));
            Assert.AreEqual("Entry3", MenuItemFormatter.FormatForDisplay(enumItem, 2));
            Assert.AreEqual("", MenuItemFormatter.FormatForDisplay(enumItem, 10));

            Assert.AreEqual("0", MenuItemFormatter.FormatToWire(enumItem, "0"));
            Assert.AreEqual("2", MenuItemFormatter.FormatToWire(enumItem, "2"));
            Assert.AreEqual("0", MenuItemFormatter.FormatToWire(enumItem, "Entry1"));
            Assert.AreEqual("1", MenuItemFormatter.FormatToWire(enumItem, "Entry2"));

            Assert.ThrowsException<MenuItemFormatException>(()=>MenuItemFormatter.FormatToWire(enumItem, "Entry99"));
            Assert.ThrowsException<MenuItemFormatException>(()=>MenuItemFormatter.FormatToWire(enumItem, "9"));
        }

        [TestMethod]
        public void TestFormattingBool()
        {
            var boolItem = new BooleanMenuItemBuilder()
                .WithName("Bool")
                .WithId(4)
                .WithNaming(BooleanNaming.TRUE_FALSE)
                .Build();

            Assert.AreEqual("True", MenuItemFormatter.FormatForDisplay(boolItem, true));
            Assert.AreEqual("False", MenuItemFormatter.FormatForDisplay(boolItem, false));

            var yesNo = new BooleanMenuItemBuilder().WithExisting(boolItem).WithNaming(BooleanNaming.YES_NO).Build();
            Assert.AreEqual("Yes", MenuItemFormatter.FormatForDisplay(yesNo, true));
            Assert.AreEqual("No", MenuItemFormatter.FormatForDisplay(yesNo, false));

            var onOff = new BooleanMenuItemBuilder().WithExisting(boolItem).WithNaming(BooleanNaming.ON_OFF).Build();
            Assert.AreEqual("On", MenuItemFormatter.FormatForDisplay(onOff, true));
            Assert.AreEqual("Off", MenuItemFormatter.FormatForDisplay(onOff, false));

            Assert.AreEqual("0", MenuItemFormatter.FormatToWire(boolItem, "False"));
            Assert.AreEqual("1", MenuItemFormatter.FormatToWire(boolItem, "true"));
            Assert.AreEqual("0", MenuItemFormatter.FormatToWire(boolItem, "Off"));
            Assert.AreEqual("1", MenuItemFormatter.FormatToWire(boolItem, "oN"));
            Assert.AreEqual("0", MenuItemFormatter.FormatToWire(boolItem, "no"));
            Assert.AreEqual("1", MenuItemFormatter.FormatToWire(boolItem, "Yes"));

        }

        [TestMethod]
        public void TestFormattingSubAction()
        {
            var action = MenuItemFixtures.AnActionItem(1, "");
            Assert.AreEqual("", MenuItemFormatter.FormatForDisplay(action, true));

            var sub = MenuItemFixtures.ASubItem(2, "");
            Assert.AreEqual("", MenuItemFormatter.FormatForDisplay(sub, true));

            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(sub, "true"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(action, "true"));

        }

        [TestMethod]
        public void TestFormattingText()
        {
            var text = new EditableTextMenuItemBuilder()
                .WithName("Name")
                .WithId(2)
                .WithEditType(EditItemType.PLAIN_TEXT)
                .WithTextLength(10)
                .Build();

            var ip = new EditableTextMenuItemBuilder().WithExisting(text).WithEditType(EditItemType.IP_ADDRESS).Build();
            var time = new EditableTextMenuItemBuilder().WithExisting(text).WithEditType(EditItemType.TIME_24_HUNDREDS).Build();

            Assert.AreEqual("Hello", MenuItemFormatter.FormatForDisplay(text, "Hello"));
            Assert.AreEqual("192.168.0.96", MenuItemFormatter.FormatForDisplay(ip, "192.168.0.96"));
            Assert.AreEqual("12:00:00AM", MenuItemFormatter.FormatForDisplay(time, "12:00:00AM"));

            Assert.AreEqual("12:00:00", MenuItemFormatter.FormatToWire(time, "12:00:00"));
            Assert.AreEqual("data123", MenuItemFormatter.FormatToWire(text, "data123"));
            Assert.AreEqual("192.168.0.2", MenuItemFormatter.FormatToWire(ip, "192.168.0.2"));

            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(time, "kdkeirk"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(text, "data123459834sdf57"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(ip, "eo349k"));
        }

        [TestMethod]
        public void TestFormattingFloat()
        {
            var floatItem = new FloatMenuItemBuilder()
                .WithName("Flt")
                .WithId(4)
                .WithDecimalPlaces(3)
                .Build();

            Assert.AreEqual("68.030", MenuItemFormatter.FormatForDisplay(floatItem, 68.03F));
            Assert.AreEqual("9999.444", MenuItemFormatter.FormatForDisplay(floatItem, 9999.44445F));

            Assert.ThrowsException<MenuItemFormatException>(()=>MenuItemFormatter.FormatToWire(floatItem, "9.9"));

        }

        [TestMethod]
        public void TestFormattingLargeNum()
        {
            var largeNum = new LargeNumberMenuItemBuilder()
                .WithName("Lge")
                .WithId(2)
                .WithTotalDigits(10)
                .WithDecimalPlaces(4)
                .Build();

            Assert.AreEqual("9384.4895", MenuItemFormatter.FormatForDisplay(largeNum, 9384.489489M));
            Assert.AreEqual("53345.3450", MenuItemFormatter.FormatForDisplay(largeNum, 53345.345M));

            Assert.AreEqual("123456", MenuItemFormatter.FormatToWire(largeNum, "123456"));
            Assert.AreEqual("123456.433567", MenuItemFormatter.FormatToWire(largeNum, "123456.433567"));
            Assert.AreEqual("-123456.433567", MenuItemFormatter.FormatToWire(largeNum, "-123456.433567"));

            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(largeNum, "1234567.33"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(largeNum, "123sdf6.z33"));

            var posOnlyLargeNum = new LargeNumberMenuItemBuilder()
                .WithName("Lge")
                .WithAllowNegative(false)
                .WithId(2)
                .WithTotalDigits(9)
                .WithDecimalPlaces(0)
                .Build();

            Assert.AreEqual("123456789", MenuItemFormatter.FormatToWire(posOnlyLargeNum, "123456789"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(posOnlyLargeNum, "1234567890"));
            Assert.ThrowsException<MenuItemFormatException>(() => MenuItemFormatter.FormatToWire(posOnlyLargeNum, "-1234"));
        }

        [TestMethod]
        public void TestFormatRgb()
        {
            var rgb = new Rgb32MenuItemBuilder()
                .WithId(233)
                .WithName("Rgb")
                .WithIncludeAlphaChannel(true)
                .Build();
            
            Assert.AreEqual("#223344FF", MenuItemFormatter.FormatForDisplay(rgb, new PortableColor("#223344FF")));
            Assert.AreEqual("#223344FF", MenuItemFormatter.FormatToWire(rgb, "#223344FF"));
        }

        [TestMethod]
        public void TestFormatScroll()
        {
            var scrollItem = new ScrollChoiceMenuItemBuilder()
                .WithId(10)
                .WithChoiceMode(ScrollChoiceMode.ARRAY_IN_RAM)
                .WithName("Scroller")
                .WithNumEntries(5).WithItemWidth(20)
                .Build();
            
            Assert.AreEqual("Hello", MenuItemFormatter.FormatForDisplay(scrollItem, new CurrentScrollPosition(10, "Hello")));
            Assert.AreEqual("10-", MenuItemFormatter.FormatToWire(scrollItem, "10"));
        }
    }
}
