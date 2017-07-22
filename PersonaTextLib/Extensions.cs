using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PersonaTextLib.Types;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace PersonaTextLib.Extensions
{
    public static class Utilities
    {
        public static void ReadShift(this List<glyphYshift> list)
        {
            list.Add(new glyphYshift { Index = 81, Shift = 2 });
            list.Add(new glyphYshift { Index = 103, Shift = 2 });
            list.Add(new glyphYshift { Index = 106, Shift = 2 });
            list.Add(new glyphYshift { Index = 112, Shift = 2 });
            list.Add(new glyphYshift { Index = 113, Shift = 2 });
            list.Add(new glyphYshift { Index = 121, Shift = 2 });
        }

        public static bool CheckEntrance(this byte[] B, byte[] Bytes, int StartIndex)
        {
            if (Bytes.Length != 0)
            {
                if (StartIndex < B.Length)
                {
                    if (B[StartIndex] == Bytes[0])
                        return B.CheckEntrance(Bytes.Skip(1).ToArray(), StartIndex + 1);
                    else return false;
                }
                else return false;
            }
            else return true;
        }

        public static List<byte[]> SplitSourceBytes(byte[] B)
        {
            List<byte[]> returned = new List<byte[]>();

            byte[] LineSplit = B.Take((B[0] - 0xF0) * 2).ToArray();

            List<byte> String = new List<byte>();
            for (int i = 0; i < B.Length; i++)
            {
                if (B.CheckEntrance(LineSplit, i))
                {
                    if (String.Count != 0)
                    {
                        returned.Add(String.ToArray());
                        String.Clear();
                    }
                }

                String.Add(B[i]);
            }

            if (String.Count != 0)
            {
                returned.Add(String.ToArray());
                String.Clear();
            }

            return returned;
        }
    }

    public static class ListExtentsion
    {
        public static void ParseString(this IList<PersonaFileTypes.MSG1.MSGs.MSGstr> StringsList, byte[] SourceBytes, CharList Old, CharList New)
        {
            StringsList.Clear();

            int Index = 0;
            foreach (var Bytes in Utilities.SplitSourceBytes(SourceBytes))
            {
                PersonaFileTypes.MSG1.MyStringElement[] temp = Bytes.parseString();
                List<PersonaFileTypes.MSG1.MyStringElement> Prefix = new List<PersonaFileTypes.MSG1.MyStringElement>();
                List<PersonaFileTypes.MSG1.MyStringElement> Postfix = new List<PersonaFileTypes.MSG1.MyStringElement>();
                List<PersonaFileTypes.MSG1.MyStringElement> Strings = new List<PersonaFileTypes.MSG1.MyStringElement>();

                int tempdown = 0;
                int temptop = temp.Length;

                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Type == PersonaFileTypes.MSG1.MyStringElement.arrayType.System)
                        Prefix.Add(temp[i]);
                    else
                    {
                        tempdown = i;
                        i = temp.Length;
                    }
                }

                for (int i = temp.Length - 1; i >= tempdown; i--)
                {
                    if (temp[i].Type == PersonaFileTypes.MSG1.MyStringElement.arrayType.System)
                        Postfix.Add(temp[i]);
                    else
                    {
                        temptop = i;
                        i = 0;
                    }
                }

                Postfix.Reverse();

                for (int i = tempdown; i <= temptop; i++)
                    Strings.Add(temp[i]);

                PersonaFileTypes.MSG1.MSGs.MSGstr NewString = new PersonaFileTypes.MSG1.MSGs.MSGstr(Old, New);
                NewString.Index = Index;
                NewString.Prefix.ElementArray = Prefix.ToArray();
                NewString.OldString.ElementArray = Strings.ToArray();
                NewString.Postfix.ElementArray = Postfix.ToArray();

                StringsList.Add(NewString);
                Index++;
            }
        }

        public static void GetMyByteArray(this IList<PersonaFileTypes.MSG1.MyStringElement> MyByteArrayList, string String, IList<FnMpData> FontMap)
        {
            MyByteArrayList.Clear();
            foreach (var a in String.GetMyByteArray(FontMap))
            {
                MyByteArrayList.Add(a);
            }
        }

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

        public static PersonaFileTypes.MSG1.MyStringElement[] parseString(this byte[] B)
        {
            List<PersonaFileTypes.MSG1.MyStringElement> returned = new List<PersonaFileTypes.MSG1.MyStringElement>();

            if (B != null)
            {
                PersonaFileTypes.MSG1.MyStringElement.arrayType type = PersonaFileTypes.MSG1.MyStringElement.arrayType.Text;
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
                                returned.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, type, temp.ToArray()));
                                Index++;
                                temp.Clear();
                            }

                            type = PersonaFileTypes.MSG1.MyStringElement.arrayType.System;
                            temp.Add(B[i]);

                            returned.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, type, temp.ToArray()));
                            Index++;
                            type = PersonaFileTypes.MSG1.MyStringElement.arrayType.Text;
                            temp.Clear();
                        }
                        else
                        {
                            if (temp.Count != 0)
                            {
                                returned.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, type, temp.ToArray()));
                                Index++;
                                type = PersonaFileTypes.MSG1.MyStringElement.arrayType.Text;
                                temp.Clear();
                            }


                            type = PersonaFileTypes.MSG1.MyStringElement.arrayType.System;
                            temp.Add(B[i]);
                            int count = (B[i] - 0xF0) * 2 - 1;
                            for (int k = 0; k < count; k++)
                            {
                                i++;
                                temp.Add(B[i]);
                            }

                            returned.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, type, temp.ToArray()));
                            Index++;
                            type = PersonaFileTypes.MSG1.MyStringElement.arrayType.Text;
                            temp.Clear();
                        }
                    }
                }

                if (temp.Count != 0)
                {
                    returned.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, type, temp.ToArray()));
                    temp.Clear();
                }
            }

            return returned.ToArray();
        }

        public static byte[] getAllBytes(this IList<PersonaFileTypes.MSG1.MyStringElement> listMyByteArray)
        {
            List<byte> returned = new List<byte>();
            foreach (var a in listMyByteArray)
            {
                returned.AddRange(a.Bytes);
            }
            return returned.ToArray();
        }

        public static string GetString(this PersonaFileTypes.MSG1.MyStringElement[] ByteCollection, IList<FnMpData> CharList)
        {
            string returned = "";

           // if (ByteCollection != null)
                foreach (var MSG in ByteCollection)
                    returned += MSG.GetText(CharList);


            return returned;
        }

        public static string GetString(this PersonaFileTypes.MSG1.MyStringElement[] ByteCollection)
        {
            string returned = "";

           // if (ByteCollection != null)
                foreach (var MSG in ByteCollection)
                    returned += MSG.GetSystem();

            return returned;
        }
    }

    public static class StreamExtension
    {
        public static string ReadString(this BinaryReader BR, int length)
        {
            byte[] buffer = BR.ReadBytes(length);
            return System.Text.Encoding.ASCII.GetString(buffer.Where(x => x != 0).ToArray());
        }

        public static void WriteString(this BinaryWriter BW, string String, int Length)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(String);
                BW.Write(buffer);
                for (int i = 0; i < Length - String.Length; i++)
                {
                    BW.Write((byte)0);
                }
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
            }
        }

        public static int[] ReadInt32Array(this BinaryReader BR, int count)
        {
            int[] returned = new int[count];

            for (int i = 0; i < count; i++)
                returned[i] = BR.ReadInt32();

            return returned;
        }

        public static int[][] ReadInt32ArrayArray(this BinaryReader BR, int count, int stride)
        {
            int[][] returned = new int[count][];

            for (int i = 0; i < count; i++)
                returned[i] = BR.ReadInt32Array(stride);

            return returned;
        }

        public static void WriteInt32Array(this BinaryWriter BW, int[] array)
        {
            for (int i = 0; i < array.Length; i++)
                BW.Write(array[i]);
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

    public static class StringExtension
    {
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

        public static byte[] GetEncodeByte(this string String, IList<FnMpData> FontMap)
        {
            List<byte> LB = new List<byte>();
            foreach (var C in String)
            {
                LB.AddChar(C, FontMap);
            }
            return LB.ToArray();
        }

        public static PersonaFileTypes.MSG1.MyStringElement[] GetMyByteArray(this string String, IList<FnMpData> FontMap)
        {
            List<PersonaFileTypes.MSG1.MyStringElement> MyByteArrayList = new List<PersonaFileTypes.MSG1.MyStringElement>();

            int Index = 0;

            string[] split = Regex.Split(String, "(\r\n|\r|\n)");

            foreach (var a in split)
            {
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                {
                    MyByteArrayList.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, PersonaFileTypes.MSG1.MyStringElement.arrayType.System, new byte[] { 0x0A }));
                    Index++;
                }
                else
                {
                    string[] splitstr = Regex.Split(a, @"({[^}]+})");

                    foreach (var b in splitstr)
                    {
                        if (Regex.IsMatch(b, @"{.+}"))
                        {
                            MyByteArrayList.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, PersonaFileTypes.MSG1.MyStringElement.arrayType.System, b.Substring(1, b.Length - 2).GetSystemByte()));
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

                            MyByteArrayList.Add(new PersonaFileTypes.MSG1.MyStringElement(Index, PersonaFileTypes.MSG1.MyStringElement.arrayType.Text, ListByte.ToArray()));
                        }
                    }
                }
            }

            return MyByteArrayList.ToArray();
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
                    Logging.Write(e.ToString());
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
                    Logging.Write(e.ToString());
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool ReadFONT(this CharList list, string filename)
        {
            try
            {
                BinaryReader BR = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read));

                MemoryStream FontDec = new MemoryStream();

                int MainHeaderSize = BR.ReadInt32();
                BR.BaseStream.Position = 0xE;
                int TotalNumberOfGlyphs = BR.ReadUInt16();
                ushort GlyphSize_1 = BR.ReadUInt16();
                list.Width = GlyphSize_1;
                ushort GlyphSize_2 = BR.ReadUInt16();
                list.Height = GlyphSize_2;
                ushort GlyphSizeInByte = BR.ReadUInt16();
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
                else Logging.Write("ReadFONT: Unknown PixelFormat");

                int GlyphCutTable_Pos = MainHeaderSize + NumberOfColor * 4 + 4;
                BR.BaseStream.Position = GlyphCutTable_Pos - 4;
                int GlyphCutTable_Size = BR.ReadInt32();

                int UnknownPos = GlyphCutTable_Pos + GlyphCutTable_Size + 4;
                BR.BaseStream.Position = UnknownPos - 4;
                int UnknownSize = BR.ReadInt32();

                int ReservedPos = UnknownPos + UnknownSize;
                int ReservedSize = TotalNumberOfGlyphs * 4;

                int DictionaryHeader_Pos = ReservedPos + ReservedSize;

                BR.BaseStream.Position = DictionaryHeader_Pos;
                int DictionaryHeader_Size = BR.ReadInt32();
                int Dictionary_Size = BR.ReadInt32();
                int CompressedFontBlock_Size = BR.ReadInt32();
                int Dictionary_Pos = DictionaryHeader_Pos + DictionaryHeader_Size;

                BR.BaseStream.Position = DictionaryHeader_Pos + 24;
                int GlyphPositionTable_Size = BR.ReadInt32();

                BR.BaseStream.Position = Dictionary_Pos;

                int[,] Dictionary = new int[Dictionary_Size / 6, 2];
                for (int i = 0; i < Dictionary_Size / 6; i++)
                {
                    BR.BaseStream.Position = BR.BaseStream.Position + 2;
                    Dictionary[i, 0] = BR.ReadUInt16();
                    Dictionary[i, 1] = BR.ReadUInt16();
                }

                int CompressedFontBlock_Pos = Dictionary_Pos + Dictionary_Size + GlyphPositionTable_Size;

                BR.BaseStream.Position = CompressedFontBlock_Pos;

                int temp = 0;
                for (int m = 0; m < CompressedFontBlock_Size; m += 2)
                {
                    int s4 = BR.ReadUInt16();
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


                BR.BaseStream.Position = MainHeaderSize;

                List<Color> ColorBMP = new List<Color>();
                for (int i = 0; i < NumberOfColor; i++)
                {
                    byte r = BR.ReadByte();
                    byte g = BR.ReadByte();
                    byte b = BR.ReadByte();
                    byte a = BR.ReadByte();
                    ColorBMP.Add(Color.FromArgb(a, r, g, b));
                }
                list.Palette = new BitmapPalette(ColorBMP);

                FontDec.Position = 0;

                BR.BaseStream.Position = GlyphCutTable_Pos;
                byte[,] GlyphCut = new byte[TotalNumberOfGlyphs, 2];
                for (int i = 0; i < TotalNumberOfGlyphs; i++)
                {
                    GlyphCut[i, 0] = BR.ReadByte();
                    GlyphCut[i, 1] = BR.ReadByte();
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

                BR.Close();
                FontDec.Close();
                return true;
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
                return false;
            }
        }

        public static string BytesToString(byte[] bytes, CharList Font)
        {
            string returned = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                {
                    FnMpData fnmp = Font.List.FirstOrDefault(x => x.Index == bytes[i]);

                    if (fnmp == null)
                    {
                        returned = returned + "<NCHAR>";
                    }
                    else
                    {
                        if (fnmp.Char != "")
                        {
                            returned = returned + fnmp.Char;
                        }
                        else
                        {
                            returned = returned + "<CHAR>";
                        }
                    }
                }
                else if (0x80 <= bytes[i] & bytes[i] < 0xF0)
                {
                    int link = (bytes[i] - 0x81) * 0x80 + bytes[i + 1] + 0x20;

                    i++;

                    FnMpData fnmp = Font.List.FirstOrDefault(x => x.Index == link);

                    if (fnmp == null)
                    {
                        returned = returned + "<NCHAR>";
                    }
                    else
                    {
                        if (fnmp.Char != "")
                        {
                            returned = returned + fnmp.Char;
                        }
                        else
                        {
                            returned = returned + "<CHAR>";
                        }
                    }
                }
            }

            return returned;
        }
    }
}