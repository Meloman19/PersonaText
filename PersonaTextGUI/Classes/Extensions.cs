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
using PersonaTextLib.Extensions;
using PersonaTextLib.Types;
using PersonaTextLib;

namespace PersonaText
{
    static class ListExtentsion
    {
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
                else MessageBox.Show("ReadFONT: Unknown PixelFormat");

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
                MessageBox.Show(e.ToString());
                return false;
            }
        }

        public static ImageData DrawText(this PersonaFileTypes.MSG1.MyStringElement[] text, CharList CharList)
        {
            if (text != null)
            {
                ImageData returned = null;
                ImageData line = null;
                foreach (var a in text)
                {
                    if (a.Type == PersonaFileTypes.MSG1.MyStringElement.arrayType.System)
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
                                ImageData glyph = new ImageData(fnmp.Image_data, CharList.PixelFormat, CharList.Width, CharList.Height);
                                glyph = temp == null ? ImageData.Crop(glyph, fnmp.Cut.Left, fnmp.Cut.Right - 1) : ImageData.Shift(ImageData.Crop(glyph, fnmp.Cut.Left, fnmp.Cut.Right - 1), temp.Shift);
                                line = ImageData.MergeLeftRight(line, glyph);
                            }
                        }
                    }
                }
                returned = ImageData.MergeUpDown(returned, line);
                return returned;
            }
            return null;
        }

        public static void GetMyByteArray(this List<PersonaFileTypes.MSG1.MyStringElement> MyByteArrayList, string String, IList<FnMpData> FontMap)
        {
            MyByteArrayList.Clear();

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
        }

        public static BitmapSource ChangePallete(this BitmapSource image, Color color)
        {
            if (image != null)
            {
                int Stride = (image.Format.BitsPerPixel * image.PixelWidth + 7) / 8;
                byte[] buffer = new byte[Stride * image.PixelHeight];
                image.CopyPixels(buffer, Stride, 0);
                return BitmapSource.Create(image.PixelWidth, image.PixelHeight, 96, 96, image.Format, Util.CreatePallete(color), buffer, Stride);
            }
            return image;
        }
    }
}