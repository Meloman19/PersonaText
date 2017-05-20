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
                    name.Add(new name { Index = Index, Old_Name_Bytes = Bytes.ToArray() });
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

            int Index = 0;
            foreach (var Bytes in SplitSourceBytes(SourceBytes))
            {
                List<MyByteArray> temp = Parse_STRING(Bytes);
                ObservableCollection<MyByteArray> Prefix = new ObservableCollection<MyByteArray>();
                ObservableCollection<MyByteArray> Postfix = new ObservableCollection<MyByteArray>();
                ObservableCollection<MyByteArray> Strings = new ObservableCollection<MyByteArray>();

                List<int> Indexes = new List<int>();

                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i].Type == "System")
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
                    if (temp[i].Type == "System")
                    {
                        Postfix.Add(temp[i]);
                        Indexes.Add(temp.IndexOf(temp[i]));
                    }
                    else
                    {
                        i = 0;
                    }
                }

                Postfix = new ObservableCollection<PersonaText.MyByteArray>(Postfix.Reverse());

                for (int i = 0; i < temp.Count; i++)
                {
                    if (Indexes.FindAll(x => x == temp.IndexOf(temp[i])).Count == 0)
                    {
                        Strings.Add(temp[i]);
                    }
                }

                returned.Add(new MyString() { Index = Index, Old_string_bytes = Strings, Postfix_bytes = Postfix, Prefix_bytes = Prefix });
                Index++;
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

        private List<MyByteArray> Parse_STRING(byte[] B)
        {
            List<MyByteArray> returned = new List<MyByteArray>();

            string type = "Text";
            List<byte> temp = new List<byte>();

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
                            returned.Add(new MyByteArray { Type = type, Bytes = temp.ToArray() });
                            type = "Text";
                            temp.Clear();
                        }

                        type = "System";
                        temp.Add(B[i]);

                        returned.Add(new MyByteArray { Type = type, Bytes = temp.ToArray() });
                        type = "Text";
                        temp.Clear();
                    }
                    else
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(new MyByteArray { Type = type, Bytes = temp.ToArray() });
                            type = "Text";
                            temp.Clear();
                        }


                        type = "System";
                        temp.Add(B[i]);
                        int count = (B[i] - 0xF0) * 2 - 1;
                        for (int k = 0; k < count; k++)
                        {
                            i++;
                            temp.Add(B[i]);
                        }

                        returned.Add(new MyByteArray { Type = type, Bytes = temp.ToArray() });
                        type = "Text";
                        temp.Clear();
                    }
                }
            }

            return returned;
        }

        #endregion ParseMSG1

        public void UpdateString()
        {
            foreach (var NAME in name)
            {
                NAME.Old_Name = BytesToString(NAME.Old_Name_Bytes);
            }
        }

        private string BytesToString(byte[] bytes)
        {
            string returned = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                {
                    if (FontMap.old_char.Exists(x => x.Index == bytes[i]))
                    {
                        fnmp fnmp = FontMap.old_char.Find(x => x.Index == bytes[i]);
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
                    if (FontMap.old_char.Exists(x => x.Index == link))
                    {
                        fnmp fnmp = FontMap.old_char.Find(x => x.Index == link);

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

        public void SaveAsNewMSG1(string FileName)
        {
            string NewFileName = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "_NEW.MSG1";

            try
            {
                FileStream FS = new FileStream(NewFileName, FileMode.Create, FileAccess.ReadWrite);

                List<List<int>> MSG_pos = new List<List<int>>();
                List<int> NAME_pos = new List<int>();
                List<int> LastBlock = new List<int>();

                Text.WriteIntStream(ref FS, 0x7);
                Text.WriteIntStream(ref FS, 0x0);
                Text.WriteStringStream(ref FS, "MSG1", 8);
                Text.WriteIntStream(ref FS, 0x0);
                Text.WriteIntStream(ref FS, 0x0);
                Text.WriteIntStream(ref FS, msg.Count);
                Text.WriteIntStream(ref FS, 0x20000);

                foreach (var MSG in msg)
                {
                    if (MSG.Type == "MSG") { Text.WriteIntStream(ref FS, 0x0); }
                    else if (MSG.Type == "SEL") { Text.WriteIntStream(ref FS, 0x1); }
                    else { MessageBox.Show("SaveMSG1 Error"); }

                    LastBlock.Add((int)FS.Position);
                    Text.WriteIntStream(ref FS, 0x0);
                }

                LastBlock.Add((int)FS.Position);
                Text.WriteIntStream(ref FS, 0x0);
                Text.WriteIntStream(ref FS, name.Count);
                Text.WriteIntStream(ref FS, 0x0);
                Text.WriteIntStream(ref FS, 0x0);

                foreach (var MSG in msg)
                {
                    List<int> MSG_o = new List<int>();
                    MSG_o.Add((int)FS.Position);

                    Text.WriteStringStream(ref FS, MSG.Name, 24);

                    if (MSG.Type == "MSG")
                    {
                        Text.WriteShortStream(ref FS, (ushort)MSG.Strings.Count);

                        if (MSG.Character_Index == -1) { Text.WriteShortStream(ref FS, 0xFFFF); }
                        else { Text.WriteShortStream(ref FS, (ushort)MSG.Character_Index); }
                    }
                    else if (MSG.Type == "SEL")
                    {
                        Text.WriteShortStream(ref FS, 0);
                        Text.WriteShortStream(ref FS, (ushort)MSG.Strings.Count);
                        Text.WriteIntStream(ref FS, 0x0);
                    }

                    int Size = 0;

                    foreach (var String in MSG.Strings)
                    {
                        LastBlock.Add((int)FS.Position);
                        Text.WriteIntStream(ref FS, 0x0);
                        foreach (var Str in String.Prefix_bytes)
                        {
                            Size = Size + Str.Bytes.Length;
                        }
                        foreach (var Str in String.New_string_bytes)
                        {
                            Size = Size + Str.Bytes.Length;
                        }
                        foreach (var Str in String.Postfix_bytes)
                        {
                            Size = Size + Str.Bytes.Length;
                        }
                    }
                    MSG_o.Add(Size);

                    Text.WriteIntStream(ref FS, 0x0);

                    foreach (var String in MSG.Strings)
                    {
                        List<byte> NewString = new List<byte>();
                        foreach (var prefix in String.Prefix_bytes)
                        {
                            NewString.AddRange(prefix.Bytes);
                        }
                        foreach (var str in String.New_string_bytes)
                        {
                            NewString.AddRange(str.Bytes);
                        }
                        foreach (var postfix in String.Postfix_bytes)
                        {
                            NewString.AddRange(postfix.Bytes);
                        }

                        MSG_o.Add((int)FS.Position);
                        FS.Write(NewString.ToArray(), 0, NewString.Count);
                    }

                    while (FS.Length % 4 != 0)
                    {
                        FS.WriteByte(0);
                    }

                    MSG_pos.Add(MSG_o);
                }

                long Name_Block_pos = FS.Length;
                FS.Position = 0x20;
                for (int i = 0; i < msg.Count; i++)
                {
                    FS.Position += 4;
                    Text.WriteIntStream(ref FS, MSG_pos[i][0] - 0x20);
                }
                Text.WriteIntStream(ref FS, (int)Name_Block_pos - 0x20);
                for (int i = 0; i < msg.Count; i++)
                {
                    FS.Position = MSG_pos[i][0];

                    if (msg[i].Type == "MSG")
                    {
                        FS.Position += 28;
                    }
                    else if (msg[i].Type == "SEL")
                    {
                        FS.Position += 32;
                    }
                    else { MessageBox.Show("SD"); }

                    for (int k = 0; k < msg[i].Strings.Count; k++)
                    {
                        Text.WriteIntStream(ref FS, MSG_pos[i][k + 2] - 0x20);
                    }
                    Text.WriteIntStream(ref FS, MSG_pos[i][1]);
                }


                FS.Position = Name_Block_pos;
                foreach (var NAME in name)
                {
                    LastBlock.Add((int)FS.Position);
                    Text.WriteIntStream(ref FS, 0);
                }
                foreach (var NAME in name)
                {
                    NAME_pos.Add((int)FS.Position);
                    FS.Write(NAME.New_Name_Bytes, 0, NAME.New_Name_Bytes.Length);
                    FS.WriteByte(0);
                }
                FS.Position = Name_Block_pos;
                for (int i = 0; i < name.Count; i++)
                {
                    Text.WriteIntStream(ref FS, NAME_pos[i] - 0x20);
                }
                FS.Position = FS.Length;
                while (FS.Length % 4 != 0)
                {
                    FS.WriteByte(0);
                }

                int LastBlockPos = (int)FS.Position;
                byte[] LastBlockBytes = getLastBlock(LastBlock);
                FS.Write(LastBlockBytes, 0, LastBlockBytes.Length);

                FS.Position = 0x10;
                Text.WriteIntStream(ref FS, LastBlockPos);
                Text.WriteIntStream(ref FS, LastBlockBytes.Length);

                FS.Position = 0x4;
                Text.WriteIntStream(ref FS, (int)FS.Length);

                FS.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private byte[] getLastBlock(List<int> Addresses)
        {
            int sum = 0;
            List<byte> returned = new List<byte>();

            for (int i = 0; i < Addresses.Count; i++)
            {
                int reloc = Addresses[i] - sum - 0x20;
                int amount = getSeq(ref Addresses, i);
                Encode(reloc, ref returned, ref sum);
                if (amount > 1)
                {
                    reloc = 7;
                    reloc |= ((amount - 2) / 2) << 4;
                    if (amount % 2 == 1)
                    {
                        reloc |= 8;
                    }
                    returned.Add((byte)reloc);
                    i += amount;
                    sum += amount * 4;
                }
            }

            return returned.ToArray();
        }

        private int getSeq(ref List<int> Addresses, int index)
        {
            if (index < Addresses.Count - 1)
            {
                if (Addresses[index + 1] - Addresses[index] == 4)
                {
                    return getSeq(ref Addresses, index + 1) + 1;
                }
            }
            return 0;
        }

        private void Encode(int reloc, ref List<byte> LastBlock, ref int sum)
        {
            if (reloc % 2 == 0)
            {
                int temp = reloc >> 1;
                if (temp <= 0xFF)
                {
                    LastBlock.Add((byte)temp);
                }
                else
                {
                    byte item = (byte)((reloc & 0xff) + 1);
                    byte num2 = (byte)((reloc & 0xff00) >> 8);
                    LastBlock.Add(item);
                    LastBlock.Add(num2);
                }

            }
            else
            {
                byte item = (byte)((reloc & 0xff) + 1);
                byte num2 = (byte)((reloc & 0xff00) >> 8);
                LastBlock.Add(item);
                LastBlock.Add(num2);
            }
            sum += reloc;
        }

        private struct Address
        {
            public int Index;
            public int Number;
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

        public void WriteIntStream(ref FileStream stream, int Number)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(Number);
                stream.Write(buffer, 0, 4);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void WriteShortStream(ref FileStream stream, ushort Number)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(Number);
                stream.Write(buffer, 0, 2);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void WriteStringStream(ref FileStream stream, string String, int Length)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(String);
                stream.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < Length - String.Length; i++)
                {
                    stream.WriteByte(0);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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
        public List<msg> msg = new List<msg>();
        public List<name> name = new List<name>();
        private string path = "";

        public Project(string Path)
        {
            path = Path;
        }

        public void SaveProject()
        {
            try
            {
                XDocument xDoc = new XDocument();
                XElement Document = new XElement("MSG1");
                xDoc.Add(Document);
                XElement CharName = new XElement("CharacterNames");
                Document.Add(CharName);

                foreach (var NAME in name)
                {
                    XElement Name = new XElement("Name");
                    Name.Add(new XAttribute("Index", NAME.Index));
                    Name.Add(new XElement("OldNameSource", BitConverter.ToString(NAME.Old_Name_Bytes)));
                    Name.Add(new XElement("OldName", NAME.Old_Name));
                    Name.Add(new XElement("NewNameByte", BitConverter.ToString(NAME.New_Name_Bytes)));
                    Name.Add(new XElement("NewName", NAME.New_Name));
                    CharName.Add(Name);
                }

                XElement MES = new XElement("MSG");
                Document.Add(MES);

                foreach (var MSG in msg)
                {
                    XElement Msg = new XElement("Message");
                    Msg.Add(new XAttribute("Index", MSG.Index));
                    Msg.Add(new XElement("Type", MSG.Type));
                    Msg.Add(new XElement("Name", MSG.Name));
                    Msg.Add(new XElement("CharacterNameIndex", MSG.Character_Index));
                    Msg.Add(new XElement("SourceBytes", BitConverter.ToString(MSG.SourceBytes)));

                    XElement Strings = new XElement("MessageStrings");
                    Msg.Add(Strings);
                    foreach (var STR in MSG.Strings)
                    {
                        XElement String = new XElement("String");
                        String.Add(new XAttribute("Index", STR.Index));
                        Strings.Add(String);

                        XElement Prefix = new XElement("Prefix");
                        String.Add(Prefix);
                        foreach (var A in STR.Prefix_bytes)
                        {
                            XElement PrefixBytes = new XElement("PrefixBytes", BitConverter.ToString(A.Bytes));
                            PrefixBytes.Add(new XAttribute("Index", STR.Prefix_bytes.IndexOf(A)));
                            PrefixBytes.Add(new XAttribute("Type", A.Type));
                            Prefix.Add(PrefixBytes);
                        }
                        Prefix.Add(new XElement("PrefixString", STR.Prefix));

                        XElement Old_String = new XElement("Old");
                        String.Add(Old_String);
                        foreach (var A in STR.Old_string_bytes)
                        {
                            XElement OldStringBytes = new XElement("OldStringBytes", BitConverter.ToString(A.Bytes));
                            OldStringBytes.Add(new XAttribute("Index", STR.Old_string_bytes.IndexOf(A)));
                            OldStringBytes.Add(new XAttribute("Type", A.Type));
                            Old_String.Add(OldStringBytes);
                        }
                        Old_String.Add(new XElement("OldString", STR.Old_string));

                        XElement New_String = new XElement("New");
                        String.Add(New_String);
                        foreach (var A in STR.New_string_bytes)
                        {
                            XElement NewStringBytes = new XElement("NewStringBytes", BitConverter.ToString(A.Bytes));
                            NewStringBytes.Add(new XAttribute("Index", STR.New_string_bytes.IndexOf(A)));
                            NewStringBytes.Add(new XAttribute("Type", A.Type));
                            New_String.Add(NewStringBytes);
                        }
                        New_String.Add(new XElement("NewString", STR.New_string));

                        XElement Postfix = new XElement("Postfix");
                        String.Add(Postfix);
                        foreach (var A in STR.Postfix_bytes)
                        {
                            XElement PostfixBytes = new XElement("PostfixBytes", BitConverter.ToString(A.Bytes));
                            PostfixBytes.Add(new XAttribute("Index", STR.Postfix_bytes.IndexOf(A)));
                            PostfixBytes.Add(new XAttribute("Type", A.Type));
                            Postfix.Add(PostfixBytes);
                        }
                        Postfix.Add(new XElement("PostfixString", STR.Postfix));
                    }

                    MES.Add(Msg);
                }

                xDoc.Save(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void OpenProject()
        {
            try
            {
                XDocument xDoc = XDocument.Load(path);
                XElement MSG1 = xDoc.Element("MSG1");

                foreach (var NAME in MSG1.Element("CharacterNames").Elements())
                {
                    int Index = Convert.ToInt32(NAME.Attribute("Index").Value);

                    byte[] OldNameSource = new byte[0];
                    string OldNameSource_str = NAME.Element("OldNameSource").Value;
                    if (OldNameSource_str != "")
                    {
                        OldNameSource = Enumerable.Range(0, OldNameSource_str.Split('-').Length).Select(x => Convert.ToByte(OldNameSource_str.Split('-')[x], 16)).ToArray();
                    }
                    string OldName = NAME.Element("OldName").Value;

                    byte[] NewNameSource = new byte[0];
                    string NewNameByte_str = NAME.Element("NewNameByte").Value;
                    if (NewNameByte_str != "")
                    {
                        NewNameSource = Enumerable.Range(0, NewNameByte_str.Split('-').Length).Select(x => Convert.ToByte(NewNameByte_str.Split('-')[x], 16)).ToArray();
                    }
                    string NewName = NAME.Element("NewName").Value;

                    name.Add(new PersonaText.name { Index = Index, Old_Name_Bytes = OldNameSource, Old_Name = OldName, New_Name_Bytes = NewNameSource, New_Name = NewName });
                }

                foreach (var Message in MSG1.Element("MSG").Elements())
                {
                    int Index = Convert.ToInt32(Message.Attribute("Index").Value);
                    string Type = Message.Element("Type").Value;
                    string Name = Message.Element("Name").Value;
                    int CharacterNameIndex = Convert.ToInt32(Message.Element("CharacterNameIndex").Value);

                    byte[] SourceBytes = new byte[0];
                    string SourceBytes_str = Message.Element("SourceBytes").Value;
                    if (SourceBytes_str != "")
                    {
                        SourceBytes = Enumerable.Range(0, SourceBytes_str.Split('-').Length).Select(x => Convert.ToByte(SourceBytes_str.Split('-')[x], 16)).ToArray();
                    }

                    ObservableCollection<MyString> MSGStrings = new ObservableCollection<PersonaText.MyString>();
                    foreach (var Strings in Message.Element("MessageStrings").Elements())
                    {
                        MyString String = new MyString();

                        String.Index = Convert.ToInt32(Strings.Attribute("Index").Value);

                        foreach (var PrefixByte in Strings.Element("Prefix").Elements("PrefixBytes"))
                        {
                            int PrefixIndex = Convert.ToInt32(PrefixByte.Attribute("Index").Value);
                            string PrefixType = PrefixByte.Attribute("Type").Value;

                            byte[] PrefixBytes = new byte[0];
                            string PrefixBytes_str = PrefixByte.Value;
                            if (PrefixBytes_str != "")
                            {
                                PrefixBytes = Enumerable.Range(0, PrefixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PrefixBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            String.Prefix_bytes.Add(new MyByteArray { Index = PrefixIndex, Type = PrefixType, Bytes = PrefixBytes });
                        }

                        foreach (var Old in Strings.Element("Old").Elements("OldStringBytes"))
                        {
                            int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
                            string OldType = Old.Attribute("Type").Value;

                            byte[] OldBytes = new byte[0];
                            string OldBytes_str = Old.Value;
                            if (OldBytes_str != "")
                            {
                                OldBytes = Enumerable.Range(0, OldBytes_str.Split('-').Length).Select(x => Convert.ToByte(OldBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            String.Old_string_bytes.Add(new MyByteArray { Index = OldIndex, Type = OldType, Bytes = OldBytes });
                        }
                        String.Old_string = Strings.Element("Old").Element("OldString").Value;

                        foreach (var New in Strings.Element("New").Elements("NewStringBytes"))
                        {
                            int NewIndex = Convert.ToInt32(New.Attribute("Index").Value);
                            string NewType = New.Attribute("Type").Value;

                            byte[] NewBytes = new byte[0];
                            string NewBytes_str = New.Value;
                            if (NewBytes_str != "")
                            {
                                NewBytes = Enumerable.Range(0, NewBytes_str.Split('-').Length).Select(x => Convert.ToByte(NewBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            String.New_string_bytes.Add(new MyByteArray { Index = NewIndex, Type = NewType, Bytes = NewBytes });
                        }
                        String.New_string = Strings.Element("New").Element("NewString").Value;

                        foreach (var PostfixByte in Strings.Element("Postfix").Elements("PostfixBytes"))
                        {
                            int PostfixIndex = Convert.ToInt32(PostfixByte.Attribute("Index").Value);
                            string PostfixType = PostfixByte.Attribute("Type").Value;

                            byte[] PostfixBytes = new byte[0];
                            string PostfixBytes_str = PostfixByte.Value;
                            if (PostfixBytes_str != "")
                            {
                                PostfixBytes = Enumerable.Range(0, PostfixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PostfixBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            String.Postfix_bytes.Add(new MyByteArray { Index = PostfixIndex, Type = PostfixType, Bytes = PostfixBytes });
                        }

                        MSGStrings.Add(String);
                    }
                    msg.Add(new PersonaText.msg { Index = Index, Type = Type, Name = Name, Character_Index = CharacterNameIndex, SourceBytes = SourceBytes, Strings = MSGStrings });
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }
}
