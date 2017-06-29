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
    public partial class CharSet : Window
    {
        List<fnmp> chlt = new List<fnmp>();
        ObservableCollection<fnmp> CharList { get; set; } = new ObservableCollection<fnmp>();

        public CharSet(object cl)
        {
            List<fnmp> CharL = cl as List<fnmp>;
            chlt = CharL;
            InitializeComponent();
            DataContext = CharList;

            foreach (var C in CharL)
            {
                CharList.Add(new fnmp { Index = C.Index, Image = C.Image, Char = C.Char });
            }
        }

        private void save()
        {
            foreach (var CL in CharList)
            {
                fnmp CL2 = chlt.Find(x => x.Index == CL.Index);
                CL2.Char = CL.Char;
            }

            this.DialogResult = true;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            save();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != true)
            {
                if (MessageBox.Show("Save changes?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    save();
                }
            }
        }
    }
}
