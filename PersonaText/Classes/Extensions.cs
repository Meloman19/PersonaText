using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace PersonaText
{
    static class ListExtentsion
    {
        public static void AddChar(this List<byte> ByteList, char Char, IList<FnMpData> FontMap)
        {
            ByteList.AddChar(Char.ToString(), FontMap);
        }

        public static void AddChar(this List<byte> ByteList, string Char, IList<FnMpData> FontMap)
        {
            if (Char != "")
            {
                FnMpData fnmp = FontMap.FirstOrDefault(x => x.Char == Char);

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

        public static string GetChar(this IList<FnMpData> FontMap, int index)
        {
            string returned = "";

            FnMpData fnmp = FontMap.FirstOrDefault(x => x.Index == index);
            if (fnmp == null)
            {
                returned += "<NCHAR>";
            }
            else
            {
                if (fnmp.Char.Length == 0)
                {
                    returned += "<CHAR>";
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

            return returned;
        }

        public static string GetString(this IList<MyByteArray> ByteCollection, IList<FnMpData> CharList, bool OnlySystem)
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

        public static void GetMyByteArray(this BindingList<MyByteArray> MyByteArrayList, string String, IList<FnMpData> FontMap)
        {
            MyByteArrayList.Clear();
            foreach (var a in String.GetMyByteArray(FontMap))
            {
                MyByteArrayList.Add(a);
            }
        }

        public static void parseString(this IList<MyByteArray> returned, byte[] B)
        {
            returned.Clear();

            arrayType type = arrayType.Text;
            List<byte> temp = new List<byte>();

            int Index = 0;

            for (int i = 0; i < B.Length; i++)
            {
                if (0x20 <= B[i] & B[i] < 0x80)
                {
                    temp.Add(B[i]);
                }
                else if (0x80 <= B[i] & B[i] < 0xF0)
                {
                    temp.Add(B[i]);
                    i = i + 1;
                    temp.Add(B[i]);
                }
                else
                {
                    if (0x00 <= B[i] & B[i] < 0x20)
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(new MyByteArray { Index = Index, Type = type, Bytes = temp.ToArray() });
                            Index++;
                            temp.Clear();
                        }

                        type = arrayType.System;
                        temp.Add(B[i]);

                        returned.Add(new MyByteArray { Index = Index, Type = type, Bytes = temp.ToArray() });
                        Index++;
                        type = arrayType.Text;
                        temp.Clear();
                    }
                    else
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(new MyByteArray { Index = Index, Type = type, Bytes = temp.ToArray() });
                            Index++;
                            type = arrayType.Text;
                            temp.Clear();
                        }


                        type = arrayType.System;
                        temp.Add(B[i]);
                        int count = (B[i] - 0xF0) * 2 - 1;
                        for (int k = 0; k < count; k++)
                        {
                            i++;
                            temp.Add(B[i]);
                        }

                        returned.Add(new MyByteArray { Index = Index, Type = type, Bytes = temp.ToArray() });
                        Index++;
                        type = arrayType.Text;
                        temp.Clear();
                    }
                }
            }

            if (temp.Count != 0)
            {
                returned.Add(new MyByteArray { Index = Index, Type = type, Bytes = temp.ToArray() });
                temp.Clear();
            }
        }

        public static byte[] getAllBytes(this IList<MyByteArray> listMyByteArray)
        {
            List<byte> returned = new List<byte>();
            foreach (var a in listMyByteArray)
            {
                returned.AddRange(a.Bytes);
            }
            return returned.ToArray();
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

        public static void GetIndex(this List<byte> ByteList, char Char)
        {
            string CharStr = Char.ToString();
            ByteList.GetIndex(CharStr);
        }

        public static void GetIndex(this List<byte> ByteList, string Char)
        {
            if (Char != "")
            {
                FnMpData fnmp = Static.FontMap.new_char.List.FirstOrDefault(x => x.Char == Char); ;
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

        public static void SaveToFile(this Stream stream, string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
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
        public static byte[] GetEncodeByte(this string String, IList<FnMpData> FontMap)
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

        public static List<MyByteArray> GetMyByteArray(this string String, IList<FnMpData> FontMap)
        {
            List<MyByteArray> MyByteArrayList = new List<MyByteArray>();

            int Index = 0;

            string[] split = Regex.Split(String, "(\r\n|\r|\n)");

            foreach (var a in split)
            {
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                {
                    MyByteArrayList.Add(new MyByteArray { Index = Index, Type = arrayType.System, Bytes = new byte[] { 0x0A } });
                    Index++;
                }
                else
                {
                    string[] splitstr = Regex.Split(a, @"({[^}]+})");

                    foreach (var b in splitstr)
                    {
                        if (Regex.IsMatch(b, @"{.+}"))
                        {
                            MyByteArrayList.Add(new MyByteArray { Index = Index, Type = arrayType.System, Bytes = b.Substring(1, b.Length - 2).GetSystemByte() });
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

                            MyByteArrayList.Add(new MyByteArray { Index = Index, Type = arrayType.Text, Bytes = ListByte.ToArray() });
                        }
                    }
                }
            }

            return MyByteArrayList;
        }
    }

    static class Extension
    {
        public static void ReadFNMP(this IList<FnMpData> list, string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open));

                    while (sr.EndOfStream == false)
                    {
                        string str = sr.ReadLine();

                        int Index = Convert.ToInt32(str.Substring(0, str.IndexOf('=')));
                        string Char = str.Substring(str.IndexOf('=') + 1);
                        if (Char.Length > 3)
                            Char = Char.Substring(0, 3);

                        FnMpData fnmp = list.FirstOrDefault(x => x.Index == Index);
                        if (fnmp == null)
                            list.Add(new FnMpData { Index = Index, Char = Char });
                        else
                            fnmp.Char = Char;
                    }

                    sr.Dispose();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        public static bool WriteFNMP(this IList<FnMpData> list, string filename)
        {
            if (File.Exists(filename))
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
                    return false;
                }
                return true;
            }
            return false;
        }

        public static void ReadFONT(this CharList list, string filename)
        {
            try
            {
                FileStream FONT = new FileStream(filename, FileMode.Open, FileAccess.Read);
                MemoryStream FontDec = new MemoryStream();

                int MainHeaderSize = FONT.ReadInt();
                FONT.Position = 0xE;
                int TotalNumberOfGlyphs = FONT.ReadUshort();
                ushort GlyphSize_1 = FONT.ReadUshort();
                list.Width = GlyphSize_1;
                ushort GlyphSize_2 = FONT.ReadUshort();
                list.Height = GlyphSize_2;
                ushort GlyphSizeInByte = FONT.ReadUshort();
                byte BitPerPixel = Convert.ToByte((double)(GlyphSizeInByte * 8) / (GlyphSize_1 * GlyphSize_2));
                int NumberOfColor = Convert.ToInt32(Math.Pow(2, BitPerPixel));

                if (BitPerPixel == 4)
                {
                    list.PixelFormat = PixelFormats.Indexed4;
                }
                else if (BitPerPixel == 8)
                {
                    list.PixelFormat = PixelFormats.Indexed8;
                }
                else MessageBox.Show("ReadFONT: Unknown PixelFormat");

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
                        temp = Dictionary[temp, s4 % 2];
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
                list.Palette = new BitmapPalette(ColorBMP);

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

                    FnMpData fnmp = list.List.FirstOrDefault(x => x.Index == k);
                    if (fnmp == null)
                        list.List.Add(new FnMpData { Index = k, Char = "", Cut = new MyByte { Left = Convert.ToByte(GlyphCut[0, 0] + 5), Right = Convert.ToByte(GlyphCut[0, 1] - 5) }, Image_data = data });
                    else
                    {
                        fnmp.Cut = new MyByte { Left = 9, Right = 18 };
                        fnmp.Image_data = data;
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

                    FnMpData fnmp = list.List.FirstOrDefault(x => x.Index == k);
                    if (fnmp == null)
                        list.List.Add(new FnMpData { Index = k, Char = "", Cut = new MyByte { Left = GlyphCut[i, 0], Right = GlyphCut[i, 1] }, Image_data = data });
                    else
                    {
                        fnmp.Cut = new MyByte { Left = GlyphCut[i, 0], Right = GlyphCut[i, 1] };
                        fnmp.Image_data = data;
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

        public static BitmapSource DrawText(this IList<MyByteArray> text, CharList CharList, Color Color)
        {
            BitmapPalette ColorPaletteBMP = Util.CreatePallete(Color);
            ImageData returned = null;
            ImageData line = null;
            foreach (var a in text)
            {
                if (a.Type == arrayType.System)
                {
                    if (Util.ByteArrayCompareWithSimplest(a.Bytes, new byte[] { 0x0A }))
                    {
                        if (returned == null)
                        {
                            if (line == null)
                            {
                                returned = new ImageData(PixelFormats.Indexed4, 32);
                            }
                            else
                            {
                                returned = line;
                                line = null;
                            }
                        }
                        else
                        {
                            if (line == null)
                            {
                                returned = ImageData.MergeUpDown(returned, new ImageData(PixelFormats.Indexed4, 32));
                            }
                            else
                            {
                                returned = ImageData.MergeUpDown(returned, line);
                                line = null;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < a.Bytes.Length; i++)
                    {
                        FnMpData fnmp;
                        if (0x20 <= a.Bytes[i] & a.Bytes[i] < 0x80)
                        {
                            fnmp = CharList.List.FirstOrDefault(x => x.Index == a.Bytes[i]);
                        }
                        else if (0x80 <= a.Bytes[i] & a.Bytes[i] < 0xF0)
                        {
                            int newindex = (a.Bytes[i] - 0x81) * 0x80 + a.Bytes[i + 1] + 0x20;

                            i++;
                            fnmp = CharList.List.FirstOrDefault(x => x.Index == newindex);
                        }
                        else
                        {
                            Console.WriteLine("ASD");
                            fnmp = null;
                        }

                        if (fnmp != null)
                        {
                            glyphYshift temp = Static.FontMap.char_shift.Find(x => x.Index == fnmp.Index);
                            ImageData glyph = new ImageData(fnmp.Image_data, PixelFormats.Indexed4, 32, 32);
                            glyph = temp == null ? ImageData.Crop(glyph, fnmp.Cut.Left, fnmp.Cut.Right - 1) : ImageData.Shift(ImageData.Crop(glyph, fnmp.Cut.Left, fnmp.Cut.Right - 1), temp.Shift);
                            line = ImageData.MergeLeftRight(line, glyph);
                        }
                    }
                }
            }
            returned = ImageData.MergeUpDown(returned, line);
            return returned == null ? null : BitmapSource.Create(returned.PixelWidth, returned.PixelHeight, 96, 96, returned.PixelFormat, ColorPaletteBMP, returned.Data, returned.Stride);
        }

        public static void GetMyByteArray(this List<MyByteArray> MyByteArrayList, string String, IList<FnMpData> FontMap)
        {
            MyByteArrayList.Clear();

            int Index = 0;

            string[] split = Regex.Split(String, "(\r\n|\r|\n)");

            foreach (var a in split)
            {
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                {
                    MyByteArrayList.Add(new MyByteArray { Index = Index, Type = arrayType.System, Bytes = new byte[] { 0x0A } });
                    Index++;
                }
                else
                {
                    string[] splitstr = Regex.Split(a, @"({[^}]+})");

                    foreach (var b in splitstr)
                    {
                        if (Regex.IsMatch(b, @"{.+}"))
                        {
                            MyByteArrayList.Add(new MyByteArray { Index = Index, Type = arrayType.System, Bytes = b.Substring(1, b.Length - 2).GetSystemByte() });
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

                            MyByteArrayList.Add(new MyByteArray { Index = Index, Type = arrayType.Text, Bytes = ListByte.ToArray() });
                        }
                    }
                }
            }
        }

        public static BitmapSource ChangePallete(this BitmapSource image, Color color)
        {
            int Stride = (image.Format.BitsPerPixel * image.PixelWidth + 7) / 8;
            byte[] buffer = new byte[Stride * image.PixelHeight];
            image.CopyPixels(buffer, Stride, 0);
            return BitmapSource.Create(image.PixelWidth, image.PixelHeight, 96, 96, image.Format, Util.CreatePallete(color), buffer, Stride);
        }
    }
}