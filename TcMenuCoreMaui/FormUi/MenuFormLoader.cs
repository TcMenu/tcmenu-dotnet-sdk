using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcMenuCoreMaui.Controls;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.FormUi
{
    public class LoadedMenuForm
    {
        public int RootId { get; }
        public FontInformation GlobalFontInformation { get; }
        public IConditionalColoring ColorScheme { get; }
        public bool Recursive { get; }
        public int GridSize { get; }

        public bool HasLayoutFor(MenuItem sub)
        {
            return false;
        }

        public List<ComponentSettings> GetFormComponentSettings(MenuItem sub)
        {
            return new List<ComponentSettings>();
        }

        public IConditionalColoring ColorSchemeAtPosition(ComponentPositioning positioning)
        {
            throw new NotImplementedException();
        }
    }

    public class MenuFormLoader
    {
        public List<IConditionalColoring> ColorSets { get; }

        public MenuFormLoader(string fileName)
        {

        }
    }
}
