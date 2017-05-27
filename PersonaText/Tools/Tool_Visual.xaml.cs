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

namespace PersonaText
{
    public partial class Tool_Visual : Window
    {
        private List<fnmp> CharList = new List<fnmp>();

        private string[] OldText = new string[1];

        public class ComboBoxList
        {
            public string Text { get; set; }
            public string Name { get; set; }
        }

        private ObservableCollection<BitmapList> VisualText = new ObservableCollection<BitmapList>();
        private ObservableCollection<BitmapList> VisualName = new ObservableCollection<BitmapList>();

        public ObservableCollection<ComboBoxList> PageList = new ObservableCollection<ComboBoxList>();
        public ObservableCollection<ComboBoxList> FontList = new ObservableCollection<ComboBoxList>();

        public Tool_Visual()
        {
            InitializeComponent();

            PageList.Add(new ComboBoxList() { Text = "Persona 4 Dialog", Name = "Persona4Dialog" });
            FontList.Add(new ComboBoxList() { Text = "Old Font", Name = "OldFont" });
            FontList.Add(new ComboBoxList() { Text = "New Font", Name = "NewFont" });
            ComboBox.DataContext = PageList;
            ComboBox_Font.DataContext = FontList;
            VisualT.DataContext = VisualText;
            VisualT1.DataContext = VisualName;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TB = sender as TextBox;
            VisualText.GetBitmapList(TB.Text, ref CharList, System.Drawing.Color.White);
        }

        private void TextBox_NameChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TB = sender as TextBox;
            VisualName.GetBitmapList(TB.Text, ref CharList, System.Drawing.Color.Black);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxList CBL = (ComboBoxList)e.AddedItems[0];
            Grid Frame = (Grid)FindName(CBL.Name);
            Frame.Visibility = Visibility.Visible;
        }

        private void ComboBox_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxList CBL = (ComboBoxList)e.AddedItems[0];
            if (CBL.Name == "OldFont")
            {
                CharList = FontMap.old_char;
            }
            else if (CBL.Name == "NewFont")
            {
                CharList = FontMap.new_char;
            }
        }
    }

    public class ConverterHeigth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double Height = (double)value;
            return Height * 0.36364;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double a = (double)value;
            return a * 0.9375;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterXpos : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            double pos = (double)value[0];
            double heigth = (double)value[1];
            if (parameter.ToString() == "Name")
            {
                heigth = heigth * 0.9375;
            }
            else
            {
                heigth = heigth * 0.36364 * 0.9375;
            }

            return (pos / 32) * heigth;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterYpos : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double pos = (double)value[0];
            double heigth = (double)value[1];
            if (parameter.ToString() == "Name")
            {
            }
            else
            {
                heigth = heigth * 0.36364;
            }
            return (pos / 32) * heigth;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}