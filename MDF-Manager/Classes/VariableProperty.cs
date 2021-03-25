using System;
using System.Collections.Generic;
using System.Text;

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
    public class Float4
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }
        public Float4(float fX, float fY, float fZ, float fW)
        {
            x = fX;
            y = fY;
            z = fZ;
            w = fW;
        }
    }
    interface IVariableProp
    {
        public string name { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public object defaultValue { get; set; }
    }
    public class FloatProperty : IVariableProp
    {
        private string _Name;
        private int _Size;//this isn't really important, but its good to keep for JSON purposes
        private string _Type;
        private Float _Default;
        public string name { get => _Name; set => _Name = value; }
        public int size { get => _Size; set => _Size = value; }
        public string type { get => _Type; set => _Type = value; }
        public object defaultValue { get => _Default; set => _Default = (Float)value; }
    }
    public class Float4Property : IVariableProp
    {
        private string _Name;
        private int _Size;//this isn't really important, but its good to keep for JSON purposes
        private string _Type;
        private Float4 _Default;
        public string name { get => _Name; set => _Name = value; }
        public int size { get => _Size; set => _Size = value; }
        public string type { get => _Type; set => _Type = value; }
        public object defaultValue { get => _Default; set => _Default = (Float4)value; }
    }
}
