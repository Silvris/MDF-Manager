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
        BaseTwoSideEnable = 1,
        BaseAlphaTestEnable = 2,
        ShadowCastDisable = 4,
        VertexShaderUsed = 8,
        EmissiveUsed = 16,
        TessellationEnable = 32,
        EnableIgnoreDepth = 64,
        AlphaMaskUsed = 128
    }
    [Flags]
    public enum Flags2
    {
        None = 0,
        ForcedTwoSideEnable = 1,
        TwoSideEnable = 2
    }
    [Flags]
    public enum Flags3
    {
        None = 0,
        RoughTransparentEnable = 1,
        ForcedAlphaTestEnable = 2,
        AlphaTestEnable = 4,
        SSSProfileUsed = 8,
        EnableStencilPriority = 16,
        RequireDualQuaternion = 32,
        PixelDepthOffsetUsed = 64,
        NoRayTracing = 128
    }

    public enum ShadingType
    {
        Standard = 0,
        Decal = 1,
        DecalWithMetallic = 2,
        DecalNRMR = 3,
        Transparent = 4,
        Distortion = 5,
        PrimitiveMesh = 6,
        PrimitiveSolidMesh = 7,
        Water = 8,
        SpeedTree = 9,
        GUI = 10,
        GUIMesh = 11,
        GUIMeshTransparent = 12,
        ExpensiveTransparent =13,
        Forward = 14,
        RenderTarget = 15,
        PostProcess = 16,
        PrimitiveMaterial = 17,
        PrimitiveSolidMaterial = 18,
        SpineMaterial = 19
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
        public int NameOffsetIndex; //applied and used only on export
        public int MMOffsetIndex;
        public int materialIndex; //used for deleting and adding new
        private void UpdateHash()
        {
            _Hash = HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(_Name));
            OnPropertyChanged("UTF16Hash");
        }
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged("Name"); UpdateHash(); } }
        public uint UTF16Hash { get => _Hash; set => _Hash = value; }
        public string MasterMaterial { get; set; }
        public ShadingType ShaderType { get; set; }
        public byte TessFactor { get; set; }
        public byte PhongFactor { get; set; }//shrug bytes are unsigned by default in C#
        public bool BaseTwoSideEnable { get; set; }
        public bool BaseAlphaTestEnable { get; set; }
        public bool ShadowCastDisable { get; set; }
        public bool VertexShaderUsed { get; set; }
        public bool EmissiveUsed { get; set; }
        public bool TessellationEnable { get; set; }
        public bool EnableIgnoreDepth { get; set; }
        public bool AlphaMaskUsed { get; set; }
        public bool ForcedTwoSideEnable { get; set; }
        public bool TwoSideEnable { get; set; }
        public bool RoughTransparentEnable { get; set; }
        public bool ForcedAlphaTestEnable { get; set; }
        public bool AlphaTestEnable { get; set; }
        public bool SSSProfileUsed { get; set; }
        public bool EnableStencilPriority { get; set; }
        public bool RequireDualQuaternion { get; set; }
        public bool PixelDepthOffsetUsed { get; set; }
        public bool NoRayTracing { get; set; }
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
                    return new FloatProperty(PropName, fData, matIndex, propIndex);
                case 4:
                    Float4 f4Data = new Float4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    br.BaseStream.Seek(EOP, SeekOrigin.Begin);
                    return new Float4Property(PropName, f4Data, matIndex, propIndex);
                default:
                    br.BaseStream.Seek(EOP, SeekOrigin.Begin);
                    return new FloatProperty(Name, new Float((float)1.0), matIndex, propIndex);//shouldn't really come up ever

            }
        }

        public int GetSize(MDFTypes type)
        {
            int baseVal = 64;
            if(type == MDFTypes.RE7)
            {
                baseVal += 8;
            }
            else if (type == MDFTypes.MHRise){
                baseVal += 16;
            }
            return baseVal;
        }

        public void GetAlphaFlagsFromByte(byte flags)
        {
            AlphaFlags alphaFlags = (AlphaFlags)flags;
            if ((alphaFlags & AlphaFlags.BaseTwoSideEnable) == AlphaFlags.BaseTwoSideEnable)
            {
                BaseTwoSideEnable = true;
            }
            else
            {
                BaseTwoSideEnable = false;
            }
            if ((alphaFlags & AlphaFlags.BaseAlphaTestEnable) == AlphaFlags.BaseAlphaTestEnable)
            {
                BaseAlphaTestEnable = true;
            }
            else
            {
                BaseAlphaTestEnable = false;
            }
            if ((alphaFlags & AlphaFlags.ShadowCastDisable) == AlphaFlags.ShadowCastDisable)
            {
                ShadowCastDisable = true;
            }
            else
            {
                ShadowCastDisable = false;
            }
            if ((alphaFlags & AlphaFlags.VertexShaderUsed) == AlphaFlags.VertexShaderUsed)
            {
                VertexShaderUsed = true;
            }
            else
            {
                VertexShaderUsed = false;
            }
            if ((alphaFlags & AlphaFlags.EmissiveUsed) == AlphaFlags.EmissiveUsed)
            {
                EmissiveUsed = true;
            }
            else
            {
                EmissiveUsed = false;
            }
            if ((alphaFlags & AlphaFlags.TessellationEnable) == AlphaFlags.TessellationEnable)
            {
                TessellationEnable = true;
            }
            else
            {
                TessellationEnable = false;
            }
            if ((alphaFlags & AlphaFlags.EnableIgnoreDepth) == AlphaFlags.EnableIgnoreDepth)
            {
                EnableIgnoreDepth = true;
            }
            else
            {
                EnableIgnoreDepth = false;
            }
            if ((alphaFlags & AlphaFlags.AlphaMaskUsed) == AlphaFlags.AlphaMaskUsed)
            {
                AlphaMaskUsed = true;
            }
            else
            {
                AlphaMaskUsed = false;
            }
        }

        public byte GetFlags2FromByte(byte flags)
        {
            Flags2 flagVals = (Flags2)flags;
            if ((flagVals & Flags2.ForcedTwoSideEnable) == Flags2.ForcedTwoSideEnable)
            {
                ForcedTwoSideEnable = true;
            }
            else
            {
                ForcedTwoSideEnable = false;
            }
            if ((flagVals & Flags2.TwoSideEnable) == Flags2.TwoSideEnable)
            {
                TwoSideEnable = true;
            }
            else
            {
                TwoSideEnable = false;
            }
            return (byte)(flags >> 2);
        }
        public void GetFlags3FromByte(byte flags)
        {
            Flags3 alphaFlags = (Flags3)flags;
            if ((alphaFlags & Flags3.RoughTransparentEnable) == Flags3.RoughTransparentEnable)
            {
                RoughTransparentEnable = true;
            }
            else
            {
                RoughTransparentEnable = false;
            }
            if ((alphaFlags & Flags3.ForcedAlphaTestEnable) == Flags3.ForcedAlphaTestEnable)
            {
                ForcedAlphaTestEnable = true;
            }
            else
            {
                ForcedAlphaTestEnable = false;
            }
            if ((alphaFlags & Flags3.AlphaTestEnable) == Flags3.AlphaTestEnable)
            {
                AlphaTestEnable = true;
            }
            else
            {
                AlphaTestEnable = false;
            }
            if ((alphaFlags & Flags3.SSSProfileUsed) == Flags3.SSSProfileUsed)
            {
                SSSProfileUsed = true;
            }
            else
            {
                SSSProfileUsed = false;
            }
            if ((alphaFlags & Flags3.EnableStencilPriority) == Flags3.EnableStencilPriority)
            {
                EnableStencilPriority = true;
            }
            else
            {
                EnableStencilPriority = false;
            }
            if ((alphaFlags & Flags3.RequireDualQuaternion) == Flags3.RequireDualQuaternion)
            {
                RequireDualQuaternion = true;
            }
            else
            {
                RequireDualQuaternion = false;
            }
            if ((alphaFlags & Flags3.PixelDepthOffsetUsed) == Flags3.PixelDepthOffsetUsed)
            {
                PixelDepthOffsetUsed = true;
            }
            else
            {
                PixelDepthOffsetUsed = false;
            }
            if ((alphaFlags & Flags3.NoRayTracing) == Flags3.NoRayTracing)
            {
                NoRayTracing = true;
            }
            else
            {
                NoRayTracing = false;
            }
        }

        public Material(BinaryReader br,MDFTypes type,int matIndex)
        {
            materialIndex = matIndex;
            Int64 MatNameOffset = br.ReadInt64();
            int MatNameHash = br.ReadInt32();//not storing, since it'll just be easier to export proper
            if(type == MDFTypes.RE7)
            {
                Int64 unknRE7 = br.ReadInt64();//I don't own RE7 so I'm just gonna hope these are 0s
            }
            int PropBlockSize = br.ReadInt32();
            int PropertyCount = br.ReadInt32();
            int TextureCount = br.ReadInt32();
            if(type == MDFTypes.MHRise)
            {
                br.ReadInt64();
            }
            ShaderType = (ShadingType)br.ReadInt32();
            GetAlphaFlagsFromByte(br.ReadByte());
            TessFactor = GetFlags2FromByte(br.ReadByte());
            PhongFactor = br.ReadByte();
            GetFlags3FromByte(br.ReadByte());
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
        public byte GenerateAlphaFlags()
        {
            AlphaFlags flags = 0;
            if (BaseTwoSideEnable)
            {
                flags = flags | AlphaFlags.BaseTwoSideEnable;
            }
            if (BaseAlphaTestEnable)
            {
                flags = flags | AlphaFlags.BaseAlphaTestEnable;
            }
            if (ShadowCastDisable)
            {
                flags = flags | AlphaFlags.ShadowCastDisable;
            }
            if (VertexShaderUsed)
            {
                flags = flags | AlphaFlags.VertexShaderUsed;
            }
            if (EmissiveUsed)
            {
                flags = flags | AlphaFlags.EmissiveUsed;
            }
            if (TessellationEnable)
            {
                flags = flags | AlphaFlags.TessellationEnable;
            }
            if (EnableIgnoreDepth)
            {
                flags = flags | AlphaFlags.EnableIgnoreDepth;
            }
            if (AlphaMaskUsed)
            {
                flags = flags | AlphaFlags.AlphaMaskUsed;
            }
            return (byte)flags;
        }
        public byte GenerateFlags2()
        {
            Flags2 flags = 0;
            if (ForcedTwoSideEnable)
            {
                flags = flags | Flags2.ForcedTwoSideEnable;
            }
            if (TwoSideEnable)
            {
                flags = flags | Flags2.TwoSideEnable;
            }
            byte finalVal = (byte)(TessFactor << 2);
            finalVal = (byte)(finalVal | (byte)flags);
            return finalVal;
        }
        public byte GenerateFlags3()
        {
            Flags3 flags = 0;
            if (RoughTransparentEnable)
            {
                flags = flags | Flags3.RoughTransparentEnable;
            }
            if (ForcedAlphaTestEnable)
            {
                flags = flags | Flags3.ForcedAlphaTestEnable;
            }
            if (AlphaTestEnable)
            {
                flags = flags | Flags3.AlphaTestEnable;
            }
            if (SSSProfileUsed)
            {
                flags = flags | Flags3.SSSProfileUsed;
            }
            if (EnableStencilPriority)
            {
                flags = flags | Flags3.EnableStencilPriority;
            }
            if (RequireDualQuaternion)
            {
                flags = flags | Flags3.RequireDualQuaternion;
            }
            if (PixelDepthOffsetUsed)
            {
                flags = flags | Flags3.PixelDepthOffsetUsed;
            }
            if (NoRayTracing)
            {
                flags = flags | Flags3.NoRayTracing;
            }
            return (byte)flags;
        }
        public void UpdateMaterialIndex(int index)
        {
            materialIndex = index;
            for(int i = 0; i < Properties.Count; i++)
            {
                Properties[i].indexes[0] = index;
            }
        }
        public void Export(BinaryWriter bw, MDFTypes type, ref long materialOffset, ref long textureOffset, ref long propHeaderOffset, long stringTableOffset, List<int> strTableOffsets, ref long propertiesOffset)
        {
            bw.BaseStream.Seek(materialOffset,SeekOrigin.Begin);
            bw.Write(stringTableOffset + strTableOffsets[NameOffsetIndex]);
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(Name)));
            if(type == MDFTypes.RE7)
            {
                bw.Write((long)0);
            }
            int propSize = 0;
            for(int i = 0; i < Properties.Count; i++)
            {
                propSize += Properties[i].GetSize();
            }
            bw.Write(propSize);
            bw.Write(Properties.Count);
            bw.Write(Textures.Count);
            if(type == MDFTypes.MHRise)
            {
                bw.Write((long)0);
            }
            bw.Write((uint)ShaderType);
            bw.Write(GenerateAlphaFlags());
            bw.Write(GenerateFlags2());
            bw.Write(PhongFactor);
            bw.Write(GenerateFlags3());
            bw.Write(propHeaderOffset);
            bw.Write(textureOffset);
            if(type == MDFTypes.MHRise)
            {
                bw.Write(stringTableOffset);
            }
            bw.Write(propertiesOffset);
            bw.Write(stringTableOffset + strTableOffsets[MMOffsetIndex]);
            //end of actual material file, now update material offset and write textures/properties
            materialOffset += GetSize(type);
            for(int i = 0; i < Textures.Count; i++)
            {
                Textures[i].Export(bw, type, ref textureOffset, stringTableOffset, strTableOffsets);
            }
            long basePropOffset = propertiesOffset;//subtract by current prop offset to make inner offset
            for(int i = 0; i < Properties.Count; i++)
            {
                Properties[i].Export(bw, type, ref propHeaderOffset, ref propertiesOffset, basePropOffset, stringTableOffset, strTableOffsets);
            }
        }
    }
}
