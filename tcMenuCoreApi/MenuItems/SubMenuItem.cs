using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    /// A menu item that purely represents a function that can be run on the embedded device. It holds no state of its own.
    /// </summary>
    public class SubMenuItem : MenuItem
    {
        public bool Secured { get; }

        public SubMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool secured, bool visible, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            Secured = secured;
        }

        public override bool HasChildren()
        {
            return true;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected bool Equals(SubMenuItem other)
        {
            return base.Equals(other) && Secured == other.Secured;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubMenuItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Secured.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Constructs an instance of an action menu item using the builder pattern.
    /// </summary>
    public class SubMenuItemBuilder : MenuItemBuilder<SubMenuItemBuilder, SubMenuItem>
    {
        private bool _secured;
        
        protected override SubMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override SubMenuItemBuilder WithExisting(SubMenuItem item)
        {
            _secured = item.Secured;
            return base.WithExisting(item);
        }

        public SubMenuItemBuilder WithSecured(bool secured)
        {
            _secured = secured;
            return this;
        }

        public override SubMenuItem Build()
        {
            return new SubMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, _secured, Visible, StaticInRam);
        }
    }
}
