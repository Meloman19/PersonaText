using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;

namespace PersonaText
{
    public partial class MainWindow : Window
    {
        public MSG1 MSG1 = new MSG1();

        string Import_FileName = "";
        public string Import_Path = "";
        Text Text = new Text();

        public List<fnmp> old_char = new List<fnmp>();
        public List<fnmp> new_char = new List<fnmp>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MSG1;

            if (File.Exists(@"OLD.TXT"))
            {
                Text.ReadFNMP(@"OLD.TXT", ref old_char);
            }
            if (File.Exists(@"NEW.TXT"))
            {
                Text.ReadFNMP(@"NEW.TXT", ref new_char);
            }
            if (File.Exists(@"OLD.FNT"))
            {
                Text.ReadFN(@"OLD.FNT", ref old_char);
            }
            if (File.Exists(@"NEW.FNT"))
            {
                Text.ReadFN(@"NEW.FNT", ref new_char);
            }

            old_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));
            new_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));
        }

        private string[] OpenFiles()
        {
            string[] str = new string[0];
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PMD files (*.PM1)|*.PM1|All files (*.*)|*.*";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                foreach (string filename in ofd.FileNames)
                {
                    Array.Resize<string>(ref str, str.Length + 1);
                    str[str.Length - 1] = filename;
                }
            }
            return str;
        }

        private void mi_import_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files|*.*";
            if (ofd.ShowDialog() == true)
            {
                Import_Path = ofd.FileName;
                Import_FileName = ofd.SafeFileName;
                open_file();
            }
        }

        private void open_file()
        {
            MSG1.FM = old_char;
            MSG1.ParseMSG1(Import_Path, false);
            MSG1.UpdateString();
            this.Title = MSG1.SelectIndex >= 0 ? "Persona Font Editor - [" + Import_FileName + " - " + Convert.ToString(MSG1.SelectIndex).PadLeft(3, '0') + "]" : "Persona Font Editor - [" + Import_FileName + "]";
        }

        private void mi_saveproject_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Import_FileName.Remove(Import_FileName.Length - 4);
            sfd.Filter = "Persona Text Project (*.PTP)|*.PTP";
            if (sfd.ShowDialog() == true)
            {
                string path = sfd.SafeFileName;
                Project Pr = new Project();
                Pr.Source_File = Import_Path;
                Pr.Save_Project(path, MSG1.msg.ToList());
            }
        }

        private void mi_openproject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Persona Text Project (*.PTP)|*.PTP";
            if (ofd.ShowDialog() == true)
            {
                Import_Path = ofd.FileName;
                Import_FileName = ofd.SafeFileName;
                Project Pr = new Project();

                MSG1.msg.Clear();
                List<msg> temp = Pr.Open_Project(Import_Path);
                foreach (var temp2 in temp)
                {
                    MSG1.msg.Add(temp2);
                }
                this.Title = "Persona Font Editor - [" + Import_FileName + "]";
            }
        }

        private void FNold(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Persona Font (*.FNT)|*.FNT";
            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
                if (File.Exists(@"OLD.FNT"))
                {
                    MessageBoxResult msr = MessageBox.Show("Change current Old Font?", "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msr == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                Text.ReadFN(path, ref old_char);
                File.Copy(path, @"OLD.FNT", true);
            }
        }

        private void FNnew(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Persona Font (*.FNT)|*.FNT";
            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
                if (File.Exists(@"NEW.FNT"))
                {
                    MessageBoxResult msr = MessageBox.Show("Change current New Font?", "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msr == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                Text.ReadFN(path, ref new_char);
                File.Copy(path, @"NEW.FNT", true);
            }
        }

        private void CharSet_old(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet();
                CS.Owner = this;
                CS.chlt = old_char;
                if (CS.ShowDialog() == true)
                {
                    Text.WriteFNMP(@"OLD.TXT", ref old_char);
                    MSG1.FM = old_char;
                    MSG1.UpdateString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CharSet_new(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;

            try
            {
                CharSet CS = new CharSet();
                CS.Owner = this;
                CS.chlt = new_char;
                if (CS.ShowDialog() == true)
                {
                    Text.WriteFNMP(@"NEW.TXT", ref new_char);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            this.Visibility = Visibility.Visible;
        }

        private void STold(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Import_FileName.Remove(Import_FileName.Length - 4);
            sfd.Filter = "Text (.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                MSG1.SaveAsText(sfd.FileName);
            }
        }

        private void mm_tools_export_Click(object sender, RoutedEventArgs e)
        {
            Tool_Export TE = new PersonaText.Tool_Export();
            TE.OVE.MSG1.FM = old_char;
            TE.Owner = this;
            this.Visibility = Visibility.Collapsed;
            TE.ShowDialog();
            this.Visibility = Visibility.Visible;
        }

        private void mm_tools_visual_Click(object sender, RoutedEventArgs e)
        {
            Tool_Visual TV = new Tool_Visual();
            TV.Owner = this;
            TV.CharList = old_char;
            TV.ShowDialog();
        }

        private void MW_Loaded(object sender, RoutedEventArgs e)
        {
            if (Import_Path != "")
            {
                MSG1.openfile = true;
                FileInfo FI = new FileInfo(Import_Path);
                Import_FileName = FI.Name;
                open_file();
            }
        }
    }

    public class ObservableVariableMainWindow : INotifyPropertyChanged
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



    }

    public class CharacterIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int Index = (int)value[0];
            MainWindow MW = (MainWindow)value[1];
            if (MW.MSG1.name.Count > Index)
            {
                return MW.MSG1.name[Index].Old_Name;
            }
            else { return "<NO_NAME>"; }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
