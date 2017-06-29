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
using System.Text.RegularExpressions;

namespace PersonaText
{
    static class Static
    {
        public static class FileInfo
        {
            public static string SourceFileName = "";
            public static string OpenFullFileName = "";
            public static long SelectPosition = -1;
        }

        public static Settings Setting = new Settings();
        public static PersonaType Personas = new PersonaType();
        public static class FontMap
        {
            public static List<fnmp> old_char = new List<fnmp>();
            public static List<fnmp> new_char = new List<fnmp>();
            public static List<glyphYshift> char_shift = new List<glyphYshift>();
        }
    }

    public partial class MainWindow : Window
    {
        public int NameCount { get { return MSG1.name.Count; } }
        public string OldNameByIndex(int index) { return MSG1.name[index].Old_Name; }

        MSG1 MSG1 = new MSG1();
        ObservableVariableMainWindow OVMW = new ObservableVariableMainWindow();
        List<string> ComboBox = new List<string>() { "Persona 3", "Persona 4" };

        public MainWindow()
        {
            InitializeComponent();

            ScrollViewer.DataContext = MSG1;
            mainmenu.DataContext = OVMW;
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";
            if (File.Exists(path + "OLD.TXT"))
            {
                Static.FontMap.old_char.ReadFNMP(path + "OLD.TXT");
            }
            if (File.Exists(path + "NEW.TXT"))
            {
                Static.FontMap.new_char.ReadFNMP(path + "NEW.TXT");
            }
            if (File.Exists(path + "OLD.FNT"))
            {
                Static.FontMap.old_char.ReadFONT(path + "OLD.FNT");
            }
            if (File.Exists(path + @"NEW.FNT"))
            {
                Static.FontMap.new_char.ReadFONT(path + "NEW.FNT");
            }

            Static.FontMap.char_shift.ReadShift();

            Static.FontMap.old_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));
            Static.FontMap.new_char.Sort((a, b) => (a.Index.CompareTo(b.Index)));

