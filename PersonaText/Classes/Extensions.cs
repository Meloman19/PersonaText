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

        public static void GetBitmapList(this ObservableCollection<BitmapList> List, List<MyByteArray> Text, List<fnmp> CharList, System.Drawing.Color Color)
        {
            List.Clear();

            List<Color> ColorBMP = new List<Color>();

            ColorBMP.Add(new Color { A = 0, R = 0, G = 0, B = 0 });
            for (int i = 1; i < 16; i++)
            {
                ColorBMP.Add(new Color
                {
                    A = Util.ByteTruncate(i * 0x10),
                    R = Color.R,
                    G = Color.G,
                    B = Color.B
                });
            }
            BitmapPalette ColorPaletteBMP = new BitmapPalette(ColorBMP);

            int y = 0;
            int linewidth = 0;
            foreach (var a in Text)
            {
                if (a.Type == "System")
                {
                    if (Util.ByteArrayCompareWithSimplest(a.Bytes, new byte[] { 0x0A }))
                    {
                        y++;
                        linewidth = 0;
                    }

                }
                else
                {
                    for (int i = 0; i < a.Bytes.Length; i++)
                    {
                        fnmp fnmp;
                        if (0x20 <= a.Bytes[i] & a.Bytes[i] < 0x80)
                        {
                            fnmp = CharList.Find(x => x.Index == a.Bytes[i]);
                        }
                        else if (0x80 <= a.Bytes[i] & a.Bytes[i] < 0xF0)
                        {
                            int newindex = (a.Bytes[i] - 0x81) * 0x80 + a.Bytes[i + 1] + 0x20;

                            i++;
                            fnmp = CharList.Find(x => x.Index == newindex);
                        }
                        else
                        {
                            Console.WriteLine("ASD");
                            fnmp = null;
                        }

                        if (fnmp != null)
                        {
                            BitmapSource BS = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Indexed4, ColorPaletteBMP, fnmp.Image_data, 16);

                            int x_pos = linewidth - fnmp.Cut.Left;
                            linewidth += fnmp.Cut.Right - fnmp.Cut.Left - 1;
                            glyphYshift temp = Static.FontMap.char_shift.Find(x => x.Index == fnmp.Index);
                            int y_pos = temp != null ? 25 * y + temp.Shift : 25 * y;

                            List.Add(new BitmapList { Bitmap = BS, posX = x_pos, posY = y_pos });
                        }
                    }
                }
            }
        }

        public static void GetBitmapList(this ObservableCollection<BitmapList> List, ObservableCollection<MyByteArray> Text, List<fnmp> CharList, System.Drawing.Color Color)
        {
            List.Clear();

            List<Color> ColorBMP = new List<Color>();

            ColorBMP.Add(new Color { A = 0, R = 0, G = 0, B = 0 });
            for (int i = 1; i < 16; i++)
            {
                ColorBMP.Add(new Color
                {
                    A = Util.ByteTruncate(i * 0x10),
                    R = Color.R,
                    G = Color.G,
                    B = Color.B
                });
            }
            BitmapPalette ColorPaletteBMP = new BitmapPalette(ColorBMP);

            int y = 0;
            int linewidth = 0;
            foreach (var a in Text)
            {
                if (a.Type == "System")
                {
                    if (Util.ByteArrayCompareWithSimplest(a.Bytes, new byte[] { 0x0A }))
                    {
                        y++;
                        linewidth = 0;
                    }

                }
                else
                {
                    for (int i = 0; i < a.Bytes.Length; i++)
                    {
                        fnmp fnmp;
                        if (0x20 <= a.Bytes[i] & a.Bytes[i] < 0x80)
                        {
                            fnmp = CharList.Find(x => x.Index == a.Bytes[i]);
                        }
                        else if (0x80 <= a.Bytes[i] & a.Bytes[i] < 0xF0)
                        {
                            int newindex = (a.Bytes[i] - 0x81) * 0x80 + a.Bytes[i + 1] + 0x20;

                            i++;
                            fnmp = CharList.Find(x => x.Index == newindex);
                        }
                        else
                        {
                            Console.WriteLine("ASD");
                            fnmp = null;
                        }

                        if (fnmp != null)
                        {
                            BitmapSource BS = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Indexed4, ColorPaletteBMP, fnmp.Image_data, 16);

                            int x_pos = linewidth - fnmp.Cut.Left;
                            linewidth += fnmp.Cut.Right - fnmp.Cut.Left - 1;
                            glyphYshift temp = Static.FontMap.char_shift.Find(x => x.Index == fnmp.Index);
                            int y_pos = temp != null ? 25 * y + temp.Shift : 25 * y;

                            List.Add(new BitmapList { Bitmap = BS, posX = x_pos, posY = y_pos });
                        }
                    }

                }
            }
        }

        public static string GetChar(this List<fnmp> FontMap, int index)
        {
            string returned = "";

            if (FontMap.Exists(x => x.Index == index))
            {
                fnmp fnmp = FontMap.Find(x => x.Index == index);

                if (fnmp.Char.Length == 0)
                {
                    returned += "{C}";
                }
                else if (fnmp.Char.Length == 1)
                {
                    returned += fnmp.Char;
                }
                else
                {
                    returned += "<" + fnmp.Char + ">";
                }
            }
            else
            {
                returned += "{NC}";
            }

            return returned;
        }

        public static string GetString(this ObservableCollection<MyByteArray> ByteCollection, List<fnmp> CharList, bool OnlySystem)
        {
            string returned = "";

            if (OnlySystem)
            {
                foreach (var MSG in ByteCollection)
                {
                    returned += MSG.GetSystem(CharList);
                }
            }
            else
            {
                foreach (var MSG in ByteCollection)
                {
                    returned += MSG.GetText(CharList);
                }
            }

            return returned;
        }

        public static string GetString(this List<MyByteArray> ByteCollection, List<fnmp> CharList, bool OnlySystem)
        {
            string returned = "";

            if (OnlySystem)
            {
                foreach (var MSG in ByteCollection)
                {
                    returned += MSG.GetSystem(CharList);
                }
            }
            else
            {
                foreach (var MSG in ByteCollection)
                {
                    returned += MSG.GetText(CharList);
                }
            }

            return returned;
        }
    }

    public static class StreamExtension
    {
        public static void WriteLong(this Stream stream, long number)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(number);
                if (BitConverter.IsLittleEndian)
                {
                    stream.Write(buffer, 0, 8);
                }
                else
                {
                    buffer = buffer.Reverse().ToArray();
                    stream.Write(buffer, 0, 8);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

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

        public static void WriteInt(this Stream Stream, long Number)
        {
            int temp = Convert.ToInt32(Number);
            try
            {
                byte[] buffer = BitConverter.GetBytes(temp);
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

        public static string ReadString(this Stream stream, int length)
        {
            byte[] buffer = stream.ReadByteArray(length);
            return System.Text.Encoding.ASCII.GetString(buffer.Where(x=>x!=0).ToArray());
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

        public static ushort ReadUshort(this Stream stream)
        {
            byte[] buffer = new byte[2];
            try
            {
                stream.Read(buffer, 0, 2);
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

        public static int ReadInt(this Stream stream)
        {
            byte[] buffer = new byte[4];
            try
            {
                stream.Read(buffer, 0, 4);
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

        public static int[] ReadIntArray(this Stream stream, int count)
        {
            List<int> returned = new List<int>();

            for (int i = 0; i < count; i++)
            {
                returned.Add(stream.ReadInt());
            }

            return returned.ToArray();
        }

        public static byte[] ReadByteArray(this Stream stream, int count)
        {
            List<byte> returned = new List<byte>();

            for (int i = 0; i < count; i++)
            {
                returned.Add((byte)stream.ReadByte());
            }

            return returned.ToArray();
        }

        public static int[][] ReadIntArrayArray(this Stream stream, int count, int stride)
        {
            int[][] returned = new int[count][];

            for (int i = 0; i < count; i++)
            {
                returned[i] = new int[stride];
                for (int k = 0; k < stride; k++)
                {
                    returned[i][k] = stream.ReadInt();
                }
            }

            return returned;
        }

        public static long ReadLong(this Stream stream)
        {
            byte[] buffer = new byte[8];
            try
            {
                stream.Read(buffer, 0, 8);
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.ToInt64(buffer, 0);
                }
                else
                {
                    return BitConverter.ToInt64(buffer.Reverse().ToArray(), 0);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 0;
            }
        }

        public static bool CheckEntrance(this Stream stream, byte[] bytes)
        {
            if (bytes.Length != 0)
            {
                if (stream.CanRead)
                {
                    if (stream.ReadByte() == bytes[0])
                    {
                        return stream.CheckEntrance(bytes.Skip(1).ToArray());
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

    public static class MemoryStreamExtension
    {
        public static void SaveToFile(this MemoryStream MemoryStream, string Path)
        {

            try
            {
                FileStream FS = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite);
                long temp = MemoryStream.Position;
                MemoryStream.Position = 0;
                MemoryStream.CopyTo(FS);
                FS.Close();
            }
            catch
            {

            }

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

        public static List<MyByteArray> GetMyByteArray(this string String, List<fnmp> FontMap)
        {
            List<MyByteArray> MyByteArrayList = new List<MyByteArray>();

            List<byte> LB = new List<byte>();
            int Index = 0;

            string[] split = Regex.Split(String, "(\r\n|\r|\n)");

            foreach (var a in split)
            {
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                {
                    MyByteArrayList.Add(new MyByteArray { Index = Index, Type = "System", Bytes = new byte[] { 0x0A } });
                    Index++;
                }
                else
                {
                    string[] splitstr = Regex.Split(a, @"({[^}]+})");

                    foreach (var b in splitstr)
                    {
                        if (Regex.IsMatch(b, @"{.+}"))
                        {
                            MyByteArrayList.Add(new MyByteArray { Index = Index, Type = "System", Bytes = b.Substring(1, b.Length - 2).GetSystemByte() });
                            Index++;
                        }
                        else
                        {
                            string[] splitsubstr = Regex.Split(b, @"(<[^>]+>)");
                            List<byte> ListByte = new List<byte>();

                            foreach (var c in splitsubstr)
                            {
                                if (Regex.IsMatch(c, @"<.+>"))
                                {
                                    ListByte.AddChar(c.Substring(1, c.Length - 2), FontMap);
                                }
                                else
                                {
                                    ListByte.AddRange(c.GetEncodeByte(FontMap));
                                }
                            }

                            MyByteArrayList.Add(new MyByteArray { Index = Index, Type = "Text", Bytes = ListByte.ToArray() });
                        }
                    }
                }
            }

            return MyByteArrayList;
        }
    }


}