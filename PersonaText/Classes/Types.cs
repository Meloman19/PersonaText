using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace PersonaText
{
    public class MyByte : INotifyPropertyChanged
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

        private byte _Left = 0;
        private byte _Right = 0;

        public byte Left
        {
            get { return _Left; }
            set
            {
                if (value != _Left)
                {
                    _Left = value;
                    Notify("Left");
                }
            }
        }

        public byte Right
        {
            get { return _Right; }
            set
            {
                if (value != _Right)
                {
                    _Right = value;
                    Notify("Right");
                }
            }
        }
    }

    public class fnmp : INotifyPropertyChanged
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

        public fnmp()
        {
            _Index = 0;
            _Char = "";
        }

        private int _Index;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    Notify("Index");
                }
            }
        }

        private string _Char;
        public string Char
        {
            get
            {
                return _Char;
            }
            set
            {
                if (value != _Char)
                {
                    _Char = value;
                    Notify("Char");
                }
            }
        }

        private BitmapSource _Image;
        public BitmapSource Image
        {
            get
            {
                return _Image;
            }
            set
            {
                if (value != _Image)
                {
                    _Image = value;
                    Notify("Image");
                }
            }
        }

        public MyByte _Cut { get; set; }
        public MyByte Cut
        {
            get
            {
                return _Cut;
            }
            set
            {
                if (value != _Cut)
                {
                    _Cut = value;
                    Notify("Cut");
                }
            }
        }
    }

    public class msg : INotifyPropertyChanged
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

        private int _Index = 0;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    Notify("Index");
                }
            }
        }

        private string _Type = "";
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    Notify("Type");
                }
            }
        }

        private string _Name = "";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    Notify("Name");
                }
            }
        }

        private int _Character_Index = 0;
        public int Character_Index
        {
            get
            {
                return _Character_Index;
            }
            set
            {
                if (value != _Character_Index)
                {
                    _Character_Index = value;
                    Notify("Character_Name");
                }
            }
        }

        private byte[] _SourceBytes = new byte[0];
        public byte[] SourceBytes
        {
            get
            {
                return _SourceBytes;
            }
            set
            {
                if (value != _SourceBytes)
                {
                    _SourceBytes = value;
                    Notify("SourceBytes");
                }
            }
        }

        private byte[] _NewBytes = new byte[0];
        public byte[] NewBytes
        {
            get
            {
                return _NewBytes;
            }
            set
            {
                if (value != _NewBytes)
                {
                    _NewBytes = value;
                    Notify("NewBytes");
                }
            }
        }

        private ObservableCollection<MyString> _Strings = new ObservableCollection<MyString>();
        public ObservableCollection<MyString> Strings
        {
            get
            {
                return _Strings;
            }
            set
            {
                _Strings = value;
            }
        }
    }

    public class name : INotifyPropertyChanged
    {

        private byte[] New_Name_Changed()
        {
            List<byte> returned = new List<byte>();

            string Char = "";
            bool Special = false;
            foreach (var C in _New_Name)
            {
                if (C == '\n' | C == '\r')
                {
                    returned.Add(0x20);
                }
                else if (Special)
                {
                    if (C == '>')
                    {
                        Special = false;
                        fnmp fnmp = FontMap.new_char.Find(x => x.Char == Char);
                        if (fnmp != null)
                        {
                            if (fnmp.Index < 0x80)
                            {
                                returned.Add((byte)fnmp.Index);
                            }
                            else
                            {
                                fnmp = null;
                            }
                        }

                        Char = "";
                    }
                    else
                    {
                        Char = Char + C;
                    }
                }
                else
                {
                    if (C == '<')
                    {
                        Special = true;
                    }
                    else
                    {
                        Char = C.ToString();
                        if (Char != "")
                        {
                            fnmp fnmp = FontMap.new_char.Find(x => x.Char == Char);
                            if (fnmp != null)
                            {
                                if (fnmp.Index < 0x80)
                                {
                                    returned.Add((byte)fnmp.Index);
                                }
                            }
                            Char = "";
                        }
                    }
                }


            }
            return returned.ToArray();
        }

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

        private int _Index = 0;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    Notify("Index");
                }
            }
        }

        private byte[] _Old_Name_Bytes = new byte[0];
        public byte[] Old_Name_Bytes
        {
            get
            {
                return _Old_Name_Bytes;
            }
            set
            {
                if (value != _Old_Name_Bytes)
                {
                    _Old_Name_Bytes = value;
                    Notify("Name_Source");
                }
            }
        }

        private string _Old_Name = "";
        public string Old_Name
        {
            get
            {
                return _Old_Name;
            }
            set
            {
                if (value != _Old_Name)
                {
                    _Old_Name = value;
                    Notify("Name");
                }
            }
        }

        private byte[] _New_Name_Bytes = new byte[0];
        public byte[] New_Name_Bytes
        {
            get
            {
                return _New_Name_Bytes;
            }
            set
            {
                if (value != _New_Name_Bytes)
                {
                    _New_Name_Bytes = value;
                    Notify("New_Name_Source");
                }
            }
        }

        private string _New_Name = "";
        public string New_Name
        {
            get
            {
                return _New_Name;
            }
            set
            {
                if (value != _New_Name)
                {
                    _New_Name = value;
                    New_Name_Bytes = New_Name_Changed();
                    Notify("New_Name");
                }
            }
        }
    }

    public class MyString : INotifyPropertyChanged
    {
        public MyString()
        {
            _Prefix_bytes = new ObservableCollection<MyByteArray>();
            _Prefix_bytes.CollectionChanged += _Prefix_bytes_CollectionChanged;
            _Postfix_bytes = new ObservableCollection<PersonaText.MyByteArray>();
            _Postfix_bytes.CollectionChanged += _Postfix_bytes_CollectionChanged;
            _Old_string_bytes = new ObservableCollection<PersonaText.MyByteArray>();
            _Old_string_bytes.CollectionChanged += _Old_string_bytes_CollectionChanged;
        }

        private void _Prefix_bytes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Prefix = "Prefix: " + BytesToHEXString(ref _Prefix_bytes);
        }

        private void _Postfix_bytes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Postfix = "Postfix: " + BytesToHEXString(ref _Postfix_bytes);
        }

        private void _Old_string_bytes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string returned = "";
            foreach (var MSG in _Old_string_bytes)
            {
                if (MSG.Type == "System")
                {
                    if (MSG.Bytes[0] == 0x0A)
                    {
                        returned = returned + "\n";
                    }
                    else
                    {
                        returned = returned + "{";
                        returned = returned + Convert.ToString(MSG.Bytes[1], 16).PadLeft(2, '0').ToUpper();
                        for (int i = 2; i < MSG.Bytes.Length; i++)
                        {
                            returned = returned + " " + Convert.ToString(MSG.Bytes[i], 16).PadLeft(2, '0').ToUpper();
                        }
                        returned = returned + "}";
                    }
                }
                else
                {
                    for (int i = 0; i < MSG.Bytes.Length; i++)
                    {
                        if (0x20 <= MSG.Bytes[i] & MSG.Bytes[i] < 0x80)
                        {
                            if (FontMap.old_char.Exists(x => x.Index == MSG.Bytes[i]))
                            {
                                fnmp fnmp = FontMap.old_char.Find(x => x.Index == MSG.Bytes[i]);

                                if (fnmp.Char.Length == 0)
                                {
                                    returned = returned + "{!}";
                                }
                                else if (fnmp.Char.Length == 1)
                                {
                                    returned = returned + fnmp.Char;
                                }
                                else
                                {
                                    returned = returned + "<" + fnmp.Char + ">";
                                }
                            }
                            else
                            {
                                returned = returned + "{!!}";
                            }
                        }
                        else if (0x80 <= MSG.Bytes[i] & MSG.Bytes[i] < 0xF0)
                        {
                            int link = (MSG.Bytes[i] - 0x81) * 0x80 + MSG.Bytes[i + 1] + 0x20;

                            i++;
                            if (FontMap.old_char.Exists(x => x.Index == link))
                            {
                                fnmp fnmp = FontMap.old_char.Find(x => x.Index == link);

                                if (fnmp.Char.Length == 0)
                                {
                                    returned = returned + "{!}";
                                }
                                else if (fnmp.Char.Length == 1)
                                {
                                    returned = returned + fnmp.Char;
                                }
                                else
                                {
                                    returned = returned + "<" + fnmp.Char + ">";
                                }
                            }
                            else
                            {
                                returned = returned + "{!!}";
                            }
                        }
                        else
                        {
                            Console.WriteLine("ASD");
                        }
                    }
                }
            }

            Old_string = returned;
        }

        private void _New_string_Changed()
        {
            New_string_bytes.Clear();
            List<byte> LB = new List<byte>();
            string Char = "";
            string Type = "Text";
            bool Special = false;
            int Index = 0;
            foreach (var C in _New_string)
            {
                if (C == '\n' | C == '\r')
                {
                    if (LB.Count > 0)
                    {
                        New_string_bytes.Add(new MyByteArray { Index = Index, Type = Type, Bytes = LB.ToArray() });
                        Index++;
                    }
                    Type = "System";
                    LB.Clear();
                    LB.Add(0x0A);
                    New_string_bytes.Add(new MyByteArray { Index = Index, Type = Type, Bytes = LB.ToArray() });
                    Index++;
                    Type = "Text";
                    LB.Clear();
                }
                else if (Type == "System")
                {
                    if (C == '}')
                    {
                        if (LB.Count > 0)
                        {
                            string[] str =  System.Text.Encoding.Default.GetString(LB.ToArray()).Split(' ');
                            LB.Clear();
                            foreach(string st in str)
                            {
                                LB.Add(Convert.ToByte(st, 16));
                            }
                            New_string_bytes.Add(new MyByteArray { Index = Index, Type = Type, Bytes = LB.ToArray() });
                            Index++;
                        }
                        Type = "Text";
                        LB.Clear();
                    }
                    else
                    {
                        LB.Add(Convert.ToByte(C));
                    }
                }
                else if (Special)
                {
                    if (C == '>')
                    {
                        Special = false;
                        fnmp fnmp = FontMap.new_char.Find(x => x.Char == Char);
                        if (fnmp != null)
                        {
                            if (fnmp.Index < 0x80)
                            {
                                LB.Add((byte)fnmp.Index);
                            }
                            else
                            {
                                fnmp = null;
                            }
                        }

                        Char = "";
                    }
                    else
                    {
                        Char = Char + C;
                    }
                }
                else
                {
                    if (C == '<')
                    {
                        Special = true;
                    }
                    else if (C == '{')
                    {
                        if (LB.Count > 0)
                        {
                            New_string_bytes.Add(new MyByteArray { Index = Index, Type = Type, Bytes = LB.ToArray() });
                            Index++;
                            LB.Clear();
                        }
                        Type = "System";
                    }
                    else
                    {
                        Char = C.ToString();
                        if (Char != "")
                        {
                            fnmp fnmp = FontMap.new_char.Find(x => x.Char == Char);
                            if (fnmp != null)
                            {
                                if (fnmp.Index < 0x80)
                                {
                                    LB.Add((byte)fnmp.Index);
                                }
                            }
                            Char = "";
                        }
                    }
                }


            }

            if (LB.Count > 0)
            {
                New_string_bytes.Add(new MyByteArray { Index = Index, Type = Type, Bytes = LB.ToArray() });
            }
        }

        private string BytesToHEXString(ref ObservableCollection<MyByteArray> bytes)
        {
            string returned = "";
            foreach (var MSG in bytes)
            {
                returned = returned + "{";
                returned = returned + Convert.ToString(MSG.Bytes[0], 16).PadLeft(2, '0').ToUpper();
                for (int i = 1; i < MSG.Bytes.Length; i++)
                {
                    returned = returned + " " + Convert.ToString(MSG.Bytes[i], 16).PadLeft(2, '0').ToUpper();
                }
                returned = returned + "}";
            }
            return returned;
        }

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

        private int _Index = 0;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    Notify("Index");
                }
            }
        }

        private ObservableCollection<MyByteArray> _Prefix_bytes;
        public ObservableCollection<MyByteArray> Prefix_bytes
        {
            get { return _Prefix_bytes; }
            set
            {
                _Prefix_bytes = value;
                _Prefix_bytes_CollectionChanged(_Prefix_bytes, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        private string _Prefix = "";
        public string Prefix
        {
            get { return _Prefix; }
            set
            {
                if (value != _Prefix)
                {
                    _Prefix = value;
                    Notify("Prefix");
                }
            }
        }

        private ObservableCollection<MyByteArray> _Old_string_bytes;
        public ObservableCollection<MyByteArray> Old_string_bytes
        {
            get { return _Old_string_bytes; }
            set
            {
                _Old_string_bytes = value;
                _Old_string_bytes_CollectionChanged(_Old_string_bytes, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        private string _Old_string = "";
        public string Old_string
        {
            get { return _Old_string; }
            set
            {
                if (value != _Old_string)
                {
                    _Old_string = value;
                    Notify("Old_string");
                }
            }
        }

        private ObservableCollection<MyByteArray> _New_string_bytes = new ObservableCollection<MyByteArray>();
        public ObservableCollection<MyByteArray> New_string_bytes
        {
            get { return _New_string_bytes; }
            set { _New_string_bytes = value; }
        }
        private string _New_string = "";
        public string New_string
        {
            get { return _New_string; }
            set
            {
                if (value != _New_string)
                {
                    _New_string = value;
                    _New_string_Changed();
                    Notify("New_string");
                }
            }
        }

        private ObservableCollection<MyByteArray> _Postfix_bytes = new ObservableCollection<MyByteArray>();
        public ObservableCollection<MyByteArray> Postfix_bytes
        {
            get { return _Postfix_bytes; }
            set
            {
                _Postfix_bytes = value;
                _Postfix_bytes_CollectionChanged(_Postfix_bytes, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        private string _Postfix = "";
        public string Postfix
        {
            get { return _Postfix; }
            set
            {
                if (value != _Postfix)
                {
                    _Postfix = value;
                    Notify("Postfix");
                }
            }
        }
    }

    public class MyByteArray : INotifyPropertyChanged
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

        private int _Index = 0;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    Notify("Index");
                }
            }
        }

        private string _Type = "";
        public string Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    Notify("Type");
                }
            }
        }

        private byte[] _Bytes = new byte[0];
        public byte[] Bytes
        {
            get { return _Bytes; }
            set
            {
                if (value != _Bytes)
                {
                    _Bytes = value;
                    Notify("Bytes");
                }
            }
        }
    }
}
