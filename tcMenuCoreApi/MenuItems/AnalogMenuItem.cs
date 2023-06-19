using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    ///  Represents an analog (numeric) menu item, it is always a zero based integer when retrieved from storage, but it can
    ///  have an offset and divisor, so therefore is able to represent decimal values. The offset can also be negative.
    ///  Rather than directly constructing an item of this type, you can use the AnalogMenuItemBuilder.
    /// </summary>
    public class AnalogMenuItem : MenuItem
    {
        public int MaximumValue { get; }
        public int Offset { get; }
        public int Divisor { get; }
        public string UnitName { get; }
        public int Step { get; }

        public AnalogMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible,
                              int maxValue, int offset, int divisor, int step, string unit, bool staticInRam) 
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            MaximumValue = maxValue;
            Divisor = divisor;
            Offset = offset;
            UnitName = unit;
            Step = step;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            var item = obj as AnalogMenuItem;
            return item != null && base.Equals(obj) &&
                   MaximumValue == item.MaximumValue &&
                   Offset == item.Offset &&
                   Divisor == item.Divisor &&
                   UnitName == item.UnitName;
        }

        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();
            hashCode = hashCode * -1521134295 + MaximumValue.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            hashCode = hashCode * -1521134295 + Divisor.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UnitName);
            return hashCode;
        }
    }

    public class AnalogMenuItemBuilder : MenuItemBuilder<AnalogMenuItemBuilder, AnalogMenuItem>
    {
        private int MaxValue;
        private int Offset;
        private int Divisor;
        private string UnitName;
        private int Step;

        /// <summary>
        /// Sets the maximum value for the item
        /// </summary>
        /// <param name="max">the maximum value</param>
        /// <returns>itself</returns>
        public AnalogMenuItemBuilder WithMaxValue(int max)
        {
            MaxValue = max;
            return this;
        }

        /// <summary>
        /// Sets the step for this analog item
        /// </summary>
        /// <param name="step">the new step value</param>
        /// <returns>itself</returns>
        public AnalogMenuItemBuilder WithStep(int step)
        {
            Step = step;
            return this;
        }

        /// <summary>
        /// Sets the offset for this item
        /// </summary>
        /// <param name="offset">the offset</param>
        /// <returns>itself</returns>
        public AnalogMenuItemBuilder WithOffset(int offset)
        {
            Offset = offset;
            return this;
        }

        /// <summary>
        /// Sets the divisor for ths item
        /// </summary>
        /// <param name="divisor">the divisor</param>
        /// <returns>itself</returns>
        public AnalogMenuItemBuilder WithDivisor(int divisor)
        {
            Divisor = divisor;
            return this;
        }

        /// <summary>
        /// Sets the unit name of this item
        /// </summary>
        /// <param name="unitName">The unit name</param>
        /// <returns>itself</returns>
        public AnalogMenuItemBuilder WithUnitName(string unitName)
        {
            UnitName = unitName;
            return this;
        }

        public override AnalogMenuItemBuilder WithExisting(AnalogMenuItem item)
        {
            MaxValue = item.MaximumValue;
            Offset = item.Offset;
            Divisor = item.Divisor;
            UnitName = item.UnitName;
            Step = item.Step;
            return base.WithExisting(item);
        }

        protected override AnalogMenuItemBuilder GetThis()
        {
            return this;
        }

        public override AnalogMenuItem Build()
        {
            return new AnalogMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, MaxValue, Offset, Divisor, Step, UnitName, StaticInRam);
        }
    }
}
