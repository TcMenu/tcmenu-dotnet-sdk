using System.Collections.Generic;

namespace TcMenu.CoreSdk.MenuItems
{
    public interface AnyMenuState
    {
        object ValueAsObject();
        bool Changed { get; }
        bool Active { get; }
        MenuItem Item { get; }
    }

    public class MenuState<T> : AnyMenuState
    {
        public bool Changed { get; }
        public bool Active { get; }
        public MenuItem Item { get; }
        public T Value { get; }

         /// <summary>
         /// normally these states are created from the menu item, instead of directly
         /// </summary>
         /// <param name="active">if the item is active</param>
         /// <param name="item">the associated item</param>
         /// <param name="changed">if the value has changed</param>
         /// <param name="value">The current value associated with the menu item</param>
        public MenuState(MenuItem item, bool changed, bool active, T value)
        {
            Changed = changed;
            Active = active;
            Value = value;
            Item = item;
        }

        public object ValueAsObject()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            var state = obj as MenuState<T>;
            return state != null &&
                   Changed == state.Changed &&
                   Active == state.Active &&
                   EqualityComparer<MenuItem>.Default.Equals(Item, state.Item) &&
                   EqualityComparer<T>.Default.Equals(Value, state.Value);
        }

        public override int GetHashCode()
        {
            var hashCode = -1926106494;
            hashCode = hashCode * -1521134295 + Changed.GetHashCode();
            hashCode = hashCode * -1521134295 + Active.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<MenuItem>.Default.GetHashCode(Item);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
