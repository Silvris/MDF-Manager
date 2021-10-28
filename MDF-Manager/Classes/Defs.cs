using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MDF_Manager.Classes
{
    public class Defs
    {
        public List<string> lastOpenFiles { get; set; }
        public string lastOpenLib { get; set; }
        public Color background { get; set; }
        public Color foreground { get; set; }
        public Color windows { get; set; }
        public Color buttons { get; set; }
        public Color text { get; set; }

        public Defs()
        {
            lastOpenFiles = new List<string>();
            lastOpenLib = "";
        }
    }
}
