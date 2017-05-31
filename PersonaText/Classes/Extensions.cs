using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace PersonaText
{
    public static class ObservableCollectionExtentsion
    {
        public static void AddChar(this List<byte> ByteList, char Char, List<fnmp> FontMap)
        {
            ByteList.AddChar(Char.ToString(), FontMap);
        }

        public static void AddChar(this List<byte> ByteList, string Char, List<fnmp> FontMap)
        {
            if (Char != "")
            {
                fnmp fnmp = FontMap.Find(x => x.Char == Char);
                if (fnmp != null)
                {
                    if (fnmp.Index < 0x80)
                    {
                        ByteList.Add((byte)fnmp.Index);
                    }
                    else
                    {
                        byte byte2 = Convert.ToByte((fnmp.Index - 0x20) % 0x80);
                        byte byte1 = Convert.ToByte(((fnmp.Index - 0x20 - byte2) / 0x80) + 0x81);

                        ByteList.Add(byte1);
                        ByteList.Add(byte2);
                    }
                }
            }
        }

        public static void GetBitmapList(this ObservableCollection<BitmapList> List, string Text, ref List<fnmp> CharList, System.Drawing.Color Color)
        {
            var split = Regex.Split(Text, "\r\n|\r|\n");

            List.Clear();

            List<System.Windows.Media.Color> ColorBMP = new List<System.Windows.Media.Color>();

            ColorBMP.Add(new System.Windows.Media.Color { A = 0, R = 0, G = 0, B = 0 });
            for (int i = 1; i < 16; i++)
            {
                ColorBMP.Add(new System.Windows.Media.Color
                {
                    A = trunc(i * 0x10),
                    R = Color.R,
                    G = Color.G,
                    B = Color.B
                });
            }
            BitmapPalette ColorPaletteBMP = new BitmapPalette(ColorBMP);

            for (int i = 0; i < split.Length; i++)
            {
                int linewidth = 0;
                for (int k = 0; k < split[i].Length; k++)
                {
                    string Char = split[i][k].ToString();
                    fnmp fnmp = CharList.Find(x => x.Char == Char);
                    if (fnmp != null)
                    {
                        BitmapSource BS = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Indexed4, ColorPaletteBMP, fnmp.Image_data, 16);

                        int x_pos = linewidth - fnmp.Cut.Left;
                        linewidth += fnmp.Cut.Right - fnmp.Cut.Left - 1;
                        glyphYshift temp = Static.FontMap.char_shift.Find(x => x.Index == fnmp.Index);
                        int y_pos = temp != null ? 25 * i + temp.Shift : 25 * i;

                        List.Add(new BitmapList { Bitmap = BS, posX = x_pos, posY = y_pos });
                    }
                }
            }
        }

        private static byte trunc(int value)
        {
            if (value < 0) { return 0; }
            else if (value > 255) { return 255; }
            else { return (byte)value; }
        }

    }

    public static class StreamExtension
    {
        public static void WriteInt(this Stream Stream, int Number)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(Number);
                if (BitConverter.IsLittleEndian)
                {
                    Stream.Write(buffer, 0, 4);
                }
                else
                {
                    buffer = buffer.Reverse().ToArray();
                    Stream.Write(buffer, 0, 4);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void WriteUshort(this Stream Stream, int Number)
        {
            Stream.WriteUshort((ushort)Number);
        }

        public static void WriteUshort(this Stream Stream, ushort Number)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(Number);
                if (BitConverter.IsLittleEndian)
                {
                    Stream.Write(buffer, 0, 2);
                }
                else
                {
                    buffer = buffer.Reverse().ToArray();
                    Stream.Write(buffer, 0, 2);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void WriteString(this Stream Stream, string String, int Length)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(String);
                Stream.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < Length - String.Length; i++)
                {
                    Stream.WriteByte(0);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static int ReadInt(this Stream Stream)
        {
            byte[] buffer = new byte[4];
            try
            {
                Stream.Read(buffer, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.ToInt32(buffer, 0);
                }
                else
                {
                    return BitConverter.ToInt32(buffer.Reverse().ToArray(), 0);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 0;
            }
        }

        public static ushort ReadUshort(this Stream Stream)
        {
            byte[] buffer = new byte[2];
            try
            {
                Stream.Read(buffer, 0, 2);
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.ToUInt16(buffer, 0);
                }
                else
                {
                    return BitConverter.ToUInt16(buffer.Reverse().ToArray(), 0);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 0;
            }
        }

        public static bool CheckEntrance(this Stream FS, byte[] Bytes)
        {
            if (Bytes.Length != 0)
            {
                if (FS.CanRead)
                {
                    if (FS.ReadByte() == Bytes[0])
                    {
                        return FS.CheckEntrance(Bytes.Skip(1).ToArray());
                    }
                    else
                    {
                        return false;
                    }
                }
                else { return false; }
            }
            else { return true; }
        }
    }

    public static class ByteArrayExtension
    {
        public static bool CheckEntrance(this byte[] B, byte[] Bytes, int StartIndex)
        {
            if (Bytes.Length != 0)
            {
                if (StartIndex < B.Length)
                {
                    if (B[StartIndex] == Bytes[0])
                    {
                        return B.CheckEntrance(Bytes.Skip(1).ToArray(), StartIndex + 1);
                    }
                    else
                    {
                        return false;
                    }
                }
                else { return false; }
            }
            else { return true; }
        }
    }

    public static class StringExtension
    {
        public static byte[] GetEncodeByte(this string String, List<fnmp> FontMap)
        {
            List<byte> LB = new List<byte>();
            foreach (var C in String)
            {
                LB.AddChar(C, FontMap);
            }
            return LB.ToArray();
        }

        public static byte[] GetSystemByte(this string String)
        {
            List<byte> ListByte = new List<byte>();
            string[] temp = String.Split(' ');
            foreach (var a in temp)
            {
                try
                {
                    
                    ListByte.Add(Convert.ToByte(a, 16));
                }
                catch
                {
                    ListByte.Clear();
                    ListByte.AddRange(System.Text.Encoding.ASCII.GetBytes(String));
                    return ListByte.ToArray();
                }
            }
            return ListByte.ToArray();
        }
    }
}