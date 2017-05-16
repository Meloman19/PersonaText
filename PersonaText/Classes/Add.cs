using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
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
        public int SelectIndex = -1;
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

        private MemoryStream MS_MSG1;

        private BindingList<msg> _msg = new BindingList<msg>();
        public BindingList<msg> msg
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

        private BindingList<name> _name = new BindingList<name>();
        public BindingList<name> name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    Notify("name");
                }
            }
        }

        public List<fnmp> FM = new List<fnmp>();

        public void ParseMSG1(string FileName, bool SaveMSG1)
        {
            MS_MSG1 = GetMSG1(FileName, ref SelectIndex);

            if (MS_MSG1 != null)
            {
                if (SaveMSG1)
                {
                    if (SelectIndex >= 0) { SaveAsMSG1(FileName, " - " + Convert.ToString(SelectIndex).PadLeft(3, '0'), MS_MSG1); }
                    else { SaveAsMSG1(FileName, "", MS_MSG1); }
                }
                MS_MSG1.Position = 0;
                ParseMSG1(MS_MSG1);
                UpdateString();
            }
        }

        public void ParseMSG1(string FileName, List<MemoryStream> LMS, bool SaveMSG1)
        {
            if (LMS.Count > 1)
            {
                foreach (var MS in LMS)
                {
                    if (MS != null)
                    {
                        if (SaveMSG1) { SaveAsMSG1(FileName, " - " + Convert.ToString(LMS.IndexOf(MS)).PadLeft(3, '0'), MS); }

                        MS.Position = 0;

                        ParseMSG1(MS);
                        UpdateString();
                        SaveAsText(FileName + " - " + Convert.ToString(LMS.IndexOf(MS)).PadLeft(3, '0') + ".txt");
                    }
                }
            }
            else if (LMS.Count == 1)
            {
                MemoryStream MS = LMS[0];
                if (MS != null)
                {
                    if (SaveMSG1) { SaveAsMSG1(FileName, "", MS); }

                    MS.Position = 0;

                    ParseMSG1(MS);
                    UpdateString();
                    SaveAsText(FileName + ".txt");
                }
            }
        }

        #region ParseMSG1

        public MemoryStream GetMSG1(string FileName, ref int SelectIndex)
        {
            FileInfo FI = new FileInfo(FileName);

            byte[] buffer = new byte[4];
            using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                FS.Position = 8;
                FS.Read(buffer, 0, 4);
            }
            string FileType = System.Text.Encoding.Default.GetString(buffer);

            if (FileType == "PMD1")
            {
                return GetMSG1fromPMD1(FileName);
            }
            else if (FileType == "FLW0")
            {
                return GetMSG1fromFLW0(FileName);
            }
            else if (FileType == "MSG1")
            {
                return GetMSG1fromFile(FileName, 0);
            }
            else
            {
                GetMSG1from GMF = new PersonaText.GetMSG1from();
                GMF.OVP.FileName = FileName;
                GMF.ShowDialog();
                SelectIndex = GMF.Select_Index;
                return GMF.MS;
            }
        }

        public MemoryStream GetMSG1fromPMD1(string FileName)
        {
            MemoryStream returned = new MemoryStream();

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
                returned.Write(buffer, 0, MSG1_Size);
            }
            catch (Exception e)
            {
                MessageBox.Show("Get MSG1 error!");
                MessageBox.Show(e.ToString());
                returned = new MemoryStream();
            }

            return returned;
        }

        public MemoryStream GetMSG1fromFLW0(string FileName)
        {
            MemoryStream returned = new MemoryStream();

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
                returned.Write(buffer, 0, MSG1_Size);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                returned = new MemoryStream();
            }

            return returned;
        }

        public MemoryStream GetMSG1fromFile(string FileName, long Position)
        {
            MemoryStream returned = new MemoryStream();
            try
            {
                FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);

                FS.Position = Position + 4;
                int Size = Text.ReadByteStream(ref FS, 4);
                FS.Position = Position;
                byte[] buffer = new byte[Size];
                FS.Read(buffer, 0, Size);
                returned.Write(buffer, 0, Size);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                returned = new MemoryStream();
            }
            return returned;
        }

        private void ParseMSG1(MemoryStream ms)
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

                int Name_Block_Position = Text.ReadByteStream(ref ms, 4);
                int Name_Count = Text.ReadByteStream(ref ms, 4);
                ms.Position = Name_Block_Position + MSG_PointBlock_Pos;
                List<long> Name_Position = new List<long>();
                for (int i = 0; i < Name_Count; i++)
                {
                    Name_Position.Add(Text.ReadByteStream(ref ms, 4));
                }

                name.Clear();
                int Index = 0;
                foreach (var a in Name_Position)
                {
                    ms.Position = a + MSG_PointBlock_Pos;
                    byte Byte = (byte)ms.ReadByte();
                    List<byte> Bytes = new List<byte>();
                    while (Byte != 0)
                    {
                        Bytes.Add(Byte);
                        Byte = (byte)ms.ReadByte();
                    }
                    name.Add(new name { Index = Index, Old_Name_Source = Bytes.ToArray() });
                    Index++;
                }


                Index = 0;
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
                    int Character_Index = 0xFFFF;

                    if (pos[0] == 0)
                    {
                        Type = "MSG";
                        int count = Text.ReadByteStream(ref ms, 2);
                        Character_Index = Text.ReadByteStream(ref ms, 2);
                        ms.Position = ms.Position + 4 * count;

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

                    msg.Add(new msg { Index = Index, Character_Index = Character_Index, Type = Type, Name = MSG_Name, SourceBytes = MSG_bytes, Strings = ParseString(MSG_bytes) });
                    Index++;
                }



            }
            catch (Exception e)
            {
                MessageBox.Show("Parse MSG1 error!");
                MessageBox.Show(e.ToString());
            }
        }

        private ObservableCollection<MyString> ParseString(byte[] SourceBytes)
        {
            ObservableCollection<MyString> returned = new ObservableCollection<MyString>();

            foreach (var Bytes in SplitSourceBytes(SourceBytes))
            {
                List<int[]> temp = Parse_STRING(Bytes);
                ObservableCollection<int[]> Prefix = new ObservableCollection<int[]>();
                ObservableCollection<int[]> Postfix = new ObservableCollection<int[]>();
                ObservableCollection<int[]> Strings = new ObservableCollection<int[]>();

                List<int> Indexes = new List<int>();

                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i][0] == -1)
                    {
                        Prefix.Add(temp[i]);
                        Indexes.Add(temp.IndexOf(temp[i]));
                    }
                    else
                    {
                        i = temp.Count;
                    }
                }


                for (int i = temp.Count - 1; i >= 0; i--)
                {
                    if (temp[i][0] == -1)
                    {
                        Postfix.Add(temp[i]);
                        Indexes.Add(temp.IndexOf(temp[i]));
                    }
                    else
                    {
                        i = 0;
                    }
                }
                Postfix.Reverse();

                for (int i = 0; i < temp.Count; i++)
                {
                    if (Indexes.FindAll(x => x == temp.IndexOf(temp[i])).Count == 0)
                    {
                        Strings.Add(temp[i]);
                    }
                }


                returned.Add(new PersonaText.MyString { Old_string_bytes = Strings, Postfix_bytes = Postfix, Prefix_bytes = Prefix, Prefix = "Prefix: " + BytesToHEXString(Prefix.ToList()), Postfix = "Postfix: " + BytesToHEXString(Postfix.ToList()) });
            }

            return returned;
        }

        private List<byte[]> SplitSourceBytes(byte[] B)
        {
            List<byte[]> returned = new List<byte[]>();

            List<byte> temp2 = new List<byte>();
            for (int i = 0; i < B.Length; i++)
            {
                if (B[i] == 0xF2)
                {
                    if (B[i + 1] == 0x05 & B[i + 2] == 0xFF & B[i + 3] == 0xFF)
                    {
                        if (temp2.Count != 0)
                        {
                            returned.Add(temp2.ToArray());
                            temp2.Clear();
                        }
                    }
                }

                temp2.Add(B[i]);
            }

            if (temp2.Count != 0)
            {
                returned.Add(temp2.ToArray());
                temp2.Clear();
            }

            return returned;
        }

        private List<int[]> Parse_STRING(byte[] B)
        {
            List<int[]> returned = new List<int[]>();

            List<int> temp = new List<int>();

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
                            returned.Add(temp.ToArray());
                            temp.Clear();
                        }

                        temp.Add(-1);
                        temp.Add(B[i]);

                        returned.Add(temp.ToArray());
                        temp.Clear();
                    }
                    else
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(temp.ToArray());
                            temp.Clear();
                        }


                        temp.Add(-1);
                        temp.Add(B[i]);
                        int count = (B[i] - 0xF0) * 2 - 1;
                        for (int k = 0; k < count; k++)
                        {
                            i++;
                            temp.Add(B[i]);
                        }

                        returned.Add(temp.ToArray());
                        temp.Clear();
                    }


                }
            }

            return returned;
        }

        #endregion ParseMSG1

        public void UpdateString()
        {
            foreach (var MSG in msg)
            {
                foreach (var MyString in MSG.Strings)
                {
                    MyString.Old_string = BytesToString(MyString.Old_string_bytes.ToList());
                }
            }
            foreach (var NAME in name)
            {
                NAME.Old_Name = BytesToString(NAME.Old_Name_Source);
            }
        }

        private string BytesToHEXString(List<int[]> bytes)
        {
            string returned = "";
            foreach (var MSG in bytes)
            {
                returned = returned + "{";
                returned = returned + Convert.ToString(MSG[1], 16).PadLeft(2, '0').ToUpper();
                for (int i = 2; i < MSG.Length; i++)
                {
                    returned = returned + " " + Convert.ToString(MSG[i], 16).PadLeft(2, '0').ToUpper();
                }
                returned = returned + "}";
            }

            return returned;
        }

        private string BytesToString(List<int[]> bytes)
        {
            string returned = "";

            foreach (var MSG in bytes)
            {
                if (MSG[0] == -1)
                {
                    if (MSG[1] == 0x0A)
                    {
                        returned = returned + " ";
                    }
                    else
                    {
                        returned = returned + "{";
                        returned = returned + Convert.ToString(MSG[1], 16).PadLeft(2, '0').ToUpper();
                        for (int i = 2; i < MSG.Length; i++)
                        {
                            returned = returned + " " + Convert.ToString(MSG[i], 16).PadLeft(2, '0').ToUpper();
                        }
                        returned = returned + "}";
                    }
                }
                else
                {
                    for (int i = 0; i < MSG.Length; i++)
                    {
                        if (0x20 <= MSG[i] & MSG[i] < 0x80)
                        {
                            if (FM.Exists(x => x.Index == MSG[i]))
                            {
                                fnmp fnmp = FM.Find(x => x.Index == MSG[i]);
                                if (fnmp.Char != "")
                                {
                                    returned = returned + fnmp.Char;
                                }
                                else
                                {
                                    returned = returned + "{!}";
                                }
                            }
                            else
                            {
                                returned = returned + "{!}";
                            }
                        }
                        else if (0x80 <= MSG[i] & MSG[i] < 0xF0)
                        {
                            int link = (MSG[i] - 0x81) * 0x80 + MSG[i + 1] + 0x20;

                            i++;
                            if (FM.Exists(x => x.Index == link))
                            {
                                fnmp fnmp = FM.Find(x => x.Index == link);

                                if (fnmp.Char != "")
                                {
                                    returned = returned + fnmp.Char;
                                }
                                else
                                {
                                    returned = returned + "{!}";
                                }
                            }
                            else
                            {
                                returned = returned + "{!}";
                            }
                        }
                        else
                        {
                            MessageBox.Show("");
                        }
                    }
                }
            }

            return returned;
        }

        private string BytesToString(byte[] bytes)
        {
            string returned = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                {
                    if (FM.Exists(x => x.Index == bytes[i]))
                    {
                        fnmp fnmp = FM.Find(x => x.Index == bytes[i]);
                        if (fnmp.Char != "")
                        {
                            returned = returned + fnmp.Char;
                        }
                        else
                        {
                            returned = returned + "{!}";
                        }
                    }
                    else
                    {
                        returned = returned + "{!}";
                    }
                }
                else if (0x80 <= bytes[i] & bytes[i] < 0xF0)
                {
                    int link = (bytes[i] - 0x81) * 0x80 + bytes[i + 1] + 0x20;

                    i++;
                    if (FM.Exists(x => x.Index == link))
                    {
                        fnmp fnmp = FM.Find(x => x.Index == link);

                        if (fnmp.Char != "")
                        {
                            returned = returned + fnmp.Char;
                        }
                        else
                        {
                            returned = returned + "{!}";
                        }
                    }
                    else
                    {
                        returned = returned + "{!}";
                    }
                }
                else
                {
                    MessageBox.Show("");
                }
            }

            return returned;
        }

        public void SaveAsText(string FileName)
        {
            StreamWriter SW = new StreamWriter(FileName);

            foreach (var Name in name)
            {
                SW.WriteLine("Name № " + Name.Index + ": " + Name.Old_Name);
            }

            SW.WriteLine();
            foreach (var MSG in msg)
            {
                SW.Write(MSG.Name + ":");
                List<name> Name = name.ToList();

                if (Name.Exists(x => x.Index == MSG.Character_Index))
                {
                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
                    SW.WriteLine(Name_i.Old_Name);
                }
                else { SW.WriteLine("<NO_NAME>"); }

                foreach (var STR in MSG.Strings)
                {
                    SW.WriteLine(STR.Old_string);
                }
                SW.WriteLine("");
            }

            SW.Close();
        }

        public void SaveAsMSG1(string FileName, string Add, MemoryStream MS)
        {
            FileInfo FI = new FileInfo(FileName);
            if (FI.Extension != ".MSG1")
            {
                MS.Position = 0;
                FileStream FS = new FileStream(FileName + Add + ".MSG1", FileMode.Create);
                MS.CopyTo(FS);
                FS.Close();
            }
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
                        List.Add(new PersonaText.fnmp { Index = Index, Char = Char });
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
                    SW.WriteLine(MSG2.Old_string.Length);
                    SW.Write(MSG2.Old_string.ToCharArray());
                    SW.WriteLine(MSG2.New_string.Length);
                    SW.Write(MSG2.New_string.ToCharArray());
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

                    MSG_strings.Add(new PersonaText.MyString { Old_string = mystr_old, New_string = mystr_new });
                }

                msg.Add(new PersonaText.msg { Name = MSG_Name, SourceBytes = MSG_bytes, Strings = new ObservableCollection<PersonaText.MyString>(MSG_strings) });
            }

            return msg;
        }

    }
}
