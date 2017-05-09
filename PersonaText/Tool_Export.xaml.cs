using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ComponentModel;

namespace PersonaText
{
    /// <summary>
    /// Логика взаимодействия для Tool_Export.xaml
    /// </summary>
    public partial class Tool_Export : Window
    {
        public List<fnmp> old_char
        {
            get
            {
                return OVE.old_char;
            }
            set
            {
                OVE.old_char = value;
            }
        }

        public ObservableVariableExport OVE = new ObservableVariableExport();

        public Tool_Export()
        {
            InitializeComponent();
            DataContext = OVE;
            OVE.bgw.DoWork += new DoWorkEventHandler(OVE.worker_DoWork);
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Directory.CreateDirectory("EXPORT");

                getlist(fbd.SelectedPath, ref OVE.FileList);

                B1.Visibility = Visibility.Hidden;
                B2.Visibility = Visibility.Hidden;
                PB.Visibility = Visibility.Visible;

                OVE.bgw.RunWorkerAsync();
            }
        }

        private void getlist(string path, ref List<FileList> FL)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            FileInfo[] info = dirInfo.GetFiles("*.PM1");
            foreach (FileInfo f in info)
            {
                FL.Add(new PersonaText.FileList { FullName = f.FullName, Name = "EXPORT\\" + f.Name });
            }

            DirectoryInfo[] subDir = dirInfo.GetDirectories();
            foreach (DirectoryInfo d in subDir)
            {
                getlist(d.FullName, ref FL);
            }
        }

        private void Files_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "PMD files (*.PM1)|*.PM1";
            if (ofd.ShowDialog() == true)
            {
                foreach (var str in ofd.FileNames)
                {
                    OVE.FileList.Add(new PersonaText.FileList { FullName = str, Name = str });
                }
                B1.Visibility = Visibility.Hidden;
                B2.Visibility = Visibility.Hidden;
                PB.Visibility = Visibility.Visible;

                OVE.bgw.RunWorkerAsync();
            }
        }

        private void PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OVE.progressbar == 1001)
            {
                this.Close();
            }
        }
    }

    public class ObservableVariableExport : INotifyPropertyChanged
    {
        public List<fnmp> old_char = new List<fnmp>();
        MSG1 MSG1 = new MSG1();

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

        private int _progressbar = 0;
        public int progressbar
        {
            get { return _progressbar; }
            set
            {
                if (value != _progressbar)
                {
                    _progressbar = value;
                    Notify("progressbar");
                }
            }
        }

        public List<FileList> FileList = new List<FileList>();

        public BackgroundWorker bgw = new BackgroundWorker();

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < FileList.Count; i++)
            {
                progressbar = (int)Math.Round((decimal)(i * 1000) / FileList.Count);

                MSG1.msg.Clear();
                MSG1.ParseMSG1(FileList[i].FullName);
                MSG1.ParseSTRINGs(old_char);
                MSG1.SaveAsText(FileList[i].Name + ".txt");
            }

            progressbar = 1001;
        }


    }

    public class FileList
    {
        public string Name { get; set; }
        public string FullName { get; set; }
    }
}
