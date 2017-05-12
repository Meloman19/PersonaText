using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace PersonaText
{
    public class MyString
    {
        private string _mystr_old = "";
        private string _mystr_new = "";

        public string mystr_old
        {
            get { return _mystr_old; }
            set { _mystr_old = value; }
        }

        public string mystr_new
        {
            get { return _mystr_new; }
            set { _mystr_new = value; }
        }
    }

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

    public class msg
    {
        private int _Index = 0;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                _Index = value;
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
                _Type = value;
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
                _Name = value;
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
                _SourceBytes = value;
            }
        }

        private ObservableCollection<byte[]> _Bytes = new ObservableCollection<byte[]>();
        public ObservableCollection<byte[]> Bytes
        {
            get
            {
                return _Bytes;
            }
            set
            {
                _Bytes = value;
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
}
