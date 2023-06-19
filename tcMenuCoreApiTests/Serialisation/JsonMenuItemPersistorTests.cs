using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Serialisation;
using TcMenu.CoreSdkTests.MenuTests;

namespace TcMenu.CoreSdkTests.Serialisation
{
    [TestClass]
    public class JsonMenuItemPersistorTests
    {
        [TestMethod]
        public void TestLoadingLargeTree()
        {
            MenuTree tree = MenuItemFixtures.LoadMenuTree(MenuItemFixtures.LARGE_MENU_TREE);
            
            AssertThatTreeIsCorrect(tree);

            // now we create a verbatim copy of the tree and re-load it
            JsonMenuItemPersistor persistor = new JsonMenuItemPersistor();
            var newJson = persistor.SerialiseItemsRecursively(MenuTree.ROOT, tree);
            MenuTree newTree = MenuItemFixtures.LoadMenuTree(newJson);

            AssertThatTreeIsCorrect(newTree);
        }

        [TestMethod]
        public void TestLoadingWithVariableName()
        {
            MenuTree tree = new MenuTree();
            tree.AddMenuItem(MenuTree.ROOT, new ActionMenuItemBuilder()
                .WithExisting(MenuItemFixtures.AnActionItem(10, "abcd", "func"))
                .WithVariableName("defg").Build());
            var sub = new SubMenuItemBuilder()
                .WithExisting(MenuItemFixtures.ASubItem(11, "sub"))
                .WithVariableName("subII").Build();
            tree.AddMenuItem(MenuTree.ROOT, sub);
            tree.AddMenuItem(sub, new BooleanMenuItemBuilder()
                .WithExisting(MenuItemFixtures.ABoolItem(12, "bool"))
                .WithVariableName("boo").Build());

            JsonMenuItemPersistor persistor = new JsonMenuItemPersistor();
            var data = persistor.SerialiseItemsRecursively(MenuTree.ROOT, tree, false);
            var listOfItems = persistor.DeSerialiseItemsFromJson(data);

            Assert.AreEqual(3, listOfItems.Count);
            Assert.AreEqual(0, listOfItems[0].ParentId);
            Assert.AreEqual(0, listOfItems[1].ParentId);
            Assert.AreEqual(11, listOfItems[2].ParentId);
            Assert.AreEqual("abcd", listOfItems[0].Item.Name);
            Assert.AreEqual("sub", listOfItems[1].Item.Name);
            Assert.AreEqual("bool", listOfItems[2].Item.Name);
            Assert.AreEqual("defg", listOfItems[0].Item.VariableName);
            Assert.AreEqual("subII", listOfItems[1].Item.VariableName);
            Assert.AreEqual("boo", listOfItems[2].Item.VariableName);
        }

