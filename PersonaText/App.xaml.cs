using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PersonaText
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow MW = new MainWindow();

            if (e.Args.Length > 0)
            {
                Static.FileInfo.OpenFullFileName = e.Args[0];
            }

            MW.Show();
        }
    }
}
