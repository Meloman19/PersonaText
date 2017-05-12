using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PersonaText
{
    public class MSG1 : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged implementation

        Text Text = new Text();

        private bool _openfile = false;
        public bool openfile
        {
            get { return _openfile; }
            set
            {
                if (value != _openfile)
                {
                    _openfile = value;
                    Notify("openfile");
                }
            }
        }

        private ObservableCollection<msg> _msg = new ObservableCollection<PersonaText.msg>();
        public ObservableCollection<msg> msg
        {
            get
            {
                return _msg;
            }
            set
            {
                if (value != _msg)
                {
                    _msg = value;
                    Notify("msg");
                }
            }
        }

        #region ParseMSG1

        public void ParseMSG1(string FileName)
        {
            MemoryStream ms = GetMSG1(FileName);
            if (ms != null)
            {
                ParseMSG1(ref ms);
            }
        }

        private MemoryStream GetMSG1(string FileName)
        {
            byte[] buffer = new byte[4];

            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            fs.Position = 8;
            fs.Read(buffer, 0, 4);
            fs.Close();
            string FileType = System.Text.Encoding.Default.GetString(buffer);

            if (FileType == "PMD1")
            {
                return ParsePMD1(FileName);
            }
            else if (FileType == "FLW0")
            {
                return ParseFLW0(FileName);
            }
            else if (FileType == "MSG1")
            {
                fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                MemoryStream ms = new MemoryStream();
                fs.CopyTo(ms);
                return ms;
            }
            else
            {
                MessageBox.Show("Not supported format.", "Attention!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private MemoryStream ParsePMD1(string FileName)
        {
            MemoryStream temp = new MemoryStream();

            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                fs.Position = 0x20;
                while (Text.ReadByteStream(ref fs, 4) != 6)
                {
                    fs.Position = fs.Position + 12;
                }
                int MSG1_Size = Text.ReadByteStream(ref fs, 4);
                fs.Position = fs.Position + 4;
                int MSG1_Position = Text.ReadByteStream(ref fs, 4);

                byte[] buffer = new byte[MSG1_Size];
                fs.Position = MSG1_Position;
                fs.Read(buffer, 0, MSG1_Size);
                temp.Write(buffer, 0, MSG1_Size);
                return temp;
            }
            catch (Exception e)
            {
                MessageBox.Show("Get MSG1 error!");
                MessageBox.Show(e.ToString());
                temp = new MemoryStream();
            }

            return temp;
        }

        private MemoryStream ParseFLW0(string FileName)
        {
            MemoryStream temp = new MemoryStream();

            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                fs.Position = 0x20;
                while (Text.ReadByteStream(ref fs, 4) != 3)
                {
                    fs.Position = fs.Position + 12;
                }
                fs.Position = fs.Position + 4;
                int MSG1_Size = Text.ReadByteStream(ref fs, 4);
                int MSG1_Position = Text.ReadByteStream(ref fs, 4);

                byte[] buffer = new byte[MSG1_Size];
                fs.Position = MSG1_Position;
                fs.Read(buffer, 0, MSG1_Size);
                temp.Write(buffer, 0, MSG1_Size);
                return temp;
            }
            catch (Exception e)
            {
                MessageBox.Show("Get MSG1 error!");
                MessageBox.Show(e.ToString());
                temp = new MemoryStream();
            }

            return temp;
        }

        private void ParseMSG1(ref MemoryStream ms)
        {
            ms.Position = 0;
            try
            {
                byte[] buffer;

                int MSG_PointBlock_Pos = 0x20;
                ms.Position = 24;
                int MSG_count = Text.ReadByteStream(ref ms, 4);
                ms.Position = MSG_PointBlock_Pos;
                List<int[]> MSG_Position = new List<int[]>();

                for (int i = 0; i < MSG_count; i++)
                {
                    int[] temp = new int[2];
                    temp[0] = Text.ReadByteStream(ref ms, 4);
                    temp[1] = Text.ReadByteStream(ref ms, 4);
                    MSG_Position.Add(temp);
                }

                int Index = 0;
                msg.Clear();
                foreach (var pos in MSG_Position)
                {
                    ms.Position = MSG_PointBlock_Pos + pos[1];
                    buffer = new byte[24];
                    ms.Read(buffer, 0, 24);
                    string MSG_Name = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
                    if (MSG_Name == "")
                    {
                        MSG_Name = "<EMPTY>";
                    }

                    byte[] MSG_bytes;
                    string Type = "";

                    if (pos[0] == 0)
                    {
                        Type = "MSG";
                        int count = Text.ReadByteStream(ref ms, 2);
                        ms.Position = ms.Position + 4 * count + 2;

                        int size = Text.ReadByteStream(ref ms, 4);

                        MSG_bytes = new byte[size];
                        ms.Read(MSG_bytes, 0, size);
                    }
                    else if (pos[0] == 1)
                    {
                        Type = "SEL";
                        ms.Position = ms.Position + 2;
                        int count = Text.ReadByteStream(ref ms, 2);
                        ms.Position = ms.Position + 4 * count + 4;

                        int size = Text.ReadByteStream(ref ms, 4);

                        MSG_bytes = new byte[size];
                        ms.Read(MSG_bytes, 0, size);
                    }
                    else
                    {
                        MessageBox.Show("Unknown message type!");
                        return;
                    }

                    msg.Add(new msg { Index = Index, Type = Type, Name = MSG_Name, SourceBytes = MSG_bytes });
                    Index++;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Parse MSG1 error!");
                MessageBox.Show(e.ToString());
            }
        }

        #endregion ParseMSG1

        public void UpdateSTRINGs(List<fnmp> old_char)
        {
            foreach (var MSG in msg)
            {
                List<byte[]> buffer = new List<byte[]>();

                if (MSG.Type == "MSG")
                {
                    buffer = MSGtoBytes(MSG.SourceBytes);
                }
                else if (MSG.Type == "SEL")
                {
                    buffer = SELtoBytes(MSG.SourceBytes);
                }

                MSG.Bytes.Clear();
                foreach (var MSG_B in buffer)
                {
                    MSG.Bytes.Add(MSG_B);
                }

                MSG.Strings.Clear();
                List<MyString> s_buffer = BytesToString(buffer, old_char);
                foreach (var MSG_S in s_buffer)
                {
                    MSG.Strings.Add(MSG_S);
                }
            }
        }

        private List<byte[]> MSGtoBytes(byte[] B)
        {
            List<byte[]> temp = new List<byte[]>();

            List<byte> temp2 = new List<byte>();
            bool textread = false;
            bool linktonameread = false;

            int length = B.Length;
            for (int i = 0; i < length; i++)
            {
                try
                {
                    if (0x00 <= B[i] & B[i] < 0x20)
                    {
                        if (B[i] == 0x00)
                        {
                            if (i + 1 != length)
                            {
                                throw new Exception("MSG Null");
                            }
                        }
                        else if (B[i] == 0x0A)
                        {
                            if (textread)
                            {
                                temp2.Add(B[i]);
                            }
                            else if (linktonameread)
                            {
                                linktonameread = false;
                                textread = true;
                                temp2.Add(B[i]);
                            }
                            else { throw new Exception("0x0A"); }
                        }
                        else { throw new Exception(); }
                    }
                    else if (0x20 <= B[i] & B[i] < 0x80)
                    {
                        if (textread)
                        {
                            temp2.Add(B[i]);
                        }
                        else if (linktonameread)
                        {

                        }
                        else { throw new Exception("0x20-0x80"); }
                    }
                    else if (0x80 <= B[i] & B[i] < 0xF0)
                    {
                        if (textread)
                        {
                            temp2.Add(B[i]);
                            i = i + 1;
                            temp2.Add(B[i]);
                        }
                        else if (linktonameread)
                        {
                            i = i + 1;
                        }
                        else { throw new Exception("0x80-0xF0"); }
                    }
                    else
                    {
                        if (B[i] == 0xF1)
                        {
                            if (B[i + 1] == 0x21)
                            {
                                textread = false;
                                temp.Add(temp2.ToArray());
                                temp2.Clear();
                                i = i + 1;
                            }
                            else if (B[i + 1] == 0x24)
                            {
                                i = i + 1;
                            }
                            else if (B[i + 1] == 0x25)
                            {
                                i = i + 1;
                            }
                            else if (B[i + 1] == 0x41)
                            {
                                textread = true;
                                i = i + 1;
                            }
                            else if (B[i + 1] == 0x81)
                            {
                                linktonameread = true;
                                textread = false;
                                i = i + 1;
                            }
                            else if (B[i + 1] == 0x82)
                            {
                                i = i + 3;
                            }
                            else if (B[i + 1] == 0x83)
                            {
                                i = i + 1;
                            }
                            else { throw new Exception("0xF1"); }
                        }
                        else if (B[i] == 0xF2)
                        {
                            if (B[i + 1] == 0x01)
                            {
                            }
                            else if (B[i + 1] == 0x22)
                            {
                            }
                            else if (B[i + 1] == 0x23)
                            {
                            }
                            else if (B[i + 1] == 0x44)
                            {
                            }
                            else if (B[i + 1] == 0x05)
                            {
                            }
                            else { throw new Exception("0xF2"); }

                            i = i + 3;
                        }
                        else if (B[i] == 0xF3)
                        {
                            if (B[i + 1] == 0x42)
                            {
                            }
                            else if (B[i + 1] == 0x88)
                            {
                            }
                            else if (B[i + 1] == 0xA8)
                            {
                            }
                            else { throw new Exception("0xF3"); }
                            i = i + 5;
                        }
                        else if (B[i] == 0xF4)
                        {
                            if (B[i + 1] == 0x45)
                            {
                            }
                            else if (B[i + 1] == 0x84)
                            {
                            }
                            else if (B[i + 1] == 0x87)
                            {
                            }
                            else if (B[i + 1] == 0xAD)
                            {
                            }
                            else { throw new Exception("0xF4"); }
                            i = i + 7;
                        }
                        else if (B[i] == 0xF5)
                        {
                            if (B[i + 1] == 0x61)
                            {
                                i = i + 9;
                            }
                            else if (B[i + 1] == 0x86)
                            {
                                i = i + 9;
                            }
                            else { throw new Exception("0xF5"); }
                        }
                        else if (B[i] == 0xF6)
                        {
                            if (B[i + 1] == 0x89)
                            {
                                i = i + 11;
                            }
                        }
                        else { throw new Exception("NOT F1-F5"); }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("MSGtBytes Error");
                    MessageBox.Show(e.ToString());
                }
            }


            return temp;
        }

        private List<byte[]> SELtoBytes(byte[] B)
        {
            List<byte[]> temp = new List<byte[]>();

            List<byte> temp2 = new List<byte>();
            bool textread = false;
            bool linktonameread = false;

            int length = B.Length;
            for (int i = 0; i < length; i++)
            {
                try
                {
                    if (0x00 <= B[i] & B[i] < 0x20)
                    {
                        if (B[i] == 0x00)
                        {
                            if (textread == false & linktonameread == false & i + 1 != length)
                            {
                                throw new Exception("MSG Null");
                            }
                            if (textread)
                            {
                                textread = false;
                                temp.Add(temp2.ToArray());
                                temp2.Clear();
                            }
                            else if (linktonameread)
                            {
                                linktonameread = false;
                                temp.Add(temp2.ToArray());
                                temp2.Clear();
                            }
                        }
                        else { throw new Exception(); }
                    }
                    else if (0x20 <= B[i] & B[i] < 0x80)
                    {
                        if (textread)
                        {
                            temp2.Add(B[i]);
                        }
                        else if (linktonameread)
                        {
                        }
                        else { throw new Exception(); }
                    }
                    else if (0x80 <= B[i] & B[i] < 0xF0)
                    {
                        if (textread)
                        {
                            temp2.Add(B[i]);
                            i = i + 1;
                            temp2.Add(B[i]);
                        }
                        else if (linktonameread)
                        {
                            i = i + 1;
                        }
                        else { throw new Exception(); }
                    }
                    else
                    {
                        if (B[i] == 0xF1)
                        {
                            if (B[i + 1] == 0x41)
                            {
                                textread = true;
                            }
                            else if (B[i + 1] == 0x81)
                            {
                                linktonameread = true;
                                textread = false;
                            }
                            else if (B[i + 1] == 0x83)
                            {
                            }
                            else { throw new Exception("0xF1"); }
                            i = i + 1;
                        }
                        else if (B[i] == 0xF2)
                        {
                            if (B[i + 1] == 0x05)
                            {
                            }
                            else if (B[i + 1] == 0x44)
                            {
                            }
                            else { throw new Exception(); }
                            i = i + 3;
                        }
                        else if (B[i] == 0xF5)
                        {
                            if (B[i + 1] == 0x47)
                            {
                                i = i + 9;
                            }
                            else { throw new Exception(); }
                        }
                        else { throw new Exception(); }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show("SELtoBytes Error");
                    MessageBox.Show(e.ToString());
                }
            }

            return temp;
        }

        private List<MyString> BytesToString(List<byte[]> bytes, List<fnmp> FM)
        {
            List<MyString> temp = new List<MyString>();
            foreach (var MSG_b in bytes)
            {
                string str = "";
                for (int i = 0; i < MSG_b.Length; i++)
                {
                    if (MSG_b[i] == 0x0A)
                    {
                        str = str + "\n";
                    }
                    else if (0x20 <= MSG_b[i] & MSG_b[i] < 0x80)
                    {
                        if (FM.Exists(x => x.Index == MSG_b[i]))
                        {
                            fnmp fnmp = FM.Find(x => x.Index == MSG_b[i]);
                            str = str + fnmp.Char;
                        }
                    }
                    else if (0x80 <= MSG_b[i] & MSG_b[i] < 0xF0)
                    {
                        int link = (MSG_b[i] - 0x81) * 0x80 + MSG_b[i + 1] + 0x20;

                        i++;
                        if (FM.Exists(x => x.Index == link))
                        {
                            fnmp fnmp = FM.Find(x => x.Index == link);
                            str = str + fnmp.Char;
                        }
                    }
                    else
                    {
                        MessageBox.Show("BytesToString Error : Unknown byte");
                    }

                }
                temp.Add(new PersonaText.MyString { mystr_old = str, mystr_new = "" });
            }
            return temp;
        }

        public void SaveAsText(string path)
        {
            StreamWriter SW = new StreamWriter(path);

            string Separator = "===================";
            string Separator2 = "-------------------";
            foreach (var MSG in msg)
            {
                SW.WriteLine(MSG.Name);
                SW.WriteLine(Separator);

                foreach (var STR in MSG.Strings)
                {
                    SW.WriteLine(Separator2);
                    string[] str_array = STR.mystr_old.Split(new string[] { "\n" }, StringSplitOptions.None);
                    foreach (var STR2 in str_array)
                    {
                        SW.WriteLine(STR2);
                    }
                }
                SW.WriteLine(Separator2);
                SW.WriteLine(Separator);
                SW.WriteLine("");
            }
            SW.Close();
        }
    }

    public class Text
    {
        public void ReadFNMP(string FileName, ref List<fnmp> List)
        {
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                while (sr.EndOfStream == false)
                {
                    string str = sr.ReadLine();

                    int Index = Convert.ToInt32(str.Substring(0, str.IndexOf('=')));
                    string Char = str.Substring(str.IndexOf('=') + 1);

                    if (List.Exists(x => x.Index == Index))
                    {
                        fnmp fnmp = List.Find(x => x.Index == Index);
                        fnmp.Char = Char;
                    }
                    else
                    {
                        List.Add(new PersonaText.fnmp { Index = Index, Char = Char});
                    }
                }

                fs.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void WriteFNMP(string FileName, ref List<fnmp> List)
        {
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                foreach (var CL in List)
                {
                    if (CL.Char != "")
                    {
                        string str = Convert.ToString(CL.Index) + "=" + Convert.ToString(CL.Char);
                        sw.WriteLine(str);
                        sw.Flush();
                    }
                }

                fs.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void ReadFN(string Path, ref List<fnmp> List)
        {
            try
            {
                FileStream FONT = new FileStream(Path, FileMode.Open, FileAccess.Read);
                MemoryStream FontDec = new MemoryStream();

                int MainHeaderSize = ReadByteStream(ref FONT, 4);
                FONT.Position = 0xE;
                int TotalNumberOfGlyphs = ReadByteStream(ref FONT, 2);
                int GlyphCutTable_Pos = MainHeaderSize + 64 + 4;
                FONT.Position = GlyphCutTable_Pos - 4;
                int GlyphCutTable_Size = ReadByteStream(ref FONT, 4);

                int DictionaryHeader_Pos = GlyphCutTable_Pos + GlyphCutTable_Size + TotalNumberOfGlyphs * 4 + 4;

                FONT.Position = DictionaryHeader_Pos;
                int DictionaryHeader_Size = ReadByteStream(ref FONT, 4);
                int Dictionary_Size = ReadByteStream(ref FONT, 4);
                int CompressedFontBlock_Size = ReadByteStream(ref FONT, 4);
                int Dictionary_Pos = DictionaryHeader_Pos + DictionaryHeader_Size;

                FONT.Position = DictionaryHeader_Pos + 24;
                int GlyphPositionTable_Size = ReadByteStream(ref FONT, 4);

                FONT.Position = Dictionary_Pos;

                int[,] Dictionary = new int[Dictionary_Size / 6, 2];
                for (int i = 0; i < Dictionary_Size / 6; i++)
                {
                    FONT.Position = FONT.Position + 2;
                    Dictionary[i, 0] = ReadByteStream(ref FONT, 2);
                    Dictionary[i, 1] = ReadByteStream(ref FONT, 2);
                }

                int CompressedFontBlock_Pos = Dictionary_Pos + Dictionary_Size + GlyphPositionTable_Size;

                FONT.Position = CompressedFontBlock_Pos;
                int temp = 0;
                bool boolean = true;

                do
                {
                    if (FONT.Position == FONT.Length)
                    {
                        boolean = false;
                    }
                    else
                    {
                        int s4 = ReadByteStream(ref FONT, 2);
                        for (int i = 0; i < 16; i++)
                        {
                            temp = Dictionary[temp, s4 % 2];
                            s4 = s4 >> 1;

                            if (Dictionary[temp, 0] == 0)
                            {
                                int a = (Dictionary[temp, 1]);
                                a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                                FontDec.WriteByte((byte)a);
                                temp = 0;
                            }
                        }
                    }
                } while (boolean);

                FONT.Position = MainHeaderSize;

                List<Color> ColorBMP = new List<Color>();
                for (int i = 0; i < 16; i++)
                {
                    byte r = (byte)FONT.ReadByte();
                    byte g = (byte)FONT.ReadByte();
                    byte b = (byte)FONT.ReadByte();
                    byte a = (byte)FONT.ReadByte();
                    ColorBMP.Add(Color.FromArgb(0xFF, r, g, b));
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
                for (int i = 0; i < TotalNumberOfGlyphs; i++)
                {
                    byte[] data = new byte[512];
                    FontDec.Read(data, 0, 512);

                    BitmapSource BMP = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Indexed4, ColorPaletteBMP, data, 16);

                    if (List.Exists(x => x.Index == k))
                    {
                        fnmp fnmp = List.Find(x => x.Index == k);
                        fnmp.Image = BMP;
                        fnmp.Cut = new PersonaText.MyByte { Left = GlyphCut[i, 0], Right = GlyphCut[i, 1] };
                    }
                    else
                    {
                        List.Add(new PersonaText.fnmp { Index = k, Image = BMP, Char = "", Cut = new PersonaText.MyByte { Left = GlyphCut[i, 0], Right = GlyphCut[i, 1] } });
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

        public int ReadByteStream(ref FileStream stream, int count)
        {
            byte[] Bytes = new byte[count];
            try
            {
                stream.Read(Bytes, 0, count);
                Array.Reverse(Bytes);
                string str = "";
                for (int i = 0; i < count; i++)
                {
                    str = str + Convert.ToString(Bytes[i], 16).PadLeft(2, '0');
                }
                return Convert.ToInt32(str, 16);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return -1;
            }
        }

        public int ReadByteStream(ref MemoryStream stream, int count)
        {
            byte[] Bytes = new byte[count];
            try
            {
                stream.Read(Bytes, 0, count);
                Array.Reverse(Bytes);
                string str = "";
                for (int i = 0; i < count; i++)
                {
                    str = str + Convert.ToString(Bytes[i], 16).PadLeft(2, '0');
                }
                return Convert.ToInt32(str, 16);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return -1;
            }
        }

        public string ByteToString(ref byte[] Bytes)
        {
            string temp = "";
            for (int k = 0; k < Bytes.Length; k++)
            {
                if (Bytes[k] != 0)
                {
                    temp = temp + Convert.ToChar(Bytes[k]);
                }
            }
            return temp;
        }

        public List<byte[]> Message2ByteArray(byte[] Bytes)
        {
            List<byte[]> temp = new List<byte[]>();

            List<byte> temp2 = new List<byte>();
            for (int i = 0; i < Bytes.Length; i++)
            {
                if (Bytes[i] == 0xF2)
                {
                    if (Bytes[i + 1] == 0x05)
                    {
                        if (Bytes[i + 2] == 0xFF)
                        {
                            if (Bytes[i + 3] == 0xFF)
                            {
                                if (Bytes[i + 4] == 0xF1)
                                {
                                    if (Bytes[i + 5] == 0x41)
                                    {
                                        temp.Add(temp2.ToArray());
                                        temp2.Clear();
                                        i = i + 6;
                                    }
                                }
                            }
                        }
                    }
                }

                temp2.Add(Bytes[i]);
            }
            temp.Add(temp2.ToArray());

            return temp;
        }
    }

    public class Project
    {
        public string Source_File = "";

        public void Save_Project(string Path, List<msg> msg)
        {
            try
            {
                MemoryStream MSG = MSGtoStream(msg);
                FileStream Project = new FileStream(Path, FileMode.Create);
                MSG.CopyTo(Project);

                Project.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public List<msg> Open_Project(string Path)
        {
            List<msg> temp = new List<PersonaText.msg>();

            try
            {
                FileStream Project = new FileStream(Path, FileMode.Open);
                MemoryStream MS = new MemoryStream();
                Project.CopyTo(MS);

                MS.Position = 0;
                temp = StreamtoMSG(MS);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return temp;
        }

        private MemoryStream MSGtoStream(List<msg> msg)
        {
            MemoryStream MS = new MemoryStream();
            StreamWriter SW = new StreamWriter(MS);
            SW.AutoFlush = true;

            SW.WriteLine(msg.Count);
            foreach (var MSG in msg)
            {
                SW.WriteLine(MSG.Name);

                string str = "";
                foreach (var B in MSG.SourceBytes)
                { str = str + Convert.ToString(B, 16).PadLeft(2, ' ') + " "; }
                SW.WriteLine(str.Length);
                SW.Write(str.ToCharArray());

                SW.WriteLine(MSG.Strings.Count);
                foreach (var MSG2 in MSG.Strings)
                {
                    SW.WriteLine(MSG2.mystr_old.Length);
                    SW.Write(MSG2.mystr_old.ToCharArray());
                    SW.WriteLine(MSG2.mystr_new.Length);
                    SW.Write(MSG2.mystr_new.ToCharArray());
                }
            }
            MS.Position = 0;
            return MS;
        }

        private List<msg> StreamtoMSG(MemoryStream MS)
        {
            List<msg> msg = new List<PersonaText.msg>();
            StreamReader SR = new StreamReader(MS);

            int Count = Convert.ToInt32(SR.ReadLine());
            for (int i = 0; i < Count; i++)
            {
                int Index = Convert.ToInt32(SR.ReadLine());
                string MSG_Name = SR.ReadLine();

                int MSG_bytes_length = Convert.ToInt32(SR.ReadLine());
                char[] MSG_bytes_chars = new char[MSG_bytes_length];
                SR.ReadBlock(MSG_bytes_chars, 0, MSG_bytes_chars.Length);
                string str = new string(MSG_bytes_chars);
                string[] str_a = str.Split(' ');
                byte[] MSG_bytes = new byte[str_a.Length - 1];
                for (int k = 0; k < str_a.Length; k++)
                {
                    if (str_a[k] != "")
                    {
                        MSG_bytes[k] = byte.Parse(str_a[k], System.Globalization.NumberStyles.HexNumber);

                    }
                }

                List<MyString> MSG_strings = new List<PersonaText.MyString>();
                int MSG_strings_count = Convert.ToInt32(SR.ReadLine());
                for (int k = 0; k < MSG_strings_count; k++)
                {
                    int mystr_old_length = Convert.ToInt32(SR.ReadLine());
                    char[] mystr_old_chars = new char[mystr_old_length];
                    SR.ReadBlock(mystr_old_chars, 0, mystr_old_length);
                    string mystr_old = new string(mystr_old_chars);

                    int mystr_new_length = Convert.ToInt32(SR.ReadLine());
                    char[] mystr_new_chars = new char[mystr_new_length];
                    SR.ReadBlock(mystr_new_chars, 0, mystr_new_length);
                    string mystr_new = new string(mystr_new_chars);

                    MSG_strings.Add(new PersonaText.MyString { mystr_old = mystr_old, mystr_new = mystr_new });
                }

                msg.Add(new PersonaText.msg { Name = MSG_Name, SourceBytes = MSG_bytes, Strings = new ObservableCollection<PersonaText.MyString>(MSG_strings) });
            }

            return msg;
        }

    }
}
