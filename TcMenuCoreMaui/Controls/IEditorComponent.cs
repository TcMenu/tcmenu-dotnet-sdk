using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{

    public interface IEditorComponent
    {
        View ViewItem { get; }
        int Id { get; }

        void OnItemUpdated(AnyMenuState newValue);

        void OnCorrelation(CorrelationId correlationId, AckStatus status);

        void Tick();
    }

    /// <summary>
    /// This class is responsible for working out the settings and position of items within a submenu. The default implementation just uses
    /// the colors as defined in settings and is always recursive.
    /// </summary>
    public interface IComponentPositionManager
    {
        /// <summary>
        /// Get the next entire row in the grid as a position
        /// </summary>
        /// <returns>a grid position of an entire row</returns>
        ComponentPositioning PositionNewEntireRow();

        /// <summary>
        /// Get the next position that fills the number of columns requested
        /// </summary>
        /// <param name="colsRequested">the columns needed</param>
        /// <returns>a grid position for the number of columns requested</returns>
        public ComponentPositioning NextPosition(int colsRequested);
        
        /// <summary>
        /// Get the next position that fills the number of columns requested
        /// </summary>
        /// <param name="item">the item to position</param>
        /// <returns>a grid position for the number of columns requested</returns>
        public ComponentPositioning NextPosition(MenuItem item);
        
        /// <summary>
        /// Get the settings for a particular menu item
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns></returns>
        ComponentSettings SettingsFor(MenuItem menuItem);

        /// <summary>
        /// Get the settings for a particular type of item
        /// </summary>
        /// <param name="type">the component type that is being drawn</param>
        /// <returns>the component settings</returns>
        ComponentSettings SettingsForType(ColorComponentType type);

        /// <summary>
        /// If the layout should recurse through other menu items, or show a new panel each time.
        /// </summary>
        bool IsRecursive { get; }

        /// <summary>
        /// Clear all positions and restart the layout at 0,0.
        /// </summary>
        void Clear();
    }

    public delegate void MenuActionConsumer(MenuItem item);

    public interface IMenuEditorFactory
    {
        IEditorComponent CreateUpDown(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateBooleanButton(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateButtonWithAction(MenuItem item, string text, ComponentSettings settings, MenuActionConsumer actionConsumer);
        IEditorComponent CreateText<TVal>(MenuItem enumItem, ComponentSettings settings);
        IEditorComponent CreateList(MenuItem item, ComponentSettings settings, RuntimeListStringAdapter adapter);
        IEditorComponent CreateDate(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateTime(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateHorizontalSlider(MenuItem item, ComponentSettings settings);
        IEditorComponent CreateRgbColor(MenuItem item, ComponentSettings settings);

        IComponentPositionManager CreatePositionManagerFor(MenuItem item, int columns);
    }

}