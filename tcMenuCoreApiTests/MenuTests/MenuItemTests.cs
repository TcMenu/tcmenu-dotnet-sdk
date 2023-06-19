using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcMenu.CoreSdk.MenuItems;
using System.Collections.Generic;

namespace TcMenu.CoreSdkTests.MenuTests
{
    [TestClass]
    public class MenuItemTests
    {
        [TestMethod]
        public void TestBooleanItem()
        {
            var item = new BooleanMenuItemBuilder()
                .WithId(102)
                .WithName("hello")
                .WithVariableName("abc123")
                .WithEepromLocation(-1)
                .WithFunctionName("Function5543")
                .WithLocalOnly(false)
                .WithReadOnly(true)
                .WithNaming(BooleanNaming.ON_OFF)
                .Build();

            Assert.AreEqual("hello", item.Name);
            Assert.AreEqual(102, item.Id);
            Assert.AreEqual(-1, item.EepromAddress);
            Assert.AreEqual("Function5543", item.FunctionName);
            Assert.AreEqual("abc123", item.VariableName);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsFalse(item.LocalOnly);
            Assert.AreEqual(BooleanNaming.ON_OFF, item.Naming);

            // the copied item should be the same.
            var copiedItem = new BooleanMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "BOOL");

            var boolState = new MenuState<bool>(item, false, false, true);
            Assert.AreEqual(item, boolState.Item);
            Assert.IsTrue(boolState.Value);
            Assert.AreEqual(true, boolState.ValueAsObject());
            Assert.IsFalse(boolState.Changed);
            Assert.IsFalse(boolState.Active);
        }

        [TestMethod]
        public void TestActionItem()
        {
            var item = new ActionMenuItemBuilder()
                .WithId(234)
                .WithEepromLocation(293)
                .WithFunctionName("Functidfsdfasfon5543")
                .WithName("dsfsd")
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .Build();

            Assert.AreEqual("dsfsd", item.Name);
            Assert.AreEqual(234, item.Id);
            Assert.AreEqual(293, item.EepromAddress);
            Assert.AreEqual("Functidfsdfasfon5543", item.FunctionName);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);

