using System.Globalization;

namespace TcMenu.CoreSdk.MenuItems
{
    public enum ScrollChoiceMode { ARRAY_IN_EEPROM = 0, ARRAY_IN_RAM = 1, CUSTOM_RENDERFN = 2 }
    
    /// <summary>
    /// Represents a choice menu item that is more configurable than the enum menu item. Unlike enum item, scroll
    /// items can be changed at runtime, both in length and in size. There is no attempt in the API to cache the
    /// possible value ranges, instead just the current text value and position are stored.
    /// </summary>
    public class ScrollChoiceMenuItem : MenuItem
    {
        public int ItemWidth { get; }
        public int EepromOffset { get; }
        public string RamVariable { get; }
        public ScrollChoiceMode ChoiceMode { get; }
        public int NumEntries { get; } // in designer, we just fill the list with item0..itemN to represent the list size

        public ScrollChoiceMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, 
            bool localOnly, bool visible, int entries, int itemWidth, int eepromOffset, string ramVariable,
            ScrollChoiceMode mode, bool staticInRam) 
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            NumEntries = entries;
            ItemWidth = itemWidth;
            EepromOffset = eepromOffset;
            RamVariable = ramVariable;
            ChoiceMode = mode;
        }
        
        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected bool Equals(ScrollChoiceMenuItem other)
        {
            return base.Equals(other) && ItemWidth == other.ItemWidth &&
                   NumEntries == other.NumEntries &&
                   EepromOffset == other.EepromOffset && 
                   RamVariable == other.RamVariable && 
                   ChoiceMode == other.ChoiceMode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ScrollChoiceMenuItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ NumEntries;
                hashCode = (hashCode * 397) ^ ItemWidth;
                hashCode = (hashCode * 397) ^ EepromOffset;
                hashCode = (hashCode * 397) ^ (RamVariable != null ? RamVariable.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) ChoiceMode;
                return hashCode;
            }
        }
    }
    
    public class ScrollChoiceMenuItemBuilder : MenuItemBuilder<ScrollChoiceMenuItemBuilder, ScrollChoiceMenuItem>
    {
        private int _itemWidth  = 10;
        private int _eepromOffset;
        private string _ramVariable;
        private ScrollChoiceMode _choiceMode;
        private int _numEntries;

        public ScrollChoiceMenuItemBuilder WithChoiceMode(ScrollChoiceMode mode)
        {
            _choiceMode = mode;
            return this;
        }
        
        public ScrollChoiceMenuItemBuilder WithItemWidth(int itemWidth)
        {
            _itemWidth = itemWidth;
            return this;
        }
        
        public ScrollChoiceMenuItemBuilder WithNumEntries(int size)
        {
            _numEntries = size;
            return this;
        }
        
        public ScrollChoiceMenuItemBuilder WithEepromOffset(int eepromOffset)
        {
            _eepromOffset = eepromOffset;
            return this;
        }

        public ScrollChoiceMenuItemBuilder WithRamVariable(string varName)
        {
            _ramVariable = varName;
            return this;
        }

        public override ScrollChoiceMenuItemBuilder WithExisting(ScrollChoiceMenuItem menuItem)
        {
            _itemWidth = menuItem.ItemWidth;
            _numEntries = menuItem.NumEntries;
            _eepromOffset = menuItem.EepromOffset;
            _choiceMode = menuItem.ChoiceMode;
            _ramVariable = menuItem.RamVariable;
            return base.WithExisting(menuItem);
        }

        public override ScrollChoiceMenuItem Build()
        {
            return new ScrollChoiceMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, _numEntries,
                                            _itemWidth, _eepromOffset, _ramVariable, _choiceMode, StaticInRam);
        }

        protected override ScrollChoiceMenuItemBuilder GetThis()
        {
            return this;
        }
    }
}