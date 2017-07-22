using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Controls;
using PersonaTextLib.Extensions;
using PersonaTextLib.Types;
using PersonaTextLib;

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
        public static class Paths
        {
            public static string ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";
        }

        //public static bool IsLittleEndian = true;
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
            public static CharList old_char = new CharList(Static.datapath, "OLD");
            public static CharList new_char = new CharList(Static.datapath, "NEW");
            public static List<glyphYshift> char_shift = new List<glyphYshift>();
        }
        public static BackgroundImage BackImageVisual = new BackgroundImage();
        public static Variable Width = new Variable();
        public static Variable VisualWidth = new Variable();
    }

    public partial class MainWindow : Window
    {
        BindingList<string> ComboBoxBackgrounds = new BindingList<string>() { "Empty" };

        ObservableVariableMainWindow OVMW = new ObservableVariableMainWindow(new BackgroundImage());

        public MainWindow()
        {
            InitializeComponent();

            ScrollViewer.DataContext = OVMW.VisualMSG1;
            MW.DataContext = OVMW;
            SelectBack.DataContext = ComboBoxBackgrounds;


            if (Directory.Exists((Static.BackgroundPath)))
            {
                DirectoryInfo DI = new DirectoryInfo(Static.BackgroundPath);
                foreach (var file in DI.GetFiles(@"*.png"))
                    ComboBoxBackgrounds.Add(file.Name);
            }

            OpenFont();

            ViewVisualizer.IsChecked = Static.Setting.ViewVisualizer;
            //if (ViewVisualizer.IsChecked) { Resources["ViewVisualizer"] = Visibility.Visible; }
            //else { Resources["ViewVisualizer"] = Visibility.Collapsed; }
            ViewPrefixPostfix.IsChecked = Static.Setting.ViewPrefixPostfix;
            //if (ViewPrefixPostfix.IsChecked) { Resources["ViewPrefixPostfix"] = Visibility.Visible; }
            //else { Resources["ViewPrefixPostfix"] = Visibility.Collapsed; }

            SelectBack.SelectedIndex = ComboBoxBackgrounds.IndexOf(Static.Setting.SelectedBackground) == -1 ? ComboBoxBackgrounds.IndexOf("Empty") : ComboBoxBackgrounds.IndexOf(Static.Setting.SelectedBackground);
        }

        void OpenFont()
        {
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
            OVMW.ViewVisualizer = Visibility.Visible;
        }

        private void ViewVisualizer_Unchecked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewVisualizer = false;
            OVMW.ViewVisualizer = Visibility.Collapsed;
        }

        private void ViewPrefixPostfix_Checked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewPrefixPostfix = true;
            OVMW.ViewPrefixPostfix = Visibility.Visible;
        }

        private void ViewPrefixPostfix_Unchecked(object sender, RoutedEventArgs e)
        {
            Static.Setting.ViewPrefixPostfix = false;
            OVMW.ViewPrefixPostfix = Visibility.Collapsed;
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

                if (Static.FontMap.old_char.ReadFONT(path))
                {
                    File.Copy(path, @"OLD.FNT", true);
                    Static.FontMap.old_char.Update();
                }
            }
        }

        void SetOldChar(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet();
                CS.chlt = Static.FontMap.old_char;
                if (CS.ShowDialog() == true)
                {
                    Static.FontMap.old_char.Update();
                    Static.FontMap.old_char.Save(@"OLD.TXT");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Save to
        void STold(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileNameWithoutExtension(Static.FileInfo.OpenFullFileName) + ".TXT";
            sfd.Filter = "Text (.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                //  MSG1.SaveAsText(sfd.FileName, "", 2);
            }
        }

        void SavetoMSG_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion Save to

        void CopyAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will overwrite all new text! Are you sure?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (MessageBox.Show("Really?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    foreach (var a in OVMW.MSG1.msg)
                    {
                        foreach (var b in a.Strings)
                        {
                            b.NewString.Text = b.OldString.Text;
                        }
                    }
                    foreach (var a in OVMW.MSG1.name)
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

                if (Static.FontMap.new_char.ReadFONT(path))
                {
                    File.Copy(path, @"NEW.FNT", true);
                    Static.FontMap.new_char.Update();
                }
            }
        }

        private void SetNewChar(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet();
                CS.chlt = Static.FontMap.new_char;
                if (CS.ShowDialog() == true)
                {
                    Static.FontMap.new_char.Update();
                    Static.FontMap.old_char.Save(@"NEW.TXT");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveNewMSG1_Click(object sender, RoutedEventArgs e)
        {
            using (MemoryStream MS = OVMW.VisualMSG1.GetNewMSG1())
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
                    MemoryStream MS = OVMW.VisualMSG1.GetNewMSG1();
                    Util.SaveToBMD(MS, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.BMD");
                }
                else if (a.Extension.ToUpper() == ".PM1")
                {
                    MemoryStream MS = OVMW.VisualMSG1.GetNewMSG1();
                    Util.SaveToPM1(MS, a.FullName, Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + "_NEW.PM1");
                }
                else if (a.Extension.ToUpper() == ".BF")
                {
                    MemoryStream MS = OVMW.VisualMSG1.GetNewMSG1();
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
            ScrollViewer.DataContext = null;
            OVMW.openfile = true;
            if (OVMW.MSGProject.OpenProject(Static.FileInfo.OpenFullFileName))
            {
                Static.FileInfo.SourceFileName = OVMW.MSGProject.SourceFileName;
                Static.FileInfo.SelectPosition = OVMW.MSGProject.SelectPosition;
                set_title();
            }
            ScrollViewer.DataContext = OVMW.VisualMSG1;
        }

        void save_project()
        {
            string Name = Static.FileInfo.SelectPosition >= 0 ? Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + " - " + Convert.ToString(Static.FileInfo.SelectPosition).PadLeft(3, '0') + ".PTP"
                : Path.GetDirectoryName(Static.FileInfo.OpenFullFileName) + "\\" + Path.GetFileNameWithoutExtension(Static.FileInfo.SourceFileName) + ".PTP";
            OVMW.MSGProject.SourceFileName = Static.FileInfo.SourceFileName;
            OVMW.MSGProject.SelectPosition = Static.FileInfo.SelectPosition;
            OVMW.MSGProject.SaveProject(Name);
        }

        void open_file()
        {
            OVMW.openfile = true;
            Static.FileInfo.SourceFileName = Path.GetFileName(Static.FileInfo.OpenFullFileName);

            OVMW.VisualMSG1.Load(Static.FileInfo.OpenFullFileName, Static.Setting.Misc.IsLittleEndian);

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
            OVMW.BackImage.RealWidth = (e.NewSize.Width - 20) / 2;
        }

        private void SelectBack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Static.Setting.SelectedBackground = (sender as ComboBox).SelectedItem as string;
            if (Static.Setting.SelectedBackground == "Empty")
            { OVMW.BackImage.Update("Empty"); }
            else
            { OVMW.BackImage.Update(Static.BackgroundPath + Static.Setting.SelectedBackground); }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Setting Set = new PersonaText.Setting();
            if (Set.ShowDialog() == true)
            {
                if (Static.Setting.SelectedBackground == "Empty")
                { OVMW.BackImage.Update("Empty"); }
                else
                { OVMW.BackImage.Update(Static.BackgroundPath + Static.Setting.SelectedBackground); }
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

        BackgroundImage _BackImage;
        public BackgroundImage BackImage
        {
            get { return _BackImage; }
            set
            {
                if (value != _BackImage)
                {
                    _BackImage = value;
                    Notify("BackImage");
                }
            }
        }

        public PersonaFileTypes.MSG1 MSG1 = new PersonaFileTypes.MSG1(Static.Setting.Misc.IsLittleEndian, Static.FontMap.old_char, Static.FontMap.new_char);

        public ObservableVariableMainWindow(BackgroundImage BackImage)
        {
            this.BackImage = BackImage;
            VisualMSG1 = new VisualMSG1(MSG1, this.BackImage);
            MSGProject = new Project(MSG1);
        }

        public Project MSGProject { get; private set; }

        public VisualMSG1 VisualMSG1 { get; private set; }

        private Visibility _ViewVisualizer = Visibility.Visible;
        public Visibility ViewVisualizer
        {
            get { return _ViewVisualizer; }
            set
            {
                if (value != _ViewVisualizer)
                {
                    _ViewVisualizer = value;
                    Notify("ViewVisualizer");
                }
            }
        }

        private Visibility _ViewPrefixPostfix = Visibility.Visible;
        public Visibility ViewPrefixPostfix
        {
            get { return _ViewPrefixPostfix; }
            set
            {
                if (value != _ViewPrefixPostfix)
                {
                    _ViewPrefixPostfix = value;
                    Notify("ViewPrefixPostfix");
                }
            }
        }
    }
}