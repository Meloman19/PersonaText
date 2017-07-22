using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Linq;
using System.Globalization;
using PersonaTextLib;
using PersonaTextLib.Extensions;
using PersonaTextLib.Types;
using static PersonaTextLib.PersonaFileTypes.MSG1;

namespace PersonaText
{
    public delegate void ColorChangedEventHandler(Color color);
    public delegate void ValueChangedEventHandler();
    public delegate void WidthChangedEventHandler(double width);
    public delegate void PixelWidthEventHandler(int pixelWidth);
    public delegate void GlyphScaleEventHandler(double glyphScale);
    public delegate void NameStringChangedEventHandler(string Name);
    delegate void OldArrayChangedEventHandler(List<PersonaFileTypes.MSG1.MyStringElement> Name);

    delegate void DoubleChangedEvent(double number);
    delegate void IntegerChangedEvent(int number);
    delegate void ImageChangeEvent(BitmapSource image);

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

    class BackgroundImage : INotifyPropertyChanged
    {
        public event DoubleChangedEvent GlyphScaleChanged;
        public event DoubleChangedEvent RealWidthChanged;
        public event ImageChangeEvent ImageChanged;
        public event ValueChangedEventHandler VisualTextChanged;
        public event ColorChangedEventHandler TextColorChanged;
        public event ColorChangedEventHandler NameColorChanged;

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
        double _RealWidth = 0;
        BitmapSource _Image;
        Color _ColorText;
        Color _ColorName;

        public double RealWidth
        {
            get { return _RealWidth; }
            set
            {
                if (value != _RealWidth)
                {
                    _RealWidth = value;
                    RealWidthChanged?.Invoke(RealWidth);
                    VisualTextChanged?.Invoke();
                }
            }
        }
        public GridLength TextStartX
        {
            get { return _textStartX; }
            private set
            {
                if (value != _textStartX)
                {
                    _textStartX = value;
                    Notify("TextStartX");
                }
            }
        }
        public GridLength TextStartY
        {
            get { return _textStartY; }
            private set
            {
                if (value != _textStartY)
                {
                    _textStartY = value;
                    Notify("TextStartY");
                }
            }
        }
        public GridLength NameStartX
        {
            get { return _nameStartX; }
            private set
            {
                if (value != _nameStartX)
                {
                    _nameStartX = value;
                    Notify("NameStartX");
                }
            }
        }
        public GridLength NameStartY
        {
            get { return _nameStartY; }
            private set
            {
                if (value != _nameStartY)
                {
                    _nameStartY = value;
                    Notify("NameStartY");
                }
            }
        }
        public double GlyphScale
        {
            get { return _glyphScale; }
            private set
            {
                if (value != _glyphScale)
                {
                    _glyphScale = value;
                    GlyphScaleChanged?.Invoke(_glyphScale);
                    VisualTextChanged?.Invoke();
                    Notify("GlyphScale");
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
                    ImageChanged?.Invoke(_Image);
                    VisualTextChanged?.Invoke();
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
                TextColorChanged?.Invoke(_ColorText);
                Notify("ColorText");
            }
        }
        public Color ColorName
        {
            get { return _ColorName; }
            set
            {
                _ColorName = value;
                NameColorChanged?.Invoke(_ColorName);
                Notify("ColorName");
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
            TextStartX = new GridLength(textX / (1 - textX), GridUnitType.Star);
            TextStartY = new GridLength(textY / (1 - textY), GridUnitType.Star);
            NameStartX = new GridLength(nameX / (1 - nameX), GridUnitType.Star);
            NameStartY = new GridLength(nameY / (1 - nameY), GridUnitType.Star);
            GlyphScale = Static.Setting.Empty.GlyphScale;
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

                GlyphScale = Convert.ToDouble(Background.Element("glyphScale").Value, culture);
                int textX = Convert.ToInt32(Background.Element("textStartX").Value, culture);
                int textY = Convert.ToInt32(Background.Element("textStartY").Value, culture);
                int nameX = Convert.ToInt32(Background.Element("nameStartX").Value, culture);
                int nameY = Convert.ToInt32(Background.Element("nameStartY").Value, culture);


                double textXd = (double)textX / Image.PixelWidth < 1 ? (double)textX / Image.PixelWidth : 0.9999;
                double textYd = (double)textY / Image.PixelHeight < 1 ? (double)textY / Image.PixelHeight : 0.9999;
                double nameXd = (double)nameX / Image.PixelWidth < 1 ? (double)nameX / Image.PixelWidth : 0.9999;
                double nameYd = (double)nameY / Image.PixelHeight < 1 ? (double)nameY / Image.PixelHeight : 0.9999;

                TextStartX = new GridLength(textXd / (1 - textXd), GridUnitType.Star);
                TextStartY = new GridLength(textYd / (1 - textYd), GridUnitType.Star);
                NameStartX = new GridLength(nameXd / (1 - nameXd), GridUnitType.Star);
                NameStartY = new GridLength(nameYd / (1 - nameYd), GridUnitType.Star);

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