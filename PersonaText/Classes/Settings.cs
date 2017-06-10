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
    public class Settings
    {
        public class CurrentSettings : Settings
        {
        }

        public static class DefaultSettings
        {
            
        }

        public class StringSettings
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public StringSettings(string Name, string Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        public List<StringSettings> _CurrentSettings { get; set; } = new List<StringSettings>();

        private void Initialization()
        {
            _CurrentSettings.Add(new StringSettings("GameType", "P4"));
        }

        public void CreateSettingsFile()
        {
            try
            {
                XDocument XDoc = new XDocument();
                XElement Setting = new XElement("Settings");
                XDoc.Add(Setting);
                XElement Strings = new XElement("Strings");
                Setting.Add(Strings);
                foreach (var a in _CurrentSettings) { Strings.Add(new XElement(a.Name, a.Value)); }

                XDoc.Save( Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\PersonaText.xml");
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
                                StringSettings temp = _CurrentSettings.Find(x => x.Name == c.Name);
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

        public string Get(string Name)
        {
            return _CurrentSettings.Find(x => x.Name == Name).Value;
        }

        public void Set(string Name, string Value)
        {
            StringSettings temp = _CurrentSettings.Find(x => x.Name == Name);
            if (temp != null)
            {
                temp.Value = Value;
                CreateSettingsFile();
            }
            else
            {
                MessageBox.Show("SetSetting fail");
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
                MessageBox.Show("Permissions problem: used default setting");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}