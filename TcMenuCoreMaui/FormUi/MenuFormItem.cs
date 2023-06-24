using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TcMenuCoreMaui.Controls;

namespace TcMenuCoreMaui.FormUi
{
    /// <summary>
    ///  A form item represents the actual entry at a position in the row, along with its calculated drawing information and its position.
    /// </summary>
    public abstract class MenuFormItem
    {
        public static MenuFormItem NoFormItemDefined = new NoFormItem();

        public MenuFormItem(IConditionalColoring settings, ComponentPositioning positioning)
        {
            Settings = settings;
            Positioning = positioning;
        }

        public abstract bool Valid { get; }

        public abstract string Description { get; }

        public FontInformation FontInfo { get; set; }

        public IConditionalColoring Settings { get; set; }

        public ComponentPositioning Positioning { get; set; }
    }

    /// <summary>
    /// Creates a blank form item that is not ready for saving, it effectively has no settings.
    /// </summary>
    public class NoFormItem : MenuFormItem
    {
        public NoFormItem() : base(null, new ComponentPositioning(0, 0))
        {
        }

        public override bool Valid => false;

        public override string Description => "Empty";
    }

    public class VerticalSpaceFormItem : MenuFormItem
    {
        public int VerticalSpace { get; }

        public VerticalSpaceFormItem(IConditionalColoring settings, ComponentPositioning positioning, int space) : base(
            settings, positioning)
        {
            VerticalSpace = space;
        }

        public override bool Valid => true;
        public override string Description => "Vertical Space";
    }

    public class TextFormItem : MenuFormItem
    {
        public TextFormItem(string txt, IConditionalColoring settings, ComponentPositioning positioning,
            PortableAlignment align) : base(settings, positioning)
        {
            Text = txt;
            Alignment = align;
        }

        public override bool Valid => Text != null;
        public string Text { get; set; }
        public override string Description => "Edit Text";

        public PortableAlignment Alignment { get; set; }
    }

    public class MenuItemFormItem : MenuFormItem
    {
        public override bool Valid => Item != null;
        public override string Description => "Edit " + Item;
        public TcMenu.CoreSdk.MenuItems.MenuItem Item { get; set; }
        public PortableAlignment Alignment { get; set; }
        public ControlType ControlType { get; set; }
        public RedrawingMode RedrawingMode { get; set; }

        public MenuItemFormItem(TcMenu.CoreSdk.MenuItems.MenuItem mi, IConditionalColoring coloring, ComponentPositioning positioning, ControlType ty,
            PortableAlignment alignment, RedrawingMode redrawingMode) : base(coloring, positioning)
        {
            Item = mi;
            ControlType = ty;
            Alignment = alignment;
            RedrawingMode = redrawingMode;
        }
    }
}
