using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.Protocol;

namespace TcMenu.CoreSdk.MenuItems
{
    public class LargeNumberMenuItem : MenuItem
    {
        public int DecimalPlaces { get; }
        public int TotalDigits { get; }
        public bool AllowNegative { get; }

        public LargeNumberMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible,
                                   int decimalPlaces, int totalDigits, bool allowNegative, bool staticInRam) 
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            DecimalPlaces = decimalPlaces;
            TotalDigits = totalDigits;
            AllowNegative = allowNegative;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return obj is LargeNumberMenuItem item &&
                   base.Equals(obj) &&
                   DecimalPlaces == item.DecimalPlaces &&
                   TotalDigits == item.TotalDigits &&
                   AllowNegative == item.AllowNegative;
        }

        public override int GetHashCode()
        {
            var hashCode = 1269443129;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DecimalPlaces.GetHashCode();
            hashCode = hashCode * -1521134295 + AllowNegative.GetHashCode();
            hashCode = hashCode * -1521134295 + TotalDigits.GetHashCode();
            return hashCode;
        }
    }

    public class LargeNumberMenuItemBuilder : MenuItemBuilder<LargeNumberMenuItemBuilder, LargeNumberMenuItem>
    {
        private int _decimalPlaces;
        private int _totalDigits;
        private bool _allowNegative = true;

        public LargeNumberMenuItemBuilder WithDecimalPlaces(int dp)
        {
            _decimalPlaces = dp;
            return this;
        }

        public LargeNumberMenuItemBuilder WithTotalDigits(int digits)
        {
            _totalDigits = digits;
            return this;
        }

        public LargeNumberMenuItemBuilder WithAllowNegative(bool allowNegative)
        {
            _allowNegative = allowNegative;
            return this;
        }
        
        public override LargeNumberMenuItemBuilder WithExisting(LargeNumberMenuItem menuItem)
        {
            _totalDigits = menuItem.TotalDigits;
            _decimalPlaces = menuItem.DecimalPlaces;
            _allowNegative = menuItem.AllowNegative;
            return base.WithExisting(menuItem);
        }

        public override LargeNumberMenuItem Build()
        {
            return new LargeNumberMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, _decimalPlaces, _totalDigits, _allowNegative, StaticInRam);
        }

        protected override LargeNumberMenuItemBuilder GetThis()
        {
            return this;
        }
    }
}
