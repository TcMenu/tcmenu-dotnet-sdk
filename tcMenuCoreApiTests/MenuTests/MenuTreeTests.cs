using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcMenu.CoreSdk.MenuItems;

namespace TcMenu.CoreSdkTests.MenuTests
{
    [TestClass]
    public class MenuTreeTests
    {
        private MenuTree menuTree;
        private AnalogMenuItem itemAnalog;
        private EnumMenuItem itemEnum;
        private SubMenuItem itemSub;
        private BooleanMenuItem itemBool;

        [TestInitialize]
        public void BeforeTestingStarts()
        {
            menuTree = new MenuTree();
            itemAnalog = MenuItemFixtures.AnAnalogItem(1, "hello");
            itemEnum = MenuItemFixtures.AnEnumItem(2, "boo");
            itemSub = MenuItemFixtures.ASubItem(100, "sub");
            itemBool = MenuItemFixtures.ABoolItem(3, "bool");
        }

        [TestMethod]
        public void TestAddingEntriesToRoot()
        {
            Assert.AreEqual(MenuTree.ROOT, menuTree.GetSubMenuById(0));
            Assert.IsNull(menuTree.GetSubMenuById(999));
            menuTree.AddMenuItem(MenuTree.ROOT, itemAnalog);
            menuTree.AddMenuItem(MenuTree.ROOT, itemEnum);

            var allInRoot = menuTree.GetMenuItems(MenuTree.ROOT);
            CollectionAssert.AreEquivalent(new List<MenuItem> { itemAnalog, itemEnum }, allInRoot);

            var allEverywhere = menuTree.GetAllMenuItems();
            CollectionAssert.AreEquivalent(new List<MenuItem> { itemAnalog, itemEnum, MenuTree.ROOT }, allEverywhere.ToList());

            Assert.AreEqual(MenuTree.ROOT, menuTree.GetMenuById(0));
            Assert.AreEqual(itemAnalog, menuTree.GetMenuById(itemAnalog.Id));
        }

        [TestMethod]
        public void TestFindParentThenRemove()
        {
            menuTree.AddMenuItem(MenuTree.ROOT, itemAnalog);
            menuTree.AddMenuItem(MenuTree.ROOT, itemSub);
            menuTree.AddMenuItem(itemSub, itemEnum);
            menuTree.AddMenuItem(itemSub, itemBool);

            var allEverywhere = menuTree.GetAllMenuItems();
            CollectionAssert.AreEquivalent(new List<MenuItem> { itemAnalog, itemEnum, itemBool, itemSub, MenuTree.ROOT }, allEverywhere.ToList());

            Assert.AreEqual(itemSub, menuTree.FindParent(itemEnum));
            Assert.AreEqual(itemSub, menuTree.FindParent(itemBool));
            Assert.AreEqual(MenuTree.ROOT, menuTree.FindParent(itemSub));
            Assert.AreEqual(MenuTree.ROOT, menuTree.FindParent(itemAnalog));

            menuTree.RemoveMenuItem(itemBool);

            allEverywhere = menuTree.GetAllMenuItems();
            CollectionAssert.AreEquivalent(new List<MenuItem> { itemAnalog, itemEnum, itemSub, MenuTree.ROOT }, allEverywhere.ToList());

            menuTree.RemoveMenuItem(MenuTree.ROOT, itemAnalog);

            allEverywhere = menuTree.GetAllMenuItems();
            CollectionAssert.AreEquivalent(new List<MenuItem> { itemEnum, itemSub, MenuTree.ROOT }, allEverywhere.ToList());
        }

