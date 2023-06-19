using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    public enum EditItemType
    {
        PLAIN_TEXT = 0, IP_ADDRESS = 1, TIME_24H = 2, TIME_12H = 3, TIME_24_HUNDREDS = 4, GREGORIAN_DATE = 5,
        TIME_DURATION_SECONDS = 6, TIME_DURATION_HUNDREDS = 7, TIME_24H_HHMM = 8, TIME_12H_HHMM = 9
    }

    /// <summary>
    /// A menu item that can only hold boolean values(true or false). The naming can be changed such that the boolean can
    /// be represented with different text. Rather than using the constructor use the BooleanMenuItemBuilder to build one.
    /// </summary>
    public class EditableTextMenuItem : MenuItem
    {
        public EditItemType EditType { get; }
        public int TextLength { get; }

        public EditableTextMenuItem(string name, string varName, int id, int eepromAddress, string functionName, bool readOnly, bool localOnly, bool visible, 
                                    EditItemType editType, int textLen, bool staticInRam)
            : base(name, varName, id, eepromAddress, functionName, readOnly, localOnly, visible, staticInRam)
        {
            EditType = editType;
            TextLength = textLen;
        }

        public override void Accept(MenuItemVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            var item = obj as EditableTextMenuItem;
            return item != null &&
                   base.Equals(obj) &&
                   EditType == item.EditType &&
                   TextLength == item.TextLength;
        }

        public override int GetHashCode()
        {
            var hashCode = -865913919;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EditType.GetHashCode();
            hashCode = hashCode * -1521134295 + TextLength.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Constructs an instance of a boolean menu item using the builder pattern.
    /// </summary>
    public class EditableTextMenuItemBuilder : MenuItemBuilder<EditableTextMenuItemBuilder, EditableTextMenuItem>
    {
        private EditItemType EditType = EditItemType.PLAIN_TEXT;
        private int TextLength = 10;

        protected override EditableTextMenuItemBuilder GetThis()
        {
            return this;
        }

        /// <summary>
        /// Set the edit type to be used for this item
        /// </summary>
        /// <param name="naming">The type of editor to be used</param>
        /// <returns>Itself for chaining</returns>
        public EditableTextMenuItemBuilder WithEditType(EditItemType editType)
        {
            EditType = editType;
            return this;
        }

        /// <summary>
        /// Sets the length of the text, where the editor supports more than one.
        /// </summary>
        /// <param name="len">the maximum length of the text</param>
        /// <returns>itself</returns>
        public EditableTextMenuItemBuilder WithTextLength(int len)
        {
            TextLength = len;
            return this;
        }

        /// <summary>
        /// Sets all the builder fields from the existing object
        /// </summary>
        /// <param name="item">object to be copied</param>
        /// <returns>itself for chaining</returns>
        public override EditableTextMenuItemBuilder WithExisting(EditableTextMenuItem item)
        {
            base.WithExisting(item);
            EditType = item.EditType;
            TextLength = item.TextLength;
            return this;
        }

        public override EditableTextMenuItem Build()
        {
            return new EditableTextMenuItem(Name, VariableName, Id, EepromAddress, FunctionName, ReadOnly, LocalOnly, Visible, EditType, TextLength, StaticInRam);
        }
    }
}