        private void AssertThatTreeIsCorrect(MenuTree tree)
        {
            // check everything is where it should be
            Assert.AreEqual("Voltage", tree.GetMenuById(1).Name);
            Assert.AreEqual("Current", tree.GetMenuById(2).Name);
            Assert.AreEqual("Limit", tree.GetMenuById(3).Name);
            Assert.AreEqual("Settings", tree.GetMenuById(4).Name);
            Assert.AreEqual("Pwr Delay", tree.GetMenuById(5).Name);
            Assert.AreEqual("Save all", tree.GetMenuById(10).Name);
            Assert.AreEqual("Advanced", tree.GetMenuById(11).Name);
            Assert.AreEqual("S-Circuit Protect", tree.GetMenuById(12).Name);
            Assert.AreEqual("Temp Check", tree.GetMenuById(13).Name);
            Assert.AreEqual("Status", tree.GetMenuById(7).Name);
            Assert.AreEqual("Volt A0", tree.GetMenuById(8).Name);
            Assert.AreEqual("Volt A1", tree.GetMenuById(9).Name);
            Assert.AreEqual("Connectivity", tree.GetMenuById(14).Name);
            Assert.AreEqual("Ip Address", tree.GetMenuById(15).Name);
            Assert.AreEqual("RotationCounter", tree.GetMenuById(16).Name);
            Assert.AreEqual("Rom Choice", tree.GetMenuById(18).Name);
            Assert.AreEqual("Custom Choice", tree.GetMenuById(19).Name);
            Assert.AreEqual("LED RGB", tree.GetMenuById(17).Name);

            // now check everything is in the right place.
            Assert.AreEqual(0, tree.FindParent(tree.GetMenuById(1)).Id); // should be in root
            Assert.AreEqual(0, tree.FindParent(tree.GetMenuById(2)).Id); // dito.

            Assert.AreEqual(0, tree.FindParent(tree.GetMenuById(7)).Id); // status parent is ROOT
            Assert.AreEqual(14, tree.FindParent(tree.GetMenuById(15)).Id); // IP parent is connectivity
            Assert.AreEqual(4, tree.FindParent(tree.GetMenuById(11)).Id); // advanced parent is settings
            Assert.AreEqual(0, tree.FindParent(tree.GetMenuById(4)).Id); // settings parent is ROOT

            // check one of each type for proper value loading
            var voltageItem = tree.GetMenuById(1) as AnalogMenuItem;
            Assert.AreEqual("onVoltageChange", voltageItem.FunctionName);
            Assert.AreEqual(2, voltageItem.EepromAddress);
            Assert.AreEqual(255, voltageItem.MaximumValue);
            Assert.AreEqual(-128, voltageItem.Offset);
            Assert.AreEqual(2, voltageItem.Divisor);
            Assert.AreEqual("V", voltageItem.UnitName);

            Assert.IsInstanceOfType(tree.GetMenuById(4), typeof(SubMenuItem));
            Assert.IsInstanceOfType(tree.GetMenuById(10), typeof(ActionMenuItem));

            var pwrDelayItem = tree.GetMenuById(5) as BooleanMenuItem;
            Assert.AreEqual(-1, pwrDelayItem.EepromAddress);
            Assert.AreEqual(null, pwrDelayItem.FunctionName);
            Assert.AreEqual(BooleanNaming.YES_NO, pwrDelayItem.Naming);

            var voltA0 = tree.GetMenuById(8) as FloatMenuItem;
            Assert.AreEqual(-1, voltA0.EepromAddress);
            Assert.AreEqual(null, voltA0.FunctionName);
            Assert.AreEqual(2, voltA0.DecimalPlaces);
            Assert.IsTrue(voltA0.Visible);
            Assert.IsTrue(voltA0.ReadOnly);

            var ipAddr = tree.GetMenuById(15) as EditableTextMenuItem;
            Assert.AreEqual(EditItemType.IP_ADDRESS, ipAddr.EditType);
            Assert.AreEqual(20, ipAddr.TextLength);
            Assert.AreEqual(10, ipAddr.EepromAddress);
            Assert.IsFalse(ipAddr.Visible);
            Assert.IsTrue(voltA0.ReadOnly);

            var enumItem = tree.GetMenuById(3) as EnumMenuItem;
            CollectionAssert.AreEqual(new string[] { "Current", "Voltage" }, enumItem.EnumEntries);
            Assert.AreEqual(6, enumItem.EepromAddress);
            Assert.AreEqual("onLimitMode", enumItem.FunctionName);
            Assert.IsFalse(enumItem.ReadOnly);
            Assert.IsTrue(enumItem.Visible);

            var largeNum = tree.GetMenuById(16) as LargeNumberMenuItem;
            Assert.AreEqual(-1, largeNum.EepromAddress);
            Assert.AreEqual(4, largeNum.DecimalPlaces);
            Assert.AreEqual(8, largeNum.TotalDigits);
            Assert.IsTrue(largeNum.Visible);

            var connectivity = tree.GetMenuById(14) as SubMenuItem;
            Assert.IsTrue(connectivity.LocalOnly);
            Assert.IsTrue(connectivity.Secured);

            var settings = tree.GetMenuById(4) as SubMenuItem;
            Assert.IsFalse(settings.LocalOnly);
            Assert.IsFalse(settings.Secured);
            
            var rgb = tree.GetMenuById(17) as Rgb32MenuItem;
            Assert.IsNotNull(rgb);
            Assert.IsTrue(rgb.IncludeAlphaChannel);
            
            var sc = tree.GetMenuById(18) as ScrollChoiceMenuItem;
            Assert.IsNotNull(sc);
            Assert.AreEqual(ScrollChoiceMode.ARRAY_IN_EEPROM, sc.ChoiceMode);
            Assert.AreEqual(10, sc.ItemWidth);
            Assert.AreEqual(40, sc.NumEntries);
            Assert.AreEqual(25, sc.EepromOffset);
            Assert.AreEqual("eeprom", sc.RamVariable);
            
            sc = tree.GetMenuById(19) as ScrollChoiceMenuItem;
            Assert.IsNotNull(sc);
            Assert.AreEqual(ScrollChoiceMode.CUSTOM_RENDERFN, sc.ChoiceMode);
        }
    }
}
