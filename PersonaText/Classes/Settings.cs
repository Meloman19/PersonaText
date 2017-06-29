using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Data;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace PersonaText
{
    public enum GameType
    {
        P3,
        P4,
    }

    class Settings
    {
        public GameType GameType
        {
            get
            {
                string temp = CurrentSettings.Find(x => x.Name == "GameType").Value;
                if (String.Equals(temp, "P3"))
                {
                    return GameType.P3;
                }
                else
                {
                    return GameType.P4;
                }
            }
            set
            {
                if (value == GameType.P3)
                {
                    CurrentSettings.Find(x => x.Name == "GameType").Value = "P3";
                }
                else
                {
                    CurrentSettings.Find(x => x.Name == "GameType").Value = "P4";
                }
            }
        }

        private class StringSettings
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public StringSettings(string Name, string Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        private List<StringSettings> CurrentSettings { get; set; } = new List<StringSettings>();

        private void Initialization()
        {
            CurrentSettings.Add(new StringSettings("GameType", "P4"));
        }

        private void CreateSettingsFile()
        {
            try
            {
                XDocument XDoc = new XDocument();
                XElement Setting = new XElement("Settings");
                XDoc.Add(Setting);
                XElement Strings = new XElement("Strings");
                Setting.Add(Strings);
                foreach (var a in CurrentSettings) { Strings.Add(new XElement(a.Name, a.Value)); }

                XDoc.Save(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\PersonaText.xml");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OpenSettingsFile(XDocument XDoc)
        {
            foreach (var a in XDoc.Elements())
            {
                if (a.Name == "Settings")
                {
                    foreach (var b in a.Elements())
                    {
                        if (b.Name == "Strings")
                        {
                            foreach (var c in b.Elements())
                            {
                                StringSettings temp = CurrentSettings.Find(x => x.Name == c.Name);
                                if (temp != null)
                                {
                                    temp.Value = c.Value;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public Settings()
        {
            Initialization();

            try
            {
                XDocument XDoc = XDocument.Load(@"PersonaText.xml");
                OpenSettingsFile(XDoc);
            }
            catch (FileNotFoundException)
            {
                CreateSettingsFile();
            }
            catch (System.Security.SecurityException)
            {
                MessageBox.Show("Permissions problem: using default setting");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}