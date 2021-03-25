using System;
using System.Collections.Generic;
using System.Text;

namespace MDF_Manager.Classes
{
    class TextureBinding
    {
        public string name { get; set; }
        public string path { get; set; }

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
