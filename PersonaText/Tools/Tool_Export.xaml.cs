using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Diagnostics;

namespace PersonaText
{
    public partial class Tool_Export : Window
    {
        public ObservableVariableExport OVE = new ObservableVariableExport();
        public Thread thread;

        public Tool_Export()
        {
            InitializeComponent();
            DataContext = OVE;
            thread = new Thread(OVE.Thread_Work);
            thread.IsBackground = true;
            OVE.timer = new System.Timers.Timer(500);
            OVE.timer.Elapsed += OVE.Timer_Elapsed;
        }

        private void ThreadStart()
        {
            Grid1.Visibility = Visibility.Collapsed;
            Grid2.Visibility = Visibility.Visible;
            
            OVE.SaveMSG1 = ExportMSG1.IsChecked ?? false;
            thread.Start();
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = "d:\\PS2\\Other\\Shin Megami Tensei - Persona 4 (USA)\\Game";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                getlist(fbd.SelectedPath, ref OVE.FileList);

                ThreadStart();
            }
        }

        private void getlist(string path, ref List<string> FL)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            FileInfo[] info = dirInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            foreach (FileInfo f in info)
            {
                FL.Add(f.FullName);
            }
        }

        private void Files_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                foreach (var str in ofd.FileNames)
                {
                    OVE.FileList.Add(str);
                }

                ThreadStart();
            }
        }

        private void PB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OVE.progressbar_total == 1001)
            {
                this.Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            OVE.Abort_total = true;
            OVE.Abort = true;
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            OVE.Abort = true;
        }
    }

    public class ObservableVariableExport : INotifyPropertyChanged
    {
        public bool Abort_total = false;
        public bool Abort = false;
        public bool SaveMSG1 = false;

        public MSG1 MSG1 = new MSG1();
        public List<string> FileList = new List<string>();
        public List<MemoryStream> LMS = new List<MemoryStream>();
        public List<long> MSG1Position = new List<long>();

        private long FS_getLMS_length = 0;
        private long FS_getLMS_pos = 0;

        private int FileList_count = 0;
        private int FileList_count2 = 0;

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

        private int _progressbar_total = 0;
        public int progressbar_total
        {
            get { return _progressbar_total; }
            set
            {
                if (value != _progressbar_total)
                {
                    _progressbar_total = value;
                    Notify("progressbar_total");
                }
            }
        }

        private string _progressbar_text = "";
        public string progressbar_text
        {
            get { return _progressbar_text; }
            set
            {
                if (value != _progressbar_text)
                {
                    _progressbar_text = value;
                    Notify("progressbar_text");
                }
            }
        }

        private string _progressbar_total_text = "";
        public string progressbar_total_text
        {
            get { return _progressbar_total_text; }
            set
            {
                if (value != _progressbar_total_text)
                {
                    _progressbar_total_text = value;
                    Notify("progressbar_total_text");
                }
            }
        }

        Stopwatch sw = new Stopwatch();

        public System.Timers.Timer timer;

        public void Thread_Work()
        {
            timer.Start();
            for (int i = 0; i < FileList.Count; i++)
            {
                if (Abort_total)
                {
                    progressbar_total = 1001;
                    return;
                }

                FileList_count2 = i + 1;

                LMS.Clear();
                MSG1Position.Clear();

                FileInfo FI = new FileInfo(FileList[i]);
                if (FI.Extension != ".MSG1" & FI.Extension != ".txt")
                {
                    byte[] buffer = new byte[4];
                    using (FileStream FileStream = new FileStream(FileList[i], FileMode.Open, FileAccess.Read))
                    {
                        FileStream.Position = 8;
                        FileStream.Read(buffer, 0, 4);
                    }

                    string FileType = System.Text.Encoding.Default.GetString(buffer);

                    if (FileType == "PMD1")
                    {
                        LMS.Add(MSG1.GetMSG1fromPMD1(FileList[i]));
                    }
                    else if (FileType == "FLW0")
                    {
                        LMS.Add(MSG1.GetMSG1fromFLW0(FileList[i]));
                    }
                    else if (FileType == "MSG1")
                    {
                        LMS.Add(MSG1.GetMSG1fromFile(FileList[i], 0));
                    }
                    else
                    {
                        Thread thread_getLMS = new Thread(new ParameterizedThreadStart(Thread_getLMS));
                        thread_getLMS.IsBackground = true;
                        thread_getLMS.Start(FileList[i]);
                        thread_getLMS.Join();

                        foreach (var Position in MSG1Position)
                        {
                            LMS.Add(MSG1.GetMSG1fromFile(FileList[i], Position));
                        }
                    }
                }

                if (Abort_total)
                {
                    progressbar_total = 1001;
                    return;
                }
                MSG1.ParseMSG1(FileList[i], LMS, SaveMSG1);
            }
            progressbar_total = 1001;
            timer.Stop();
        }

        public void Thread_getLMS(object obj)
        {
            Abort = false;
            sw.Reset();
            sw.Start();

            string FileName = obj as string;

            Text Text = new PersonaText.Text();

            byte[] buffer = new byte[4];

            FileStream FS_getLMS = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            byte[] Bytes = new byte[] { 0x4D, 0x53, 0x47, 0x31, 0x00, 0x00, 0x00, 0x00 };

            FS_getLMS_length = FS_getLMS.Length;
            while (FS_getLMS.Position != FS_getLMS.Length)
            {
                if (Abort)
                {
                    sw.Stop();
                    FS_getLMS.Close();
                    progressbar = 1001;
                    return;
                }

                FS_getLMS_pos = FS_getLMS.Position;
                if (FindPosition(ref FS_getLMS, Bytes))
                {
                    MSG1Position.Add(FS_getLMS.Position - 16);
                }
            }

            sw.Stop();
            FS_getLMS.Close();
            FS_getLMS_length = 0;
            progressbar = 1001;
            progressbar_text = "";
        }

        private bool FindPosition(ref FileStream FS, byte[] Bytes)
        {
            if (Bytes.Length != 0)
            {
                if (FS.CanRead)
                {
                    if (FS.ReadByte() == Bytes[0])
                    {
                        return FindPosition(ref FS, Bytes.Skip(1).ToArray());
                    }
                    else
                    {
                        return false;
                    }
                }
                else { return false; }
            }
            else { return true; }
        }

        public void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (FS_getLMS_length != 0)
            {
                progressbar = (int)Math.Round((decimal)(FS_getLMS_pos * 1000) / FS_getLMS_length);
                double t = sw.Elapsed.TotalSeconds;
                if (progressbar > 0)
                {
                    progressbar_text = Convert.ToString(Math.Round(((t * 1001) / progressbar) - t)) + " s";
                }
            }

            if (FileList_count2 != FileList_count)
            {
                FileList_count = FileList_count2;

                progressbar_total = (FileList_count * 1000) / FileList.Count;
                progressbar_total_text = Convert.ToString(progressbar_total * 100 / 1001) + " %";
            }
        }
    }
}
