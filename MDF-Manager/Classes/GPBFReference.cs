using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MDF_Manager.Classes
{
    public class GPBFReference
    {
        public int NameOffsetIndex;
        public int PathOffsetIndex;
        public string name { get; set; }
        public string path { get; set; }
        public static int Unkn0 { get => 0; }
        public static int Unkn1 { get => 1; }

        public GPBFReference(string name, string path) {
            this.name = name;
            this.path = path;
        }

        public void Export(BinaryWriter bw, MDFTypes type, ref long offset, long stringTableOffset, List<int> strTableOffs)
        {
            if (type < MDFTypes.MHRiseRE8) return;
            bw.BaseStream.Seek(offset, SeekOrigin.Begin);
            bw.Write(stringTableOffset + strTableOffs[NameOffsetIndex]);
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.ASCII.GetBytes(name)));//potentially UTF8 rather than ASCII, but further testing would be required
            bw.Write(stringTableOffset + strTableOffs[PathOffsetIndex]);
            bw.Write(Unkn0);
            bw.Write(Unkn1);
            offset += GetSize(type);
        }

        public int GetSize(MDFTypes type) { return 32; }

    }
}
