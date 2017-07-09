using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections;
using System.Linq;

namespace PersonaText
{
    public partial class CharSet : Window
    {
        CharList chlt;
        BindingList<FnMpImg> CharList { get; set; } = new BindingList<FnMpImg>();

        public CharSet(object cl)
        {
            chlt = cl as CharList;
            InitializeComponent();
            DataContext = CharList;
            foreach (var C in chlt.List)
            {
                CharList.Add(new FnMpImg
                {
                    Index = C.Index,
                    Image = BitmapSource.Create(chlt.Width, chlt.Height, 96, 96, PixelFormats.Indexed4, chlt.Palette, C.Image_data, 16),
                    Char = C.Char
                });
            }
        }

        private void save()
        {
            foreach (var CL in CharList)
            {
                FnMpData CL2 = chlt.List.FirstOrDefault(x => x.Index == CL.Index);
                CL2.Char = CL.Char;
            }

            this.DialogResult = true;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            save();
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.DialogResult != true)
            {
                if (MessageBox.Show("Save changes?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    save();
                }
            }
            CharList.Clear();
        }
    }
}