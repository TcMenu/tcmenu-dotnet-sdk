using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdkTests.MenuTests
{
    [TestClass]
    public class MenuItemHelperTests
    {
        [TestMethod]
        public void TestCreateFromExistingWithNewId()
        {
            var analog = MenuItemFixtures.AnAnalogItem(100, "Blah");
            var analogNew = MenuItemHelper.CreateFromExistingWithNewId(analog, 1000) as AnalogMenuItem;
            Assert.AreEqual(analog.MaximumValue, analogNew.MaximumValue);
            Assert.AreEqual(1000, analogNew.Id);
            Assert.AreEqual(-1, analogNew.EepromAddress);

            var enumItem = MenuItemFixtures.AnEnumItem(100, "Enum", 101);
            var newEnum = MenuItemHelper.CreateFromExistingWithNewId(enumItem, 1000) as EnumMenuItem;
            Assert.AreEqual(1000, newEnum.Id);
            Assert.AreEqual(-1, newEnum.EepromAddress);

            var boolItem = MenuItemFixtures.ABoolItem(100, "Bool");
            var newBool = MenuItemHelper.CreateFromExistingWithNewId(boolItem, 1000) as BooleanMenuItem;
            Assert.AreEqual(1000, newBool.Id);
            Assert.AreEqual(-1, newEnum.EepromAddress);

            var floatItem = MenuItemFixtures.AFloatItem(100, "Flt", 203);
            var newFloat = MenuItemHelper.CreateFromExistingWithNewId(floatItem, 1000) as FloatMenuItem;
            Assert.AreEqual(1000, newFloat.Id);
            Assert.AreEqual(-1, newFloat.EepromAddress);

            var subItem = MenuItemFixtures.ASubItem(100, "Sub", 203);
            var newSub = MenuItemHelper.CreateFromExistingWithNewId(subItem, 1000) as SubMenuItem;
            Assert.AreEqual(1000, newSub.Id);
            Assert.AreEqual(-1, newSub.EepromAddress);

            var actionItem = MenuItemFixtures.AnActionItem(100, "Act", "Fn");
            var newAction = MenuItemHelper.CreateFromExistingWithNewId(actionItem, 1000) as ActionMenuItem;
            Assert.AreEqual(1000, newAction.Id);
            Assert.AreEqual(-1, newAction.EepromAddress);

            var textItem = MenuItemFixtures.ATextItem(100, "Txt", 203);
            var newText = MenuItemHelper.CreateFromExistingWithNewId(textItem, 1000) as EditableTextMenuItem;
            Assert.AreEqual(1000, newText.Id);
            Assert.AreEqual(-1, newText.EepromAddress);

            var largeNumItem = MenuItemFixtures.ALargeNumberItem(100, "Lge", 203);
            var newLargeNum = MenuItemHelper.CreateFromExistingWithNewId(largeNumItem, 1000) as LargeNumberMenuItem;
            Assert.AreEqual(1000, newLargeNum.Id);
            Assert.AreEqual(-1, newLargeNum.EepromAddress);
        }

        [TestMethod]
        public void TestGetNextLargestIdAndEeprom()
        {
            var tree = MenuItemFixtures.LoadMenuTree(MenuItemFixtures.LARGE_MENU_TREE);

            Assert.AreEqual(20, MenuItemHelper.FindAvailableMenuId(tree));
            
            Assert.AreEqual(14, MenuItemHelper.FindAvailableEEPROMLocation(tree));

            var newTree = new MenuTree();
            Assert.AreEqual(2, MenuItemHelper.FindAvailableEEPROMLocation(newTree));
            Assert.AreEqual(1, MenuItemHelper.FindAvailableMenuId(newTree));
        }

        [TestMethod]
        public void TestGetEepromStorageRequirement()
        {
            Assert.AreEqual(2, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.AnAnalogItem(122, "hello")));
            Assert.AreEqual(2, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.AnEnumItem(211, "hello")));
            Assert.AreEqual(1, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.ABoolItem(132, "hello")));
            Assert.AreEqual(10, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.ATextItem(232, "hello", 10, EditItemType.PLAIN_TEXT)));
            Assert.AreEqual(4, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.ATextItem(243, "hello", 22, EditItemType.IP_ADDRESS)));
            Assert.AreEqual(4, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.ATextItem(242, "hello", 22, EditItemType.TIME_12H)));
            Assert.AreEqual(8, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.ALargeNumberItem(349, "Avin it large", 12)));
            Assert.AreEqual(0, MenuItemHelper.GetEEPROMStorageRequirement(MenuItemFixtures.AFloatItem(10, "Flt")));
        }

        [TestMethod]
        public void TestCreateStateFunction()
        {
            var analogItem = MenuItemFixtures.AnAnalogItem(100, "Blah");
            var enumItem = MenuItemFixtures.AnEnumItem(100, "Blah");
            var boolMenuItem = MenuItemFixtures.ABoolItem(100, "Blah");
            var floatItem = MenuItemFixtures.AFloatItem(100, "Blah");
            var textItem = MenuItemFixtures.ATextItem(100, "Blah");
            var largeNum = MenuItemFixtures.ALargeNumberItem(10, "hello");
            var listItem = new RuntimeListMenuItemBuilder().WithId(10).WithCreationMode(ListCreationMode.CustomRtCall)
                .WithName("hello").WithInitialRows(2).Build();
            var scrollItem = new ScrollChoiceMenuItemBuilder().WithId(190).WithName("scroll").WithNumEntries(10)
                .WithChoiceMode(ScrollChoiceMode.CUSTOM_RENDERFN).Build();
            var rgbItem = new Rgb32MenuItemBuilder().WithId(1).WithName("hello").Build();

            CheckState(analogItem, typeof(int), 10, true, false);
            CheckState(analogItem, typeof(int), 102, true, true, 20);
            CheckState(analogItem, typeof(int), "1033", false, true, 20); // above maximum
            CheckState(analogItem, typeof(int), -200, false, true, 0); // below min
            CheckState(boolMenuItem, typeof(bool), "true", false, true, true);
            CheckState(boolMenuItem, typeof(bool), "0", false, false, false);
            CheckState(boolMenuItem, typeof(bool), "1", false, false, true);
            CheckState(boolMenuItem, typeof(bool), 1, false, true, true);
            CheckState(boolMenuItem, typeof(bool), 0, true, false, false);
            CheckState(boolMenuItem, typeof(bool), "Y", false, false, true);
            CheckState(floatItem, typeof(float), "100.4", false, true, 100.4F);
            CheckState(floatItem, typeof(float), 10034.3, false, false, 10034.3F);
            CheckState(enumItem, typeof(int), 4, false, true, 1); // exceeds max
            CheckState(enumItem, typeof(int), "1", true, false, 1);
            CheckState(enumItem, typeof(int), "-221", true, false, 0); // below 0
            CheckState(textItem, typeof(string), "12345", true, true);
            CheckState(largeNum, typeof(decimal), "12345.432", true, true, 12345.432m);
            CheckState(largeNum, typeof(decimal), 12345.432m, true, false);
            CheckState(listItem, typeof(List<string>), new List<string> { "1", "2" }, true, false);
            CheckState(scrollItem, typeof(CurrentScrollPosition), "1-My Sel", true, false, new CurrentScrollPosition(1, "My Sel"));
            CheckState(scrollItem, typeof(CurrentScrollPosition), new CurrentScrollPosition(1, "Sel 123"), true, false);
            CheckState(rgbItem, typeof(PortableColor), "#ff00aa", true, false, new PortableColor("#ff00aa"));
            CheckState(rgbItem, typeof(PortableColor), new PortableColor("#000000"), true, false);
        }

        private void CheckState(MenuItem item, Type ty, Object value, bool changed, bool active)
        {
            CheckState(item, ty, value, changed, active, value);
        }


        private void CheckState(MenuItem item, Type ty, Object value, bool changed, bool active, Object actual)
        {
            var state = MenuItemHelper.StateForMenuItem(item, value, changed, active);
            Assert.AreEqual(ty, state.GetType().GenericTypeArguments[0]);
            Assert.AreEqual(changed, state.Changed);
            Assert.AreEqual(active, state.Active);
            if (actual is float) {
                Assert.AreEqual((float)actual, (float)state.ValueAsObject(), (float)0.00001);
            }
            else
            {
                Assert.AreEqual(actual, state.ValueAsObject());
            }
        }

        [TestMethod]
        public void TestSettingDeltaStateInteger()
        {
            var analogItem = new AnalogMenuItemBuilder().WithId(10).WithName("hello").WithDivisor(1).WithMaxValue(50).WithOffset(0).Build();
            var tree = new MenuTree();
            tree.AddMenuItem(MenuTree.ROOT, analogItem);
            MenuItemHelper.SetMenuState(analogItem, 10, tree);
            Assert.AreEqual(10, (int)MenuItemHelper.GetValueFor(analogItem, tree, -1));
            MenuItemHelper.ApplyIncrementalValueChange(analogItem, 1, tree);
            Assert.AreEqual(11, (int)MenuItemHelper.GetValueFor(analogItem, tree, -1));
            Assert.AreEqual(analogItem, MenuItemHelper.ApplyIncrementalValueChange(analogItem, -2, tree).Item);
            Assert.AreEqual(9, (int)MenuItemHelper.GetValueFor(analogItem, tree, -1));
            Assert.IsTrue(MenuItemHelper.ApplyIncrementalValueChange(analogItem, -100, tree) == null);
            Assert.AreEqual(9, (int)MenuItemHelper.GetValueFor(analogItem, tree, -1));
        }

        [TestMethod]
        public void TestSettingDeltaStateEnum()
        {
            var enumItem = new EnumMenuItemBuilder().WithId(10).WithName("hello").WithEntries(new List<string> { "1", "2", "3" }).Build();
            var tree = new MenuTree();
            tree.AddMenuItem(MenuTree.ROOT, enumItem);
            MenuItemHelper.SetMenuState(enumItem, 1, tree);
            Assert.AreEqual(1, (int)MenuItemHelper.GetValueFor(enumItem, tree, -1));
            MenuItemHelper.ApplyIncrementalValueChange(enumItem, -1, tree);
            Assert.AreEqual(0, (int)MenuItemHelper.GetValueFor(enumItem, tree, -1));
            // can't go past 0.
            MenuItemHelper.ApplyIncrementalValueChange(enumItem, -1, tree);
            Assert.AreEqual(0, (int)MenuItemHelper.GetValueFor(enumItem, tree, -1));

        }

        [TestMethod]
        public void TestSettingDeltaStatePosition()
        {
            var tree = new MenuTree();
            var item = new ScrollChoiceMenuItemBuilder().WithChoiceMode(ScrollChoiceMode.CUSTOM_RENDERFN).WithId(10)
                .WithNumEntries(10).WithName("scroll").Build();
            tree.AddMenuItem(MenuTree.ROOT, item);
            MenuItemHelper.SetMenuState(item, 1, tree);
            Assert.AreEqual(1, MenuItemHelper.GetValueFor(item, tree, new CurrentScrollPosition("0-")).Position);
            MenuItemHelper.ApplyIncrementalValueChange(item, 1, tree);
            Assert.AreEqual(2, MenuItemHelper.GetValueFor(item, tree, new CurrentScrollPosition("0-")).Position);
        }

        [TestMethod]
        public void TestGetValueFor()
        {
            var analogItem = new AnalogMenuItemBuilder().WithId(10).WithName("hello").WithDivisor(1).WithMaxValue(50).WithOffset(0).Build();
            var tree = new MenuTree();
            tree.AddMenuItem(MenuTree.ROOT, analogItem);

            Assert.AreEqual(-1, (int)MenuItemHelper.GetValueFor(analogItem, tree, -1));
            MenuItemHelper.SetMenuState(analogItem, 22, tree);
            Assert.AreEqual(22, (int)MenuItemHelper.GetValueFor(analogItem, tree, -1));
        }
    }
}