        [TestMethod]
        public void TestAddingItemState()
        {
            menuTree.AddMenuItem(MenuTree.ROOT, itemAnalog);
            menuTree.AddMenuItem(MenuTree.ROOT, itemEnum);

            menuTree.ChangeItemState(itemAnalog, new MenuState<int>(itemAnalog, true, false, 100));
            menuTree.ChangeItemState(itemEnum, new MenuState<int>(itemEnum, true, false, 2));
            if (menuTree.GetState(itemAnalog) is MenuState<int> state)
            {
                Assert.AreEqual(itemAnalog, state.Item);
                Assert.AreEqual(100, state.Value);
                Assert.IsTrue(state.Changed);
                Assert.IsFalse(state.Active);
            }
            else Assert.Fail();

            if (menuTree.GetState(itemEnum) is MenuState<int> enState)
            {
                Assert.AreEqual(itemEnum, enState.Item);
                Assert.AreEqual(2, enState.Value);
            }
            else Assert.Fail();

            Assert.AreEqual(itemAnalog, menuTree.GetMenuById(itemAnalog.Id));
            Assert.AreEqual(itemEnum, menuTree.GetMenuById(itemEnum.Id));
            Assert.IsNull(menuTree.GetMenuById(101));
        }

        [TestMethod]
        public void TestMovingItemsInList()
        {
            menuTree.AddMenuItem(MenuTree.ROOT, itemAnalog);
            menuTree.AddMenuItem(MenuTree.ROOT, itemBool);
            menuTree.AddMenuItem(MenuTree.ROOT, itemEnum);
            CollectionAssert.AreEqual(new List<MenuItem> { itemAnalog, itemBool, itemEnum }, menuTree.GetMenuItems(MenuTree.ROOT));

            menuTree.MoveItem(MenuTree.ROOT, itemEnum, MoveType.MOVE_UP);
            CollectionAssert.AreEqual(new List<MenuItem> { itemAnalog, itemEnum, itemBool }, menuTree.GetMenuItems(MenuTree.ROOT));

            menuTree.MoveItem(MenuTree.ROOT, itemEnum, MoveType.MOVE_UP);
            CollectionAssert.AreEqual(new List<MenuItem> { itemEnum, itemAnalog, itemBool }, menuTree.GetMenuItems(MenuTree.ROOT));

            menuTree.MoveItem(MenuTree.ROOT, itemEnum, MoveType.MOVE_UP);
            CollectionAssert.AreEqual(new List<MenuItem> { itemEnum, itemAnalog, itemBool }, menuTree.GetMenuItems(MenuTree.ROOT));

            menuTree.MoveItem(MenuTree.ROOT, itemAnalog, MoveType.MOVE_DOWN);
            CollectionAssert.AreEqual(new List<MenuItem> { itemEnum, itemBool, itemAnalog }, menuTree.GetMenuItems(MenuTree.ROOT));

            menuTree.MoveItem(MenuTree.ROOT, itemAnalog, MoveType.MOVE_DOWN);
            CollectionAssert.AreEqual(new List<MenuItem> { itemEnum, itemBool, itemAnalog }, menuTree.GetMenuItems(MenuTree.ROOT));
        }

        [TestMethod]
        public void TestReplacementsOnMenu()
        {
            menuTree.AddMenuItem(MenuTree.ROOT, itemAnalog);
            menuTree.AddMenuItem(MenuTree.ROOT, itemBool);
            menuTree.AddMenuItem(MenuTree.ROOT, itemSub);
            menuTree.AddMenuItem(itemSub, itemEnum);

            var itemAnalog2 = new AnalogMenuItemBuilder()
                .WithExisting(itemAnalog)
                .WithName("New Name")
                .Build();

            menuTree.ReplaceMenuById(itemAnalog2);

            var theItem = menuTree.GetMenuById(itemAnalog.Id);
            Assert.AreEqual("New Name", theItem.Name);

            var itemSub2 = new SubMenuItemBuilder()
                .WithExisting(itemSub)
                .WithName("NewSub")
                .Build();

            menuTree.ReplaceMenuById(itemSub2);
            theItem = menuTree.GetMenuById(itemSub.Id);
            Assert.AreEqual("NewSub", theItem.Name);
            
            //TODO check the sub menu list.
        }

    }
}
