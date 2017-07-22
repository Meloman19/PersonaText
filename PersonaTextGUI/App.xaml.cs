using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace PersonaText
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 4)
            {
                if (e.Args[0] == "-o")
                {
                    if (File.Exists(e.Args[1]) & e.Args[2] == "-st")
                        OpenAndSaveAsTXT(e.Args[1], e.Args[3]);



                }
            }
            else
            {
                MainWindow MW = new MainWindow();

                if (e.Args.Length == 1)
                    if (File.Exists(e.Args[0]))
                        Static.FileInfo.OpenFullFileName = e.Args[0];



                MW.Show();
            }
        }

        void OpenAndSaveAsTXT(string fileopen, string filesave)
        {
            if (Path.GetExtension(fileopen).ToUpper() == ".PTP")
            {
            }
            else
            {
             //   MSG1 MSG1 = new MSG1(Static.IsLittleEndian);
              //  MSG1.ParseMSG1(fileopen, false);
            }
        }
    }
}
