using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Serilog;
using TcMenu.CoreSdk.Serialisation;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    /// A series of helper methods for menu items
    /// </summary>
    public static class MenuItemHelper
    {
        /// <summary>
        /// provides a means to visit menu items and get a result back from the visit
        /// </summary>
        /// <typeparam name="T">The return type required</typeparam>
        /// <param name="item">The item to be visited</param>
        /// <param name="visitor">The class extending from AbstractMenuItemVisitor</param>
        /// <returns>The last item stored in Result</returns>
        public static T VisitWithResult<T>(MenuItem item, AbstractMenuItemVisitor<T> visitor)
        {
            item.Accept(visitor);
            return visitor.Result;
        }

        /// <summary>
        /// Find the next available menu ID that's available.
        /// </summary>
        /// <param name="tree">the tree to evaluate</param>
        /// <returns>A non-conflicting menu id</returns>    
        public static int FindAvailableMenuId(MenuTree tree)
        {
            return tree.GetAllMenuItems()
                .Select(item => item.Id)
                .Max() + 1;
        }

        /// <summary>
        /// Finds the next available EEPROM location
        /// </summary>
        /// <param name="tree">the tree to evaluate</param>
        /// <returns>an available eeprom location</returns>
        public static int FindAvailableEEPROMLocation(MenuTree tree)
        {
            var loc = tree.GetAllMenuItems()
                .Where(item => item.EepromAddress != -1)
                .Select(item => item.EepromAddress + GetEEPROMStorageRequirement(item))
                .DefaultIfEmpty(2)
                .Max();
            return loc;
        }

        /// <summary>
        /// Returns the amount of EEPROM storage needed for the menu item
        /// </summary>
        /// <param name="item">the item to find the size of</param>
        /// <returns>the size as an int</returns>
        public static int GetEEPROMStorageRequirement(MenuItem item)
        {
            switch (item)
            {
                case AnalogMenuItem _: return 2;
                case BooleanMenuItem _: return 1;
                case EnumMenuItem _: return 2;
                case LargeNumberMenuItem _: return 8;
                case Rgb32MenuItem _: return 4;
                case ScrollChoiceMenuItem _: return 2;
                case EditableTextMenuItem txt:
                    if (txt.EditType == EditItemType.IP_ADDRESS) return 4;
                    else if (txt.EditType == EditItemType.PLAIN_TEXT) return txt.TextLength;
                    else return 4; // time always 4
                default: return 0;
            }
        }

        public static MenuItem CreateFromExistingWithNewId(MenuItem existing, int id)
        {
            switch (existing)
            {
                case AnalogMenuItem i:
                    return new AnalogMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case EnumMenuItem i:
                    return new EnumMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case BooleanMenuItem i:
                    return new BooleanMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case ActionMenuItem i:
                    return new ActionMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case SubMenuItem i:
                    return new SubMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case FloatMenuItem i:
                    return new FloatMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case EditableTextMenuItem i:
                    return new EditableTextMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case LargeNumberMenuItem i:
                    return new LargeNumberMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case RuntimeListMenuItem i:
                    return new RuntimeListMenuItemBuilder().WithExisting(i).WithId(id).WithEepromLocation(-1).Build();
                case Rgb32MenuItem rgb:
                    return new Rgb32MenuItemBuilder().WithExisting(rgb).WithId(id).WithEepromLocation(-1).Build();
                case ScrollChoiceMenuItem sc:
                    return new ScrollChoiceMenuItemBuilder().WithExisting(sc).WithId(id).WithEepromLocation(-1).Build();
                default: return null;
            }
        }


        /// <summary>
        /// Get the value from the tree or put the default into the tree if empty
        /// </summary>
        /// <typeparam name="T">the type is inferred from the default value</typeparam>
        /// <param name="item">the item to lookup the value for</param>
        /// <param name="tree">the tree</param>
        /// <param name="defVal">the default value (defines the returned type)</param>
        /// <returns>the value from the tree, or the default</returns>
        public static T GetValueFor<T>(MenuItem item, MenuTree tree, T defVal)
        {
            if (tree.GetState(item) != null)
            {
                try
                {
                    return (T)tree.GetState(item).ValueAsObject();
                }
                catch (Exception e)
                {
                    Log.Logger.Information(e, "State type incorrect");
                }
            }
            tree.ChangeItemState(item, MenuItemHelper.StateForMenuItem(item, defVal, false, false));
            return defVal;
        }

        /// <summary>
        /// This gets the value from the tree state, if it is not available calls getDefaultValue
        /// Same as getValueFor(item, tree, defVal) but this just calls getDefaultFor(..) to get the default.
        /// </summary>
        /// <param name="item">the item to get the state of</param>
        /// <param name="tree">the tree holding the state</param>
        /// <returns>the items current value or the default</returns>
        public static object GetValueFor(MenuItem item, MenuTree tree)
        {
            return GetValueFor(item, tree, GetDefaultFor(item));

        }

        /// <summary>
        ///  Gets the default item value for a menu item, such that the value could be used in call to set state.
        /// </summary>
        /// <param name="item">the item to get the default for</param>
        /// <returns>the default value for that type</returns>
        public static object GetDefaultFor(MenuItem item)
        {
            switch (item)
            {
                case AnalogMenuItem _:
                case EnumMenuItem _:
                    return 0;
                case FloatMenuItem _:
                    return 0.0F;
                case BooleanMenuItem _:
                case SubMenuItem _:
                case ActionMenuItem _:
                    return false;
                case LargeNumberMenuItem _:
                    return 0M;
                case EditableTextMenuItem _:
                    return "";
                case Rgb32MenuItem _:
                    return new PortableColor(0, 0, 0);
                case RuntimeListMenuItem _:
                    return new List<string>();
                case ScrollChoiceMenuItem _:
                    return new CurrentScrollPosition(0, "");
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try and apply an incremental delta value update to a menu tree. This works for integer, enum and scroll items,
        /// it loads the existing value and tries to apply the delta offset, if the min/max would not be exceeded.
        /// </summary>
        /// <param name="item">the item to apply the change to</param>
        /// <param name="delta">the amount to change by</param>
        /// <param name="tree">the tree for the item</param>
        /// <returns>a new item if the operation was possible or null</returns>
        public static AnyMenuState ApplyIncrementalValueChange(MenuItem item, int delta, MenuTree tree)
        {
            var state = tree.GetState(item) ?? MenuItemHelper.StateForMenuItem(item, 0, false, false);

            if (state.GetType().GenericTypeArguments[0] == typeof(int))
            {
                var intState = (MenuState<int>)state;
                var val = intState.Value + delta;

                if (val < 0 || (item is AnalogMenuItem am && val > am.MaximumValue) ||
                        (item is EnumMenuItem em && val > em.EnumEntries.Count)) {
                    return null;
                }

                AnyMenuState menuState = StateForMenuItem(intState, item, intState.Value + delta);
                tree.ChangeItemState(item, menuState);
                return menuState;
            }
            else if(state.GetType().GenericTypeArguments[0] == typeof(CurrentScrollPosition))
            {
                var scrState = (MenuState<CurrentScrollPosition>)state;
                var val = scrState.Value.Position + delta;
                if (val <= 0 || (item is ScrollChoiceMenuItem sci && val >= sci.NumEntries)) {
                    return null;
                }
                var currentScrollPosition = new CurrentScrollPosition(scrState.Value.Position + delta, "");
                AnyMenuState menuState = StateForMenuItem(scrState, item, currentScrollPosition);
                tree.ChangeItemState(item, menuState);
                return menuState;
            }
            return null;
        }

        /// <summary>
        /// Set the state in the tree for an item with a new value, setting it changed if it genuinely has.
        /// </summary>
        /// <param name="item">the item</param>
        /// <param name="value">the value to replace</param>
        /// <param name="tree">the tree in which to set</param>
        public static void SetMenuState(MenuItem item, object value, MenuTree tree)
        {
            var oldState = tree.GetState(item);
            if (oldState != null)
            {
                tree.ChangeItemState(item,
                    StateForMenuItem(item, value, !value.Equals(oldState.ValueAsObject()), oldState.Active));
            }
            else
            {
                tree.ChangeItemState(item, StateForMenuItem(item, value, false, false));
            }
        }

        public static AnyMenuState StateForMenuItem(AnyMenuState existingState, MenuItem item, object val)
        {
            bool changed = false;
            bool active = false;
            if (existingState != null)
            {
                changed = existingState.Changed;
                active = existingState.Active;
            }
            return StateForMenuItem(item, val, changed, active);
        }

        public static AnyMenuState StateForMenuItem(MenuItem item, object v, bool changed, bool active)
        {
            if (item == null)
            {
                return new MenuState<bool>(item, false, false, false);
            }

            var val = v ?? GetDefaultFor(item);

            if (item is AnalogMenuItem analog)
            {
                int res = (val is string) ? int.Parse(val.ToString()) : ((int)val);
                if (res < 0) res = 0;
                if (res > analog.MaximumValue) res = analog.MaximumValue;
                return new MenuState<int>(item, changed, active, res);
            }
            else if (item is BooleanMenuItem bi)
            {
                bool ret = false;
                if (val is string s)
                {
                    if (s.Length == 1)
                    {
                        ret = s[0] == '1' || s[0] == 'Y';
                    }
                    else
                    {
                        ret = bool.Parse(s);
                    }
                }
                else if (val is int i)
                {
                    ret = i != 0;
                }
                else
                {
                    ret = (bool)val;
                }

                return new MenuState<bool>(item, changed, active, ret);
            }
            else if (item is EnumMenuItem en)
            {
                int res = (val is string) ? int.Parse(val.ToString()) : (int)val;
                if (res < 0) res = 0;
                if (res >= en.EnumEntries.Count) res = en.EnumEntries.Count - 1;
                return new MenuState<int>(item, changed, active, res);
            }
            else if (item is SubMenuItem sm)
            {
                return new MenuState<bool>(item, changed, active, false);
            }
            else if (item is EditableTextMenuItem ed)
            {
                return (new MenuState<string>(item, changed, active, val.ToString()));
            }
            else if (item is ActionMenuItem act)
            {
                return (new MenuState<bool>(item, changed, active, false));
            }
            else if (item is FloatMenuItem flt)
            {
                float res = (val is string s) ? float.Parse(val.ToString()) : (val is double d) ? Convert.ToSingle(d) : (float)val;
                return (new MenuState<float>(item, changed, active, res));
            }
            else if (item is RuntimeListMenuItem lst)
            {
                return (new MenuState<List<string>>(item, changed, active, (List<string>)val));

            }
            else if (item is LargeNumberMenuItem numItem)
            {
                decimal dec = (val is string) ? decimal.Parse(val.ToString()) : (decimal)val;
                return (new MenuState<decimal>(item, changed, active, dec));
            }
            else if (item is ScrollChoiceMenuItem scrollItem)
            {
                CurrentScrollPosition pos;
                if (val is int i) pos = new CurrentScrollPosition(i, "");
                else if (val is CurrentScrollPosition position) pos = position;
                else pos = new CurrentScrollPosition(val.ToString());
                if (pos.Position >= 0 && pos.Position < scrollItem.NumEntries)
                {
                    return new MenuState<CurrentScrollPosition>(item, changed, active, pos);
                }

                return (new MenuState<CurrentScrollPosition>(scrollItem, changed, active,
                    new CurrentScrollPosition(0, "No entries")));
            }
            else if (item is Rgb32MenuItem)
            {
                PortableColor res = (val is string s) ? new PortableColor(s) : (PortableColor)val;
                return new MenuState<PortableColor>(item, changed, active, res);
            }
            else
            {
                return new MenuState<bool>(item, changed, active, false);
            }
        }
    }
}
