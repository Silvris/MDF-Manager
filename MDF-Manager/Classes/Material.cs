using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MDF_Manager.Classes
{
    [Flags]
    public enum AlphaFlags
    {
        None = 0,
        DoubleSided = 1,
        Transparency = 2,
        Unkn3 = 4,
        SingleSided = 8,
        Default5 = 16,
        Unkn6 = 32,
        Unkn7 = 64,
        Default8 = 128
    }

    public enum MDFTypes
    {
        RE7 = 6,
        RE2DMC5 = 10,
        RE3 = 13,
        MHRise = 19
    }

    public class Material : INotifyPropertyChanged
    {
        private string _Name;
        private uint _Hash;
        private void UpdateHash()
        {
            _Hash = HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(_Name));
            OnPropertyChanged("UTF16Hash");
        }
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); UpdateHash(); } }
        public uint UTF16Hash { get => _Hash; set => _Hash = value; }
        public string MasterMaterial { get; set; }
        public int ShaderType { get; set; }
        public byte Unkn1 { get; set; }
        public byte Unkn2 { get; set; }
        public byte Unkn3 { get; set; }
        public bool DoubleSided { get; set; }
        public bool Transparency { get; set; }
        public bool Bool3 { get; set; }
        public bool SingleSided { get; set; }
        public bool Bool5 { get; set; }
        public bool Bool6 { get; set; }
        public bool Bool7 { get; set; }
        public bool Bool8 { get; set; }
        public List<TextureBinding> Textures { get; set; }
        public List<IVariableProp> Properties { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public TextureBinding ReadTextureBinding(BinaryReader br, MDFTypes type)
        {
            Int64 TextureTypeOff = br.ReadInt64();
            uint UTF16MMH3Hash = br.ReadUInt32();
            uint ASCIIMMH3Hash = br.ReadUInt32();
            Int64 FilePathOff = br.ReadInt64();
            if(type >= MDFTypes.MHRise)
            {
                Int64 Unkn0 = br.ReadInt64(); //value of 0 in most mdf, possible alignment?
            }
            Int64 EOT = br.BaseStream.Position;
            br.BaseStream.Seek(TextureTypeOff, SeekOrigin.Begin);
            string TextureType = HelperFunctions.ReadUniNullTerminatedString(br);
            br.BaseStream.Seek(FilePathOff, SeekOrigin.Begin);
            string FilePath = HelperFunctions.ReadUniNullTerminatedString(br);
            br.BaseStream.Seek(EOT, SeekOrigin.Begin);
            TextureBinding tb = new TextureBinding(TextureType,FilePath);
            return tb;
        }

        public IVariableProp ReadProperty(BinaryReader br, Int64 dataOff, MDFTypes type, int matIndex, int propIndex)
        {
            Int64 PropNameOff = br.ReadInt64();
            uint UTF16MMH3Hash = br.ReadUInt32();
            uint ASCIIMMH3Hash = br.ReadUInt32();
            int PropDataOff = 0;
            int ParamCount = 0;
            if (type >= MDFTypes.RE3)
            {
                PropDataOff = br.ReadInt32();
                ParamCount = br.ReadInt32();
            }
            else
            {
                ParamCount = br.ReadInt32();
                PropDataOff = br.ReadInt32();
            }

            Int64 EOP = br.BaseStream.Position;
            br.BaseStream.Seek(PropNameOff, SeekOrigin.Begin);
            string PropName = HelperFunctions.ReadUniNullTerminatedString(br);
            br.BaseStream.Seek(dataOff + PropDataOff, SeekOrigin.Begin);
            switch (ParamCount)
            {
                case 1:
                    Float fData = new Float(br.ReadSingle());
                    br.BaseStream.Seek(EOP, SeekOrigin.Begin);
                    return new FloatProperty(PropName, fData);
                case 4:
                    Float4 f4Data = new Float4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    br.BaseStream.Seek(EOP, SeekOrigin.Begin);
                    return new Float4Property(PropName, f4Data, matIndex, propIndex);
                default:
                    br.BaseStream.Seek(EOP, SeekOrigin.Begin);
                    return new FloatProperty(Name, new Float((float)1.0));//shouldn't really come up ever

            }
        }

        public int GetSize()
        {
            return 64;
        }

        public Material(BinaryReader br,MDFTypes type,int matIndex)
        {
            Int64 MatNameOffset = br.ReadInt64();
            int MatNameHash = br.ReadInt32();//not storing, since it'll just be easier to export proper
            int PropBlockSize = br.ReadInt32();
            int PropertyCount = br.ReadInt32();
            int TextureCount = br.ReadInt32();
            if(type == MDFTypes.MHRise)
            {
                br.ReadInt64();
            }
            ShaderType = br.ReadInt32();
            AlphaFlags alphaFlags = (AlphaFlags)br.ReadByte();
            if((alphaFlags & AlphaFlags.DoubleSided) == AlphaFlags.DoubleSided)
            {
                DoubleSided = true;
            }
            else
            {
                DoubleSided = false;
            }
            if ((alphaFlags & AlphaFlags.Transparency) == AlphaFlags.Transparency)
            {
                Transparency = true;
            }
            else
            {
                Transparency = false;
            }
            if ((alphaFlags & AlphaFlags.Unkn3) == AlphaFlags.Unkn3)
            {
                Bool3 = true;
            }
            else
            {
                Bool3 = false;
            }
            if ((alphaFlags & AlphaFlags.SingleSided) == AlphaFlags.SingleSided)
            {
                SingleSided = true;
            }
            else
            {
                SingleSided = false;
            }
            if ((alphaFlags & AlphaFlags.Default5) == AlphaFlags.Default5)
            {
                Bool5 = true;
            }
            else
            {
                Bool5 = false;
            }
            if ((alphaFlags & AlphaFlags.Unkn6) == AlphaFlags.Unkn6)
            {
                Bool6 = true;
            }
            else
            {
                Bool6 = false;
            }
            if ((alphaFlags & AlphaFlags.Unkn7) == AlphaFlags.Unkn7)
            {
                Bool7 = true;
            }
            else
            {
                Bool7 = false;
            }
            if ((alphaFlags & AlphaFlags.Default8) == AlphaFlags.Default8)
            {
                Bool8 = true;
            }
            else
            {
                Bool8 = false;
            }
            Unkn1 = br.ReadByte();
            Unkn2 = br.ReadByte();
            Unkn3 = br.ReadByte();
            Int64 PropHeadersOff = br.ReadInt64();
            Int64 TexHeadersOff = br.ReadInt64();
            if(type == MDFTypes.MHRise)
            {
                Int64 StringTableOff = br.ReadInt64();//not at all useful, given everything uses absolute offsets
                //it's possible that this is an offset for something that is not used by most mdfs, this will need to be looked into
                //given the extra Int64, I find this very likely
            }
            Int64 PropDataOff = br.ReadInt64();
            Int64 MMTRPathOff = br.ReadInt64();
            Int64 EOM = br.BaseStream.Position;//to return to after reading the rest of the parameters
            Textures = new List<TextureBinding>();
            Properties = new List<IVariableProp>();
            //now we'll go grab names and values
            br.BaseStream.Seek(MatNameOffset, SeekOrigin.Begin);
            Name = HelperFunctions.ReadUniNullTerminatedString(br);
            br.BaseStream.Seek(MMTRPathOff, SeekOrigin.Begin);
            MasterMaterial = HelperFunctions.ReadUniNullTerminatedString(br);

            //read textures
            br.BaseStream.Seek(TexHeadersOff,SeekOrigin.Begin);
            for(int i = 0; i < TextureCount; i++)
            {
                Textures.Add(ReadTextureBinding(br,type));
            }

            //read properties
            br.BaseStream.Seek(PropHeadersOff, SeekOrigin.Begin);
            for(int i = 0; i < PropertyCount; i++)
            {
                Properties.Add(ReadProperty(br,PropDataOff,type,matIndex,i));
            }
            br.BaseStream.Seek(EOM,SeekOrigin.Begin);
        }
    }
}
