using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace PersonaText
{
    public partial class MainWindow : Window
    {
        public MSG1 MSG1 = new MSG1();

        string Import_FileName = "";
        string Import_Path = "";
        Text Text = new Text();

        public List<fnmp> old_char = new List<fnmp>();
        public List<fnmp> new_char = new List<fnmp>();

        public MainWindow()
        {
            InitializeComponent();
            MSG1.msg.CollectionChanged += Msg_CollectionChanged;
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

        private void Msg_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string a = e.ToString();
            //throw new NotImplementedException();
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
            MSG1.openfile = true;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PMD files (*.PM1)|*.PM1";
            if (ofd.ShowDialog() == true)
            {
                MSG1.msg.Clear();

                Import_Path = ofd.FileName;
                Import_FileName = ofd.SafeFileName;
                MSG1.ParseMSG1(Import_Path);
                MSG1.ParseSTRINGs(old_char);
                this.Title = "Persona Font Editor - [" + Import_FileName + "]";
            }

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

        private void CharSet(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem temp = (MenuItem)sender;
                CharSet CS = new CharSet();
                CS.chlt = old_char;
                CS.Owner = this;
                CS.ShowDialog();
                if (CS.saving)
                {
                    if (temp.Name == "Old_Char_Set")
                    {
                        Text.WriteFNMP(@"OLD.TXT", ref old_char);
                        MSG1.ParseSTRINGs(old_char);
                    }
                    else if (temp.Name == "New_Char_Set")
                    {
                        Text.WriteFNMP(@"NEW.TXT", ref new_char);
                    }
                    else { throw new Exception("Char set error!"); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CSnew(object sender, RoutedEventArgs e)
        {
            try
            {
                CharSet CS = new CharSet();
                CS.chlt = new_char;
                CS.Owner = this;
                CS.ShowDialog();
                if (CS.saving)
                {
                    Text.WriteFNMP(@"NEW.TXT", ref new_char);
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
            TE.old_char = old_char;
            TE.Owner = this;
            TE.ShowDialog();
        }
    }
}