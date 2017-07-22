using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using PersonaTextLib.Types;
using PersonaTextLib;

namespace PersonaText
{
    public partial class ToolVisual : Window
    {
        #region Visual
        //Visual VisualText = new Visual();
        //Visual VisualName = new Visual();

        private void BackImageVisual_GlyphScaleChanged(double glyphScale)
        {
           // VisualName.Back_GlyphScaleChanged(glyphScale);
           // VisualText.Back_GlyphScaleChanged(glyphScale);
        }

        private void BackImageVisual_PixelWidthChanged(int pixelWidth)
        {
           // VisualName.Back_PixelWidthChanged(pixelWidth);
           // VisualText.Back_PixelWidthChanged(pixelWidth);
        }

        private void VisualWidth_ValueChanged(double width)
        {
          //  VisualName.Width_ValueChanged(width);
           // VisualText.Width_ValueChanged(width);
        }

        private void Name_ColorChanged(Color color)
        {
           // VisualName.Text = VisualName.Text?.ChangePallete(color);
        }

        private void Text_ColorChanged(Color color)
        {
          //  VisualText.Text = VisualText.Text?.ChangePallete(color);
        }

        private void BackgroundImageChanged_Changed()
        {
            Resources["TextXStart"] = Static.BackImageVisual.TextStartX;
            Resources["TextYStart"] = Static.BackImageVisual.TextStartY;
            Resources["NameXStart"] = Static.BackImageVisual.NameStartX;
            Resources["NameYStart"] = Static.BackImageVisual.NameStartY;
            Resources["Back"] = Static.BackImageVisual.Image;
        }

        #endregion Visual

        BindingList<string> Backgrounds = new BindingList<string>();
        ObservableVariable OV = new ObservableVariable();

        List<PersonaFileTypes.MSG1.MyStringElement> TEXT = new List<PersonaFileTypes.MSG1.MyStringElement>();
        List<PersonaFileTypes.MSG1.MyStringElement> NAME = new List<PersonaFileTypes.MSG1.MyStringElement>();

        private CharList CharList = null;

        private string[] OldText = new string[1];

        public class ComboBoxList
        {
            public string Text { get; set; }
            public string Name { get; set; }
        }

        public ObservableCollection<ComboBoxList> PageList = new ObservableCollection<ComboBoxList>();
        public ObservableCollection<ComboBoxList> FontList = new ObservableCollection<ComboBoxList>();

        public ToolVisual()
        {
            Static.VisualWidth.ValueChanged += VisualWidth_ValueChanged;
            VisualWidth_ValueChanged(Static.VisualWidth.Value);
            //Static.BackImageVisual.PixelWidthChanged += BackImageVisual_PixelWidthChanged;
            BackImageVisual_PixelWidthChanged(Static.BackImageVisual.Image.PixelWidth);
            //Static.BackImageVisual.GlyphScaleChanged += BackImageVisual_GlyphScaleChanged;
            BackImageVisual_GlyphScaleChanged(Static.BackImageVisual.GlyphScale);
            Static.Setting.Empty.Text.ColorChanged += Text_ColorChanged;
            Text_ColorChanged(Static.Setting.Empty.Text.Color);
            Static.Setting.Empty.Name.ColorChanged += Name_ColorChanged;
            Name_ColorChanged(Static.Setting.Empty.Name.Color);

           // Static.BackImageVisual.BackgroundImageChanged += BackgroundImageChanged_Changed; ;
            InitializeComponent();

            SelectBack.DataContext = Backgrounds;
            Backgrounds.Add("Empty");

            if (Directory.Exists((Static.BackgroundPath)))
            {
                DirectoryInfo DI = new DirectoryInfo(Static.BackgroundPath);
                foreach (var file in DI.GetFiles(@"*.png"))
                    Backgrounds.Add(file.Name);
            }

            SelectBack.SelectedIndex = Backgrounds.IndexOf(Static.Setting.SelectedBackgroundVisual);

            PageList.Add(new ComboBoxList() { Text = "Persona 4 Dialog", Name = "Persona4Dialog" });
            FontList.Add(new ComboBoxList() { Text = "Old Font", Name = "OldFont" });
            FontList.Add(new ComboBoxList() { Text = "New Font", Name = "NewFont" });
            ComboBox_Font.DataContext = FontList;
            SelectBack.DataContext = Backgrounds;

         //   VisText.DataContext = VisualText;
          //  VisName.DataContext = VisualName;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TB = sender as TextBox;
            TEXT.GetMyByteArray(TB.Text, CharList.List);

            string text = "";
            foreach (var a in TEXT)
                if (a.Bytes.Length != 0)
                    text += BitConverter.ToString(a.Bytes).Replace('-', ' ') + " ";

            HEX.Text = text;

            //VisualText.Text = TEXT.DrawText(CharList, Static.BackImageVisual.ColorText);
        }

        private void TextBox_NameChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TB = sender as TextBox;
            NAME.GetMyByteArray(TB.Text, CharList.List);
            //VisualName.Text = NAME.DrawText(CharList, Static.BackImageVisual.ColorName);
        }

        private void ComboBox_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxList CBL = (ComboBoxList)e.AddedItems[0];
            if (CBL.Name == "OldFont")
            {
                CharList = Static.FontMap.old_char;
            }
            else if (CBL.Name == "NewFont")
            {
                CharList = Static.FontMap.new_char;
            }

            TextBox_TextChanged(TextBoxText, null);
            TextBox_NameChanged(TextBoxName, null);
        }

        class ObservableVariable : INotifyPropertyChanged
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

            public double Width { get; set; }
        }

        private void Bitmap_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Static.VisualWidth.Value = e.NewSize.Width;
        }

        private void SelectBack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Static.Setting.SelectedBackgroundVisual = (sender as ComboBox).SelectedItem as string;
            if (Static.Setting.SelectedBackgroundVisual == "Empty")
            { Static.BackImageVisual.Update("Empty"); }
            else
            { Static.BackImageVisual.Update(Static.BackgroundPath + Static.Setting.SelectedBackgroundVisual); }

            TextBox_TextChanged(TextBoxText, null);
            TextBox_NameChanged(TextBoxName, null);
        }
    }
}