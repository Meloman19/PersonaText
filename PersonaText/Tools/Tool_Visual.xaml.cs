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

namespace PersonaText
{
    public partial class Tool_Visual : Window
    {
        ObservableCollection<BitmapSource> VisualText = new ObservableCollection<BitmapSource>();
        public List<fnmp> CharList = new List<fnmp>();

        MainWindow MW;

        public Tool_Visual()
        {
            InitializeComponent();
            MW = this.Owner as MainWindow;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TB = sender as TextBox;
            var split = Regex.Split(TB.Text, "\r\n|\r|\n");
            foreach (var String in split)
            {
                
            }
        }
    }
}