            // the copied item should be the same.
            var copiedItem = new ActionMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "ACTION");
        }

        [TestMethod]
        public void TestFloatItem()
        {
            var item = new FloatMenuItemBuilder()
                .WithId(234)
                .WithName("floating")
                .WithLocalOnly(false)
                .WithReadOnly(true)
                .Build();

            Assert.AreEqual("floating", item.Name);
            Assert.AreEqual(234, item.Id);
            Assert.AreEqual(-1, item.EepromAddress);
            Assert.AreEqual("", item.FunctionName);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsFalse(item.LocalOnly);

            // the copied item should be the same.
            var copiedItem = new FloatMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "FLOAT");
        }

        [TestMethod]
        public void TestRgb32MenuItem()
        {
            var item = new Rgb32MenuItemBuilder()
                .WithId(222)
                .WithName("RgbItem")
                .WithFunctionName("SomeFunc")
                .WithIncludeAlphaChannel(true)
                .WithEepromLocation(444)
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .Build();

            Assert.AreEqual("RgbItem", item.Name);
            Assert.AreEqual(222, item.Id);
            Assert.AreEqual(444, item.EepromAddress);
            Assert.AreEqual("SomeFunc", item.FunctionName);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);
            Assert.IsTrue(item.IncludeAlphaChannel);

            // the copied item should be the same.
            var copiedItem = new Rgb32MenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "RGB");
        }

        [TestMethod]
        public void TestScrollChoiceMenuItem()
        {
            var item = new ScrollChoiceMenuItemBuilder()
                .WithId(222)
                .WithName("RgbItem")
                .WithNumEntries(10)
                .WithEepromLocation(44)
                .WithItemWidth(8)
                .WithEepromOffset(25)
                .WithChoiceMode(ScrollChoiceMode.CUSTOM_RENDERFN)
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .Build();

            Assert.AreEqual("RgbItem", item.Name);
            Assert.AreEqual(222, item.Id);
            Assert.AreEqual(44, item.EepromAddress);
            Assert.AreEqual("", item.FunctionName);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);
            Assert.AreEqual(ScrollChoiceMode.CUSTOM_RENDERFN ,item.ChoiceMode);
            Assert.AreEqual(10, item.NumEntries);
            Assert.AreEqual(25,item.EepromOffset);
            Assert.AreEqual(8,item.ItemWidth);
            
            // the copied item should be the same.
            var copiedItem = new ScrollChoiceMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "SCRL");
        }

        [TestMethod]
        public void TestRuntimeListItem()
        {
            var item = new RuntimeListMenuItemBuilder()
                .WithId(1)
                .WithName("RunList")
                .WithFunctionName("RuntimeListFn")
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .WithInitialRows(2)
                .WithCreationMode(ListCreationMode.FlashArray)
                .Build();

            Assert.AreEqual("RunList", item.Name);
            Assert.AreEqual(1, item.Id);
            Assert.AreEqual(-1, item.EepromAddress);
            Assert.AreEqual("RuntimeListFn", item.FunctionName);
            Assert.AreEqual(ListCreationMode.FlashArray, item.CreationMode);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);
            Assert.AreEqual(2, item.InitialRows);

            // the copied item should be the same.
            var copiedItem = new RuntimeListMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "LIST");

            var listOfItems = new List<string> { "item1", "item2" };
            var listState = new MenuState<List<string>>(item, false, true, listOfItems);

            Assert.AreEqual(item, listState.Item);
            Assert.AreEqual(listOfItems, listState.Value);
            Assert.IsFalse(listState.Changed);
            Assert.IsTrue(listState.Active);

        }

        [TestMethod]
        public void TestSubMenuItem()
        {
            var item = new SubMenuItemBuilder()
                .WithId(321)
                .WithName("submenu")
                .WithVariableName("OverrideName")
                .WithLocalOnly(false)
                .WithReadOnly(false)
                .WithSecured(true)
                .Build();

            Assert.AreEqual("submenu", item.Name);
            Assert.AreEqual(321, item.Id);
            Assert.AreEqual(-1, item.EepromAddress);
            Assert.AreEqual("", item.FunctionName);
            Assert.AreEqual("OverrideName", item.VariableName);
            Assert.IsFalse(item.ReadOnly);
            Assert.IsFalse(item.LocalOnly);
            Assert.IsTrue(item.Secured);

            // the copied item should be the same.
            var copiedItem = new SubMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "SUB");
        }

        [TestMethod]
        public void TestAnalogItem()
        {
            var item = new AnalogMenuItemBuilder()
                .WithId(103)
                .WithEepromLocation(1000)
                .WithFunctionName("Function")
                .WithName("Name123")
                .WithLocalOnly(true)
                .WithReadOnly(false)
                .WithDivisor(10)
                .WithMaxValue(250)
                .WithOffset(-100)
                .WithStep(2)
                .WithUnitName("AB")
                .Build();

            Assert.AreEqual("Name123", item.Name);
            Assert.AreEqual(103, item.Id);
            Assert.AreEqual(1000, item.EepromAddress);
            Assert.AreEqual("Function", item.FunctionName);
            Assert.AreEqual(2, item.Step);
            Assert.IsFalse(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);
            Assert.AreEqual(10, item.Divisor);
            Assert.AreEqual(250, item.MaximumValue);
            Assert.AreEqual(-100, item.Offset);
            Assert.AreEqual("AB", item.UnitName);

            // copied item should match the item
            var copiedItem = new AnalogMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "ANALOG");

            var intState = new MenuState<int>(item, true, true, 102);
            Assert.AreEqual(item, intState.Item);
            Assert.AreEqual(102, intState.Value);
            Assert.AreEqual(102, intState.ValueAsObject());
            Assert.IsTrue(intState.Changed);
            Assert.IsTrue(intState.Active);
        }

        [TestMethod]
        public void TestEnumItem()
        {
            var item = new EnumMenuItemBuilder()
                .WithId(111)
                .WithEepromLocation(222)
                .WithFunctionName("FnSome")
                .WithName("HelloThere")
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .WithNewEntry("Entry1")
                .WithNewEntry("Entry2")
                .Build();

            var verbatimItem = new EnumMenuItemBuilder()
                .WithId(111)
                .WithEepromLocation(222)
                .WithFunctionName("FnSome")
                .WithName("HelloThere")
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .WithNewEntry("Entry1")
                .WithNewEntry("Entry2")
                .Build();

            Assert.AreEqual("HelloThere", item.Name);
            Assert.AreEqual(111, item.Id);
            Assert.AreEqual(222, item.EepromAddress);
            Assert.AreEqual("FnSome", item.FunctionName);
            Assert.AreEqual(verbatimItem, item);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);
            CollectionAssert.AreEqual(item.EnumEntries, new List<string> { "Entry1", "Entry2" });

            // the copied item should be the same.
            var copiedItem = new EnumMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "ENUM");
        }

        [TestMethod]
        public void TestLargeNumberMenuItem()
        {
            var largeItem = new LargeNumberMenuItemBuilder()
                .WithId(101)
                .WithName("LargeNum")
                .WithEepromLocation(102)
                .WithFunctionName("Room101")
                .WithTotalDigits(10)
                .WithDecimalPlaces(5)
                .Build();

            Assert.AreEqual(101, largeItem.Id);
            Assert.AreEqual("LargeNum", largeItem.Name);
            Assert.AreEqual("Room101", largeItem.FunctionName);
            Assert.AreEqual(102, largeItem.EepromAddress);
            Assert.AreEqual(10, largeItem.TotalDigits);
            Assert.AreEqual(5, largeItem.DecimalPlaces);

            var copiedItem = new LargeNumberMenuItemBuilder().WithExisting(largeItem).Build();
            Assert.AreEqual(copiedItem, largeItem);

            AssertThatVisitorIsOfType(largeItem, "NUMBER");
        }

        [TestMethod]
        public void TestEditableTextItem()
        {
            var item = new EditableTextMenuItemBuilder()
                .WithId(3453)
                .WithEepromLocation(-1)
                .WithName("Blah")
                .WithVariableName("VarName")
                .WithLocalOnly(true)
                .WithReadOnly(true)
                .WithEditType(EditItemType.IP_ADDRESS)
                .WithTextLength(42)
                .Build();

            Assert.AreEqual("Blah", item.Name);
            Assert.AreEqual(3453, item.Id);
            Assert.AreEqual(-1, item.EepromAddress);
            Assert.AreEqual("", item.FunctionName);
            Assert.AreEqual("VarName", item.VariableName);
            Assert.IsTrue(item.ReadOnly);
            Assert.IsTrue(item.LocalOnly);
            Assert.AreEqual(EditItemType.IP_ADDRESS, item.EditType);
            Assert.AreEqual(42, item.TextLength);

            // the copied item should be the same.
            var copiedItem = new EditableTextMenuItemBuilder().WithExisting(item).Build();
            Assert.AreEqual(item, copiedItem);

            AssertThatVisitorIsOfType(item, "EDITABLE");

            var strState = new MenuState<string>(item, false, true, "hello");
            Assert.AreEqual(item, strState.Item);
            Assert.AreEqual("hello", strState.Value);
            Assert.AreEqual("hello", strState.ValueAsObject());
            Assert.IsFalse(strState.Changed);
            Assert.IsTrue(strState.Active);
        }

        [TestMethod]
        public void TestVisibleFlag()
        {
            var item = MenuItemFixtures.ABoolItem(1, "abc");
            Assert.IsTrue(item.Visible);
            item = new BooleanMenuItemBuilder().WithExisting(item).Build();
            Assert.IsTrue(item.Visible);
            item = new BooleanMenuItemBuilder().WithExisting(item).WithVisible(false).Build();
            Assert.IsFalse(item.Visible);
            item = new BooleanMenuItemBuilder().WithExisting(item).Build();
            Assert.IsFalse(item.Visible);
        }

        private void AssertThatVisitorIsOfType(MenuItem item, string expected)
        {
            var visitor = new TestMenuVisitor();
            item.Accept(visitor);
            Assert.IsTrue(visitor.Result.Contains(expected));
        }
    }

    class TestMenuVisitor : MenuItemVisitor
    {
        public string Result { get; set; } = "";

        public void Visit(BooleanMenuItem item)
        {
            Result += "BOOL";
        }

        public void Visit(AnalogMenuItem item)
        {
            Result += "ANALOG";
        }

        public void Visit(ActionMenuItem item)
        {
            Result += "ACTION";
        }

        public void Visit(EnumMenuItem item)
        {
            Result += "ENUM";
        }
        
        public void Visit(ScrollChoiceMenuItem item)
        {
            Result += "SCRL";
        }
        public void Visit(EditableTextMenuItem item)
        {
            Result += "EDITABLE";
        }

        public void Visit(Rgb32MenuItem item)
        {
            Result += "RGB";
        }

        public void Visit(FloatMenuItem item)
        {
            Result += "FLOAT";
        }

        public void Visit(SubMenuItem item)
        {
            Result += "SUB";
        }

        public void Visit(RuntimeListMenuItem item)
        {
            Result += "LIST";
        }

        public void Visit(LargeNumberMenuItem item)
        {
            Result += "NUMBER";
        }
    }
}
