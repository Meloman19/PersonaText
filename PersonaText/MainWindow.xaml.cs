using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Controls;

namespace PersonaText
{
    static class FontMap
    {
        public static List<fnmp> old_char = new List<fnmp>();
        public static List<fnmp> new_char = new List<fnmp>();
    }

    public partial class MainWindow : Window
    {
        public MSG1 MSG1 = new MSG1();
        ObservableVariableMainWindow OVMW = new ObservableVariableMainWindow();

        string Import_FileName = "";
        public string Import_Path = "";
        Text Text = new Text();

        public MainWindow()
        {
            InitializeComponent();
            ScrollViewer.DataContext = MSG1;
            mainmenu.DataContext = OVMW;

            if (File.Exists(@"OLD.TXT"))
            {
                Text.ReadFNMP(@"OLD.TXT", ref FontMap.old_char);
            }
            if (File.Exists(@"NEW.TXT"))
            {
                Text.ReadFNMP(@"NEW.TXT", ref FontMap.new_char);
            }
            if (File.Exists(@"OLD.FNT"))
            {
                Text.ReadFN(@"OLD.FNT", ref FontMap.old_char);
            }
            if (File.Exists(@"NEW.FNT"))
            {
                Text.ReadFN(@"NEW.FNT", ref FontMap.new_char);
            }

            FontMap.old_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));
            FontMap.new_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));
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
                Import_FileName = Path.GetFileNameWithoutExtension(Import_Path);
                open_file();
            }
        }

        private void open_file()
        {
            OVMW.openfile = true;
            MSG1.ParseMSG1(Import_Path, false);
            MSG1.UpdateString();
            this.Title = MSG1.SelectIndex >= 0 ? "Persona Font Editor - [" + Import_FileName + " - " + Convert.ToString(MSG1.SelectIndex).PadLeft(3, '0') + "]" : "Persona Font Editor - [" + Import_FileName + "]";
        }

        private void open_project()
        {
            OVMW.openfile = true;
            Project Pr = new Project(Import_Path);
            Pr.OpenProject();
            MSG1.msg.Clear();
            MSG1.name.Clear();
            foreach (var MSG in Pr.msg)
            {
                MSG1.msg.Add(MSG);
            }
            foreach (var NAME in Pr.name)
            {
                MSG1.name.Add(NAME);
            }
            this.Title = "Persona Font Editor - [" + Import_FileName + "]";
        }

        private void mi_saveproject_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = MSG1.SelectIndex >= 0 ? Import_FileName + " - " + Convert.ToString(MSG1.SelectIndex).PadLeft(3, '0') + ".PTP" : Import_FileName + ".PTP";
            sfd.Filter = "Persona Text Project (*.PTP)|*.PTP";
            if (sfd.ShowDialog() == true)
            {
                Project Pr = new Project(sfd.SafeFileName);
                Pr.name = MSG1.name.ToList();
                Pr.msg = MSG1.msg.ToList();
                Pr.SaveProject();
            }
        }

        private void mi_openproject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Persona Text Project (*.PTP)|*.PTP";
            if (ofd.ShowDialog() == true)
            {
                OVMW.openfile = true;
                Import_Path = ofd.FileName;
                Import_FileName = Path.GetFileNameWithoutExtension(Import_Path);
                Project Pr = new Project(Import_Path);
                Pr.OpenProject();
                MSG1.msg.Clear();
                MSG1.name.Clear();
                foreach (var MSG in Pr.msg)
                {
                    MSG1.msg.Add(MSG);
                }
                foreach (var NAME in Pr.name)
                {
                    MSG1.name.Add(NAME);
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

                Text.ReadFN(path, ref FontMap.old_char);
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

                Text.ReadFN(path, ref FontMap.new_char);
                File.Copy(path, @"NEW.FNT", true);
            }
        }

        private void CharSet_old(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet(FontMap.old_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Text.WriteFNMP(@"OLD.TXT", ref FontMap.old_char);
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
                CharSet CS = new CharSet(FontMap.new_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                //    Text.WriteFNMP(@"NEW.TXT", ref new_char);
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
            TE.Owner = this;
            this.Visibility = Visibility.Collapsed;
            TE.ShowDialog();
            this.Visibility = Visibility.Visible;
        }

        private void mm_tools_visual_Click(object sender, RoutedEventArgs e)
        {
            Tool_Visual TV = new Tool_Visual();
            TV.Owner = this;
            TV.CharList = FontMap.old_char;
            TV.ShowDialog();
        }

        private void MW_Loaded(object sender, RoutedEventArgs e)
        {
            if (Import_Path != "")
            {
                if (File.Exists(Import_Path))
                {
                    OVMW.openfile = true;
                    FileInfo FI = new FileInfo(Import_Path);
                    if (FI.Extension == ".PTP")
                    {
                        open_project();
                    }
                    else
                    {
                        open_file();
                    }
                }
            }
        }

        private void New_Save_Click(object sender, RoutedEventArgs e)
        {
            if (Import_Path != "")
            {
                MSG1.SaveAsNewMSG1(Import_Path);
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

        private bool _openfile = false;
        public bool openfile
        {
            get { return _openfile; }
            set
            {
                if (value != _openfile)
                {
                    _openfile = value;
                    Notify("openfile");
                }
            }
        }
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
