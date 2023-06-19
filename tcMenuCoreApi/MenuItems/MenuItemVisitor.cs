using System;
using System.Collections.Generic;
using System.Text;

namespace TcMenu.CoreSdk.MenuItems
{
    /// <summary>
    ///  An implementation of the visitor pattern for TcMenu. Each menu item has a visit method, that takes an
    ///  implementation of this class as it's parameter. It will call the appropriate method on this class for
    ///  it's type. This is useful to avoid if and switch statements when dealing with menus.
    /// </summary>
    public interface MenuItemVisitor
    {
        /// <summary>
        /// Visitor method that will be called by the item Accept method for BooleanMenuItem
        /// </summary>
        /// <param name="item">the item that is being visited</param>
        void Visit(BooleanMenuItem item);

        /// <summary>
        /// Visitor method that will be called by the item Accept method for AnalogMenuItem
        /// </summary>
        /// <param name="item">the analog item being visited</param>
        void Visit(AnalogMenuItem item);

        /// <summary>
        /// Visitor method that will be called by the item Accept method for ActionMenuItem
        /// </summary>
        /// <param name="item">the menu item</param>
        void Visit(ActionMenuItem item);

        /// <summary>
        /// Visitor method that will be called by the item Accept method for EnumMenuItem
        /// </summary>
        /// <param name="item">the menu item</param>
        void Visit(EnumMenuItem item);
        
        /// <summary>
        /// Visitor method that will be called by the item Accept method for ScrollChoiceMenuItem
        /// </summary>
        /// <param name="item">the menu item</param>
        void Visit(ScrollChoiceMenuItem item);

        /// <summary>
        /// Visitor mehtod that will be called by the item Accept method for EditableTextMenuItem
        /// </summary>
        /// <param name="item">the item</param>
        void Visit(EditableTextMenuItem item);

        /// <summary>
        /// Visitor mehtod that will be called by the item Accept method for FloatMenuItem
        /// </summary>
        /// <param name="item">the item</param>
        void Visit(FloatMenuItem item);
        
        /// <summary>
        /// Visitor method that will be called by the item Accept method for Rgb32 items
        /// </summary>
        /// <param name="item">the item</param>
        void Visit(Rgb32MenuItem item);
        
        /// <summary>
        /// Visitor mehtod that will be called by the item Accept method for SubMenuItem
        /// </summary>
        /// <param name="item">the item</param>
        void Visit(SubMenuItem item);

        /// <summary>
        /// Visitor mehtod that will be called by the item Accept method for RuntimeListMenuItems
        /// </summary>
        /// <param name="item">the item as a runtime list</param>
        void Visit(RuntimeListMenuItem item);

        /// <summary>
        /// Visitor method that will be called by the item Accept method for Large Numbers
        /// </summary>
        /// <param name="item">the item as a large number</param>
        void Visit(LargeNumberMenuItem item);
    }

    /// <summary>
    /// An implementation of MenuItemVisitor that calls AnyItem for all items, unless at
    /// least AnyItem is overriden an exception will be thrown on all calls.
    /// </summary>
    /// <typeparam name="T">Optional return value if required</typeparam>
    public abstract class AbstractMenuItemVisitor<T> : MenuItemVisitor
    {
        public T Result { get; set; }

        public virtual void Visit(BooleanMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(AnalogMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(ActionMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(EnumMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(ScrollChoiceMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(EditableTextMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(FloatMenuItem item)
        {
            AnyItem(item);
        }
        
        public virtual void Visit(Rgb32MenuItem item)
        {
            AnyItem(item);
        }
        
        public virtual void Visit(SubMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(RuntimeListMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void Visit(LargeNumberMenuItem item)
        {
            AnyItem(item);
        }

        public virtual void AnyItem(MenuItem item)
        {
            throw new NotSupportedException("AnyItem of " + GetType().Name + " is not implemented");
        }
    }
}