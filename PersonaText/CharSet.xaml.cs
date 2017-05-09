using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace PersonaText
{
    /// <summary>
    /// Логика взаимодействия для CharSet.xaml
    /// </summary>
    public partial class CharSet : Window
    {
        Text Text = new Text();
        public bool saving = false;
        public List<fnmp> chlt = new List<fnmp>();
        public ObservableCollection<fnmp> _CharList = new ObservableCollection<fnmp>();
        public ObservableCollection<fnmp> CharList
        {
            get { return _CharList; }
            set { _CharList = value; }
        }

        public CharSet()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void save()
        {
            foreach (var CL in CharList)
            {
                fnmp CL2 = chlt.Find(x => x.Index == CL.Index);
                CL2.Char = CL.Char;
            }
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            save();
            saving = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var CL in chlt)
            {
                CharList.Add(new fnmp { Index = CL.Index, Image = CL.Image, Char = CL.Char });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (saving == false)
            {
                MessageBoxResult dr = MessageBox.Show("Save changes?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dr == MessageBoxResult.Yes)
                {
                    save();
                }
            }
        }


    }
}
