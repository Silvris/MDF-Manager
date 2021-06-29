using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace MDF_Manager.Classes
{
    public class Float
    {
        public float data { get; set; }
        public Float(float fData)
        {
            data = fData;
        }
    }
    public class Float4 : INotifyPropertyChanged
    {
        private Color _mColor;
        private Brush _Brush;
        private float _X;
        private float _Y;
        private float _Z;
        private float _W;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateBrush()
        {
            byte[] hexArray = { _mColor.R, _mColor.G, _mColor.B };
            string hexBrush = "#" + BitConverter.ToString(hexArray).Replace("-", "");
            mBrush = HelperFunctions.GetBrushFromHex(hexBrush);
            OnPropertyChanged("mBrush");
        }
        public void UpdateColor()
        {
            _mColor.ScR = HelperFunctions.Clamp(x, 0, 1);
            _mColor.ScG = HelperFunctions.Clamp(y, 0, 1);
            _mColor.ScB = HelperFunctions.Clamp(z, 0, 1);
            _mColor.ScA = HelperFunctions.Clamp(w, 0, 1);
            UpdateBrush();
        }
        public float x { get => _X; set { _X = value; UpdateColor(); OnPropertyChanged("x"); } }
        public float y { get => _Y; set { _Y = value; UpdateColor(); OnPropertyChanged("y"); } }
        public float z { get => _Z; set { _Z = value; UpdateColor(); OnPropertyChanged("z"); } }
        public float w { get => _W; set { _W = value; UpdateColor(); OnPropertyChanged("w"); } }
        public Color mColor { get { return _mColor; } set { _mColor = value; UpdateBrush(); } }
        public Brush mBrush { get { return _Brush; } set { _Brush = value; } }
        public Float4(float fX, float fY, float fZ, float fW)
        {
            x = fX;
            y = fY;
            z = fZ;
            w = fW;
        }

    }
    public interface IVariableProp
    {
        int NameOffsetIndex { get; set; }
        int ValOffset { get; set; }
        string name { get; set; }
        object value { get; set; }
        int[] indexes { get; set; }
        int GetSize();
        int GetPropHeaderSize();
        void Export(BinaryWriter bw, MDFTypes type, ref long propHeadOff, ref long propOff, long basePropOff, long stringTableOff, List<int> strTableOffs);
    }
    public class FloatProperty : IVariableProp
    {
        public int NameOffsetIndex { get; set; }
        public int ValOffset { get; set; }
        private string _Name;
        private Float _Default;
        public string name { get => _Name; set => _Name = value; }
        public object value { get => _Default; set => _Default = (Float)value; }
        public int[] indexes { get; set;}
        public FloatProperty(string Name, Float Value,int matIndex, int propIndex)
        {
            indexes = new int[2];
            name = Name;
            value = Value;
            indexes[0] = matIndex;
            indexes[1] = propIndex;
        }
        public int GetSize()
        {
            return 4;
        }
        public int GetPropHeaderSize()
        {
            return 24;
        }

        public void Export(BinaryWriter bw, MDFTypes type, ref long propHeadOff, ref long propOff, long basePropOff, long stringTableOff, List<int> strTableOffs)
        {
            uint innerPropOff = (uint)(propOff - basePropOff);
            bw.BaseStream.Seek(propHeadOff, SeekOrigin.Begin);
            bw.Write(stringTableOff + strTableOffs[NameOffsetIndex]);
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.ASCII.GetBytes(name)));//potentially UTF8 rather than ASCII, but further testing would be required
            if(type >= MDFTypes.RE3)
            {
                bw.Write(innerPropOff);
                bw.Write(1);
            }
            else
            {
                bw.Write(1);
                bw.Write(innerPropOff);
            }
            //update propHeadOff then write value to floatarr
            propHeadOff += GetPropHeaderSize();

            bw.BaseStream.Seek(propOff, SeekOrigin.Begin);
            bw.Write(_Default.data);
            propOff += GetSize();
        }
    }
    public class Float4Property : IVariableProp, INotifyPropertyChanged
    {
        public int NameOffsetIndex { get; set; }
        public int ValOffset { get; set; }
        private string _Name;
        private Float4 _Default;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string name { get => _Name; set => _Name = value; }
        public object value { get => _Default; set { _Default = (Float4)value; OnPropertyChanged("value"); _Default.UpdateColor(); } }
        public int[] indexes { get; set; }

        public Float4Property(string Name, Float4 Value, int matIndex, int propIndex)
        {
            indexes = new int[2];
            name = Name;
            value = Value;
            indexes[0] = matIndex;
            indexes[1] = propIndex;
        }
        public int GetPropHeaderSize()
        {
            return 24;
        }
        public int GetSize()
        {
            return 16;
        }

        public void Export(BinaryWriter bw, MDFTypes type, ref long propHeadOff, ref long propOff, long basePropOff, long stringTableOff, List<int> strTableOffs)
        {
            uint innerPropOff = (uint)(propOff - basePropOff);
            bw.BaseStream.Seek(propHeadOff, SeekOrigin.Begin);
            bw.Write(stringTableOff + strTableOffs[NameOffsetIndex]);
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(HelperFunctions.Murmur3Hash(Encoding.ASCII.GetBytes(name)));//potentially UTF8 rather than ASCII, but further testing would be required
            if (type >= MDFTypes.RE3)
            {
                bw.Write(innerPropOff);
                bw.Write(4);
            }
            else
            {
                bw.Write(4);
                bw.Write(innerPropOff);
            }
            //update propHeadOff then write value to floatarr
            propHeadOff += GetPropHeaderSize();

            bw.BaseStream.Seek(propOff, SeekOrigin.Begin);
            bw.Write(_Default.x);
            bw.Write(_Default.y);
            bw.Write(_Default.z);
            bw.Write(_Default.w);
            propOff += GetSize();
        }
    }
}
