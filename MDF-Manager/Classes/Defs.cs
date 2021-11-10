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
        public string lastOpenComp { get; set; }
        public string lastOpenLib { get; set; }
        private Color _Background = new Color { R = 226, G = 226, B = 226, A = 255 };
        private Color _Foreground = new Color { R = 255, G = 255, B = 255, A = 255 };
        private Color _Windows = new Color { R = 255, G = 255, B = 255, A = 255 };
        private Color _Button = new Color { R = 226, G = 226, B = 226, A = 255 };
        private Color _Text = new Color { R = 0, G = 0, B = 0, A = 255 };
        public Color background { get => _Background; set => _Background = value; }
        public Color foreground { get => _Foreground; set => _Foreground = value; }
        public Color windows { get => _Windows; set => _Windows = value; }
        public Color buttons { get => _Button; set => _Button = value; }
        public Color text { get => _Text; set => _Text = value; }

        public Defs()
        {
            lastOpenFiles = new List<string>();
            lastOpenLib = "";
            lastOpenComp = "";
        }
    }
}
