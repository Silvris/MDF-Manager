using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MDF_Manager.Classes
{
    public class MDFFile
    {
        public string Header { get; set; }
        public string FileName = "";
        static byte[] magic = { (byte)'M', (byte)'D', (byte)'F', 0x00 };
        UInt16 unkn = 1;
        public List<Material> Materials { get; set; }

        public MDFFile(string fileName, BinaryReader br, MDFTypes types)
        {
            Materials = new List<Material>();
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

        public List<byte> GenerateStringTable()
        {
            List<string> strings = new List<string>();
            for(int i = 0; i < Materials.Count; i++)
            {
                if (!strings.Contains(Materials[i].Name))
                {
                    strings.Add(Materials[i].Name);
                }
                if (!strings.Contains(Materials[i].MasterMaterial))
                {
                    strings.Add(Materials[i].MasterMaterial);
                }
            }
            for(int i = 0; i < Materials.Count; i++)
            {
                for(int j = 0; j < Materials[i].Textures.Count; j++)
                {
                    if (!strings.Contains(Materials[i].Textures[j].name))
                    {
                        strings.Add(Materials[i].Textures[j].name);
                    }
                    if (!strings.Contains(Materials[i].Textures[j].path))
                    {
                        strings.Add(Materials[i].Textures[j].path);
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
                    }
                }
            }
            List<byte> outputBuff = new List<byte>();
            for(int i = 0; i < strings.Count; i++)
            {
                byte[] inBytes = Encoding.Unicode.GetBytes(strings[i]);
                for(int j = 0; j < inBytes.Length; j++)
                {
                    outputBuff.Add(inBytes[j]);
                }
                outputBuff.Add(0);
                outputBuff.Add(0);
            }
            return outputBuff;
        }

        public void Export(BinaryWriter bw)
        {
            bw.Write(magic);
            bw.Write((short)1);
        }
    }
}
