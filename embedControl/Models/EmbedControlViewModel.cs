using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using embedCONTROL.Services;

namespace embedControl.Models
{
    public class EmbedControlViewModel
    {
        private static EmbedControlViewModel _instance = null;

        public string AppTitle => "Embed Control " + ApplicationContext.Instance.Version;
        public static object Instance => _instance ??= new EmbedControlViewModel();
        
    }
}
