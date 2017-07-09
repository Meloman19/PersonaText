using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Controls;

namespace PersonaText
{
    class Variable
    {
        double _Value = 0;

        public double Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                ValueChanged?.Invoke(value);
            }
        }

        public event WidthChangedEventHandler ValueChanged;
    }

    static class Static
    {
        public static string datapath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";
        public static string BackgroundPath = datapath + "\\background\\";
        public static int SUM = 0;
        public static bool ProjectSave = false;
        public static class FileInfo
        {
            public static string SourceFileName = "";
            public static string OpenFullFileName = "";
            public static long SelectPosition = -1;
        }

        public static Settings Setting = new Settings(@"PersonaText.xml");
        public static class FontMap
        {
            public static CharList old_char = new CharList();
            public static CharList new_char = new CharList();
            public static List<glyphYshift> char_shift = new List<glyphYshift>();
        }
        public static BackgroundImage BackImage = new BackgroundImage();
        public static BackgroundImage BackImageVisual = new BackgroundImage();
        public static Variable Width = new Variable();
        public static Variable VisualWidth = new Variable();
    }

    public partial class MainWindow : Window
    {
        void SetEvents()
        {

        }

        private void BackgroundImage_Changed()
        {
            Resources["TextXStart"] = Static.BackImage.textStartX;
            Resources["TextYStart"] = Static.BackImage.textStartY;
            Resources["NameXStart"] = Static.BackImage.nameStartX;
            Resources["NameYStart"] = Static.BackImage.nameStartY;
            Resources["Back"] = Static.BackImage.Image;
        }

        BindingList<string> Backgrounds = new BindingList<string>();

        MSG1 MSG1;
        ObservableVariableMainWindow OVMW = new ObservableVariableMainWindow();

        public MainWindow()
        {
            SetEvents();
            Static.BackImage.BackgroundImageChanged += BackgroundImage_Changed;
            MSG1 = new MSG1();
            InitializeComponent();
            Backgrounds.Add("Empty");

            DataContext = OVMW;
            SelectBack.DataContext = Backgrounds;
            ScrollViewer.DataContext = MSG1;
            mainmenu.DataContext = OVMW;

            if (Directory.Exists((Static.BackgroundPath)))
            {
                DirectoryInfo DI = new DirectoryInfo(Static.BackgroundPath);
                foreach (var file in DI.GetFiles(@"*.png"))
                    Backgrounds.Add(file.Name);
            }


            OpenFont();

            ViewVisualizer.IsChecked = Static.Setting.ViewVisualizer;
            if (ViewVisualizer.IsChecked) { Resources["ViewVisualizer"] = Visibility.Visible; }
            else { Resources["ViewVisualizer"] = Visibility.Collapsed; }
            ViewPrefixPostfix.IsChecked = Static.Setting.ViewPrefixPostfix;
            if (ViewPrefixPostfix.IsChecked) { Resources["ViewPrefixPostfix"] = Visibility.Visible; }
            else { Resources["ViewPrefixPostfix"] = Visibility.Collapsed; }

            SelectBack.SelectedIndex = Backgrounds.IndexOf(Static.Setting.SelectedBackground) == -1 ? Backgrounds.IndexOf("Empty") : Backgrounds.IndexOf(Static.Setting.SelectedBackground);
        }

        void OpenFont()
        {
            if (File.Exists(Static.datapath + "OLD.TXT"))
            {
                Static.FontMap.old_char.ReadFNMP(Static.datapath + "OLD.TXT");
            }
            if (File.Exists(Static.datapath + "NEW.TXT"))
            {
                Static.FontMap.new_char.ReadFNMP(Static.datapath + "NEW.TXT");
            }
            if (File.Exists(Static.datapath + "OLD.FNT"))
            {
                Static.FontMap.old_char.ReadFONT(Static.datapath + "OLD.FNT");
            }
            if (File.Exists(Static.datapath + @"NEW.FNT"))
            {
                Static.FontMap.new_char.ReadFONT(Static.datapath + "NEW.FNT");
            }

            Static.FontMap.char_shift.ReadShift();

            Static.FontMap.old_char.List.Sort((a, b) => (a.Index.CompareTo(b.Index)));
            Static.FontMap.new_char.List.Sort((a, b) => (a.Index.CompareTo(b.Index)));
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

        #region View
        private void ViewVisualizer_Checked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewVisualizer = true;
            Resources["ViewVisualizer"] = Visibility.Visible;
        }

        private void ViewVisualizer_Unchecked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewVisualizer = false;
            Resources["ViewVisualizer"] = Visibility.Collapsed;
        }

        private void ViewPrefixPostfix_Checked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewPrefixPostfix = true;
            Resources["ViewPrefixPostfix"] = Visibility.Visible;
        }

        private void ViewPrefixPostfix_Unchecked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewPrefixPostfix = true;
            Resources["ViewPrefixPostfix"] = Visibility.Collapsed;
        }
        #endregion View

        #region Tools
        private void mm_tools_export_Click(object sender, RoutedEventArgs e)
        {
            ToolExport TE = new ToolExport();
            TE.Owner = this;
            this.Visibility = Visibility.Collapsed;
            TE.ShowDialog();
            this.Visibility = Visibility.Visible;
        }

        private void mm_tools_visual_Click(object sender, RoutedEventArgs e)
        {
            ToolVisual TV = new ToolVisual();
            TV.Owner = this;
            Hide();
            TV.ShowDialog();
            Show();
        }
        #endregion Tools

        #endregion MainMenu

        #region Old Font

        void OpenOldFONT(object sender, RoutedEventArgs e)
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
                Static.FontMap.old_char.Update();
            }
        }

        void SetOldChar(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet(Static.FontMap.old_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Static.FontMap.old_char.Update();
                    Static.FontMap.old_char.WriteFNMP(@"OLD.TXT");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void STold(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileNameWithoutExtension(Static.FileInfo.OpenFullFileName) + ".TXT";
            sfd.Filter = "Text (.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                MSG1.SaveAsText(sfd.FileName, "", 2);
            }
        }

        void CopyAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will overwrite all new text! Are you sure?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (MessageBox.Show("Really?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    foreach (var a in MSG1.msg)
                    {
                        foreach (var b in a.Strings)
                        {
                            b.New_string = b.Old_string;
                        }
                    }
                    foreach (var a in MSG1.name)
                    {
                        a.NewName = a.OldName;
                    }
                }
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
                Static.FontMap.new_char.Update();
            }
        }

        private void SetNewChar(object sender, RoutedEventArgs e)
        {
            Hide();

            try
            {
                CharSet CS = new CharSet(Static.FontMap.new_char);
                CS.Owner = this;
                if (CS.ShowDialog() == true)
                {
                    Static.FontMap.new_char.Update();
                    Static.FontMap.new_char.WriteFNMP(@"NEW.TXT");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Show();
        }

        private void SaveNewMSG1_Click(object sender, RoutedEventArgs e)
        {
            using (MemoryStream MS = MSG1.GetNewMSG1())
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
                if (a.Extension.ToUpper() == ".BMD")
                {
                    MemoryStream MS = MSG1.GetNewMSG1();
                    Util.SaveToBMD(MS, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.BMD");
                }
                else if (a.Extension.ToUpper() == ".PM1")
                {
                    MemoryStream MS = MSG1.GetNewMSG1();
                    Util.SaveToPM1(MS, a.FullName, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.PM1");
                }
                else if (a.Extension.ToUpper() == ".BF")
                {
                    MemoryStream MS = MSG1.GetNewMSG1();
                    Util.SaveToBF(MS, a.FullName, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.BF");
                }
            }
        }

        #endregion New Font

        #region MainWindow

        void MW_Loaded(object sender, RoutedEventArgs e)
        {
            open();
        }

        void MW_Closing(object sender, CancelEventArgs e)
        {
            Static.Setting.Save(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\PersonaText.xml");
            if (OVMW.openfile)
            {
                MessageBoxResult a = MessageBox.Show("Save Project?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (a == MessageBoxResult.Yes)
                {
                    save_project();
                }
            }
        }

        void MW_Drop(object sender, DragEventArgs e)
        {

            var a = ((DataObject)e.Data).GetFileDropList();
            var b = a.Count > 0 ? a[0] : null;
            if (!string.IsNullOrEmpty(b))
            {
                Static.FileInfo.OpenFullFileName = b;
                open();
            }
        }

        #endregion MainWindow

        void open()
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

        void open_project()
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

        void save_project()
        {
            string Name = Static.FileInfo.SelectPosition >= 0 ? Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + " - " + Convert.ToString(Static.FileInfo.SelectPosition).PadLeft(3, '0') + ".PTP"
                : Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + ".PTP";
            Project Pr = new Project(Name);
            Pr.name = MSG1.name.ToList();
            Pr.msg = MSG1.msg.ToList();
            Pr.SaveProject();
        }

        void open_file()
        {
            OVMW.openfile = true;
            Static.FileInfo.SourceFileName = Path.GetFileName(Static.FileInfo.OpenFullFileName);

            MSG1.ParseMSG1(Static.FileInfo.OpenFullFileName, false);

            set_title();
        }

        void set_title()
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

        void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Static.Width.Value = (e.NewSize.Width - 20) / 2;
        }

        private void SelectBack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Static.Setting.SelectedBackground = (sender as ComboBox).SelectedItem as string;
            if (Static.Setting.SelectedBackground == "Empty")
            { Static.BackImage.Update("Empty"); }
            else
            { Static.BackImage.Update(Static.BackgroundPath + Static.Setting.SelectedBackground); }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Setting Set = new PersonaText.Setting();
            if (Set.ShowDialog() == true)
            {
                if (Static.Setting.SelectedBackground == "Empty")
                { Static.BackImage.Update("Empty"); }
                else
                { Static.BackImage.Update(Static.BackgroundPath + Static.Setting.SelectedBackground); }
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