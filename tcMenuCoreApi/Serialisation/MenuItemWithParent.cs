using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.MenuItems;

namespace TcMenu.CoreSdk.Serialisation
{
    /// <summary>
    /// A simple value object representing a menu item and it's parent ID. Used during serialisation to hold both the
    /// menu item and it's location in the tree.
    /// </summary>
    public class MenuItemWithParent
    {
        public MenuItem Item { get; set; }
        public int ParentId { get; set; }

        public MenuItemWithParent(int parentId, MenuItem item)
        {
            Item = item;
            ParentId = parentId;
        }
    }

}
