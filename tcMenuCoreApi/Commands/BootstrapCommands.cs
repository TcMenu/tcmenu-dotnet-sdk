using System;
using System.Collections.Generic;
using System.Text;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdk.Commands
{
    /// <summary>
    /// All bootstrap commands share a common base, as their purpose is similar. There is always
    /// a menu item to be created in a given submenu, with its current value. Although the subtypes
    /// may seem pointless, they are to allow for strong typing when dealing with these commands.
    /// </summary>
    /// <typeparam name="T">The menu item type</typeparam>
    /// <typeparam name="V">the associated value</typeparam>
    public abstract class BootstrapMenuCommand<T, V> : MenuCommand where T : MenuItem
    {
        /// <summary>
        /// This proprerty represents the submenu ID of the item
        /// </summary>
        public int SubMenuId { get;  }

        /// <summary>
        /// The menu item itself to be updated or added to the tree.
        /// </summary>
        public T Item { get;  }

        /// <summary>
        /// The current value.
        /// </summary>
        public V CurrentValue { get; }

        public BootstrapMenuCommand(int subMenuId, T menuItem, V value, ushort msgId) : base(msgId)
        {
            SubMenuId = subMenuId;
            Item = menuItem;
            CurrentValue = value;
        }

        /// <summary>
        /// Creates a new menu state based on the previous one (null accepted as previous), with the
        /// change and active flags set as appropriate 
        /// </summary>
        /// <param name="previousState">the previous state or null</param>
        /// <returns>a new menu state with the latest values</returns>
        public MenuState<V> NewMenuState(AnyMenuState previousState)
        {
            var prev = previousState as MenuState<V> ?? new MenuState<V>(Item, false, false, CurrentValue);
            bool same = CurrentValue?.Equals(prev.Value) ?? false;
            bool active = prev.Active;
            return new MenuState<V>(Item, !same, active, CurrentValue);
        }

        public override string ToString()
        {
            return $"{GetType().Name}[Item={Item}, Value={CurrentValue}]";
        }

    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// analog menu items.
    /// </summary>
    public class AnalogBootstrapCommand : BootstrapMenuCommand<AnalogMenuItem, int>
    {
        public static readonly ushort ANALOG_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'A');

        public AnalogBootstrapCommand(int subMenuId, AnalogMenuItem item, int currentValue) :
                    base(subMenuId, item, currentValue, ANALOG_BOOT_CMD) { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// enumeration menu items.
    /// </summary>
    public class EnumBootstrapCommand : BootstrapMenuCommand<EnumMenuItem, int>
    {
        public static readonly ushort ENUM_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'E');

        public EnumBootstrapCommand(int subMenuId, EnumMenuItem item, int currentValue) :
                    base(subMenuId, item, currentValue, ENUM_BOOT_CMD) { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// sub menu items.
    /// </summary>
    public class SubMenuBootstrapCommand : BootstrapMenuCommand<SubMenuItem, bool>
    {
        public static readonly ushort SUBMENU_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'M');

        public SubMenuBootstrapCommand(int subMenuId, SubMenuItem item, bool currentValue) :
                    base(subMenuId, item, currentValue, SUBMENU_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// action menu items.
    /// </summary>
    public class ActionBootstrapCommand : BootstrapMenuCommand<ActionMenuItem, bool>
    {
        public static readonly ushort ACTION_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'C');

        public ActionBootstrapCommand(int subMenuId, ActionMenuItem item, bool currentValue) :
                    base(subMenuId, item, currentValue, ACTION_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// boolean menu items.
    /// </summary>
    public class BooleanBootstrapCommand : BootstrapMenuCommand<BooleanMenuItem, bool>
    {
        public static readonly ushort BOOLEAN_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'B');

        public BooleanBootstrapCommand(int subMenuId, BooleanMenuItem item, bool currentValue) :
                    base(subMenuId, item, currentValue, BOOLEAN_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// float menu items.
    /// </summary>
    public class FloatBootstrapCommand : BootstrapMenuCommand<FloatMenuItem, float>
    {
        public static readonly ushort FLOAT_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'F');

        public FloatBootstrapCommand(int subMenuId, FloatMenuItem item, float currentValue) :
                    base(subMenuId, item, currentValue, FLOAT_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// editable large numbers.
    /// </summary>
    public class LargeNumberBootstrapCommand : BootstrapMenuCommand<LargeNumberMenuItem, decimal>
    {
        public static readonly ushort DECIMAL_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'N');

        public LargeNumberBootstrapCommand(int subMenuId, LargeNumberMenuItem item, decimal currentValue) :
                    base(subMenuId, item, currentValue, DECIMAL_BOOT_CMD)
        { }

        public static decimal StrToDecimalSafe(string rawStr)
        {
            rawStr = rawStr.Replace("[", "");
            rawStr = rawStr.Replace("]", "");
            return string.IsNullOrEmpty(rawStr) ? 0 : decimal.Parse(rawStr);
        }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// text menu items.
    /// </summary>
    public class TextBootstrapCommand : BootstrapMenuCommand<EditableTextMenuItem, string>
    {
        public static readonly ushort TEXT_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'T');

        public TextBootstrapCommand(int subMenuId, EditableTextMenuItem item, string currentValue) :
                    base(subMenuId, item, currentValue, TEXT_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenu for RGB types
    /// </summary>
    public class Rgb32BootstrapCommand : BootstrapMenuCommand<Rgb32MenuItem, PortableColor>
    {
        public static readonly ushort RGB32_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'K');

        public Rgb32BootstrapCommand(int subMenuId, Rgb32MenuItem item, PortableColor currentValue) :
            base(subMenuId, item, currentValue, RGB32_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenu for scroll choice types
    /// </summary>
    public class ScrollChoiceBootstrapCommand : BootstrapMenuCommand<ScrollChoiceMenuItem, CurrentScrollPosition>
    {
        public static readonly ushort SCROLL_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'Z');

        public ScrollChoiceBootstrapCommand(int subMenuId, ScrollChoiceMenuItem item, CurrentScrollPosition currentValue) :
            base(subMenuId, item, currentValue, SCROLL_BOOT_CMD)
        { }
    }

    /// <summary>
    /// An override of the BootstrapMenuCommand type that provides the functionality for
    /// action menu items.
    /// </summary>
    public class RuntimeListBootstrapCommand : BootstrapMenuCommand<RuntimeListMenuItem, List<string>>
    {
        public static readonly ushort LIST_BOOT_CMD = MenuCommand.MakeCmdPair('B', 'L');

        public RuntimeListBootstrapCommand(int subMenuId, RuntimeListMenuItem item, List<string> currentValue) :
                    base(subMenuId, item, currentValue, LIST_BOOT_CMD)
        { }
    }
}