            Combobox_Gametype.DataContext = ComboBox;
            if (Static.Setting.GameType == GameType.P3)
            {
                Combobox_Gametype.SelectedIndex = ComboBox.IndexOf("Persona 3");
            }
            else
            {
                Combobox_Gametype.SelectedIndex = ComboBox.IndexOf("Persona 4");
            }
        }

        #region MainMenu

        #region File
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Persona Text Project (*.PTP)|*.PTP";
            if (ofd.ShowDialog() == true)
            {
                Static.FileInfo.OpenFullFileName = ofd.FileName;
                open_project();
            }
        }

        private void SaveProject_click(object sender, RoutedEventArgs e)
        {
            save_project();
        }

        private void Import_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files|*.*";
            if (ofd.ShowDialog() == true)
            {
                Static.FileInfo.OpenFullFileName = ofd.FileName;
                open_file();
            }
        }

        #endregion File

        #region Tools
        private void mm_tools_export_Click(object sender, RoutedEventArgs e)
        {
            ToolExport TE = new PersonaText.ToolExport();
            TE.Owner = this;
            this.Visibility = Visibility.Collapsed;
            TE.ShowDialog();
            this.Visibility = Visibility.Visible;
        }

        private void mm_tools_visual_Click(object sender, RoutedEventArgs e)
        {
            ToolVisual TV = new ToolVisual();
            TV.Owner = this;
            TV.ShowDialog();
        }
        #endregion Tools

        private void Combobox_Gametype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == "Persona 3")
            {
                Static.Setting.GameType = GameType.P3;
            }
            else
            {
                Static.Setting.GameType = GameType.P4;
            }
        }

        #endregion MainMenu

        #region Old Font

        private void OpenOldFONT(object sender, RoutedEventArgs e)
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

                Static.FontMap.old_char.ReadFONT(path);
                File.Copy(path, @"OLD.FNT", true);
            }
        }

        private void SetOldChar(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet(Static.FontMap.old_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Static.FontMap.old_char.WriteFNMP(@"OLD.TXT");
                    MSG1.UpdateString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void STold(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileNameWithoutExtension(Static.FileInfo.OpenFullFileName) + ".TXT";
            sfd.Filter = "Text (.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                MSG1.SaveAsText(sfd.FileName, "", 2);
            }
        }

        #endregion Old Font

        #region New Font

        private void OpenNewFONT(object sender, RoutedEventArgs e)
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

                Static.FontMap.new_char.ReadFONT(path);
                File.Copy(path, @"NEW.FNT", true);
            }
        }

        private void SetNewChar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;

            try
            {
                CharSet CS = new CharSet(Static.FontMap.new_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Static.FontMap.new_char.WriteFNMP(@"NEW.TXT");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            this.Visibility = Visibility.Visible;
        }

        #endregion New Font

        private void MW_Loaded(object sender, RoutedEventArgs e)
        {
            open();
        }

        private void MW_Closing(object sender, CancelEventArgs e)
        {
            if (OVMW.openfile)
            {
                MessageBoxResult a = MessageBox.Show("Save Project?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (a == MessageBoxResult.Yes)
                {
                    save_project();
                }
            }
        }

        private void open()
        {
            if (Static.FileInfo.OpenFullFileName != "")
            {
                if (File.Exists(Static.FileInfo.OpenFullFileName))
                {
                    FileInfo FI = new FileInfo(Static.FileInfo.OpenFullFileName);
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

        private void open_project()
        {
            OVMW.openfile = true;
            Project Pr = new Project(Static.FileInfo.OpenFullFileName);
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

            set_title();
        }

        private void save_project()
        {
            string Name = Static.FileInfo.SelectPosition >= 0 ? Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + " - " + Convert.ToString(Static.FileInfo.SelectPosition).PadLeft(3, '0') + ".PTP"
                : Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + ".PTP";
            Project Pr = new Project(Name);
            Pr.name = MSG1.name.ToList();
            Pr.msg = MSG1.msg.ToList();
            Pr.SaveProject();
        }

        private void open_file()
        {
            OVMW.openfile = true;
            Static.FileInfo.SourceFileName = Path.GetFileName(Static.FileInfo.OpenFullFileName);

            MSG1.ParseMSG1(Static.FileInfo.OpenFullFileName, false);
            MSG1.UpdateString();

            set_title();
        }

        private void set_title()
        {
            if (Static.FileInfo.SelectPosition >= 0)
            {
                this.Title = "Persona Text Editor - [" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + " - " + Convert.ToString(Static.FileInfo.SelectPosition).PadLeft(3, '0') + "]";
            }
            else
            {
                this.Title = "Persona Font Editor - [" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "]";
            }
        }

        private void SaveNewMSG1_Click(object sender, RoutedEventArgs e)
        {
            using (MemoryStream MS = MSG1.GetNewMSG1)
            {
                string Name = Static.FileInfo.SelectPosition >= 0 ? Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + " - " + Convert.ToString(Static.FileInfo.SelectPosition).PadLeft(3, '0') + "_NEW.MSG1"
                : Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.MSG1";
                MS.SaveToFile(Name);
            }
        }

        private void SaveNewFile_Click(object sender, RoutedEventArgs e)
        {
            FileInfo[] FI = new DirectoryInfo(Path.GetDirectoryName(Static.FileInfo.OpenFullFileName)).GetFiles(Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + ".*");
            foreach (var a in FI)
            {
                if (a.Extension == ".BMD")
                {
                    MemoryStream MS = MSG1.GetNewMSG1;
                    Util.SaveToBMD(MS, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.BMD");
                }
                else if (a.Extension == ".PM1")
                {
                    MemoryStream MS = MSG1.GetNewMSG1;
                    Util.SaveToPM1(MS, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Static.FileInfo.SourceFileName, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.PM1");
                }
            }
        }

        private void MW_Drop(object sender, DragEventArgs e)
        {

            var a = ((DataObject)e.Data).GetFileDropList();
            var b = a.Count > 0 ? a[0] : null;
            if (!string.IsNullOrEmpty(b))
            {
                Static.FileInfo.OpenFullFileName = b;
                open();
            }
        }
    }

    class ObservableVariableMainWindow : INotifyPropertyChanged
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