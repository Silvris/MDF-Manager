using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDF_Manager.Classes
{
    public class TextureBinding
    {
        public int NameOffsetIndex;
        public int PathOffsetIndex;
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

        public void Export(BinaryWriter bw, MDFTypes type, ref long offset, long stringTableOffset, List<int> strTableOffs)
        {
            bw.BaseStream.Seek(offset,SeekOrigin.Begin);
            bw.Write(stringTableOffset + strTableOffs[NameOffsetIndex]);
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.ASCII.GetBytes(name)));//potentially UTF8 rather than ASCII, but further testing would be required
            bw.Write(stringTableOffset + strTableOffs[PathOffsetIndex]);
            if(type == MDFTypes.MHRise)
            {
                bw.Write((long)0);
            }
            offset += GetSize(type);
        }

    }
}
