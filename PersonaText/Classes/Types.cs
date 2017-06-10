using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace PersonaText
{
    public class PersonaType
    {
        private static PersonaСontainer Persona3 = new PersonaСontainer(new byte[] { 0xF2, 0x08, 0xFF, 0xFF });
        private static PersonaСontainer Persona4 = new PersonaСontainer(new byte[] { 0xF2, 0x05, 0xFF, 0xFF });

        public PersonaСontainer this[string Game]
        {
            get
            {
                if (Game == "P3")
                {
                    return Persona3;
                }
                else if (Game == "P4")
                {
                    return Persona4;
                }
                else
                {
                    return Persona4;
                }
            }
        }

        public class PersonaСontainer
        {
            public PersonaСontainer(byte[] LineSplit)
            {
                _LineSplit = LineSplit;
            }

            private byte[] _LineSplit;
            public byte[] LineSplit { get { return _LineSplit; } }
        }
    }

    public class BitmapList
    {
        public BitmapSource Bitmap { get; set; }
        public double posX { get; set; }
        public double posY { get; set; }
    }

    public class glyphYshift
    {
        public int Index { get; set; }
        public byte Shift { get; set; }
    }

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

        private void _Image_Changed()
        {
            if (_Cut != null)
            {
                if (_Cut.Right - _Cut.Left != 0)
                {
                    _Image = new CroppedBitmap(_Image, new Int32Rect(_Cut.Left, 0, _Cut.Right - _Cut.Left, 32));
                }
            }

            DrawingVisual DV = new DrawingVisual();
            DrawingContext DI = DV.RenderOpen();

            DI.DrawImage(_Image, new Rect(new System.Windows.Size(32, 32)));
            DI.Close();
        }

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

        private byte[] _Image_data;
        public byte[] Image_data
        {
            get
            {
                return _Image_data;
            }
            set
            {
                if (value != _Image_data)
                {
                    _Image_data = value;
                    //_Image_Changed();
                    Notify("Image_data");
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
                    //_Image_Changed();
                    Notify("Image");
                }
            }
        }

        private MyByte _Cut;
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

        private MyByteArray _Old_Name_Bytes = new MyByteArray() { Index = 0, Type = "Text", Bytes = new byte[0] };
        public byte[] Old_Name_Bytes
        {
            get
            {
                return _Old_Name_Bytes.Bytes;
            }
            set
            {
                if (value != _Old_Name_Bytes.Bytes)
                {
                    _Old_Name_Bytes.Bytes = value;
                    Old_Name = _Old_Name_Bytes.GetText(Static.FontMap.old_char);
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

        private MyByteArray _New_Name_Bytes = new MyByteArray() { Index = 0, Type = "Text", Bytes = new byte[0] };
        public byte[] New_Name_Bytes
        {
            get
            {
                return _New_Name_Bytes.Bytes;
            }
            set
            {
                if (value != _New_Name_Bytes.Bytes)
                {
                    _New_Name_Bytes.Bytes = value;
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
                    New_Name_Bytes = _New_Name.GetEncodeByte(Static.FontMap.new_char);
                    Notify("New_Name");
                }
            }
        }
    }

    public class MyString : INotifyPropertyChanged
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

        public MyString()
        {
            _Prefix_bytes = new ObservableCollection<MyByteArray>();
            _Prefix_bytes.CollectionChanged += _Prefix_bytes_CollectionChanged;
            _Postfix_bytes = new ObservableCollection<PersonaText.MyByteArray>();
            _Postfix_bytes.CollectionChanged += _Postfix_bytes_CollectionChanged;
            _Old_string_bytes = new ObservableCollection<PersonaText.MyByteArray>();
            _Old_string_bytes.CollectionChanged += _Old_string_bytes_CollectionChanged;
        }

        private void _Prefix_bytes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Prefix = "Prefix: " + _Prefix_bytes.GetString(Static.FontMap.old_char, true);
        }

        private void _Postfix_bytes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Postfix = "Postfix: " + _Postfix_bytes.GetString(Static.FontMap.old_char, true);
        }

        private void _Old_string_bytes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            string returned = _Old_string_bytes.GetString(Static.FontMap.old_char, false);

            VisualOldText.GetBitmapList(Old_string_bytes, Static.FontMap.old_char, System.Drawing.Color.White);

            Old_string = returned;
        }

        private void _New_string_Changed()
        {
            New_string_bytes.Clear();
            List<MyByteArray> temp = New_string.GetMyByteArray(Static.FontMap.new_char);

            foreach (var a in temp)
            {
                New_string_bytes.Add(a);
            }

            VisualNewText.GetBitmapList(New_string_bytes, Static.FontMap.new_char, System.Drawing.Color.White);
        }

        private void GetIndex(ref List<byte> ByteList, char Char)
        {
            string CharStr = Char.ToString();
            GetIndex(ref ByteList, CharStr);
        }
        private void GetIndex(ref List<byte> ByteList, string CharStr)
        {
            if (CharStr != "")
            {
                fnmp fnmp = Static.FontMap.new_char.Find(x => x.Char == CharStr);
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

        private ObservableCollection<BitmapList> _VisualOldText = new ObservableCollection<BitmapList>();
        private ObservableCollection<BitmapList> _VisualOldName = new ObservableCollection<BitmapList>();
        public ObservableCollection<BitmapList> VisualOldText
        {
            get { return _VisualOldText; }
            set { _VisualOldText = value; }
        }
        public ObservableCollection<BitmapList> VisualOldName
        {
            get { return _VisualOldName; }
            set { _VisualOldName = value; }
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

        private ObservableCollection<BitmapList> _VisualNewText = new ObservableCollection<BitmapList>();
        private ObservableCollection<BitmapList> _VisualNewName = new ObservableCollection<BitmapList>();
        public ObservableCollection<BitmapList> VisualNewText
        {
            get { return _VisualNewText; }
            set { _VisualNewText = value; }
        }
        public ObservableCollection<BitmapList> VisualNewName
        {
            get { return _VisualNewName; }
            set { _VisualNewName = value; }
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
        public string GetText(List<fnmp> CharList)
        {
            string returned = "";
            if (_Type == "System")
            {
                if (_Bytes[0] == 0x0A)
                {
                    returned += "\n";
                }
                else
                {
                    returned += "{";
                    returned += Convert.ToString(_Bytes[0], 16).PadLeft(2, '0').ToUpper();

                    for (int i = 1; i < _Bytes.Length; i++)
                    {
                        returned += " " + Convert.ToString(_Bytes[i], 16).PadLeft(2, '0').ToUpper();
                    }
                    returned += "}";
                }
            }
            else
            {
                for (int i = 0; i < _Bytes.Length; i++)
                {
                    if (0x20 <= _Bytes[i] & _Bytes[i] < 0x80)
                    {
                        returned += CharList.GetChar(_Bytes[i]);
                    }
                    else if (0x80 <= _Bytes[i] & _Bytes[i] < 0xF0)
                    {
                        int newindex = (_Bytes[i] - 0x81) * 0x80 + _Bytes[i + 1] + 0x20;

                        i++;
                        returned += CharList.GetChar(newindex);
                    }
                    else
                    {
                        Console.WriteLine("ASD");
                    }
                }
            }
            return returned;
        }

        public string GetSystem(List<fnmp> CharList)
        {
            string returned = "";

            returned += "{";
            returned += Convert.ToString(_Bytes[0], 16).PadLeft(2, '0').ToUpper();

            for (int i = 1; i < _Bytes.Length; i++)
            {
                returned += " " + Convert.ToString(_Bytes[i], 16).PadLeft(2, '0').ToUpper();
            }
            returned += "}";

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
