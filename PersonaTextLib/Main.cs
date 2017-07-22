using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PersonaTextLib.Extensions;
using PersonaTextLib.Types;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Diagnostics;

namespace PersonaTextLib
{
    static class Logging
    {
        public static void Write(string String)
        {
            File.AppendAllText("PersonaText.log", DateTime.Now + ": " + String + "\r\n", Encoding.UTF8);
        }
    }

    public class PersonaFileTypes
    {
        public class PMD1
        {
            bool IsLittleEndian { get; set; } = true;

            class Header
            {
                private int Head { get; set; }
                public int Size { get; private set; }
                public long Name { get; private set; }
                public int TableLineCount { get; private set; }
                private int[] Unknown { get; set; }

                public Header(BinaryReader BR)
                {
                    Head = BR.ReadInt32();
                    Size = BR.ReadInt32();
                    Name = BR.ReadInt64();
                    TableLineCount = BR.ReadInt32();
                    Unknown = new int[3] { BR.ReadInt32(), BR.ReadInt32(), BR.ReadInt32() };
                }

                public MemoryStream GetMS(bool IsLittleEndian)
                {
                    BinaryWriter BW;

                    if (IsLittleEndian)
                        BW = new BinaryWriter(new MemoryStream());
                    else
                        BW = new BinaryWriterBE(new MemoryStream());

                    BW.Write(Head);
                    BW.Write(Size);
                    BW.Write(Name);
                    BW.Write(TableLineCount);
                    BW.WriteInt32Array(Unknown);

                    byte[] buffer = new byte[BW.BaseStream.Length];
                    BW.BaseStream.Position = 0;
                    BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                    return new MemoryStream(buffer);
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

                public MemoryStream GetMS(bool IsLittleEndian)
                {
                    BinaryWriter BW;

                    if (IsLittleEndian)
                        BW = new BinaryWriter(new MemoryStream());
                    else
                        BW = new BinaryWriterBE(new MemoryStream());

                    foreach (var line in PM1Table)
                    {
                        BW.Write(line.Index);
                        BW.Write(line.Size);
                        BW.Write(line.Count);
                        BW.Write(line.Position);
                    }

                    byte[] buffer = new byte[BW.BaseStream.Length];
                    BW.BaseStream.Position = 0;
                    BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                    return new MemoryStream(buffer);
                }

                public void Shift(int shift)
                {
                    PM1Table.FindAll(x => x.Index > 0x6).ForEach(a => a.Position += shift);
                }
            }

            enum Map
            {
                FileList = 0x1,
                T3HeadList = 0x2,
                RMDHeadList = 0x3,
                MSG = 0x6,
                EPLHeadList = 0x7,
                EPL = 0x8,
                RMD = 0x9
            }

            Header _Header;
            Table _Table;

            class PM1element
            {
                public Map Type { get; set; }
                public List<MemoryStream> StreamList { get; set; }
            }

            List<PM1element> PM1List = new List<PM1element>();

            public PMD1(string FileName, bool IsLittleEndian)
            {
                this.IsLittleEndian = IsLittleEndian;

                BinaryReader BR;

                if (IsLittleEndian)
                    BR = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read));
                else
                    BR = new BinaryReaderBE(new FileStream(FileName, FileMode.Open, FileAccess.Read));

                _Header = new Header(BR);
                _Table = new Table(BR.ReadInt32ArrayArray(_Header.TableLineCount, 4));


                foreach (var element in _Table.PM1Table)
                {
                    if (element.Size * element.Count > 0)
                    {
                        PM1element temp = new PM1element()
                        {
                            Type = (Map)element.Index,
                            StreamList = GetListMS(BR, element)
                        };
                        PM1List.Add(temp);
                    }
                }
            }

            private List<MemoryStream> GetListMS(BinaryReader BR, Table.Element line)
            {
                List<MemoryStream> returned = new List<MemoryStream>();
                BR.BaseStream.Position = line.Position;

                for (int i = 0; i < line.Count; i++)
                    returned.Add(new MemoryStream(BR.ReadBytes(line.Size)));

                return returned;
            }

            public void SaveNew(string FileName)
            {
                using (FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    _Header.GetMS(IsLittleEndian).CopyTo(FS);
                    _Table.GetMS(IsLittleEndian).CopyTo(FS);
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

            int ChangeMSG(MemoryStream NewMSG)
            {
                PM1element temp = PM1List.Find(x => x.Type == Map.MSG);
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
                        Logging.Write("Exception: 2 or more MSG");
                        return -1;
                    }
                    else
                    {
                        Logging.Write("Exception: 0 MSG");
                        return -1;
                    }
                }
                else
                {
                    Logging.Write("File does not contain MSG");
                    return -1;
                }
            }

