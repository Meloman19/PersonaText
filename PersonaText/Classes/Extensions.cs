using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace PersonaText
{
    static class ObservableCollectionExtentsion
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
                    returned += MSG.GetSystem();
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
                    returned += MSG.GetSystem();
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

    static class ListExtentsion
    {
        public static void ReadFNMP(this List<fnmp> list, string filename)
        {
            try
            {
                StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open));

                while (sr.EndOfStream == false)
                {
                    string str = sr.ReadLine();

                    int Index = Convert.ToInt32(str.Substring(0, str.IndexOf('=')));
                    string Char = str.Substring(str.IndexOf('=') + 1);

                    if (list.Exists(x => x.Index == Index))
                    {
                        fnmp fnmp = list.Find(x => x.Index == Index);
                        fnmp.Char = Char;
                    }
                    else
                    {
                        list.Add(new PersonaText.fnmp { Index = Index, Char = Char });
                    }
                }

                sr.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void WriteFNMP(this List<fnmp> list, string filename)
        {
            try
            {
                StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create));

                foreach (var CL in list)
                {
                    if (CL.Char != "")
                    {
                        string str = Convert.ToString(CL.Index) + "=" + Convert.ToString(CL.Char);
                        sw.WriteLine(str);
                        sw.Flush();
                    }
                }

                sw.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void ReadFONT(this List<fnmp> list, string filename)
        {
            try
            {
                FileStream FONT = new FileStream(filename, FileMode.Open, FileAccess.Read);
                MemoryStream FontDec = new MemoryStream();

                int MainHeaderSize = FONT.ReadInt();
                FONT.Position = 0xE;
                int TotalNumberOfGlyphs = FONT.ReadUshort();
                ushort GlyphSize_1 = FONT.ReadUshort();
                ushort GlyphSize_2 = FONT.ReadUshort();
                ushort GlyphSizeInByte = FONT.ReadUshort();
                byte BitPerPixel = Convert.ToByte((double)(GlyphSizeInByte * 8) / (GlyphSize_1 * GlyphSize_2));
                int NumberOfColor = Convert.ToInt32(Math.Pow(2, BitPerPixel));

                int GlyphCutTable_Pos = MainHeaderSize + NumberOfColor * 4 + 4;
                FONT.Position = GlyphCutTable_Pos - 4;
                int GlyphCutTable_Size = FONT.ReadInt();

                int UnknownPos = GlyphCutTable_Pos + GlyphCutTable_Size + 4;
                FONT.Position = UnknownPos - 4;
                int UnknownSize = FONT.ReadInt();

                int ReservedPos = UnknownPos + UnknownSize;
                int ReservedSize = TotalNumberOfGlyphs * 4;

                int DictionaryHeader_Pos = ReservedPos + ReservedSize;

                FONT.Position = DictionaryHeader_Pos;
                int DictionaryHeader_Size = FONT.ReadInt();
                int Dictionary_Size = FONT.ReadInt();
                int CompressedFontBlock_Size = FONT.ReadInt();
                int Dictionary_Pos = DictionaryHeader_Pos + DictionaryHeader_Size;

                FONT.Position = DictionaryHeader_Pos + 24;
                int GlyphPositionTable_Size = FONT.ReadInt();

                FONT.Position = Dictionary_Pos;

                int[,] Dictionary = new int[Dictionary_Size / 6, 2];
                for (int i = 0; i < Dictionary_Size / 6; i++)
                {
                    FONT.Position = FONT.Position + 2;
                    Dictionary[i, 0] = FONT.ReadUshort();
                    Dictionary[i, 1] = FONT.ReadUshort();
                }

                int CompressedFontBlock_Pos = Dictionary_Pos + Dictionary_Size + GlyphPositionTable_Size;

                FONT.Position = CompressedFontBlock_Pos;

                int temp = 0;
                for (int m = 0; m < CompressedFontBlock_Size; m += 2)
                {
                    int s4 = FONT.ReadUshort();
                    for (int i = 0; i < 16; i++)
                    {
                        temp =  Dictionary[temp, s4 % 2];
                        s4 = s4 >> 1;

                        if (Dictionary[temp, 0] == 0)
                        {
                            int a = Dictionary[temp, 1];

                            if (BitPerPixel == 4)
                            {
                                a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                            }

                            FontDec.WriteByte((byte)a);
                            temp = 0; 
                        }
                    }
                }

                FONT.Position = MainHeaderSize;

                List<Color> ColorBMP = new List<Color>();
                for (int i = 0; i < NumberOfColor; i++)
                {
                    byte r = (byte)FONT.ReadByte();
                    byte g = (byte)FONT.ReadByte();
                    byte b = (byte)FONT.ReadByte();
                    byte a = (byte)FONT.ReadByte();
                    ColorBMP.Add(Color.FromArgb(a, r, g, b));
                }
                BitmapPalette ColorPaletteBMP = new BitmapPalette(ColorBMP);

                FontDec.Position = 0;

                FONT.Position = GlyphCutTable_Pos;
                byte[,] GlyphCut = new byte[TotalNumberOfGlyphs, 2];
                for (int i = 0; i < TotalNumberOfGlyphs; i++)
                {
                    GlyphCut[i, 0] = (byte)FONT.ReadByte();
                    GlyphCut[i, 1] = (byte)FONT.ReadByte();
                }

                int k = 32;

                double ko = (double)BitPerPixel / 8;
                int size = Convert.ToInt32(GlyphSize_1 * GlyphSize_1 * ko);

                try
                {
                    byte[] data = new byte[size];
                    FontDec.Read(data, 0, size);

                    BitmapSource BMP = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Indexed4, ColorPaletteBMP, data, 16);

                    if (list.Exists(x => x.Index == k))
                    {
                        fnmp fnmp = list.Find(x => x.Index == k);
                        fnmp.Cut = new MyByte { Left = Convert.ToByte(GlyphCut[0, 0] + 5), Right = Convert.ToByte(GlyphCut[0, 1] - 5) };
                        fnmp.Image = BMP;
                        fnmp.Image_data = data;
                    }
                    else
                    {
                        list.Add(new fnmp { Index = k, Char = "", Cut = new MyByte { Left = Convert.ToByte(GlyphCut[0, 0] + 5), Right = Convert.ToByte(GlyphCut[0, 1] - 5) }, Image = BMP, Image_data = data });
                    }

                    k++;
                }
                catch
                {

                }

                for (int i = 1; i < TotalNumberOfGlyphs; i++)
                {
                    byte[] data = new byte[size];
                    FontDec.Read(data, 0, size);

                    BitmapSource BMP = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Indexed4, ColorPaletteBMP, data, 16);

                    if (list.Exists(x => x.Index == k))
                    {
                        fnmp fnmp = list.Find(x => x.Index == k);
                        fnmp.Cut = new MyByte { Left = GlyphCut[i, 0], Right = GlyphCut[i, 1] };
                        fnmp.Image = BMP;
                        fnmp.Image_data = data;
                    }
                    else
                    {
                        list.Add(new fnmp { Index = k, Char = "", Cut = new MyByte { Left = GlyphCut[i, 0], Right = GlyphCut[i, 1] }, Image = BMP, Image_data = data });
                    }

                    k++;
                }

                FontDec.Close();
                FONT.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void ReadShift(this List<glyphYshift> list)
        {
            list.Add(new glyphYshift { Index = 81, Shift = 2 });
            list.Add(new glyphYshift { Index = 103, Shift = 2 });
            list.Add(new glyphYshift { Index = 106, Shift = 2 });
            list.Add(new glyphYshift { Index = 112, Shift = 2 });
            list.Add(new glyphYshift { Index = 113, Shift = 2 });
            list.Add(new glyphYshift { Index = 121, Shift = 2 });
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    static class StreamExtension
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

        public static void WriteIntArray(this Stream stream, int[] Number)
        {
            foreach (var N in Number)
            {
                stream.WriteInt(N);
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
            return System.Text.Encoding.ASCII.GetString(buffer.Where(x => x != 0).ToArray());
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

        public static void SaveToFile(this Stream stream, string Path)
        {
            using (FileStream FS = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite))
            {
                long temp = stream.Position;
                stream.Position = 0;
                stream.CopyTo(FS);
                stream.Position = temp;
            }
        }
    }

    static class ByteArrayExtension
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

    static class StringExtension
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