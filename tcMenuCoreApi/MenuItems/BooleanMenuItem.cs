using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    public enum BooleanNaming
    {
        TRUE_FALSE = 0, ON_OFF = 1, YES_NO = 2, CHECKBOX = 3
    }

    /// <summary>
    /// A menu item that can only hold boolean values(true or false). The naming can be changed such that the boolean can
    /// be represented with different text. Rather than using the constructor use the BooleanMenuItemBuilder to build one.
    /// </summary>
    public class BooleanMenuItem : MenuItem
    {
        public BooleanNaming Naming { get; }

        public BooleanMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible, BooleanNaming naming, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            Naming = naming;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            var item = obj as BooleanMenuItem;
            return item != null && base.Equals(item) &&
                   Naming == item.Naming;
        }

        public override int GetHashCode()
        {
            return 1233279961 + Naming.GetHashCode() + base.GetHashCode();
        }
    }

    /// <summary>
    /// Constructs an instance of a boolean menu item using the builder pattern.
    /// </summary>
    public class BooleanMenuItemBuilder : MenuItemBuilder<BooleanMenuItemBuilder, BooleanMenuItem>
    {
        private BooleanNaming Naming = BooleanNaming.TRUE_FALSE;

        protected override BooleanMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Set the naming to be used for this item
        /// </summary>
        /// <param name="naming">The type of naming to be used</param>
        /// <returns>Itself for chaining</returns>
        public BooleanMenuItemBuilder WithNaming(BooleanNaming naming)
        {
            Naming = naming;
            return this;
        }

        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override BooleanMenuItemBuilder WithExisting(BooleanMenuItem item)
        {
            base.WithExisting(item);
            Naming = item.Naming;
            return this;
        }

        public override BooleanMenuItem Build()
        {
            return new BooleanMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, Naming, StaticInRam);
        }
    }
}