            private void ShiftEPLHeader(int shift)
            {
                PM1element temp = PM1List.Find(x => x.Type == Map.EPLHeadList);
                if (temp != null)
                {
                    if (temp.StreamList.Count >= 1)
                    {
                        foreach (var EPL in temp.StreamList)
                        {
                            BinaryReader BR;

                            if (IsLittleEndian)
                                BR = new BinaryReader(EPL);
                            else
                                BR = new BinaryReaderBE(EPL);

                            BR.BaseStream.Position = 4;
                            int Size = BR.ReadInt32();
                            Size += shift;
                            BR.BaseStream.Position = 4;

                            BinaryWriter BW;

                            if (IsLittleEndian)
                                BW = new BinaryWriter(EPL);
                            else
                                BW = new BinaryWriterBE(EPL);

                            BW.Write(Size);
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
                PM1element temp = PM1List.Find(x => x.Type == Map.RMDHeadList);
                if (temp != null)
                {
                    if (temp.StreamList.Count >= 1)
                    {
                        foreach (var RMD in temp.StreamList)
                        {
                            BinaryReader BR;

                            if (IsLittleEndian)
                                BR = new BinaryReader(RMD);
                            else
                                BR = new BinaryReaderBE(RMD);

                            BR.BaseStream.Position = 0x10;
                            int Size = BR.ReadInt32();
                            Size += shift;
                            BR.BaseStream.Position = 0x10;

                            BinaryWriter BW;

                            if (IsLittleEndian)
                                BW = new BinaryWriter(RMD);
                            else
                                BW = new BinaryWriterBE(RMD);

                            BW.Write(Size);
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

            public byte[] GetMSG
            {
                get { return PM1List.Find(x => x.Type == Map.MSG).StreamList.First().ToArray(); }
            }
        }

        public class FLW0
        {
            bool IsLittleEndian { get; set; } = true;

            class Header
            {
                private int Head { get; set; }
                public int Size { get; private set; }
                public long Name { get; private set; }
                public int TableLineCount { get; private set; }
                private int[] Unknown { get; set; }

                public Header(BinaryReader BR)
                {
                    Head = BR.ReadInt32();
                    Size = BR.ReadInt32();
                    Name = BR.ReadInt64();
                    TableLineCount = BR.ReadInt32();
                    Unknown = new int[3] { BR.ReadInt32(), BR.ReadInt32(), BR.ReadInt32() };
                }

                public MemoryStream GetMS(bool IsLittleEndian)
                {
                    BinaryWriter BW;

                    if (IsLittleEndian)
                        BW = new BinaryWriter(new MemoryStream());
                    else
                        BW = new BinaryWriterBE(new MemoryStream());

                    BW.Write(Head);
                    BW.Write(Size);
                    BW.Write(Name);
                    BW.Write(TableLineCount);
                    BW.WriteInt32Array(Unknown);

                    byte[] buffer = new byte[BW.BaseStream.Length];
                    BW.BaseStream.Position = 0;
                    BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                    return new MemoryStream(buffer);
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

                public MemoryStream GetMS(bool IsLittleEndian)
                {
                    BinaryWriter BW;

                    if (IsLittleEndian)
                        BW = new BinaryWriter(new MemoryStream());
                    else
                        BW = new BinaryWriterBE(new MemoryStream());

                    for (int i = 0; i < 3; i++)
                    {
                        BW.Write(FLW0Table[i].Index);
                        BW.Write(FLW0Table[i].Size);
                        BW.Write(FLW0Table[i].Count);
                        BW.Write(FLW0Table[i].Position);
                    }
                    for (int i = 3; i < FLW0Table.Count; i++)
                    {
                        BW.Write(FLW0Table[i].Index);
                        BW.Write(FLW0Table[i].Count);
                        BW.Write(FLW0Table[i].Size);
                        BW.Write(FLW0Table[i].Position);
                    }

                    byte[] buffer = new byte[BW.BaseStream.Length];
                    BW.BaseStream.Position = 0;
                    BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                    return new MemoryStream(buffer);
                }

                public void Shift(int shift)
                {
                    FLW0Table.FindAll(x => x.Index > 0x3).ForEach(a => a.Position += shift);
                }
            }

            Header _Header;
            Table _Table;

            enum Map
            {
                E0 = 0x0,
                E1 = 0x1,
                E2 = 0x2,
                MSG = 0x3,
                E4 = 0x4
            }

            class ElementFLW0
            {
                public Map Type { get; set; }
                public List<MemoryStream> StreamList { get; set; }
            }

            List<ElementFLW0> FLW0List = new List<ElementFLW0>();

            public FLW0(string SourceFile, bool IsLittleEndian)
            {
                this.IsLittleEndian = IsLittleEndian;
                BinaryReader BR;

                if (IsLittleEndian)
                    BR = new BinaryReader(new FileStream(SourceFile, FileMode.Open, FileAccess.Read));
                else
                    BR = new BinaryReaderBE(new FileStream(SourceFile, FileMode.Open, FileAccess.Read));

                _Header = new Header(BR);

                _Table = new Table(BR.ReadInt32ArrayArray(_Header.TableLineCount, 4));

                foreach (var element in _Table.FLW0Table)
                {
                    if (element.Size * element.Count > 0)
                    {
                        ElementFLW0 temp = new ElementFLW0()
                        {
                            Type = (Map)element.Index,
                            StreamList = GetListMS(BR, element)
                        };
                        FLW0List.Add(temp);
                    }
                }
            }

            public void SaveNew(string FileName)
            {
                using (FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    _Header.GetMS(IsLittleEndian).CopyTo(FS);
                    _Table.GetMS(IsLittleEndian).CopyTo(FS);
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

            private List<MemoryStream> GetListMS(BinaryReader BR, Table.Element line)
            {
                List<MemoryStream> returned = new List<MemoryStream>();
                BR.BaseStream.Position = line.Position;

                for (int i = 0; i < line.Count; i++)
                    returned.Add(new MemoryStream(BR.ReadBytes(line.Size)));

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
                ElementFLW0 temp = FLW0List.Find(x => x.Type == Map.MSG);
                if (temp != null)
                {
                    if (temp.StreamList.Count == 1)
                    {

                        MemoryStream newMSG = new MemoryStream();

                        NewMSG.CopyTo(newMSG);

                        int SizeShift = Convert.ToInt32(newMSG.Length - temp.StreamList.First().Length);
                        temp.StreamList.Clear();
                        temp.StreamList.Add(newMSG);

                        _Table.FLW0Table.Find(x => x.Index == 0x3).Size = (int)newMSG.Length;

                        return SizeShift;
                    }
                    else if (temp.StreamList.Count > 1)
                    {
                        Logging.Write("Exception: 2 or more MSG");
                        return -1;
                    }
                    else
                    {
                        Logging.Write("Exception: 0 MSG");
                        return -1;
                    }
                }
                else
                {
                    Logging.Write("File does not contain MSG");
                    return -1;
                }
            }

            public byte[] GetMSG
            {
                get { return FLW0List.Find(x => x.Type == Map.MSG).StreamList.First().ToArray(); }
            }
        }

        public class MSG1
        {
            public static MemoryStream GetMSG1FromFile(string FileName, bool IsLittleEndian)
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
                    PMD1 PMD1 = new PMD1(FileName, IsLittleEndian);
                    return new MemoryStream(PMD1.GetMSG);
                }
                else if (FileType == "FLW0")
                {
                    FLW0 FLW0 = new FLW0(FileName, IsLittleEndian);
                    return new MemoryStream(FLW0.GetMSG);
                }
                else if (FileType == "MSG1" | FileType == "1GSM")
                {
                    MemoryStream returned = null;
                    using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    {
                        returned = new MemoryStream();
                        FS.CopyTo(returned);
                    }
                    return returned;
                }
                else
                    return null;
            }

            public MSG1(bool IsLittleEndian, CharList OldChar, CharList NewChar)
            {
                this.OldChar = OldChar;
                this.NewChar = NewChar;
                this.IsLittleEndian = IsLittleEndian;
            }

            public int SaveAsTextOption { get; set; } = 0;

            public delegate void StringChangedEventHandler(string String);
            public delegate void ArrayChangedEventHandler(byte[] array);
            public delegate void ListChangedEventHandler(List<MyStringElement> list);
            public delegate void ElementArrayEventHandler(MyStringElement[] array);

            public struct MyStringElement
            {
                public enum arrayType
                {
                    Empty,
                    System,
                    Text
                }

                public string GetText(IList<FnMpData> CharList)
                {
                    if (Type == arrayType.System)
                    {
                        if (Bytes[0] == 0x0A)
                        {
                            return "\n";
                        }
                        else
                        {
                            return GetSystem();
                        }
                    }
                    else
                    {
                        string returned = "";
                        for (int i = 0; i < Bytes.Length; i++)
                        {
                            if (0x20 <= Bytes[i] & Bytes[i] < 0x80)
                            {
                                returned += CharList.GetChar(Bytes[i]);
                            }
                            else if (0x80 <= Bytes[i] & Bytes[i] < 0xF0)
                            {
                                int newindex = (Bytes[i] - 0x81) * 0x80 + Bytes[i + 1] + 0x20;

                                i++;
                                returned += CharList.GetChar(newindex);
                            }
                            else
                            {
                                Console.WriteLine("ASD");
                            }
                        }
                        return returned;
                    }
                }

                public string GetSystem()
                {
                    string returned = "";

                    if (Bytes.Length > 0)
                    {
                        returned += "{" + Convert.ToString(Bytes[0], 16).PadLeft(2, '0').ToUpper();
                        for (int i = 1; i < Bytes.Length; i++)
                        {
                            returned += " " + Convert.ToString(Bytes[i], 16).PadLeft(2, '0').ToUpper();
                        }
                        returned += "}";
                    }

                    return returned;
                }

                public MyStringElement(int Index, arrayType Type, byte[] Bytes)
                {
                    this.Index = Index;
                    this.Type = Type;
                    this.Bytes = Bytes;
                }

                public int Index { get; private set; }
                public arrayType Type { get; private set; }
                public byte[] Bytes { get; private set; }
            }

            public class Names : INotifyPropertyChanged
            {
                public event StringChangedEventHandler OldNameChanged;
                public event StringChangedEventHandler NewNameChanged;
                public event ArrayChangedEventHandler OldNameArrayChanged;
                public event ArrayChangedEventHandler NewNameArrayChanged;

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

                public CharList OldCharList { get; set; }
                public CharList NewCharList { get; set; }

                public Names(CharList Old, CharList New, int Index, byte[] OldNameBytes, string NewName)
                {
                    OldCharList = Old;
                    NewCharList = New;
                    this.Index = Index;
                    this.OldNameBytes = OldNameBytes;
                    this.NewName = NewName;

                    OldCharList.CharListChanged += OLD_CharListChanged;
                    NewCharList.CharListChanged += NEW_CharListChanged;
                }

                private void NEW_CharListChanged()
                {
                    NewNameBytes = NewName.GetMyByteArray(NewCharList.List).getAllBytes();
                }

                private void OLD_CharListChanged()
                {
                    OldName = OldNameBytes.parseString().GetString(OldCharList.List);
                }

                public int Index { get; set; } = 0;

                byte[] _OldNameBytes;
                byte[] _NewNameBytes;
                string _NewName = "";
                string _OldName = "";

                public byte[] OldNameBytes
                {
                    get { return _OldNameBytes; }
                    set
                    {
                        _OldNameBytes = value;
                        OldName = _OldNameBytes.parseString().GetString(OldCharList.List);
                        OldNameArrayChanged?.Invoke(_OldNameBytes);
                        Notify("OldNameBytes");
                    }
                }
                public string OldName
                {
                    get { return _OldName; }
                    set
                    {
                        if (value != _OldName)
                        {
                            _OldName = value;
                            OldNameChanged?.Invoke(_OldName);
                            Notify("OldName");
                        }
                    }
                }
                public byte[] NewNameBytes
                {
                    get { return _NewNameBytes; }
                    set
                    {
                        _NewNameBytes = value;
                        NewNameArrayChanged?.Invoke(_NewNameBytes);
                        Notify("NewNameBytes");
                    }
                }
                public string NewName
                {
                    get
                    {
                        return _NewName;
                    }
                    set
                    {
                        if (value != _NewName)
                        {
                            _NewName = value;
                            NewNameBytes = _NewName.GetMyByteArray(NewCharList.List).getAllBytes();
                            NewNameChanged?.Invoke(_NewName);
                            Notify("NewName");
                        }
                    }
                }
            }

            public class MSGs
            {
                public class MSGstr
                {
                    public class MSGstrElement : INotifyPropertyChanged
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

                        public enum Mode
                        {
                            ArrayToText,
                            ArrayToTextSYS,
                            TextToArray
                        }

                        public event StringChangedEventHandler TextChanged;
                        public event ElementArrayEventHandler ElementArrayChanged;

                        private Mode CurrentMode { get; set; }
                        public CharList CurrentCharList { get; set; }

                        private MyStringElement[] _ElementArray;
                        public MyStringElement[] ElementArray
                        {
                            get { return _ElementArray; }
                            set
                            {
                                _ElementArray = value;
                                if (CurrentMode == Mode.ArrayToText)
                                    Text = _ElementArray.GetString(CurrentCharList.List);
                                else if (CurrentMode == Mode.ArrayToTextSYS)
                                    Text = _ElementArray.GetString();
                                ElementArrayChanged?.Invoke(_ElementArray);
                            }
                        }

                        private string _Text = "";
                        public string Text
                        {
                            get { return _Text; }
                            set
                            {
                                if (value != _Text)
                                {
                                    _Text = value;
                                    TextChanged?.Invoke(Text);
                                    if (CurrentMode == Mode.TextToArray)
                                        ElementArray = Text.GetMyByteArray(CurrentCharList.List);
                                }
                            }
                        }

                        public MSGstrElement(CharList CharList, Mode Mode)
                        {
                            CurrentMode = Mode;

                            if (CurrentMode != Mode.ArrayToTextSYS)
                            {
                                CurrentCharList = CharList;
                                CurrentCharList.CharListChanged += CurrentCharList_CharListChanged;
                            }
                        }

                        private void CurrentCharList_CharListChanged()
                        {
                            if (CurrentMode == Mode.ArrayToText)
                                Text = _ElementArray.GetString(CurrentCharList.List);
                            else if (CurrentMode == Mode.TextToArray)
                                ElementArray = Text.GetMyByteArray(CurrentCharList.List);
                        }
                    }

                    public MSGstr(CharList Old, CharList New)
                    {
                        Prefix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
                        OldString = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToText);
                        NewString = new MSGstrElement(New, MSGstrElement.Mode.TextToArray);
                        Postfix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
                    }

                    public int Index { get; set; } = 0;
                    public MSGstrElement Prefix { get; set; }
                    public MSGstrElement OldString { get; set; }
                    public MSGstrElement NewString { get; set; }
                    public MSGstrElement Postfix { get; set; }
                }

                public enum MsgType
                {
                    MSG = 0,
                    SEL = 1
                }

                public MSGs(int Index, string Name, MsgType Type, int CharacterIndex, byte[] MsgBytes, CharList Old, CharList New)
                {
                    this.Index = Index;
                    this.Type = Type;
                    this.Name = Name;
                    this.CharacterIndex = CharacterIndex;
                    this.MsgBytes = MsgBytes;
                }

                public int Index { get; set; }
                public MsgType Type { get; set; }
                public string Name { get; set; }
                public int CharacterIndex { get; set; }
                public byte[] MsgBytes { get; set; }
                public List<MSGstr> Strings { get; set; } = new List<MSGstr>();
            }

            public bool IsLittleEndian { get; private set; }
            public CharList OldChar { get; private set; }
            public CharList NewChar { get; private set; }

            public BindingList<MSGs> msg { get; set; } = new BindingList<MSGs>();
            public BindingList<Names> name { get; set; } = new BindingList<Names>();

            public bool Load(string FileName, bool IsLittleEndian)
            {
                Stopwatch SW = new Stopwatch();
                SW.Start();
                this.IsLittleEndian = IsLittleEndian;

                if (File.Exists(FileName))
                {
                    try
                    {
                        MemoryStream MemoryStreamMSG1 = GetMSG1FromFile(FileName, IsLittleEndian);
                        ParseMSG1(MemoryStreamMSG1);
                    }
                    catch (Exception e)
                    {
                        Logging.Write(e.ToString());
                        return false;
                    }
                }
                else
                {
                    Logging.Write("The file does not exist");
                    return false;
                }

                SW.Stop();
                Console.WriteLine(SW.Elapsed.TotalSeconds);
                return true;
            }

            public MemoryStream GetNewMSG
            {
                get { return GetNewMSG1.Get(msg, name, IsLittleEndian); }
            }

            static class GetNewMSG1
            {
                public static MemoryStream Get(IList<MSGs> msg, IList<Names> name, bool IsLittleEndian)
                {
                    byte[] buffer;

                    BinaryWriter BW;

                    if (IsLittleEndian)
                        BW = new BinaryWriter(new MemoryStream());
                    else
                        BW = new BinaryWriterBE(new MemoryStream());

                    List<List<int>> MSG_pos = new List<List<int>>();
                    List<int> NAME_pos = new List<int>();
                    List<int> LastBlock = new List<int>();

                    buffer = new byte[4] { 7, 0, 0, 0 };
                    BW.Write(buffer);
                    BW.Write((int)0x0);

                    buffer = System.Text.Encoding.ASCII.GetBytes("MSG1");
                    if (!IsLittleEndian)
                        Array.Reverse(buffer);

                    BW.Write(buffer);

                    BW.Write((int)0x0);
                    BW.Write((int)0x0);
                    BW.Write((int)0x0);
                    BW.Write(msg.Count);
                    BW.Write((ushort)0);
                    BW.Write((ushort)0x2);

                    foreach (var MSG in msg)
                    {
                        if (MSG.Type == MSGs.MsgType.MSG) { BW.Write((int)0x0); }
                        else if (MSG.Type == MSGs.MsgType.SEL) { BW.Write((int)0x1); }
                        else
                        {
                            Logging.Write("Error: Unknown MSG Type");
                            return null;
                        }

                        LastBlock.Add((int)BW.BaseStream.Position);
                        BW.Write((int)0x0);
                    }

                    LastBlock.Add((int)BW.BaseStream.Position);
                    BW.Write((int)0x0);
                    BW.Write(name.Count);
                    BW.Write((int)0x0);
                    BW.Write((int)0x0);

                    foreach (var MSG in msg)
                    {
                        List<int> MSG_o = new List<int>();
                        MSG_o.Add((int)BW.BaseStream.Position);

                        BW.WriteString(MSG.Name, 24);

                        if (MSG.Type == MSGs.MsgType.MSG)
                        {
                            BW.Write((ushort)MSG.Strings.Count);

                            if (MSG.CharacterIndex == -1) { BW.Write((ushort)0xFFFF); }
                            else { BW.Write((ushort)MSG.CharacterIndex); }
                        }
                        else if (MSG.Type == MSGs.MsgType.SEL)
                        {
                            BW.Write((ushort)0);
                            BW.Write((ushort)MSG.Strings.Count);
                            BW.Write((int)0x0);
                        }

                        int Size = 0;

                        foreach (var String in MSG.Strings)
                        {
                            LastBlock.Add((int)BW.BaseStream.Position);
                            BW.Write((int)0x0);
                            foreach (var Str in String.Prefix.ElementArray)
                            {
                                Size += Str.Bytes.Length;
                            }
                            foreach (var Str in String.NewString.ElementArray)
                            {
                                Size += Str.Bytes.Length;
                            }
                            foreach (var Str in String.Postfix.ElementArray)
                            {
                                Size += Str.Bytes.Length;
                            }
                        }
                        MSG_o.Add(Size);

                        BW.Write((int)0x0);

                        foreach (var String in MSG.Strings)
                        {
                            List<byte> NewString = new List<byte>();
                            foreach (var prefix in String.Prefix.ElementArray)
                            {
                                NewString.AddRange(prefix.Bytes);
                            }
                            foreach (var str in String.NewString.ElementArray)
                            {
                                NewString.AddRange(str.Bytes);
                            }
                            foreach (var postfix in String.Postfix.ElementArray)
                            {
                                NewString.AddRange(postfix.Bytes);
                            }

                            MSG_o.Add((int)BW.BaseStream.Position);
                            BW.Write(NewString.ToArray());
                        }

                        while (BW.BaseStream.Length % 4 != 0)
                        {
                            BW.Write((byte)0);
                        }

                        MSG_pos.Add(MSG_o);
                    }

                    long Name_Block_pos = BW.BaseStream.Length;
                    BW.BaseStream.Position = 0x20;
                    for (int i = 0; i < msg.Count; i++)
                    {
                        BW.BaseStream.Position += 4;
                        BW.Write((int)MSG_pos[i][0] - 0x20);
                    }
                    BW.Write((int)Name_Block_pos - 0x20);
                    for (int i = 0; i < msg.Count; i++)
                    {
                        BW.BaseStream.Position = MSG_pos[i][0];

                        if (msg[i].Type == MSGs.MsgType.MSG)
                        {
                            BW.BaseStream.Position += 28;
                        }
                        else if (msg[i].Type == MSGs.MsgType.SEL)
                        {
                            BW.BaseStream.Position += 32;
                        }

                        for (int k = 0; k < msg[i].Strings.Count; k++)
                        {
                            BW.Write((int)MSG_pos[i][k + 2] - 0x20);
                        }
                        BW.Write((int)MSG_pos[i][1]);
                    }


                    BW.BaseStream.Position = Name_Block_pos;
                    for (int i = 0; i < name.Count; i++)
                    {
                        LastBlock.Add((int)BW.BaseStream.Position);
                        BW.Write((int)0);
                    }

                    foreach (var NAME in name)
                    {
                        NAME_pos.Add((int)BW.BaseStream.Position);
                        if (NAME.NewNameBytes.Length == 0)
                            BW.Write(NAME.OldNameBytes);
                        else
                            BW.Write(NAME.NewNameBytes);

                        BW.Write((byte)0);
                    }
                    BW.BaseStream.Position = Name_Block_pos;
                    for (int i = 0; i < name.Count; i++)
                    {
                        BW.Write((int)NAME_pos[i] - 0x20);
                    }
                    BW.BaseStream.Position = BW.BaseStream.Length;
                    while (BW.BaseStream.Length % 4 != 0)
                    {
                        BW.Write((byte)0);
                    }

                    int LastBlockPos = (int)BW.BaseStream.Position;
                    byte[] LastBlockBytes = getLastBlock(LastBlock);
                    BW.Write(LastBlockBytes);

                    BW.BaseStream.Position = 0x10;
                    BW.Write((int)LastBlockPos);
                    BW.Write((int)LastBlockBytes.Length);

                    BW.BaseStream.Position = 0x4;
                    BW.Write((int)BW.BaseStream.Length);

                    BW.BaseStream.Position = 0;

                    buffer = new byte[BW.BaseStream.Length];
                    BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                    return new MemoryStream(buffer);
                }

                static byte[] getLastBlock(List<int> Addresses)
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

                static int getSeq(ref List<int> Addresses, int index)
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

                static void Encode(int reloc, ref List<byte> LastBlock, ref int sum)
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

            void ParseMSG1(MemoryStream MemoryStreamMSG1)
            {
                BinaryReader BR;

                if (IsLittleEndian)
                    BR = new BinaryReader(MemoryStreamMSG1);
                else
                    BR = new BinaryReaderBE(MemoryStreamMSG1);

                BR.BaseStream.Position = 0;
                try
                {
                    name.Clear();
                    msg.Clear();

                    byte[] buffer;

                    int MSG_PointBlock_Pos = 0x20;
                    BR.BaseStream.Position = 24;
                    int MSG_count = BR.ReadInt32();
                    BR.BaseStream.Position = MSG_PointBlock_Pos;
                    List<int[]> MSG_Position = new List<int[]>();

                    for (int i = 0; i < MSG_count; i++)
                    {
                        int[] temp = new int[2];
                        temp[0] = BR.ReadInt32();
                        temp[1] = BR.ReadInt32();
                        MSG_Position.Add(temp);
                    }

                    int Name_Block_Position = BR.ReadInt32();
                    int Name_Count = BR.ReadInt32();
                    BR.BaseStream.Position = Name_Block_Position + MSG_PointBlock_Pos;
                    List<long> Name_Position = new List<long>();
                    for (int i = 0; i < Name_Count; i++)
                    {
                        Name_Position.Add(BR.ReadInt32());
                    }


                    int Index = 0;
                    foreach (var a in Name_Position)
                    {
                        BR.BaseStream.Position = a + MSG_PointBlock_Pos;
                        byte Byte = BR.ReadByte();
                        List<byte> Bytes = new List<byte>();
                        while (Byte != 0)
                        {
                            Bytes.Add(Byte);
                            Byte = BR.ReadByte();
                        }
                        name.Add(new Names(OldChar, NewChar, Index, Bytes.ToArray(), ""));
                        Index++;
                    }

                    Index = 0;

                    foreach (var pos in MSG_Position)
                    {
                        BR.BaseStream.Position = MSG_PointBlock_Pos + pos[1];
                        buffer = BR.ReadBytes(24);
                        string MSG_Name = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
                        if (string.IsNullOrEmpty(MSG_Name))
                        {
                            MSG_Name = "<EMPTY>";
                        }

                        byte[] MSG_bytes;
                        MSGs.MsgType Type;
                        int Character_Index = 0xFFFF;

                        if (pos[0] == 0)
                        {
                            Type = MSGs.MsgType.MSG;
                            int count = BR.ReadUInt16();
                            Character_Index = BR.ReadUInt16();
                            BR.BaseStream.Position = BR.BaseStream.Position + 4 * count;

                            int size = BR.ReadInt32();

                            MSG_bytes = BR.ReadBytes(size);
                        }
                        else if (pos[0] == 1)
                        {
                            Type = MSGs.MsgType.SEL;
                            BR.BaseStream.Position += 2;
                            int count = BR.ReadUInt16();
                            BR.BaseStream.Position += 4 * count + 4;

                            int size = BR.ReadInt32();

                            MSG_bytes = BR.ReadBytes(size);
                        }
                        else
                        {
                            Logging.Write("Error: Unknown message type!");
                            return;
                        }

                        MSGs MSG = new MSGs(Index, MSG_Name, Type, Character_Index, MSG_bytes, OldChar, NewChar);
                        MSG.Strings.ParseString(MSG.MsgBytes, OldChar, NewChar);

                        msg.Add(MSG);

                        Index++;
                    }
                }
                catch (Exception e)
                {
                    Logging.Write("Error: Parse MSG1 error!");
                    Logging.Write(e.ToString());
                    name.Clear();
                    msg.Clear();
                }
            }

            //public void SaveAsText(string FileName, string Index, int Option)
            //{
            //    if (Option == 1)
            //    {
            //        SaveAsTextOp1(FileName, Index);
            //    }
            //    else if (Option == 2)
            //    {
            //        SaveAsTextOp2(FileName, Index);
            //    }
            //    else
            //    {
            //        Logging.Write("SaveAsText Option invalid");
            //    }
            //}

            //void SaveAsTextOp1(string FileName, string Index)
            //{
            //    Directory.CreateDirectory("Export Text");

            //    string FileNameWE = Path.GetFileName(FileName);
            //    FileStream FS = new FileStream(@"Export Text\\NAMES.TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //    FS.Position = FS.Length;
            //    using (StreamWriter SW = new StreamWriter(FS))
            //        foreach (var NAME in name)
            //        {
            //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
            //        }


            //    string DirectoryName = new DirectoryInfo(Path.GetDirectoryName(FileName)).Name;
            //    FS = new FileStream("Export Text\\" + DirectoryName.ToUpper() + ".TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //    FS.Position = FS.Length;
            //    using (StreamWriter SW = new StreamWriter(FS))
            //    {
            //        List<name> Name = name.ToList();
            //        foreach (var MSG in msg)
            //        {
            //            foreach (var STR in MSG.Strings)
            //            {
            //                SW.Write(FileNameWE + "\t");
            //                SW.Write(Index + "\t");
            //                SW.Write(MSG.Name + "\t");
            //                SW.Write(STR.Index + "\t");
            //                if (Name.Exists(x => x.Index == MSG.Character_Index))
            //                {
            //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
            //                    SW.Write(Name_i.OldName);
            //                }
            //                else if (MSG.Type == "SEL")
            //                {
            //                    SW.Write("<SELECT>");
            //                }
            //                else { SW.Write("<NO_NAME>"); }

            //                SW.Write("\t");
            //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
            //                SW.Write(split[0]);
            //                for (int i = 1; i < split.Length; i++)
            //                {
            //                    SW.Write(" " + split[i]);
            //                }

            //                SW.WriteLine();
            //            }
            //        }
            //    }
            //}

            //void SaveAsTextOp2(string FileName, string Index)
            //{

            //    string newFileName = Index == "" ? Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".TXT"
            //        : Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "-" + Index + ".TXT";

            //    using (StreamWriter SW = new StreamWriter(new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            //    {
            //        foreach (var NAME in name)
            //        {
            //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
            //        }
            //        SW.WriteLine();

            //        List<name> Name = name.ToList();
            //        foreach (var MSG in msg)
            //        {
            //            foreach (var STR in MSG.Strings)
            //            {
            //                SW.Write(MSG.Name + "\t");
            //                SW.Write(STR.Index + "\t");
            //                if (Name.Exists(x => x.Index == MSG.Character_Index))
            //                {
            //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
            //                    SW.Write(Name_i.OldName);
            //                }
            //                else if (MSG.Type == "SEL")
            //                {
            //                    SW.Write("<SELECT>");
            //                }
            //                else { SW.Write("<NO_NAME>"); }

            //                SW.Write("\t");
            //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
            //                SW.Write(split[0]);
            //                for (int i = 1; i < split.Length; i++)
            //                {
            //                    SW.Write(" " + split[i]);
            //                }

            //                SW.WriteLine();
            //            }
            //        }
            //    }
            //}

        }
    }

    public class Project
    {
        PersonaFileTypes.MSG1 MSG1;

        public string SourceFileName = "";
        public long SelectPosition = -1;

        public Project(PersonaFileTypes.MSG1 MSG1)
        {
            this.MSG1 = MSG1;
        }

        public Project(PersonaFileTypes.MSG1 MSG1, string SourceFileName, long SelectPosition)
        {
            this.MSG1 = MSG1;
            this.SourceFileName = SourceFileName;
            this.SelectPosition = SelectPosition;
        }

        public void SaveProject(string path)
        {
            try
            {
                XDocument xDoc = new XDocument();
                XElement Document = new XElement("MSG1");
                Document.Add(new XAttribute("SourceFileName", SourceFileName));
                Document.Add(new XAttribute("Position", SelectPosition));
                xDoc.Add(Document);
                XElement CharName = new XElement("CharacterNames");
                Document.Add(CharName);

                foreach (var NAME in MSG1.name)
                {
                    XElement Name = new XElement("Name");
                    Name.Add(new XAttribute("Index", NAME.Index));
                    Name.Add(new XElement("OldNameSource", BitConverter.ToString(NAME.OldNameBytes)));
                    Name.Add(new XElement("NewName", NAME.NewName));
                    CharName.Add(Name);
                }

                XElement MES = new XElement("MSG");
                Document.Add(MES);

                foreach (var MSG in MSG1.msg)
                {
                    XElement Msg = new XElement("Message");
                    Msg.Add(new XAttribute("Index", MSG.Index));
                    Msg.Add(new XElement("Type", MSG.Type));
                    Msg.Add(new XElement("Name", MSG.Name));
                    Msg.Add(new XElement("CharacterNameIndex", MSG.CharacterIndex));
                    Msg.Add(new XElement("SourceBytes", BitConverter.ToString(MSG.MsgBytes)));

                    XElement Strings = new XElement("MessageStrings");
                    Msg.Add(Strings);
                    foreach (var STR in MSG.Strings)
                    {
                        XElement String = new XElement("String");
                        String.Add(new XAttribute("Index", STR.Index));
                        Strings.Add(String);

                        foreach (var A in STR.Prefix.ElementArray)
                        {
                            XElement PrefixBytes = new XElement("PrefixBytes", BitConverter.ToString(A.Bytes));
                            PrefixBytes.Add(new XAttribute("Index", Array.IndexOf(STR.Prefix.ElementArray, A)));
                            PrefixBytes.Add(new XAttribute("Type", A.Type));
                            String.Add(PrefixBytes);
                        }

                        foreach (var A in STR.OldString.ElementArray)
                        {
                            XElement OldStringBytes = new XElement("OldStringBytes", BitConverter.ToString(A.Bytes));
                            OldStringBytes.Add(new XAttribute("Index", Array.IndexOf(STR.OldString.ElementArray, A)));
                            OldStringBytes.Add(new XAttribute("Type", A.Type));
                            String.Add(OldStringBytes);
                        }

                        String.Add(new XElement("NewString", STR.NewString.Text));

                        foreach (var A in STR.Postfix.ElementArray)
                        {
                            XElement PostfixBytes = new XElement("PostfixBytes", BitConverter.ToString(A.Bytes));
                            PostfixBytes.Add(new XAttribute("Index", Array.IndexOf(STR.Postfix.ElementArray, A)));
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
                Logging.Write(e.ToString());
            }
        }

        public bool OpenProject(string path)
        {
            MSG1.msg.Clear();
            MSG1.name.Clear();

            try
            {
                XDocument xDoc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
                XElement MSG1Doc = xDoc.Element("MSG1");
                XAttribute temp = MSG1Doc.Attribute("Position");
                SelectPosition = temp != null ? Convert.ToInt32(temp.Value) : -1;

                temp = MSG1Doc.Attribute("SourceFileName");
                SourceFileName = temp != null ? temp.Value : "";

                foreach (var NAME in MSG1Doc.Element("CharacterNames").Elements())
                {
                    int Index = Convert.ToInt32(NAME.Attribute("Index").Value);

                    byte[] OldNameSource = new byte[0];
                    string OldNameSource_str = NAME.Element("OldNameSource").Value;
                    if (OldNameSource_str != "")
                        OldNameSource = Enumerable.Range(0, OldNameSource_str.Split('-').Length).Select(x => Convert.ToByte(OldNameSource_str.Split('-')[x], 16)).ToArray();

                    string NewName = NAME.Element("NewName").Value;

                    MSG1.name.Add(new PersonaFileTypes.MSG1.Names(MSG1.OldChar, MSG1.NewChar, Index, OldNameSource, NewName));
                }

                foreach (var Message in MSG1Doc.Element("MSG").Elements())
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

                    PersonaFileTypes.MSG1.MSGs MSG = new PersonaFileTypes.MSG1.MSGs(Index, Name, (PersonaFileTypes.MSG1.MSGs.MsgType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MSGs.MsgType), Type),
                        CharacterNameIndex, SourceBytes, MSG1.OldChar, MSG1.NewChar);

                    foreach (var Strings in Message.Element("MessageStrings").Elements())
                    {
                        PersonaFileTypes.MSG1.MSGs.MSGstr String = new PersonaFileTypes.MSG1.MSGs.MSGstr(MSG1.OldChar, MSG1.NewChar);

                        String.Index = Convert.ToInt32(Strings.Attribute("Index").Value);

                        List<PersonaFileTypes.MSG1.MyStringElement> TempList = new List<PersonaFileTypes.MSG1.MyStringElement>();
                        foreach (var PrefixByte in Strings.Elements("PrefixBytes"))
                        {
                            int PrefixIndex = Convert.ToInt32(PrefixByte.Attribute("Index").Value);
                            PersonaFileTypes.MSG1.MyStringElement.arrayType PrefixType = (PersonaFileTypes.MSG1.MyStringElement.arrayType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MyStringElement.arrayType), PrefixByte.Attribute("Type").Value);

                            byte[] PrefixBytes = new byte[0];
                            string PrefixBytes_str = PrefixByte.Value;
                            if (PrefixBytes_str != "")
                            {
                                PrefixBytes = Enumerable.Range(0, PrefixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PrefixBytes_str.Split('-')[x], 16)).ToArray();
                            }
                            TempList.Add(new PersonaFileTypes.MSG1.MyStringElement(PrefixIndex, PrefixType, PrefixBytes));
                        }
                        String.Prefix.ElementArray = TempList.ToArray();

                        TempList = new List<PersonaFileTypes.MSG1.MyStringElement>();
                        foreach (var Old in Strings.Elements("OldStringBytes"))
                        {
                            int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
                            PersonaFileTypes.MSG1.MyStringElement.arrayType OldType = (PersonaFileTypes.MSG1.MyStringElement.arrayType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MyStringElement.arrayType), Old.Attribute("Type").Value);

                            byte[] OldBytes = new byte[0];
                            string OldBytes_str = Old.Value;
                            if (OldBytes_str != "")
                            {
                                OldBytes = Enumerable.Range(0, OldBytes_str.Split('-').Length).Select(x => Convert.ToByte(OldBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            TempList.Add(new PersonaFileTypes.MSG1.MyStringElement(OldIndex, OldType, OldBytes));
                        }
                        String.OldString.ElementArray = TempList.ToArray();

                        String.NewString.Text = Strings.Element("NewString").Value;

                        TempList = new List<PersonaFileTypes.MSG1.MyStringElement>();
                        foreach (var PostfixByte in Strings.Elements("PostfixBytes"))
                        {
                            int PostfixIndex = Convert.ToInt32(PostfixByte.Attribute("Index").Value);
                            PersonaFileTypes.MSG1.MyStringElement.arrayType PostfixType = (PersonaFileTypes.MSG1.MyStringElement.arrayType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MyStringElement.arrayType), PostfixByte.Attribute("Type").Value);

                            byte[] PostfixBytes = new byte[0];
                            string PostfixBytes_str = PostfixByte.Value;
                            if (PostfixBytes_str != "")
                            {
                                PostfixBytes = Enumerable.Range(0, PostfixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PostfixBytes_str.Split('-')[x], 16)).ToArray();
                            }

                            TempList.Add(new PersonaFileTypes.MSG1.MyStringElement(PostfixIndex, PostfixType, PostfixBytes));
                        }
                        String.Postfix.ElementArray = TempList.ToArray();

                        MSG.Strings.Add(String);
                    }

                    MSG1.msg.Add(MSG);
                }
            }
            catch (Exception e)
            {
                Logging.Write(e.ToString());
                return false;
            }
            return true;
        }
    }
}