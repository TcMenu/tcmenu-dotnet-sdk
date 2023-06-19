using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    /// A menu item that can hold an enumeration of values, and a current choice in the enumeration
    /// </summary>
    public class EnumMenuItem : MenuItem
    {
        public List<string> EnumEntries { get; }

        public EnumMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, 
                            bool visible, List<string> entries, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            EnumEntries = entries;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            var item = obj as EnumMenuItem;
            if (!(item != null && base.Equals(obj))) return false;

            if (item.EnumEntries == null || EnumEntries == null) return false;

            bool same = false;
            // item counts must match to be equal - short cut
            if(item.EnumEntries.Count == EnumEntries.Count)
            {
                same = true;
                foreach(var entry in EnumEntries)
                {
                    // every entry in this list must be in the other.
                    same = same && item.EnumEntries.Contains(entry);
                }
            }
            return same;
        }

        public override int GetHashCode()
        {
            var hashCode = -1923258634;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(EnumEntries);
            return hashCode;
        }
    }

    /// <summary>
    /// Constructs an instance of an enum menu item using the builder pattern.
    /// </summary>
    public class EnumMenuItemBuilder : MenuItemBuilder<EnumMenuItemBuilder, EnumMenuItem>
    {
        private List<string> EntryList = new List<string>();

        protected override EnumMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Add a new entry to the list of entries.
        /// </summary>
        /// <param name="entry">The new entry to add</param>
        /// <returns>Itself for chaining</returns>
        public EnumMenuItemBuilder WithNewEntry(string entry)
        {
            EntryList.Add(entry);
            return this;
        }

        /// <summary>
        /// Clears the entry list
        /// </summary>
        /// <returns>itself</returns>
        public EnumMenuItemBuilder ClearEntryList()
        {
            EntryList.Clear();
            return this;
        }

        /// <summary>
        /// Change the entry list to the provided one.
        /// </summary>
        /// <param name="list"></param>
        public EnumMenuItemBuilder WithEntries(List<string> list)
        {
            EntryList = list;
            return this;
        }
        
        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override EnumMenuItemBuilder WithExisting(EnumMenuItem item)
        {
            base.WithExisting(item);
            EntryList = item.EnumEntries;
            return this;
        }

        public override EnumMenuItem Build()
        {
            return new EnumMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, EntryList, StaticInRam);
        }
    }
}
