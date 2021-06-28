using System;
using System.Collections.Generic;
using System.Text;

namespace MDF_Manager.Classes
{
    public class TextureBinding
    {
        public string name { get; set; }
        public string path { get; set; }
        public int GetSize(MDFTypes type)
        {
            if (type == MDFTypes.MHRise)
            {
                return 32;
            }
            else 
            {
                return 24;
            }
        }
        public TextureBinding()//this should never be used, but needed for JSON iirc
        {
            name = "BaseMetalMap";
            path = "systems/rendering/nullblack.tex";
        }
        public TextureBinding(string pName, string pPath)
        {
            name = pName;
            path = pPath;
        }

    }
}
