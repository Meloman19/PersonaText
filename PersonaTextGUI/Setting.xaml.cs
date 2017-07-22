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
using System.ComponentModel;
using System.Reflection;
namespace PersonaText
{
    public partial class Setting : Window
    {
        BindingList<ColorL> ColorList { get; set; }

        class ColorL
        {
            public string Name { get; set; }
            public Color Color { get; set; }
        }

        public Setting()
        {
            InitializeComponent();
            WidthSet = Static.Setting.Empty.Width.ToString();
            HeightSet = Static.Setting.Empty.Height.ToString();
            TextXSet = Static.Setting.Empty.Text.X.ToString();
            TextYSet = Static.Setting.Empty.Text.Y.ToString();
            NameXSet = Static.Setting.Empty.Name.X.ToString();
            NameYSet = Static.Setting.Empty.Name.Y.ToString();
            GlyphScale = Static.Setting.Empty.GlyphScale.ToString();
            WidthSetting.Text = WidthSet;
            HeightSetting.Text = HeightSet;
            TextXSetting.Text = TextXSet;
            TextYSetting.Text = TextYSet;
            NameXSetting.Text = NameXSet;
            NameYSetting.Text = NameYSet;
            GlyphScaleSetting.Text = GlyphScale;

            ColorList = new BindingList<ColorL>(typeof(Colors).GetProperties().Select(x => new ColorL() { Name = x.Name, Color = (Color)ColorConverter.ConvertFromString(x.Name) }).ToList());
            ColorBackSetting.DataContext = ColorList;
            ColorNameSetting.DataContext = ColorList;
            ColorTextSetting.DataContext = ColorList;
            ColorBack = Static.Setting.Empty.BackgroundColor;
            ColorText = Static.Setting.Empty.Text.Color;
            ColorName = Static.Setting.Empty.Name.Color;
            ColorBackSetting.SelectedItem = ColorList.FirstOrDefault(x => x.Color == ColorBack);
            ColorTextSetting.SelectedItem = ColorList.FirstOrDefault(x => x.Color == ColorText);
            ColorNameSetting.SelectedItem = ColorList.FirstOrDefault(x => x.Color == ColorName);
        }

        void save()
        {
            Static.Setting.Empty.Height = Convert.ToInt32(HeightSet);
            Static.Setting.Empty.Width = Convert.ToInt32(WidthSet);
            Static.Setting.Empty.Text.X = Convert.ToInt32(TextXSet);
            Static.Setting.Empty.Text.Y = Convert.ToInt32(TextYSet);
            Static.Setting.Empty.Name.X = Convert.ToInt32(NameXSet);
            Static.Setting.Empty.Name.Y = Convert.ToInt32(NameYSet);
            Static.Setting.Empty.GlyphScale = Convert.ToDouble(GlyphScale);
            Static.Setting.Empty.BackgroundColor = ColorBack;
            Static.Setting.Empty.Text.Color = ColorText;
            Static.Setting.Empty.Name.Color = ColorName;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            save();
            DialogResult = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                if (MessageBox.Show("Save changes?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    save();
                }
            }
        }

        #region Empty

        string WidthSet = "";
        private void Width_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            if (int.TryParse(((TextBox)sender).Text, out temp))
                WidthSet = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = WidthSet;
        }

        string HeightSet = "";
        private void Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            if (int.TryParse(((TextBox)sender).Text, out temp))
                HeightSet = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = HeightSet;
        }

        string TextXSet = "";
        private void TextXSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            if (int.TryParse(((TextBox)sender).Text, out temp))
                TextXSet = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = TextXSet;
        }

        string TextYSet = "";
        private void TextYSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            if (int.TryParse(((TextBox)sender).Text, out temp))
                TextYSet = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = TextYSet;
        }

        string NameXSet = "";
        private void NameXSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            if (int.TryParse(((TextBox)sender).Text, out temp))
                NameXSet = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = NameXSet;
        }

        string NameYSet = "";
        private void NameYSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            if (int.TryParse(((TextBox)sender).Text, out temp))
                NameYSet = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = NameYSet;
        }

        string GlyphScale = "";
        private void GlyphScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            double temp;
            if (double.TryParse(((TextBox)sender).Text, out temp))
                GlyphScale = ((TextBox)sender).Text;
            else ((TextBox)sender).Text = GlyphScale;
        }

        #endregion Empty

        Color ColorBack;

        Color ColorText;

        Color ColorName;

        private void ColorBackSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorBack = ((ColorL)((ComboBox)sender).SelectedItem).Color;
            ViewColorBack.Background = new SolidColorBrush(ColorBack);
        }

        private void ColorTextSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorText = ((ColorL)((ComboBox)sender).SelectedItem).Color;
            ViewColorText.Background = new SolidColorBrush(ColorText);
        }

        private void ColorNameSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorName = ((ColorL)((ComboBox)sender).SelectedItem).Color;
            ViewColorName.Background = new SolidColorBrush(ColorName);
        }

    }
}
