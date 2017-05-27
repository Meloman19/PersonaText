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
using System.ComponentModel;
using System.Timers;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;

namespace PersonaText
{
    public partial class GetMSG1from : Window
    {
        public bool Select = false;
        public int Select_Index = -1;

        public MemoryStream MS;
        public ObservableVariableParseMSG1 OVP = new ObservableVariableParseMSG1();
        public Thread thread;

        public GetMSG1from()
        {
            InitializeComponent();
            thread = new Thread(OVP.Thread_Work);
            thread.IsBackground = true;
            DataContext = OVP;
            OVP.timer.Elapsed += OVP.Timer_Elapsed;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OVP.progressbar == 1001)
            {
                if (OVP.Abort)
                {
                    this.Close();
                    return;
                }

                if (OVP.MSG1Position.Count == 1)
                {
                    MS = OVP.MSG1.GetMSG1fromFile(OVP.FileName, OVP.MSG1Position[0]);
                    this.Close();
                }
                else if (OVP.MSG1Position.Count > 1)
                {
                    Grid1.Visibility = Visibility.Collapsed;
                    Grid2.Visibility = Visibility.Visible;

                    Binding bind = new Binding("MSG1Position");
                    bind.Mode = BindingMode.OneWay;
                    listBox.SetBinding(ListBox.ItemsSourceProperty, bind);
                    Grid2.Height = 100;
                    Grid2.Width = 200;
                }
                else
                {
                    this.Close();
                    return;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            thread.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            OVP.Abort = true;
            thread.Join();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            OVP.Abort = true;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox temp = (ListBox)sender;
            long Position = (long)temp.SelectedItem;
            MS = OVP.MSG1.GetMSG1fromFile(OVP.FileName, Position);
            Select_Index = OVP.MSG1Position.IndexOf(Position);
            this.Close();
        }
    }

    public class ObservableVariableParseMSG1 : INotifyPropertyChanged
    {
        public MSG1 MSG1 = new MSG1();

        public bool Abort = false;

        FileStream FS;
        Stopwatch sw = new Stopwatch();

        private ObservableCollection<long> _MSG1Position = new ObservableCollection<long>();
        public ObservableCollection<long> MSG1Position
        {
            get { return _MSG1Position; }
            set
            {
                if (value != _MSG1Position)
                {
                    _MSG1Position = value;
                    Notify("MSG1Position");
                }
            }
        }

        public System.Timers.Timer timer = new System.Timers.Timer(500);

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

        private string _FileName = "";
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    Notify("title");
                }
            }
        }

        public MemoryStream MS;

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

        public void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (FS != null)
            {
                if (FS.Length != 0)
                {
                    progressbar = (int)Math.Round((decimal)(FS.Position * 1000) / FS.Length);
                    double t = sw.Elapsed.TotalSeconds;
                    if (progressbar > 0)
                    {
                        progressbar_text = Convert.ToString(Math.Round(((t * 1001) / progressbar) - t)) + " s";
                    }
                }
            }

        }

        public void Thread_Work()
        {
            sw.Start();
            timer.Start();

            Text Text = new PersonaText.Text();

            byte[] buffer = new byte[4];

            FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);


            byte[] Bytes = new byte[] { 0x4D, 0x53, 0x47, 0x31, 0x00, 0x00, 0x00, 0x00 };

            while (FS.Position != FS.Length)
            {
                if (Abort)
                {
                    FS.Close();
                    timer.Stop();
                    sw.Stop();
                    progressbar = 1001;
                    return;
                }

                if (FindPosition(ref FS, Bytes))
                {
                    MSG1Position.Add(FS.Position - 16);
                }
            }

            if (MSG1Position.Count == 1)
            {
                MS = new MemoryStream();
                FS.Position = MSG1Position.First() + 4;
                int Size = FS.ReadInt();
                FS.Position = MSG1Position.First();
                buffer = new byte[Size];
                FS.Read(buffer, 0, Size);
                MS.Write(buffer, 0, Size);
            }

            FS.Close();
            timer.Stop();
            sw.Stop();
            progressbar = 1001;
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
    }
}
