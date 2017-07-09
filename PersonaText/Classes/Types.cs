using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Linq;
using System.Globalization;

namespace PersonaText
{
    public delegate void ColorChangedEventHandler(Color color);
    public delegate void ValueChangedEventHandler();
    public delegate void WidthChangedEventHandler(double width);
    public delegate void PixelWidthEventHandler(int pixelWidth);
    public delegate void GlyphScaleEventHandler(double glyphScale);
    public delegate void NameStringChangedEventHandler(string Name);
    delegate void OldArrayChangedEventHandler(List<MyByteArray> Name);

    enum arrayType
    {
        Empty,
        System,
        Text
    }

    class BitmapList
    {
        public BitmapSource Bitmap { get; set; }
        public double posX { get; set; }
        public double posY { get; set; }
    }

    class glyphYshift
    {
        public int Index { get; set; } = 0;
        public byte Shift { get; set; } = 0;
    }

    class MyByte
    {
        public byte Left { get; set; } = 0;
        public byte Right { get; set; } = 0;
    }

    class CharList
    {
        public event ValueChangedEventHandler CharListChanged;

        public void Update()
        {
            CharListChanged?.Invoke();
        }

        public List<FnMpData> List { get; set; } = new List<FnMpData>();
        public BitmapPalette Palette { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelFormat PixelFormat { get; set; }
    }

    class FnMpData
    {
        public event ValueChangedEventHandler ValueChange;

        string _Char = "";


        public int Index { get; set; } = 0;
        public string Char
        {
            get { return _Char; }
            set
            {
                _Char = value;
                ValueChange?.Invoke();
            }
        }
        public byte[] Image_data { get; set; } = new byte[0];
        public MyByte Cut { get; set; }
    }

    class FnMpImg
    {
        public int Index { get; set; } = 0;
        public string Char { get; set; } = "";
        public BitmapSource Image { get; set; }
    }

    class name : INotifyPropertyChanged
    {
        public event OldArrayChangedEventHandler OldNameChanged;
        public event NameStringChangedEventHandler NewNameChanged;

        public name()
        {
            Static.FontMap.new_char.CharListChanged += New_char_CharListChanged;
            Static.FontMap.old_char.CharListChanged += Old_char_CharListChanged;
        }

        private void Old_char_CharListChanged()
        {
            OldName = _OldNameBytes.GetString(Static.FontMap.old_char.List, false);
        }

