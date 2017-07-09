using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace PersonaText
{
    static class Util
    {
        public static BitmapPalette CreatePallete(Color color)
        {
            List<Color> ColorBMP = new List<Color>();
            ColorBMP.Add(new Color { A = 0, R = 0, G = 0, B = 0 });
            for (int i = 1; i < 16; i++)
            {
                ColorBMP.Add(new Color
                {
                    A = Util.ByteTruncate(i * 0x10),
                    R = color.R,
                    G = color.G,
                    B = color.B
                });
            }
            return new BitmapPalette(ColorBMP);
        }

        public static byte ByteTruncate(int value)
        {
            if (value < 0) { return 0; }
            else if (value > 255) { return 255; }
            else { return (byte)value; }
        }

        public static bool ByteArrayCompareWithSimplest(byte[] BytesLeft, byte[] BytesRight)
        {
            if (BytesLeft.Length != BytesRight.Length)
                return false;

            var length = BytesLeft.Length;

            for (int i = 0; i < length; i++)
            {
                if (BytesLeft[i] != BytesRight[i])
                    return false;
            }

            return true;
        }

        public static void SaveToBMD(MemoryStream NewMSG1, string FileName)
        {
            NewMSG1.SaveToFile(FileName);
        }

        public static void SaveToPM1(MemoryStream NewMSG1, string SourceFile, string FileName)
        {
            PersonaFileTypes.PMD1 PM1 = new PersonaFileTypes.PMD1(SourceFile);
            PM1.SetNewMSG1(NewMSG1);
            PM1.SaveNew(FileName);
        }

        public static void SaveToBF(MemoryStream NewMSG1, string SourceFile, string FileName)
        {
            PersonaFileTypes.FLW0 FLW0 = new PersonaFileTypes.FLW0(SourceFile);
            FLW0.SetNewMSG1(NewMSG1);
            FLW0.SaveNew(FileName);
        }

        public static arrayType GetType(string type)
        {
            if (type == "System")
            {
                return arrayType.System;
            }
            else if (type == "Text")
            {
                return arrayType.Text;
            }
            else
            {
                return arrayType.Empty;
            }
        }
    }

    delegate name GetString(int index);

    class MSG1
    {
        public MSG1()
        {
            name.ListChanged += Name_ListChanged;
            msg.ListChanged += Msg_ListChanged;
        }

        private void Msg_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                foreach (var a in name)
                {
                    if (a.Index == msg[e.NewIndex].Character_Index)
                    {
                        msg[e.NewIndex].Name_PropertyChanged(a, new PropertyChangedEventArgs("CreateEventName"));
                    }
                }
            }
        }

        private void Name_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                foreach (var a in msg)
                {
                    if (a.Character_Index == e.NewIndex)
                    {
                        a.Name_PropertyChanged(name[e.NewIndex], new PropertyChangedEventArgs("CreateEventName"));
                    }
                }
            }
        }

        public int SaveAsTextOption { get; set; } = 0;

        MemoryStream MS_MSG1;

        public BindingList<msg> msg { get; set; } = new BindingList<msg>();
        public BindingList<name> name { get; set; } = new BindingList<name>();

        public void ParseMSG1(string FileName, bool SaveMSG1)
        {
            MS_MSG1 = GetMSG1(FileName);

            if (MS_MSG1 != null)
            {
                if (SaveMSG1)
                {
                    if (Static.FileInfo.SelectPosition >= 0) { SaveAsMSG1(FileName, " - " + Convert.ToString(Static.FileInfo.SelectPosition).PadLeft(3, '0'), MS_MSG1); }
                    else { SaveAsMSG1(FileName, MS_MSG1); }
                }
                MS_MSG1.Position = 0;
                ParseMSG1(MS_MSG1);
                //UpdateString();
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
                        // UpdateString();
                        SaveAsText(FileName, Convert.ToString(LMS.IndexOf(MS)).PadLeft(3, '0'), SaveAsTextOption);
                    }
                }
            }
            else if (LMS.Count == 1)
            {
                MemoryStream MS = LMS[0];
                if (MS != null)
                {
                    if (SaveMSG1) { SaveAsMSG1(FileName, MS); }

                    MS.Position = 0;

                    ParseMSG1(MS);
                    //UpdateString();
                    SaveAsText(FileName, "000", SaveAsTextOption);
                }
            }
        }

        #region ParseMSG1

        MemoryStream GetMSG1(string FileName)
        {
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
                GetMSG1From GMF = new GetMSG1From(FileName);
                GMF.ShowDialog();
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
                while (fs.ReadInt() != 6)
                {
                    fs.Position = fs.Position + 12;
                }
                int MSG1_Size = fs.ReadInt();
                fs.Position = fs.Position + 4;
                int MSG1_Position = fs.ReadInt();

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
                while (fs.ReadInt() != 3)
                {
                    fs.Position = fs.Position + 12;
                }
                fs.Position = fs.Position + 4;
                int MSG1_Size = fs.ReadInt();
                int MSG1_Position = fs.ReadInt();

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
                int Size = FS.ReadInt();
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

        void ParseMSG1(MemoryStream ms)
        {
            ms.Position = 0;
            try
            {
                byte[] buffer;

                int MSG_PointBlock_Pos = 0x20;
                ms.Position = 24;
                int MSG_count = ms.ReadInt();
                ms.Position = MSG_PointBlock_Pos;
                List<int[]> MSG_Position = new List<int[]>();

                for (int i = 0; i < MSG_count; i++)
                {
                    int[] temp = new int[2];
                    temp[0] = ms.ReadInt();
                    temp[1] = ms.ReadInt();
                    MSG_Position.Add(temp);
                }

                int Name_Block_Position = ms.ReadInt();
                int Name_Count = ms.ReadInt();
                ms.Position = Name_Block_Position + MSG_PointBlock_Pos;
                List<long> Name_Position = new List<long>();
                for (int i = 0; i < Name_Count; i++)
                {
                    Name_Position.Add(ms.ReadInt());
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
                    name.Add(new name { Index = Index, OldNameBytes = Bytes.ToArray() });
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
                    if (string.IsNullOrEmpty(MSG_Name))
                    {
                        MSG_Name = "<EMPTY>";
                    }

                    byte[] MSG_bytes;
                    string Type = "";
                    int Character_Index = 0xFFFF;

                    if (pos[0] == 0)
                    {
                        Type = "MSG";
                        int count = ms.ReadUshort();
                        Character_Index = ms.ReadUshort();
                        ms.Position = ms.Position + 4 * count;

                        int size = ms.ReadInt();

                        MSG_bytes = new byte[size];
                        ms.Read(MSG_bytes, 0, size);
                    }
                    else if (pos[0] == 1)
                    {
                        Type = "SEL";
                        ms.Position = ms.Position + 2;
                        int count = ms.ReadUshort();
                        ms.Position = ms.Position + 4 * count + 4;

                        int size = ms.ReadInt();

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

        BindingList<MyString> ParseString(byte[] SourceBytes)
        {
            BindingList<MyString> returned = new BindingList<MyString>();

            int Index = 0;
            foreach (var Bytes in SplitSourceBytes(SourceBytes))
            {
                List<MyByteArray> temp = new List<MyByteArray>();
                temp.parseString(Bytes);
                BindingList<MyByteArray> Prefix = new BindingList<MyByteArray>();
                BindingList<MyByteArray> Postfix = new BindingList<MyByteArray>();
                BindingList<MyByteArray> Strings = new BindingList<MyByteArray>();

                List<int> Indexes = new List<int>();

                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i].Type == arrayType.System)
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
                    if (temp[i].Type == arrayType.System)
                    {
                        Postfix.Add(temp[i]);
                        Indexes.Add(temp.IndexOf(temp[i]));
                    }
                    else
                    {
                        i = 0;
                    }
                }

                Postfix = new BindingList<MyByteArray>(Postfix.Reverse().ToList());

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

        List<byte[]> SplitSourceBytes(byte[] B)
        {
            List<byte[]> returned = new List<byte[]>();

            byte[] LineSplit = B.Take((B[0] - 0xF0) * 2).ToArray();

            List<byte> temp2 = new List<byte>();
            for (int i = 0; i < B.Length; i++)
            {
                if (B.CheckEntrance(LineSplit, i))
                {
                    if (temp2.Count != 0)
                    {
                        returned.Add(temp2.ToArray());
                        temp2.Clear();
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

        #endregion ParseMSG1

        private string BytesToString(byte[] bytes)
        {
            string returned = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                {
                    FnMpData fnmp = Static.FontMap.old_char.List.FirstOrDefault(x => x.Index == bytes[i]);

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

                    FnMpData fnmp = Static.FontMap.old_char.List.FirstOrDefault(x => x.Index == link);

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
                else
                {
                    MessageBox.Show("");
                }
            }

            return returned;
        }

        public void SaveAsText(string FileName, string Index, int Option)
        {
            if (Option == 1)
            {
                SaveAsTextOp1(FileName, Index);
            }
            else if (Option == 2)
            {
                SaveAsTextOp2(FileName, Index);
            }
            else
            {
                MessageBox.Show("SaveAsText Option invalid");
            }
        }

        public void SaveAsTextOp1(string FileName, string Index)
        {
            Directory.CreateDirectory("Export Text");

            string FileNameWE = Path.GetFileName(FileName);
            FileStream FS = new FileStream(@"Export Text\\NAMES.TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FS.Position = FS.Length;
            using (StreamWriter SW = new StreamWriter(FS))
                foreach (var NAME in name)
                {
                    SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
                }


            string DirectoryName = new DirectoryInfo(Path.GetDirectoryName(FileName)).Name;
            FS = new FileStream("Export Text\\" + DirectoryName.ToUpper() + ".TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FS.Position = FS.Length;
            using (StreamWriter SW = new StreamWriter(FS))
            {
                List<name> Name = name.ToList();
                foreach (var MSG in msg)
                {
                    foreach (var STR in MSG.Strings)
                    {
                        SW.Write(FileNameWE + "\t");
                        SW.Write(Index + "\t");
                        SW.Write(MSG.Name + "\t");
                        SW.Write(STR.Index + "\t");
                        if (Name.Exists(x => x.Index == MSG.Character_Index))
                        {
                            name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
                            SW.Write(Name_i.OldName);
                        }
                        else if (MSG.Type == "SEL")
                        {
                            SW.Write("<SELECT>");
                        }
                        else { SW.Write("<NO_NAME>"); }

                        SW.Write("\t");
                        var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
                        SW.Write(split[0]);
                        for (int i = 1; i < split.Length; i++)
                        {
                            SW.Write(" " + split[i]);
                        }

                        SW.WriteLine();
                    }
                }
            }
        }

        public void SaveAsTextOp2(string FileName, string Index)
        {

            string newFileName = Index == "" ? Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".TXT"
                : Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "-" + Index + ".TXT";

            using (StreamWriter SW = new StreamWriter(new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
                foreach (var NAME in name)
                {
                    SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
                }
                SW.WriteLine();

                List<name> Name = name.ToList();
                foreach (var MSG in msg)
                {
                    foreach (var STR in MSG.Strings)
                    {
                        SW.Write(MSG.Name + "\t");
                        SW.Write(STR.Index + "\t");
                        if (Name.Exists(x => x.Index == MSG.Character_Index))
                        {
                            name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
                            SW.Write(Name_i.OldName);
                        }
                        else if (MSG.Type == "SEL")
                        {
                            SW.Write("<SELECT>");
                        }
                        else { SW.Write("<NO_NAME>"); }

                        SW.Write("\t");
                        var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
                        SW.Write(split[0]);
                        for (int i = 1; i < split.Length; i++)
                        {
                            SW.Write(" " + split[i]);
                        }

                        SW.WriteLine();
                    }
                }
            }
        }

        private void SaveAsMSG1(string FileName, Stream MS)
        {
            SaveAsMSG1(FileName, "", MS);
        }

        private void SaveAsMSG1(string FileName, string Add, Stream MS)
        {
            FileInfo FI = new FileInfo(FileName);
            if (FI.Extension != ".MSG1")
                MS.SaveToFile(FileName + Add + ".MSG1");
        }

        public MemoryStream GetNewMSG1()
        {
            byte[] buffer;

            using (MemoryStream MS = new MemoryStream())
            {
                List<List<int>> MSG_pos = new List<List<int>>();
                List<int> NAME_pos = new List<int>();
                List<int> LastBlock = new List<int>();

                MS.WriteInt(0x7);
                MS.WriteInt(0x0);
                MS.WriteString("MSG1", 8);
                MS.WriteInt(0x0);
                MS.WriteInt(0x0);
                MS.WriteInt(msg.Count);
                MS.WriteInt(0x20000);

                foreach (var MSG in msg)
                {
                    if (MSG.Type == "MSG") { MS.WriteInt(0x0); }
                    else if (MSG.Type == "SEL") { MS.WriteInt(0x1); }
                    else { MessageBox.Show("SaveMSG1 Error"); }

                    LastBlock.Add((int)MS.Position);
                    MS.WriteInt(0x0);
                }

                LastBlock.Add((int)MS.Position);
                MS.WriteInt(0x0);
                MS.WriteInt(name.Count);
                MS.WriteInt(0x0);
                MS.WriteInt(0x0);

                foreach (var MSG in msg)
                {
                    List<int> MSG_o = new List<int>();
                    MSG_o.Add((int)MS.Position);

                    MS.WriteString(MSG.Name, 24);

                    if (MSG.Type == "MSG")
                    {
                        MS.WriteUshort(MSG.Strings.Count);

                        if (MSG.Character_Index == -1) { MS.WriteUshort(0xFFFF); }
                        else { MS.WriteUshort(MSG.Character_Index); }
                    }
                    else if (MSG.Type == "SEL")
                    {
                        MS.WriteUshort(0);
                        MS.WriteUshort((ushort)MSG.Strings.Count);
                        MS.WriteInt(0x0);
                    }

                    int Size = 0;

                    foreach (var String in MSG.Strings)
                    {
                        LastBlock.Add((int)MS.Position);
                        MS.WriteInt(0x0);
                        foreach (var Str in String.Prefix_bytes)
                        {
                            Size += Str.Bytes.Length;
                        }
                        foreach (var Str in String.New_string_bytes)
                        {
                            Size += Str.Bytes.Length;
                        }
                        foreach (var Str in String.Postfix_bytes)
                        {
                            Size += Str.Bytes.Length;
                        }
                    }
                    MSG_o.Add(Size);

                    MS.WriteInt(0x0);

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

                        MSG_o.Add((int)MS.Position);
                        MS.Write(NewString.ToArray(), 0, NewString.Count);
                    }

                    while (MS.Length % 4 != 0)
                    {
                        MS.WriteByte(0);
                    }

                    MSG_pos.Add(MSG_o);
                }

                long Name_Block_pos = MS.Length;
                MS.Position = 0x20;
                for (int i = 0; i < msg.Count; i++)
                {
                    MS.Position += 4;
                    MS.WriteInt(MSG_pos[i][0] - 0x20);
                }
                MS.WriteInt((int)Name_Block_pos - 0x20);
                for (int i = 0; i < msg.Count; i++)
                {
                    MS.Position = MSG_pos[i][0];

                    if (msg[i].Type == "MSG")
                    {
                        MS.Position += 28;
                    }
                    else if (msg[i].Type == "SEL")
                    {
                        MS.Position += 32;
                    }
                    else { MessageBox.Show("SD"); }

                    for (int k = 0; k < msg[i].Strings.Count; k++)
                    {
                        MS.WriteInt(MSG_pos[i][k + 2] - 0x20);
                    }
                    MS.WriteInt(MSG_pos[i][1]);
                }


                MS.Position = Name_Block_pos;
                for (int i = 0; i < name.Count; i++)
                {
                    LastBlock.Add((int)MS.Position);
                    MS.WriteInt(0);
                }

                foreach (var NAME in name)
                {
                    NAME_pos.Add((int)MS.Position);
                    if (NAME.NewNameBytes.Length == 0)
                    {
                        MS.Write(NAME.OldNameBytes, 0, NAME.OldNameBytes.Length);
                    }
                    else
                    {
                        MS.Write(NAME.NewNameBytes, 0, NAME.NewNameBytes.Length);
                    }
                    MS.WriteByte(0);
                }
                MS.Position = Name_Block_pos;
                for (int i = 0; i < name.Count; i++)
                {
                    MS.WriteInt(NAME_pos[i] - 0x20);
                }
                MS.Position = MS.Length;
                while (MS.Length % 4 != 0)
                {
                    MS.WriteByte(0);
                }

                int LastBlockPos = (int)MS.Position;
                byte[] LastBlockBytes = getLastBlock(LastBlock);
                MS.Write(LastBlockBytes, 0, LastBlockBytes.Length);

                MS.Position = 0x10;
                MS.WriteInt(LastBlockPos);
                MS.WriteInt(LastBlockBytes.Length);

                MS.Position = 0x4;
                MS.WriteInt((int)MS.Length);

                MS.Position = 0;
                buffer = new byte[MS.Length];
                MS.Read(buffer, 0, (int)MS.Length);
            }

            return new MemoryStream(buffer);
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
                else
                {
                    return 0;
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
    }

    class Project
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
                Document.Add(new XAttribute("SourceFileName", Static.FileInfo.SourceFileName));
                Document.Add(new XAttribute("Position", Static.FileInfo.SelectPosition));
                xDoc.Add(Document);
                XElement CharName = new XElement("CharacterNames");
                Document.Add(CharName);

                foreach (var NAME in name)
                {
                    XElement Name = new XElement("Name");
                    Name.Add(new XAttribute("Index", NAME.Index));
                    Name.Add(new XElement("OldNameSource", BitConverter.ToString(NAME.OldNameBytes)));
                    Name.Add(new XElement("NewName", NAME.NewName));
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

                        foreach (var A in STR.Prefix_bytes)
                        {
                            XElement PrefixBytes = new XElement("PrefixBytes", BitConverter.ToString(A.Bytes));
                            PrefixBytes.Add(new XAttribute("Index", STR.Prefix_bytes.IndexOf(A)));
                            PrefixBytes.Add(new XAttribute("Type", A.Type));
                            String.Add(PrefixBytes);
                        }

                        foreach (var A in STR.Old_string_bytes)
                        {
                            XElement OldStringBytes = new XElement("OldStringBytes", BitConverter.ToString(A.Bytes));
                            OldStringBytes.Add(new XAttribute("Index", STR.Old_string_bytes.IndexOf(A)));
                            OldStringBytes.Add(new XAttribute("Type", A.Type));
                            String.Add(OldStringBytes);
                        }

                        String.Add(new XElement("NewString", STR.New_string));

                        foreach (var A in STR.Postfix_bytes)
                        {
                            XElement PostfixBytes = new XElement("PostfixBytes", BitConverter.ToString(A.Bytes));
                            PostfixBytes.Add(new XAttribute("Index", STR.Postfix_bytes.IndexOf(A)));
                            PostfixBytes.Add(new XAttribute("Type", A.Type));
                            String.Add(PostfixBytes);
                        }
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
                XDocument xDoc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
                XElement MSG1 = xDoc.Element("MSG1");
                XAttribute temp = MSG1.Attribute("Position");
                Static.FileInfo.SelectPosition = temp != null ? Convert.ToInt32(temp.Value) : -1;

                temp = MSG1.Attribute("SourceFileName");
                Static.FileInfo.SourceFileName = temp != null ? temp.Value : "";

                foreach (var NAME in MSG1.Element("CharacterNames").Elements())
                {
                    int Index = Convert.ToInt32(NAME.Attribute("Index").Value);

                    byte[] OldNameSource = new byte[0];
                    string OldNameSource_str = NAME.Element("OldNameSource").Value;
                    if (OldNameSource_str != "")
                    {
                        OldNameSource = Enumerable.Range(0, OldNameSource_str.Split('-').Length).Select(x => Convert.ToByte(OldNameSource_str.Split('-')[x], 16)).ToArray();
                    }

                    string NewName = NAME.Element("NewName").Value;

                    name.Add(new name { Index = Index, OldNameBytes = OldNameSource, NewName = NewName });
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

                    BindingList<MyString> MSGStrings = new BindingList<MyString>();
                    foreach (var Strings in Message.Element("MessageStrings").Elements())
                    {
                        MyString String = new MyString();

                        String.Index = Convert.ToInt32(Strings.Attribute("Index").Value);

                        foreach (var PrefixByte in Strings.Elements("PrefixBytes"))
                        {
                            int PrefixIndex = Convert.ToInt32(PrefixByte.Attribute("Index").Value);
                            arrayType PrefixType = Util.GetType(PrefixByte.Attribute("Type").Value);

                            byte[] PrefixBytes = new byte[0];
                            string PrefixBytes_str = PrefixByte.Value;
                            if (PrefixBytes_str != "")
                            {
                                PrefixBytes = Enumerable.Range(0, PrefixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PrefixBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            String.Prefix_bytes.Add(new MyByteArray { Index = PrefixIndex, Type = PrefixType, Bytes = PrefixBytes });
                        }

                        foreach (var Old in Strings.Elements("OldStringBytes"))
                        {
                            int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
                            arrayType OldType = Util.GetType(Old.Attribute("Type").Value);

                            byte[] OldBytes = new byte[0];
                            string OldBytes_str = Old.Value;
                            if (OldBytes_str != "")
                            {
                                OldBytes = Enumerable.Range(0, OldBytes_str.Split('-').Length).Select(x => Convert.ToByte(OldBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            String.Old_string_bytes.Add(new MyByteArray { Index = OldIndex, Type = OldType, Bytes = OldBytes });
                        }

                        String.New_string = Strings.Element("NewString").Value;

                        foreach (var PostfixByte in Strings.Elements("PostfixBytes"))
                        {
                            int PostfixIndex = Convert.ToInt32(PostfixByte.Attribute("Index").Value);
                            arrayType PostfixType = Util.GetType(PostfixByte.Attribute("Type").Value);

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
                    msg.Add(new msg { Index = Index, Type = Type, Name = Name, Character_Index = CharacterNameIndex, SourceBytes = SourceBytes, Strings = MSGStrings });
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }

    class PersonaFileTypes
    {
        public class PMD1
        {
            class Header
            {
                private int Head { get; set; }
                public int Size { get; private set; }
                public long Name { get; private set; }
                public int TableLineCount { get; private set; }
                private int[] Unknown { get; set; }

                public Header(MemoryStream header)
                {
                    Head = header.ReadInt();
                    Size = header.ReadInt();
                    Name = header.ReadLong();
                    TableLineCount = header.ReadInt();
                    Unknown = header.ReadIntArray(3);
                }

                public MemoryStream GetMS()
                {
                    MemoryStream returned = new MemoryStream();
                    returned.WriteInt(Head);
                    returned.WriteInt(Size);
                    returned.WriteLong(Name);
                    returned.WriteInt(TableLineCount);
                    returned.WriteIntArray(Unknown);

                    returned.Position = 0;
                    return returned;
                }

                public void Shift(int shift)
                {
                    Size += shift;
                }
            }

            class Table
            {
                public class Element
                {
                    public int Index { get; set; } = -1;
                    public int Size { get; set; } = 0;
                    public int Count { get; set; } = 0;
                    public int Position { get; set; } = 0;

                    public Element(int index, int size, int count, int position)
                    {
                        Index = index;
                        Size = size;
                        Count = count;
                        Position = position;
                    }
                }

                public List<Element> PM1Table { get; private set; } = new List<Element>();

                public Table(int[][] array)
                {
                    for (int i = 0; i < array.Length; i++)
                        PM1Table.Add(new Element(array[i][0], array[i][1], array[i][2], array[i][3]));
                }

                public MemoryStream GetMS()
                {
                    MemoryStream returned = new MemoryStream();

                    foreach (var line in PM1Table)
                    {
                        returned.WriteInt(line.Index);
                        returned.WriteInt(line.Size);
                        returned.WriteInt(line.Count);
                        returned.WriteInt(line.Position);
                    }

                    returned.Position = 0;
                    return returned;
                }

                public void Shift(int shift)
                {
                    PM1Table.FindAll(x => x.Index > 0x6).ForEach(a => a.Position += shift);
                }
            }

            class MapElement
            {
                public string Name { get; set; }
                public int Index { get; set; }

            }
            static List<MapElement> Map = new List<MapElement>()
            {
                new MapElement { Index = 0x1, Name = "File List" },
                new MapElement { Index = 0x2, Name = "T3 Header List" },
                new MapElement { Index = 0x3, Name = "RMD Header List"}, // E603_001.PM1 P3FES PB Header List
                new MapElement { Index = 0x6, Name = "MSG" },
                new MapElement { Index = 0x7, Name = "EPL Header List" },
                new MapElement { Index = 0x8, Name = "EPL" },
                new MapElement { Index = 0x9, Name = "RMD" }, // E603_001.PM1 P3FES PB
            };

            Header _Header;
            Table _Table;

            class PM1element
            {
                public string Name { get; set; }
                public List<MemoryStream> StreamList { get; set; }
            }

            List<PM1element> PM1List = new List<PM1element>();

            public PMD1(string SourceFile)
            {
                FileStream FileStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
                _Header = new Header(new MemoryStream(FileStream.ReadByteArray(0x20)));
                _Table = new Table(FileStream.ReadIntArrayArray(_Header.TableLineCount, 4));

                foreach (var element in _Table.PM1Table)
                {
                    if (element.Size * element.Count > 0)
                    {
                        MapElement tempmap = Map.Find(x => x.Index == element.Index);
                        if (tempmap != null)
                        {
                            PM1element temp = new PM1element();
                            temp.Name = tempmap.Name;
                            temp.StreamList = GetListMS(FileStream, element);
                            PM1List.Add(temp);
                        }
                        else
                        {
                            MessageBox.Show("PM1: unknown element");
                            return;
                        }
                    }
                }



            }

            private List<MemoryStream> GetListMS(FileStream FS, Table.Element line)
            {
                List<MemoryStream> returned = new List<MemoryStream>();
                FS.Position = line.Position;

                for (int i = 0; i < line.Count; i++)
                {
                    returned.Add(new MemoryStream(FS.ReadByteArray(line.Size)));
                }

                return returned;
            }

            public void SaveNew(string FileName)
            {
                using (FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    _Header.GetMS().CopyTo(FS);
                    _Table.GetMS().CopyTo(FS);
                    foreach (var a in PM1List)
                    {
                        foreach (var b in a.StreamList)
                        {
                            b.Position = 0;
                            b.CopyTo(FS);
                        }
                    }
                }
            }

            public void SetNewMSG1(MemoryStream NewMSG)
            {
                int SizeShift = ChangeMSG(NewMSG);
                ShiftEPLHeader(SizeShift);
                ShiftRMDHeader(SizeShift);
                _Table.Shift(SizeShift);
                _Header.Shift(SizeShift);
            }

            private int ChangeMSG(MemoryStream NewMSG)
            {
                PM1element temp = PM1List.Find(x => x.Name == "MSG");
                if (temp != null)
                {
                    if (temp.StreamList.Count == 1)
                    {

                        MemoryStream newMSG = new MemoryStream();

                        NewMSG.CopyTo(newMSG);
                        while (newMSG.Length % 16 != 0)
                        {
                            newMSG.WriteByte(0);
                        }

                        int SizeShift = Convert.ToInt32(newMSG.Length - temp.StreamList.First().Length);
                        temp.StreamList.Clear();
                        temp.StreamList.Add(newMSG);

                        _Table.PM1Table.Find(x => x.Index == 0x6).Size = (int)newMSG.Length;

                        return SizeShift;
                    }
                    else if (temp.StreamList.Count > 1)
                    {
                        MessageBox.Show("Exception: 2 or more MSG");
                        return -1;
                    }
                    else
                    {
                        MessageBox.Show("Exception: 0 MSG");
                        return -1;
                    }
                }
                else
                {
                    Console.WriteLine("File does not contain MSG");
                    return -1;
                }
            }

            private void ShiftEPLHeader(int shift)
            {
                PM1element temp = PM1List.Find(x => x.Name == "EPL Header List");
                if (temp != null)
                {
                    if (temp.StreamList.Count >= 1)
                    {
                        foreach (var EPL in temp.StreamList)
                        {
                            EPL.Position = 4;
                            int Size = EPL.ReadInt();
                            Size += shift;
                            EPL.Position = 4;
                            EPL.WriteInt(Size);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Exception: 0 EPL Header");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("File does not contain EPL Header");
                    return;
                }
            }

            private void ShiftRMDHeader(int shift)
            {
                PM1element temp = PM1List.Find(x => x.Name == "RMD Header List");
                if (temp != null)
                {
                    if (temp.StreamList.Count >= 1)
                    {
                        foreach (var RMD in temp.StreamList)
                        {
                            RMD.Position = 0x10;
                            int Size = RMD.ReadInt();
                            Size += shift;
                            RMD.Position = 0x10;
                            RMD.WriteInt(Size);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Exception: 0 EPL Header");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("File does not contain EPL Header");
                    return;
                }
            }
        }

        public class FLW0
        {
            class Header
            {
                private int Head { get; set; }
                public int Size { get; private set; }
                public long Name { get; private set; }
                public int TableLineCount { get; private set; }
                private int[] Unknown { get; set; }

                public Header(MemoryStream header)
                {
                    Head = header.ReadInt();
                    Size = header.ReadInt();
                    Name = header.ReadLong();
                    TableLineCount = header.ReadInt();
                    Unknown = header.ReadIntArray(3);
                }

                public MemoryStream GetMS()
                {
                    MemoryStream returned = new MemoryStream();
                    returned.WriteInt(Head);
                    returned.WriteInt(Size);
                    returned.WriteLong(Name);
                    returned.WriteInt(TableLineCount);
                    returned.WriteIntArray(Unknown);

                    returned.Position = 0;
                    return returned;
                }

                public void Shift(int shift)
                {
                    Size += shift;
                }
            }

            class Table
            {
                public class Element
                {
                    public int Index { get; set; } = -1;
                    public int Size { get; set; } = 0;
                    public int Count { get; set; } = 0;
                    public int Position { get; set; } = 0;

                    public Element(int index, int size, int count, int position)
                    {
                        Index = index;
                        Size = size;
                        Count = count;
                        Position = position;
                    }
                }

                public List<Element> FLW0Table { get; private set; } = new List<Element>();

                public Table(int[][] array)
                {
                    for (int i = 0; i < 3; i++)
                        FLW0Table.Add(new Element(array[i][0], array[i][1], array[i][2], array[i][3]));
                    for (int i = 3; i < array.Length; i++)
                        FLW0Table.Add(new Element(array[i][0], array[i][2], array[i][1], array[i][3]));
                }

                public MemoryStream GetMS()
                {
                    MemoryStream returned = new MemoryStream();

                    for (int i = 0; i < 3; i++)
                    {
                        returned.WriteInt(FLW0Table[i].Index);
                        returned.WriteInt(FLW0Table[i].Size);
                        returned.WriteInt(FLW0Table[i].Count);
                        returned.WriteInt(FLW0Table[i].Position);
                    }
                    for (int i = 3; i < FLW0Table.Count; i++)
                    {
                        returned.WriteInt(FLW0Table[i].Index);
                        returned.WriteInt(FLW0Table[i].Size);
                        returned.WriteInt(FLW0Table[i].Count);
                        returned.WriteInt(FLW0Table[i].Position);
                    }

                    returned.Position = 0;
                    return returned;
                }

                public void Shift(int shift)
                {
                    FLW0Table.FindAll(x => x.Index > 0x3).ForEach(a => a.Position += shift);
                }
            }

            Header _Header;
            Table _Table;

            class elementFLW0
            {
                public string Name { get; set; }
                public List<MemoryStream> StreamList { get; set; }
            }

            List<elementFLW0> FLW0List = new List<elementFLW0>();

            class MapElement
            {
                public string Name { get; set; }
                public int Index { get; set; }

            }
            static List<MapElement> Map = new List<MapElement>()
            {
                new MapElement { Index = 0x0, Name = "0" },
                new MapElement { Index = 0x1, Name = "1" },
                new MapElement { Index = 0x2, Name = "2" },
                new MapElement { Index = 0x3, Name = "MSG" },
                new MapElement { Index = 0x4, Name = "4" },
            };

            public FLW0(string SourceFile)
            {
                FileStream FileStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
                _Header = new Header(new MemoryStream(FileStream.ReadByteArray(0x20)));
                _Table = new Table(FileStream.ReadIntArrayArray(_Header.TableLineCount, 4));

                foreach (var element in _Table.FLW0Table)
                {
                    if (element.Size * element.Count > 0)
                    {
                        MapElement tempmap = Map.Find(x => x.Index == element.Index);
                        if (tempmap != null)
                        {
                            elementFLW0 temp = new elementFLW0();
                            temp.Name = tempmap.Name;
                            temp.StreamList = GetListMS(FileStream, element);
                            FLW0List.Add(temp);
                        }
                        else
                        {
                            MessageBox.Show("FLW0: unknown element");
                            return;
                        }
                    }
                }
            }

            public void SaveNew(string FileName)
            {
                using (FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    _Header.GetMS().CopyTo(FS);
                    _Table.GetMS().CopyTo(FS);
                    foreach (var a in FLW0List)
                    {
                        foreach (var b in a.StreamList)
                        {
                            b.Position = 0;
                            b.CopyTo(FS);
                        }
                    }
                }
            }

            private List<MemoryStream> GetListMS(FileStream FS, Table.Element line)
            {
                List<MemoryStream> returned = new List<MemoryStream>();
                FS.Position = line.Position;

                for (int i = 0; i < line.Count; i++)
                {
                    returned.Add(new MemoryStream(FS.ReadByteArray(line.Size)));
                }

                return returned;
            }

            public void SetNewMSG1(MemoryStream NewMSG)
            {
                int SizeShift = ChangeMSG(NewMSG);
                _Table.Shift(SizeShift);
                _Header.Shift(SizeShift);
            }

            private int ChangeMSG(MemoryStream NewMSG)
            {
                elementFLW0 temp = FLW0List.Find(x => x.Name == "MSG");
                if (temp != null)
                {
                    if (temp.StreamList.Count == 1)
                    {

                        MemoryStream newMSG = new MemoryStream();

                        NewMSG.CopyTo(newMSG);
                        while (newMSG.Length % 16 != 0)
                        {
                            newMSG.WriteByte(0);
                        }

                        int SizeShift = Convert.ToInt32(newMSG.Length - temp.StreamList.First().Length);
                        temp.StreamList.Clear();
                        temp.StreamList.Add(newMSG);

                        _Table.FLW0Table.Find(x => x.Index == 0x3).Size = (int)newMSG.Length;

                        return SizeShift;
                    }
                    else if (temp.StreamList.Count > 1)
                    {
                        MessageBox.Show("Exception: 2 or more MSG");
                        return -1;
                    }
                    else
                    {
                        MessageBox.Show("Exception: 0 MSG");
                        return -1;
                    }
                }
                else
                {
                    Console.WriteLine("File does not contain MSG");
                    return -1;
                }
            }

        }
    }
}