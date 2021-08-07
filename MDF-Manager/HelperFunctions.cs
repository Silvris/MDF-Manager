using Murmur;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace MDF_Manager
{
    class HelperFunctions
    {
        public static HashAlgorithm mm3h = MurmurHash.Create32(seed:0xFFFFFFFF);
        public static string ReadUniNullTerminatedString(BinaryReader br)
        {
            
            List<char> stringC = new List<char>();
            char newByte = br.ReadChar();
            while (newByte != 0)
            {
                stringC.Add(newByte);
                newByte = br.ReadChar();
            }
            return new string(stringC.ToArray());
        }

        public static void WriteUniNullTerminatedString(BinaryWriter bw, string str)
        {
            bw.Write(Encoding.Unicode.GetBytes(str.ToCharArray()));
            bw.Write((Int16)0);
        }

        public static Brush GetBrushFromHex(string hexColor)
        {
            BrushConverter bc = new BrushConverter();
            Brush newBrush = (Brush)bc.ConvertFrom(hexColor);
            return newBrush;
        }

        public static Brush GetBrushFromColor(Color c)
        {
            BrushConverter bc = new BrushConverter();
            Brush nb = (Brush)bc.ConvertFrom(c.ToString());
            return nb;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static uint Murmur3Hash(byte[] str)
        {
            return BitConverter.ToUInt32(mm3h.ComputeHash(str),0);
        }

        public static float Clamp(float input, float min, float max)
        {
            if (input > max)
            {
                return max;
            }
            else
            {
                if(input < min)
                {
                    return min;
                }
                else
                {
                    return input;
                }
            }
        }
        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}
