using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PersonaText
{
    public class MyByte
    {
        private byte _Left = 0;
        private byte _Right = 0;

        public byte Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        public byte Right
        {
            get { return _Right; }
            set { _Right = value; }
        }
    }

    public class fnmp
    {
        public int Index { get; set; }

        public string Char { get; set; }

        public BitmapSource Image { get; set; }

        public MyByte Cut { get; set; }
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

    public class name: INotifyPropertyChanged
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

        private byte[] _Old_Name_Source = new byte[0];
        public byte[] Old_Name_Source
        {
            get
            {
                return _Old_Name_Source;
            }
            set
            {
                if (value != _Old_Name_Source)
                {
                    _Old_Name_Source = value;
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

        private ObservableCollection<int[]> _Old_string_bytes = new ObservableCollection<int[]>();
        private ObservableCollection<int[]> _New_string_bytes = new ObservableCollection<int[]>();
        private string _Old_string = "";
        private string _New_string = "";

        private ObservableCollection<int[]> _Prefix_bytes = new ObservableCollection<int[]>();
        private ObservableCollection<int[]> _Postfix_bytes = new ObservableCollection<int[]>();
        private string _Prefix = "";
        private string _Postfix = "";

        public ObservableCollection<int[]> Old_string_bytes
        {
            get { return _Old_string_bytes; }
            set { _Old_string_bytes = value; }
        }

        public ObservableCollection<int[]> New_string_bytes
        {
            get { return _New_string_bytes; }
            set { _New_string_bytes = value; }
        }

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

        public string New_string
        {
            get { return _New_string; }
            set
            {
                if (value != _New_string)
                {
                    _New_string = value;
                    Notify("New_string");
                }
            }
        }

        public ObservableCollection<int[]> Prefix_bytes
        {
            get { return _Prefix_bytes; }
            set { _Prefix_bytes = value; }
        }

        public ObservableCollection<int[]> Postfix_bytes
        {
            get { return _Postfix_bytes; }
            set { _Postfix_bytes = value; }
        }

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
}
