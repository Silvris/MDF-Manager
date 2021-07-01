using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDF_Manager.Classes
{
    public class Defs
    {
        public List<string> lastOpenFiles { get; set; }
        public string lastOpenLib { get; set; }

        public Defs()
        {
            lastOpenFiles = new List<string>();
            lastOpenLib = "";
        }
    }
}
