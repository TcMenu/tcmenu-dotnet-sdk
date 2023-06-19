using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    public enum ListCreationMode
    {
        CustomRtCall, RamArray, FlashArray
    }

    /// <summary>
    /// A menu item that purely represents a function that can be run on the embedded device. It holds no state of its own.
    /// </summary>
    public class RuntimeListMenuItem : MenuItem
    {
        public int InitialRows { get; }
        public ListCreationMode CreationMode { get; }

        public RuntimeListMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible, int rows, 
                                   ListCreationMode creationMode, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            CreationMode = creationMode;
            InitialRows = rows;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            var item = obj as RuntimeListMenuItem;
            return item != null &&
                   base.Equals(obj) &&
                   InitialRows == item.InitialRows;
        }

        public override int GetHashCode()
        {
            var hashCode = -1840374876;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + InitialRows.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Constructs an instance of an action menu item using the builder pattern.
    /// </summary>
    public class RuntimeListMenuItemBuilder : MenuItemBuilder<RuntimeListMenuItemBuilder, RuntimeListMenuItem>
    {
        private int _initialRows;
        private ListCreationMode _creationMode;

        protected override RuntimeListMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override RuntimeListMenuItemBuilder WithExisting(RuntimeListMenuItem item)
        {
            _creationMode = item.CreationMode;
            _initialRows = item.InitialRows;
            return base.WithExisting(item);
        }

        public RuntimeListMenuItemBuilder WithInitialRows(int rows)
        {
            _initialRows = rows;
            return this;
        }

        public RuntimeListMenuItemBuilder WithCreationMode(ListCreationMode creationMode)
        {
            _creationMode = creationMode;
            return this;
        }

        public override RuntimeListMenuItem Build()
        {
            return new RuntimeListMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, 
                _initialRows, _creationMode, StaticInRam);
        }
    }
}
