using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    /// A menu item that can only hold boolean values(true or false). The naming can be changed such that the boolean can
    /// be represented with different text. Rather than using the constructor use the BooleanMenuItemBuilder to build one.
    /// </summary>
    public class FloatMenuItem : MenuItem
    {
        public int DecimalPlaces { get; }

        public FloatMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible, 
                            int decPlaces, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            DecimalPlaces = decPlaces;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            var item = obj as FloatMenuItem;
            return item != null &&
                   base.Equals(obj) &&
                   DecimalPlaces == item.DecimalPlaces;
        }

        public override int GetHashCode()
        {
            var hashCode = 2025334034;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DecimalPlaces.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Constructs an instance of a boolean menu item using the builder pattern.
    /// </summary>
    public class FloatMenuItemBuilder : MenuItemBuilder<FloatMenuItemBuilder, FloatMenuItem>
    {
        private int DecimalPlaces;

        protected override FloatMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Set the naming to be used for this item
        /// </summary>
        /// <param name="naming">The type of naming to be used</param>
        /// <returns>Itself for chaining</returns>
        public FloatMenuItemBuilder WithDecimalPlaces(int dp)
        {
            DecimalPlaces = dp;
            return this;
        }

        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override FloatMenuItemBuilder WithExisting(FloatMenuItem item)
        {
            base.WithExisting(item);
            DecimalPlaces = item.DecimalPlaces;
            return this;
        }

        public override FloatMenuItem Build()
        {
            return new FloatMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, DecimalPlaces, StaticInRam);
        }
    }
}
