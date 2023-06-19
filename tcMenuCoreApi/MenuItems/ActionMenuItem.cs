using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    /// A menu item that purely represents a function that can be run on the embedded device. It holds no state of its own.
    /// </summary>
    public class ActionMenuItem : MenuItem
    {
        public ActionMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    /// <summary>
    /// Constructs an instance of an action menu item using the builder pattern.
    /// </summary>
    public class ActionMenuItemBuilder : MenuItemBuilder<ActionMenuItemBuilder, ActionMenuItem>
    {
        protected override ActionMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override ActionMenuItemBuilder WithExisting(ActionMenuItem item)
        {
            return base.WithExisting(item);
        }

        public override ActionMenuItem Build()
        {
            return new ActionMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, StaticInRam);
        }
    }
}
