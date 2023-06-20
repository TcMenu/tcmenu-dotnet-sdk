using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.FormUi
{
    
    public interface IMauiNavigation
    {
        public void PushMenuNavigation(MenuItem toPush, LoadedMenuForm loadedForm);
        public void PopNavigation();
    }
}
