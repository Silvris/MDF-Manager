using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MDF_Manager.Classes
{
    public class MDFFile : INotifyPropertyChanged
    {
        private string _Header;
        public string Header { get => _Header; set { _Header = value; OnPropertyChanged("Header"); } }
        public DataTemplate HeaderTemplate { get; set; }
        public string FileName = "";
        static byte[] magic = { (byte)'M', (byte)'D', (byte)'F', 0x00 };
        UInt16 unkn = 1;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<Material> Materials { get; set; }

        public MDFFile(string fileName, BinaryReader br, MDFTypes types)
        {
            Materials = new ObservableCollection<Material>();
            Header = fileName;
            FileName = fileName;
            byte[] mBytes = br.ReadBytes(4);
            if (Encoding.ASCII.GetString(mBytes) != Encoding.ASCII.GetString(magic)) 
            {
                MessageBox.Show("Not a valid MDF file!");
                return;
            }
            UInt16 unkn1 = br.ReadUInt16();
            if(unkn1 != unkn)
            {
                MessageBox.Show("Potentially bad MDF file.");
            }
            UInt16 MaterialCount = br.ReadUInt16();
            br.ReadUInt64();
            for(int i = 0; i < MaterialCount; i++)
            {
                Materials.Add(new Material(br, types,i));
            }

        }

        public List<byte> GenerateStringTable(ref List<int> offsets)
        {
            List<string> strings = new List<string>();
            for(int i = 0; i < Materials.Count; i++)
            {
                if (!strings.Contains(Materials[i].Name))
                {
                    strings.Add(Materials[i].Name);
                    Materials[i].NameOffsetIndex = strings.Count - 1;
                }
                else
                {
                    Materials[i].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].Name);
                }
                if (!strings.Contains(Materials[i].MasterMaterial))
                {
                    strings.Add(Materials[i].MasterMaterial);
                    Materials[i].MMOffsetIndex = strings.Count - 1;
                }
                else
                {
                    Materials[i].MMOffsetIndex = strings.FindIndex(name => name == Materials[i].MasterMaterial);
                }
            }
            for(int i = 0; i < Materials.Count; i++)
            {
                for(int j = 0; j < Materials[i].Textures.Count; j++)
                {
                    if (!strings.Contains(Materials[i].Textures[j].name))
                    {
                        strings.Add(Materials[i].Textures[j].name);
                        Materials[i].Textures[j].NameOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].Textures[j].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].Textures[j].name);
                    }
                    if (!strings.Contains(Materials[i].Textures[j].path))
                    {
                        strings.Add(Materials[i].Textures[j].path);
                        Materials[i].Textures[j].PathOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].Textures[j].PathOffsetIndex = strings.FindIndex(name => name == Materials[i].Textures[j].path);
                    }
                }
            }
            for (int i = 0; i < Materials.Count; i++)
            {
                for (int j = 0; j < Materials[i].Properties.Count; j++)
                {
                    if (!strings.Contains(Materials[i].Properties[j].name))
                    {
                        strings.Add(Materials[i].Properties[j].name);
                        Materials[i].Properties[j].NameOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].Properties[j].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].Properties[j].name);
                    }
                }
            }
            for (int i = 0; i < Materials.Count; i++)
            {
                for (int j = 0; j < Materials[i].GPBFReferences.Count; j++)
                {
                    if (!strings.Contains(Materials[i].GPBFReferences[j].name))
                    {
                        strings.Add(Materials[i].GPBFReferences[j].name);
                        Materials[i].GPBFReferences[j].NameOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].GPBFReferences[j].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].GPBFReferences[j].name);
                    }
                    if (!strings.Contains(Materials[i].GPBFReferences[j].path))
                    {
                        strings.Add(Materials[i].GPBFReferences[j].path);
                        Materials[i].GPBFReferences[j].PathOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].GPBFReferences[j].PathOffsetIndex = strings.FindIndex(name => name == Materials[i].GPBFReferences[j].path);
                    }
                }
            }
            List<byte> outputBuff = new List<byte>();
            offsets.Add(0);
            for(int i = 0; i < strings.Count; i++)
            {
                byte[] inBytes = Encoding.Unicode.GetBytes(strings[i]);
                for(int j = 0; j < inBytes.Length; j++)
                {
                    outputBuff.Add(inBytes[j]);
                }
                outputBuff.Add(0);
                outputBuff.Add(0);
                offsets.Add(outputBuff.Count);//think this will end with the very last one being unused but that's fine

            }
            return outputBuff;
        }

        public void Export(BinaryWriter bw,MDFTypes type)
        {
            bw.Write(magic);
            bw.Write((short)1);
            bw.Write((short)Materials.Count);
            bw.Write((long)0);
            //before going further, we need accurate lengths for 5 of the 6 main sections of the mdf
            /*
             * header -set size
             * materials - set size
             * textures - set size
             * propHeaders - set size
             * gpbfs - set size
             * stringtable - generate in a separate function
             * prop values - based off of prop headers
             */
            List<int> strTableOffsets = new List<int>();
            List<byte> stringTable = GenerateStringTable(ref strTableOffsets);
            //this function handles the biggest problem of writing materials, getting the name offsets
            long materialOffset = bw.BaseStream.Position;
            while ((materialOffset % 16) != 0)
            {
                materialOffset++;
            }
            long textureOffset = materialOffset;
            for(int i = 0; i < Materials.Count; i++)
            {
                textureOffset += Materials[i].GetSize(type);
            }
            while((textureOffset%16) != 0)
            {
                textureOffset++;
            }
            long propHeadersOffset = textureOffset;
            for(int i = 0; i < Materials.Count; i++)
            {
                for(int j = 0; j < Materials[i].Textures.Count; j++)
                {
                    propHeadersOffset += Materials[i].Textures[j].GetSize(type);
                }
            }
            while ((propHeadersOffset % 16) != 0)
            {
                propHeadersOffset++;
            }
            long gpbfOffset = propHeadersOffset;
            for (int i = 0; i < Materials.Count; i++)
            {
                for (int j = 0; j < Materials[i].Properties.Count; j++)
                {
                    gpbfOffset += Materials[i].Properties[j].GetPropHeaderSize();
                }
            }
            while ((gpbfOffset % 16) != 0)
            {
                gpbfOffset++;
            }
            long stringTableOffset = gpbfOffset;
            for (int i = 0; i < Materials.Count; i++)
            {
                for (int j = 0; j < Materials[i].GPBFReferences.Count; j++)
                {
                    stringTableOffset += Materials[i].GPBFReferences[j].GetSize(type);
                }
            }
            long propertiesOffset = stringTableOffset + stringTable.Count;
            while ((propertiesOffset % 16) != 0)
            {
                propertiesOffset++;
            }
            bw.BaseStream.Seek(stringTableOffset,SeekOrigin.Begin);
            for(int i = 0; i < stringTable.Count; i++)
            {
                bw.Write(stringTable[i]);
            }
            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].Export(bw,type, ref materialOffset, ref textureOffset, ref propHeadersOffset, ref gpbfOffset, stringTableOffset, strTableOffsets, ref propertiesOffset);
            }
        }
        public static IList<ShadingType> ShadingTypes
        {
            get
            {
                return Enum.GetValues(typeof(ShadingType)).Cast<ShadingType>().ToList<ShadingType>();
            }
        }
    }
}
