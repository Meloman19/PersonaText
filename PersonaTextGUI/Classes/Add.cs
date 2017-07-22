using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Text.RegularExpressions;
using PersonaTextLib.Extensions;
using PersonaTextLib.Types;
using PersonaTextLib;
using System.Windows.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
            PersonaFileTypes.PMD1 PM1 = new PersonaFileTypes.PMD1(SourceFile, Static.Setting.Misc.IsLittleEndian);
            PM1.SetNewMSG1(NewMSG1);
            PM1.SaveNew(FileName);
        }

        public static void SaveToBF(MemoryStream NewMSG1, string SourceFile, string FileName)
        {
            PersonaFileTypes.FLW0 FLW0 = new PersonaFileTypes.FLW0(SourceFile, Static.Setting.Misc.IsLittleEndian);
            FLW0.SetNewMSG1(NewMSG1);
            FLW0.SaveNew(FileName);
        }
    }

    class VisualMSG1
    {
        PersonaFileTypes.MSG1 MSG1;

        BackgroundImage BackImage;

        public VisualMSG1(PersonaFileTypes.MSG1 MSG1, BackgroundImage BackImage)
        {
            this.MSG1 = MSG1;
            this.BackImage = BackImage;
            MSG1.name.ListChanged += Name_ListChanged;
            MSG1.msg.ListChanged += Msg_ListChanged;
        }

        private void Name_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                name.Add(new VisualNames(MSG1.name[e.NewIndex], BackImage));
            }
            else if (e.ListChangedType == ListChangedType.Reset)
            {
                name.Clear();
            }
            else
            {

            }
        }

        private void Msg_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                Visual VisualOldName = name.FirstOrDefault(x => x.CurrentName.Index == MSG1.msg[e.NewIndex].CharacterIndex)?.VisualOldName;
                Visual VisualNewName = name.FirstOrDefault(x => x.CurrentName.Index == MSG1.msg[e.NewIndex].CharacterIndex)?.VisualNewName;

                msg.Add(new VisualMSGs(MSG1.msg[e.NewIndex], BackImage, VisualOldName, VisualNewName));
            }
            else if (e.ListChangedType == ListChangedType.Reset)
            {
                msg.Clear();
            }
            else
            {

            }
        }

        public class Visual : INotifyPropertyChanged
        {
            public enum Type
            {
                Text,
                Name
            }

            CharList CharLST;
            Color CurrentColor { get; set; }
            BitmapPalette ColorPaletteBMP { get; set; }

            public Visual(CharList CharLST, BackgroundImage BackImage, Type Type)
            {
                this.CharLST = CharLST;
                this.BackImage = BackImage;
                BackImage.VisualTextChanged += BackImage_VisualTextChanged;
                if (Type == Type.Text)
                {
                    ColorPaletteBMP = Util.CreatePallete(BackImage.ColorText);
                    BackImage.TextColorChanged += BackImage_TextColorChanged;
                }
                else
                {
                    ColorPaletteBMP = Util.CreatePallete(BackImage.ColorName);
                    BackImage.NameColorChanged += BackImage_TextColorChanged;
                }
            }

            private void BackImage_TextColorChanged(Color color)
            {
                ColorPaletteBMP = Util.CreatePallete(color);
                Notify("Text");
            }

            private void BackImage_VisualTextChanged()
            {
                Update();
            }

            public void ElementArray_Changed(PersonaFileTypes.MSG1.MyStringElement[] array)
            {
                if (Static.Setting.ViewVisualizer)
                    ImageData = array.DrawText(CharLST);
            }

            public void ArrayChanged(byte[] Name)
            {
                if (Static.Setting.ViewVisualizer)
                    ImageData = Name.parseString().DrawText(CharLST);
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

            public BackgroundImage BackImage { get; set; }

            double _Width = 1;
            double _Height = 1;
            ImageData _ImageData;
            ImageData ImageData
            {
                get { return _ImageData; }
                set
                {
                    _ImageData = value;
                    Update();
                    Notify("Text");
                }
            }

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
                get { return ImageData == null ? null : BitmapSource.Create(ImageData.PixelWidth, ImageData.PixelHeight, 96, 96, ImageData.PixelFormat, ColorPaletteBMP, ImageData.Data, ImageData.Stride); }
            }

            public void Update()
            {
                if (ImageData != null)
                {
                    double OnePixelHeight = ((BackImage.RealWidth / BackImage.Image.PixelWidth) * BackImage.GlyphScale);
                    double OnePixelWidth = OnePixelHeight * 0.9375;
                    Height = ImageData.PixelHeight * OnePixelHeight;
                    Width = ImageData.PixelWidth * OnePixelWidth;
                }
            }
        }

        public class VisualNames : INotifyPropertyChanged
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

            public VisualNames(PersonaFileTypes.MSG1.Names Name, BackgroundImage BackImage)
            {
                CurrentName = Name;

                _VisualOldName = new Visual(CurrentName.OldCharList, BackImage, Visual.Type.Name);
                CurrentName.OldNameArrayChanged += _VisualOldName.ArrayChanged;
                _VisualOldName.ArrayChanged(CurrentName.OldNameBytes);

                _VisualNewName = new Visual(CurrentName.NewCharList, BackImage, Visual.Type.Name);
                CurrentName.NewNameArrayChanged += _VisualNewName.ArrayChanged;
                _VisualNewName.ArrayChanged(CurrentName.NewNameBytes);
            }

            PersonaFileTypes.MSG1.Names _CurrentName;
            public PersonaFileTypes.MSG1.Names CurrentName
            {
                get { return _CurrentName; }
                set
                {
                    if (value != _CurrentName)
                    {
                        _CurrentName = value;
                        Notify("CurrentName");
                    }
                }
            }

            Visual _VisualOldName;
            public Visual VisualOldName
            {
                get { return _VisualOldName; }
                set
                {
                    _VisualOldName = value;
                    Notify("VisualOldName");
                }
            }
            Visual _VisualNewName;
            public Visual VisualNewName
            {
                get { return _VisualNewName; }
                set
                {
                    _VisualOldName = value;
                    Notify("VisualNewName");
                }
            }
        }

        public MemoryStream GetNewMSG1()
        {
            return MSG1.GetNewMSG;
        }

        public class VisualMSGs : INotifyPropertyChanged
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

            public VisualMSGs(PersonaFileTypes.MSG1.MSGs MSG, BackgroundImage BackImage, Visual OldName, Visual NewName)
            {
                CurrentMSG = MSG;

                foreach (var Str in CurrentMSG.Strings)
                    Strings.Add(new VisualMSGstrings(Str, BackImage, OldName, NewName));
            }

            PersonaFileTypes.MSG1.MSGs _CurrentMSG;
            public PersonaFileTypes.MSG1.MSGs CurrentMSG
            {
                get { return _CurrentMSG; }
                set
                {
                    if (value != _CurrentMSG)
                    {
                        _CurrentMSG = value;
                        Notify("CurrentMSG");
                    }
                }
            }

            public class VisualMSGstrings : INotifyPropertyChanged
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

                public VisualMSGstrings(PersonaFileTypes.MSG1.MSGs.MSGstr Strings, BackgroundImage BackImage, Visual OldName, Visual NewName)
                {
                    CurrentMSGstrings = Strings;
                    this.OldName = OldName;
                    this.NewName = NewName;

                    OldString = new Visual(CurrentMSGstrings.OldString.CurrentCharList, BackImage, Visual.Type.Text);
                    CurrentMSGstrings.OldString.ElementArrayChanged += OldString.ElementArray_Changed;
                    OldString.ElementArray_Changed(CurrentMSGstrings.OldString.ElementArray);

                    NewString = new Visual(CurrentMSGstrings.NewString.CurrentCharList, BackImage, Visual.Type.Text);
                    CurrentMSGstrings.NewString.ElementArrayChanged += NewString.ElementArray_Changed;
                    NewString.ElementArray_Changed(CurrentMSGstrings.NewString.ElementArray);
                }

                PersonaFileTypes.MSG1.MSGs.MSGstr _CurrentMSGstrings;
                public PersonaFileTypes.MSG1.MSGs.MSGstr CurrentMSGstrings
                {
                    get { return _CurrentMSGstrings; }
                    set
                    {
                        if (value != _CurrentMSGstrings)
                        {
                            _CurrentMSGstrings = value;
                            Notify("CurrentMSGstrings");
                        }
                    }
                }

                public Visual OldName { get; set; }
                public Visual NewName { get; set; }
                Visual _OldString;
                public Visual OldString
                {
                    get { return _OldString; }
                    set
                    {
                        _OldString = value;
                        Notify("OldString");
                    }
                }
                Visual _NewString;
                public Visual NewString
                {
                    get { return _NewString; }
                    set
                    {
                        _NewString = value;
                        Notify("NewString");
                    }
                }
            }

            public BindingList<VisualMSGstrings> Strings { get; set; } = new BindingList<VisualMSGstrings>();
        }

        public BindingList<VisualMSGs> msg { get; set; } = new BindingList<VisualMSGs>();
        public BindingList<VisualNames> name { get; set; } = new BindingList<VisualNames>();

        public bool Load(string FileName, bool IsLittleEndian)
        {
            return MSG1.Load(FileName, IsLittleEndian);
        }
    }
}