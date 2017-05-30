using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace PersonaText
{
    public class Static
    {
        public static Settings Setting = new Settings();
        public static PersonaType Personas = new PersonaType();
        public static class FontMap
        {
            public static List<fnmp> old_char = new List<fnmp>();
            public static List<fnmp> new_char = new List<fnmp>();
            public static List<glyphYshift> char_shift = new List<glyphYshift>();
        }
        public static string SelectedGameType = "";
    }

    public partial class MainWindow : Window
    {
        public MSG1 MSG1 = new MSG1();
        ObservableVariableMainWindow OVMW = new ObservableVariableMainWindow();
        List<string> ComboBox = new List<string>() { "Persona 3 FES", "Persona 4" };

        public string Import_Path = "";
        Text Text = new Text();

        public MainWindow()
        {
            InitializeComponent();

            ScrollViewer.DataContext = MSG1;
            mainmenu.DataContext = OVMW;

            if (File.Exists(@"OLD.TXT"))
            {
                Text.ReadFNMP(@"OLD.TXT", ref Static.FontMap.old_char);
            }
            if (File.Exists(@"NEW.TXT"))
            {
                Text.ReadFNMP(@"NEW.TXT", ref Static.FontMap.new_char);
            }
            if (File.Exists(@"OLD.FNT"))
            {
                Text.ReadFN(@"OLD.FNT", ref Static.FontMap.old_char);
            }
            if (File.Exists(@"NEW.FNT"))
            {
                Text.ReadFN(@"NEW.FNT", ref Static.FontMap.new_char);
            }

            Text.ReadShift(ref Static.FontMap.char_shift);

            Static.FontMap.old_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));
            Static.FontMap.new_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));

            Combobox_Gametype.DataContext = ComboBox;
            if(Static.Setting.Get("GameType") == "P3FES")
            {
                Combobox_Gametype.SelectedIndex = ComboBox.IndexOf("Persona 3 FES");
            }
            else
            {
                Combobox_Gametype.SelectedIndex = ComboBox.IndexOf("Persona 4");
            }
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
                open_file();
            }
        }

        private void open_file()
        {
            OVMW.openfile = true;
            MSG1.ParseMSG1(Import_Path, false);
            MSG1.UpdateString();
            this.Title = MSG1.SelectIndex >= 0 ? "Persona Font Editor - [" + Path.GetFileNameWithoutExtension(Import_Path) + " - " + Convert.ToString(MSG1.SelectIndex).PadLeft(3, '0') + "]" : "Persona Font Editor - [" + Path.GetFileNameWithoutExtension(Import_Path) + "]";
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
            this.Title = "Persona Font Editor - [" + Path.GetFileNameWithoutExtension(Import_Path) + "]";
        }

        private void mi_saveproject_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = MSG1.SelectIndex >= 0 ? Path.GetFileNameWithoutExtension(Import_Path) + " - " + Convert.ToString(MSG1.SelectIndex).PadLeft(3, '0') + ".PTP" : Path.GetFileNameWithoutExtension(Import_Path) + ".PTP";
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

                this.Title = "Persona Font Editor - [" + Path.GetFileNameWithoutExtension(Import_Path) + "]";
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

                Text.ReadFN(path, ref Static.FontMap.old_char);
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

                Text.ReadFN(path, ref Static.FontMap.new_char);
                File.Copy(path, @"NEW.FNT", true);
            }
        }

        private void CharSet_old(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet(Static.FontMap.old_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Text.WriteFNMP(@"OLD.TXT", ref Static.FontMap.old_char);
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
                CharSet CS = new CharSet(Static.FontMap.new_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Text.WriteFNMP(@"NEW.TXT", ref Static.FontMap.new_char);
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
            sfd.FileName = Path.GetFileNameWithoutExtension(Import_Path) + ".TXT";
            sfd.Filter = "Text (.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                MSG1.SaveAsText(sfd.FileName, "000");
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

        private void Combobox_Gametype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == "Persona 3 FES")
            {
                Static.Setting.Set("GameType", "P3FES");
                Static.SelectedGameType = "P3FES";
            }
            else
            {
                Static.Setting.Set("GameType", "P4");
                Static.SelectedGameType = "P4";
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
}