        private void New_char_CharListChanged()
        {
            _NewNameBytes = _NewName.GetMyByteArray(Static.FontMap.new_char.List);
            NewNameChanged?.Invoke(_NewName);
            Notify("NewName");
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
        public List<MyByteArray> _OldNameBytes = new List<MyByteArray>();
        List<MyByteArray> _NewNameBytes = new List<MyByteArray>();
        string _OldName = "";
        string _NewName = "";

        public int Index { get; set; } = 0;
        public byte[] OldNameBytes
        {
            get { return _OldNameBytes.getAllBytes(); }
            set
            {
                _OldNameBytes.parseString(value);
                OldNameChanged?.Invoke(_OldNameBytes);
                OldName = _OldNameBytes.GetString(Static.FontMap.old_char.List, false);
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
                    Notify("OldName");
                }
            }
        }
        public byte[] NewNameBytes
        {
            get { return _NewNameBytes.getAllBytes(); }
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
                    _NewNameBytes = _NewName.GetMyByteArray(Static.FontMap.new_char.List);
                    NewNameChanged?.Invoke(value);
                    Notify("NewName");
                }
            }
        }
    }

    class msg
    {
        public void Name_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CreateEventName")
            {
                ((name)sender).OldNameChanged += Msg_OldNameChanged;
                Msg_OldNameChanged(((name)sender)._OldNameBytes);
                ((name)sender).NewNameChanged += Msg_NewNameChanged;
                Msg_NewNameChanged(((name)sender).NewName);
            }
        }

        private void Msg_NewNameChanged(string Name)
        {
            foreach (var a in Strings)
            {
                a.NewNameString = Name;
            }
        }

        private void Msg_OldNameChanged(List<MyByteArray> Name)
        {
            foreach (var a in Strings)
            {
                a.OldNameString = Name;
                FullName = this.Name + " : " + Name.GetString(Static.FontMap.old_char.List, false);
            }
        }

        public string FullName { get; set; }
        public int Index { get; set; } = 0;
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public int Character_Index { get; set; } = 0;
        public byte[] SourceBytes { get; set; } = new byte[0];
        public byte[] NewBytes { get; set; } = new byte[0];
        public BindingList<MyString> Strings { get; set; } = new BindingList<MyString>();
    }

    class MyString : INotifyPropertyChanged
    {
        public List<MyByteArray> OldNameString { set { NameOld.Text = value?.DrawText(Static.FontMap.old_char, Static.BackImage.ColorName); } }

        string _NewNameString = "";
        public string NewNameString
        {
            get { return _NewNameString; }
            set
            {
                _NewNameString = value;
                NameNew.Text = _NewNameString?.GetMyByteArray(Static.FontMap.new_char.List).DrawText(Static.FontMap.new_char, Static.BackImage.ColorName);
            }
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

        public int Index { get; set; } = 0;

        private BindingList<MyByteArray> _Prefix_bytes;
        private BindingList<MyByteArray> _Old_string_bytes;
        private BindingList<MyByteArray> _New_string_bytes;
        private string _New_string = "";
        private BindingList<MyByteArray> _Postfix_bytes;

        public BindingList<MyByteArray> Prefix_bytes
        {
            get { return _Prefix_bytes; }
            set
            {
                _Prefix_bytes = value;
                _Prefix_bytes_ListChanged(_Prefix_bytes, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }
        public string Prefix { get; private set; } = "";
        public BindingList<MyByteArray> Old_string_bytes
        {
            get { return _Old_string_bytes; }
            set
            {
                if (value != _Old_string_bytes)
                {
                    _Old_string_bytes = value;
                    _Old_string_bytes_ListChanged(_Old_string_bytes, new ListChangedEventArgs(ListChangedType.Reset, -1));
                }
            }
        }
        public string Old_string { get; private set; } = "";
        public BindingList<MyByteArray> New_string_bytes
        {
            get { return _New_string_bytes; }
            set { _New_string_bytes = value; }
        }
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
        public BindingList<MyByteArray> Postfix_bytes
        {
            get { return _Postfix_bytes; }
            set
            {
                _Postfix_bytes = value;
                _Postfix_bytes_ListChanged(_Postfix_bytes, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }
        public string Postfix { get; private set; } = "";

        #region Visual

        public Visual TextOld { get; set; } = new Visual();
        public Visual TextNew { get; set; } = new Visual();
        public Visual NameOld { get; set; } = new Visual();
        public Visual NameNew { get; set; } = new Visual();

        #endregion Visual

        #region Events

        private void BackImage_GlyphScaleChanged(double glyphScale)
        {
            TextOld.Back_GlyphScaleChanged(glyphScale);
            TextNew.Back_GlyphScaleChanged(glyphScale);
            NameOld.Back_GlyphScaleChanged(glyphScale);
            NameNew.Back_GlyphScaleChanged(glyphScale);
        }

        private void BackImage_PixelWidthChanged(int pixelWidth)
        {
            TextOld.Back_PixelWidthChanged(pixelWidth);
            TextNew.Back_PixelWidthChanged(pixelWidth);
            NameOld.Back_PixelWidthChanged(pixelWidth);
            NameNew.Back_PixelWidthChanged(pixelWidth);
        }

        private void Width_ValueChanged(double width)
        {
            TextOld.Width_ValueChanged(width);
            TextNew.Width_ValueChanged(width);
            NameOld.Width_ValueChanged(width);
            NameNew.Width_ValueChanged(width);
        }

        private void Name_ColorChanged(Color color)
        {
            NameOld.Text = NameOld.Text?.ChangePallete(color);
            NameNew.Text = NameNew.Text?.ChangePallete(color);
        }

        private void Text_ColorChanged(Color color)
        {
            TextOld.Text = TextOld.Text?.ChangePallete(color);
            TextNew.Text = TextNew.Text?.ChangePallete(color);
        }

        #endregion Events

        void SetEvents()
        {
            Static.Width.ValueChanged += Width_ValueChanged;
            Width_ValueChanged(Static.Width.Value);

            Static.BackImage.ColorTextChanged += Text_ColorChanged;
            Text_ColorChanged(Static.BackImage.ColorText);
            Static.BackImage.ColorNameChanged += Name_ColorChanged;
            Name_ColorChanged(Static.BackImage.ColorName);
            Static.BackImage.PixelWidthChanged += BackImage_PixelWidthChanged;
            BackImage_PixelWidthChanged(Static.BackImage.Image.PixelWidth);
            Static.BackImage.GlyphScaleChanged += BackImage_GlyphScaleChanged;
            BackImage_GlyphScaleChanged(Static.BackImage.glyphScale);

            Static.FontMap.old_char.CharListChanged += Old_char_CharListChanged;
            Static.FontMap.new_char.CharListChanged += New_char_CharListChanged;
        }

        private void New_char_CharListChanged()
        {
            _New_string_Changed();
        }

        private void Old_char_CharListChanged()
        {
            _Old_string_bytes_ListChanged(_Old_string_bytes, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public MyString()
        {
            _Prefix_bytes = new BindingList<MyByteArray>();
            _Prefix_bytes.ListChanged += _Prefix_bytes_ListChanged;
            _Postfix_bytes = new BindingList<MyByteArray>();
            _Postfix_bytes.ListChanged += _Postfix_bytes_ListChanged;
            _Old_string_bytes = new BindingList<MyByteArray>();
            _Old_string_bytes.ListChanged += _Old_string_bytes_ListChanged;
            _New_string_bytes = new BindingList<MyByteArray>();
            _New_string_Changed();

            SetEvents();
        }

        private void _Prefix_bytes_ListChanged(object sender, ListChangedEventArgs e)
        {
            Prefix = "Prefix: " + _Prefix_bytes.GetString(Static.FontMap.old_char.List, true);
        }

        private void _Postfix_bytes_ListChanged(object sender, ListChangedEventArgs e)
        {
            Postfix = "Postfix: " + Postfix_bytes.GetString(Static.FontMap.old_char.List, true);
        }

        private void _Old_string_bytes_ListChanged(object sender, ListChangedEventArgs e)
        {
            Old_string = Old_string_bytes.GetString(Static.FontMap.old_char.List, false);
            if (Static.Setting.ViewVisualizer)
            {
                TextOld.Text = Old_string_bytes.DrawText(Static.FontMap.old_char, Static.BackImage.ColorText);
            }
        }

        private void _New_string_Changed()
        {
            New_string_bytes.GetMyByteArray(New_string, Static.FontMap.new_char.List);
            if (Static.Setting.ViewVisualizer)
            {
                TextNew.Text = New_string_bytes.DrawText(Static.FontMap.new_char, Static.BackImage.ColorText);
            }
        }
    }

    class Visual : INotifyPropertyChanged
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

        double readlWidth = 0;
        int PixelWidth = 100;
        double GlyphScale = 1;

        private double _Width = 1;
        private double _Height = 1;
        private BitmapSource _VisualText;

        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    Notify("Width");
                }
            }
        }
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    Notify("Height");
                }
            }
        }
        public BitmapSource Text
        {
            get { return _VisualText; }
            set
            {
                if (value != _VisualText)
                {
                    _VisualText = value;
                    Update();
                    Notify("Text");
                }
            }
        }

        public void Back_GlyphScaleChanged(double glyphScale)
        {
            GlyphScale = glyphScale;
            Update();
        }

        public void Back_PixelWidthChanged(int pixelWidth)
        {
            PixelWidth = pixelWidth;
            Update();
        }

        public void Width_ValueChanged(double width)
        {
            readlWidth = width;
            Update();
        }

        public void Update()
        {
            if (Text != null)
            {
                double OnePixelHeight = ((readlWidth / PixelWidth) * GlyphScale);
                double OnePixelWidth = OnePixelHeight * 0.9375;
                Height = Text.PixelHeight * OnePixelHeight;
                Width = Text.PixelWidth * OnePixelWidth;
            }
        }
    }

    class ImageData
    {
        public byte[] Data { get; private set; }
        public PixelFormat PixelFormat { get; private set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public int Stride { get; private set; }

        public ImageData(byte[] data, PixelFormat pixelformat, int pixelwidth, int pixelheight)
        {
            Data = data;
            PixelFormat = pixelformat;
            PixelWidth = pixelwidth;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
        }

        public ImageData(PixelFormat pixelformat, int pixelheight)
        {
            PixelFormat = pixelformat;
            PixelWidth = 1;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
            Data = new byte[Stride * pixelheight];
        }

        public static ImageData MergeLeftRight(ImageData left, ImageData right)
        {
            if (left == null)
            {
                return right;
            }
            else if (right == null)
            {
                return left;
            }

            byte[][] buffer = GetMergePixelsLR(GetPixels(left), GetPixels(right));
            byte[] data = GetNewData(buffer, left.PixelFormat);
            return new ImageData(data, left.PixelFormat, buffer[0].Length, buffer.Length);
        }

        public static ImageData MergeUpDown(ImageData up, ImageData down)
        {
            if (up == null)
            {
                return down;
            }
            else if (down == null)
            {
                return up;
            }
            else if (up == null & down == null)
            {
                return null;
            }

            byte[][] buffer = GetMergePixelsUD(GetPixels(up), GetPixels(down));
            byte[] data = GetNewData(buffer, up.PixelFormat);
            return new ImageData(data, up.PixelFormat, buffer[0].Length, buffer.Length);
        }

        public static ImageData Crop(ImageData image, int leftpixel, int rightpixel)
        {
            if (rightpixel - leftpixel < 0)
            {
                return image;
            }
            byte[][] buffer = GetPixels(image);
            if (buffer == null)
                return image;

            buffer = GetCropPixels(buffer, leftpixel, rightpixel);
            byte[] data = GetNewData(buffer, image.PixelFormat);
            return new ImageData(data, image.PixelFormat, rightpixel - leftpixel, image.PixelHeight);
        }

        public static ImageData Shift(ImageData image, int shift)
        {
            byte[][] buffer = GetPixels(image);
            buffer = MovePixels(buffer, shift);
            byte[] data = GetNewData(buffer, image.PixelFormat);
            return new ImageData(data, image.PixelFormat, buffer[0].Length, buffer.Length);
        }

        static byte[] GetNewData(byte[][] buffer, PixelFormat pixelformat)
        {
            byte[] returned = new byte[buffer.Length * buffer[0].Length];
            if (pixelformat.BitsPerPixel == 4)
            {
                int index = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    for (int k = 0; k < buffer[i].Length; k++)
                    {
                        if (k + 1 < buffer[i].Length)
                        {
                            returned[index] = Convert.ToByte((buffer[i][k] << 4) + buffer[i][k + 1]);
                            index++;
                        }
                        else
                        {
                            returned[index] = Convert.ToByte(buffer[i][k] << 4);
                            index++;
                        }
                        k++;
                    }
                }
            }
            else
            {
                return null;
            }
            return returned;
        }

        static byte[][] MovePixels(byte[][] buffer, int y)
        {
            byte[][] returned = new byte[buffer.Length][];
            for (int i = 0; i < returned.Length; i++)
                returned[i] = new byte[buffer[i].Length];

            for (int i = y; i < returned.Length; i++)
            {
                for (int k = 0; k < returned[i].Length; k++)
                {
                    returned[i][k] = buffer[i - y][k];
                }
            }

            return returned;
        }

        static byte[][] GetMergePixelsUD(byte[][] up, byte[][] down)
        {
            byte[][] returned = new byte[up.Length + down.Length - 7][];
            int max = Math.Max(up[0].Length, down[0].Length);
            for (int i = 0; i < returned.Length; i++)
                returned[i] = new byte[max];

            for (int i = 0; i < up.Length; i++)
            {
                for (int k = 0; k < up[i].Length; k++)
                {
                    returned[i][k] = up[i][k];
                }
            }

            for (int i = 0; i < down.Length; i++)
            {
                for (int k = 0; k < down[i].Length; k++)
                {
                    if (returned[i + up.Length - 7][k] == 0)
                    {
                        returned[i + up.Length - 7][k] = down[i][k];
                    }
                }
            }

            return returned;
        }

        static byte[][] GetMergePixelsLR(byte[][] left, byte[][] right)
        {
            if (left.Length != right.Length)
            {
                MessageBox.Show("Image doesn't merge");
                return left;
            }

            byte[][] returned = new byte[left.Length][];
            for (int i = 0; i < returned.Length; i++)
            {
                returned[i] = new byte[left[0].Length + right[0].Length];
                int index = 0;
                for (int k = 0; k < left[i].Length; k++)
                {
                    returned[i][index] = left[i][k];
                    index++;
                }
                for (int k = 0; k < right[i].Length; k++)
                {
                    returned[i][index] = right[i][k];
                    index++;
                }
            }
            return returned;
        }

        static byte[][] GetCropPixels(byte[][] buffer, int left, int right)
        {
            byte[][] returned = new byte[buffer.Length][];
            for (int i = 0; i < buffer.Length; i++)
            {
                returned[i] = new byte[right - left];
                for (int k = 0; k < right - left; k++)
                {
                    returned[i][k] = buffer[i][k + left];
                }
            }
            return returned;
        }

        static byte[][] GetPixels(ImageData image)
        {
            if (image.PixelFormat.BitsPerPixel == 4)
            {
                byte[][] returned = new byte[image.PixelHeight][];
                int index = 0;
                for (int i = 0; i < image.PixelHeight; i++)
                {
                    returned[i] = new byte[image.PixelWidth];
                    for (int k = 0; k < image.PixelWidth; k += 2)
                    {
                        returned[i][k] = Convert.ToByte(image.Data[index] >> 4);
                        if (k + 1 < image.PixelWidth)
                            returned[i][k + 1] = (Convert.ToByte(image.Data[index] - (image.Data[index] >> 4 << 4)));
                        index++;
                    }
                }
                return returned;
            }
            else
            {
                MessageBox.Show("Unknown PixelFormat!");
                return null;
            }
        }
    }

    class MyByteArray
    {
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

            returned += "{";
            returned += Convert.ToString(Bytes[0], 16).PadLeft(2, '0').ToUpper();

            for (int i = 1; i < Bytes.Length; i++)
            {
                returned += " " + Convert.ToString(Bytes[i], 16).PadLeft(2, '0').ToUpper();
            }
            returned += "}";

            return returned;
        }

        public int Index { get; set; } = 0;
        public arrayType Type { get; set; } = arrayType.Empty;
        public byte[] Bytes { get; set; } = new byte[0];
    }

    public class BackgroundImage : INotifyPropertyChanged
    {
        public event ValueChangedEventHandler BackgroundImageChanged;
        public event PixelWidthEventHandler PixelWidthChanged;
        public event GlyphScaleEventHandler GlyphScaleChanged;
        public event ColorChangedEventHandler ColorTextChanged;
        public event ColorChangedEventHandler ColorNameChanged;

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
        GridLength _textStartX = new GridLength(0, GridUnitType.Star);
        GridLength _textStartY = new GridLength(0, GridUnitType.Star);
        GridLength _nameStartX = new GridLength(0, GridUnitType.Star);
        GridLength _nameStartY = new GridLength(0, GridUnitType.Star);
        double _glyphScale = 1;
        BitmapSource _Image;
        Color _ColorText;
        Color _ColorName;

        public GridLength textStartX
        {
            get { return _textStartX; }
            private set
            {
                if (value != _textStartX)
                {
                    _textStartX = value;
                    Notify("textStartX");
                }
            }
        }
        public GridLength textStartY
        {
            get { return _textStartY; }
            private set
            {
                if (value != _textStartY)
                {
                    _textStartY = value;
                    Notify("textStartY");
                }
            }
        }
        public GridLength nameStartX
        {
            get { return _nameStartX; }
            private set
            {
                if (value != _nameStartX)
                {
                    _nameStartX = value;
                    Notify("nameStartX");
                }
            }
        }
        public GridLength nameStartY
        {
            get { return _nameStartY; }
            private set
            {
                if (value != _nameStartY)
                {
                    _nameStartY = value;
                    Notify("nameStartY");
                }
            }
        }
        public double glyphScale
        {
            get { return _glyphScale; }
            private set
            {
                if (value != _glyphScale)
                {
                    _glyphScale = value;
                    GlyphScaleChanged?.Invoke(value);
                    Notify("glyphScale");
                }
            }
        }
        public BitmapSource Image
        {
            get { return _Image; }
            private set
            {
                if (value != _Image)
                {
                    _Image = value;
                    PixelWidthChanged?.Invoke(_Image.PixelWidth);
                    Notify("Image");
                }
            }
        }
        public Color ColorText
        {
            get { return _ColorText; }
            set
            {
                _ColorText = value;
                ColorTextChanged?.Invoke(value);
            }
        }
        public Color ColorName
        {
            get { return _ColorName; }
            set
            {
                _ColorName = value;
                ColorNameChanged?.Invoke(value);
            }
        }

        public BackgroundImage()
        {
            SetEmpty();
        }

        public void Update(string FileName)
        {
            if (Equals(FileName, "Empty"))
                SetEmpty();
            else
            {
                Image = new BitmapImage(new Uri(FileName));
                string xml = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".xml";
                ParseDescription(xml);
            }

            BackgroundImageChanged?.Invoke();
        }

        void SetEmpty()
        {
            int Width = Static.Setting.Empty.Width;
            int Height = Static.Setting.Empty.Height;
            double textX = Static.Setting.Empty.Text.X;
            double textY = Static.Setting.Empty.Text.Y;
            double nameX = Static.Setting.Empty.Name.X;
            double nameY = Static.Setting.Empty.Name.Y;
            textX = textX / Width < 1 ? textX / Width : 0.9999;
            textY = textY / Height < 1 ? textY / Height : 0.9999;
            nameX = nameX / Width < 1 ? nameX / Width : 0.9999;
            nameY = nameY / Height < 1 ? nameY / Height : 0.9999;
            textStartX = new GridLength(textX / (1 - textX), GridUnitType.Star);
            textStartY = new GridLength(textY / (1 - textY), GridUnitType.Star);
            nameStartX = new GridLength(nameX / (1 - nameX), GridUnitType.Star);
            nameStartY = new GridLength(nameY / (1 - nameY), GridUnitType.Star);
            glyphScale = Static.Setting.Empty.GlyphScale;
            ColorName = Static.Setting.Empty.Name.Color;
            ColorText = Static.Setting.Empty.Text.Color;

            Image = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Static.Setting.Empty.BackgroundColor }), new byte[Width * Height], Width);
        }

        void ParseDescription(string FileName)
        {
            try
            {
                var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
                culture.NumberFormat.NumberDecimalSeparator = ".";
                XDocument XDoc = XDocument.Load(FileName);
                XElement Background = XDoc.Element("Background");

                glyphScale = Convert.ToDouble(Background.Element("glyphScale").Value, culture);
                int textX = Convert.ToInt32(Background.Element("textStartX").Value, culture);
                int textY = Convert.ToInt32(Background.Element("textStartY").Value, culture);
                int nameX = Convert.ToInt32(Background.Element("nameStartX").Value, culture);
                int nameY = Convert.ToInt32(Background.Element("nameStartY").Value, culture);


                double textXd = (double)textX / Image.PixelWidth < 1 ? (double)textX / Image.PixelWidth : 0.9999;
                double textYd = (double)textY / Image.PixelHeight < 1 ? (double)textY / Image.PixelHeight : 0.9999;
                double nameXd = (double)nameX / Image.PixelWidth < 1 ? (double)nameX / Image.PixelWidth : 0.9999;
                double nameYd = (double)nameY / Image.PixelHeight < 1 ? (double)nameY / Image.PixelHeight : 0.9999;

                textStartX = new GridLength(textXd / (1 - textXd), GridUnitType.Star);
                textStartY = new GridLength(textYd / (1 - textYd), GridUnitType.Star);
                nameStartX = new GridLength(nameXd / (1 - nameXd), GridUnitType.Star);
                nameStartY = new GridLength(nameYd / (1 - nameYd), GridUnitType.Star);

                ColorName = (Color)ColorConverter.ConvertFromString(Background.Element("ColorName").Value);
                ColorText = (Color)ColorConverter.ConvertFromString(Background.Element("ColorText").Value);
            }
            catch (FormatException)
            {
                MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that the numeric values(except for GlyphScale) are Integer.");
                SetEmpty();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Background load error:\nThere is no description file.");
                SetEmpty();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that all the required values are present.");
                SetEmpty();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetType().ToString());
                MessageBox.Show(e.ToString());
                SetEmpty();
            }
        }
    }
}