using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Dynamic;
using System.Windows.Data;
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
    class Set
    {
        #region ElementClass
        public class ElementBoolean
        {
            public ElementBooleanNames Name { get; set; }
            public bool Value { get; set; }
            public bool Really { get; set; } = false;
            public ElementBoolean(ElementBooleanNames Name, bool Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        public class ElementString
        {
            public ElementStringNames Name { get; set; }
            public string Value { get; set; }
            public bool Really { get; set; } = false;
            public ElementString(ElementStringNames Name, string Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        public class ElementInteger
        {
            public ElementIntegerNames Name { get; set; }
            public int Value { get; set; }
            public bool Really { get; set; } = false;
            public ElementInteger(ElementIntegerNames Name, int Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        public class ElementDouble
        {
            public ElementDoubleNames Name { get; set; }
            public double Value { get; set; }
            public bool Really { get; set; } = false;
            public ElementDouble(ElementDoubleNames Name, double Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        public class ElementColor
        {
            public ElementColorNames Name { get; set; }
            public Color Value { get; set; }
            public bool Really { get; set; } = false;
            public ElementColor(ElementColorNames Name, Color Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }
        #endregion ElementClass

        #region Enumeration
        public enum ElementBooleanNames
        {
            ViewVisualizer,
            ViewPrefixPostfix,
            IsLittleEndian
        }

        public enum ElementStringNames
        {
            SelectedBackground,
            SelectedBackgroundVisual
        }

        public enum ElementIntegerNames
        {
            Width, Height,
            X, Y
        }

        public enum ElementDoubleNames
        {
            GlyphScale
        }

        public enum ElementColorNames
        {
            Color, BackgroundColor
        }

        #endregion Enumeration

        public class ListOfElements
        {
            public string Name { get; set; } = "";
            public List<ListOfElements> Nest { get; set; } = new List<ListOfElements>();

            List<ElementBoolean> ListBoolean = new List<ElementBoolean>();
            List<ElementString> ListString = new List<ElementString>();
            List<ElementInteger> ListInteger = new List<ElementInteger>();
            List<ElementDouble> ListDouble = new List<ElementDouble>();
            List<ElementColor> ListColor = new List<ElementColor>();

            #region Get
            public bool Get(ElementBooleanNames Name)
            {
                return ListBoolean.Find(x => x.Name == Name).Value;
            }

            public string Get(ElementStringNames Name)
            {
                return ListString.Find(x => x.Name == Name).Value;
            }

            public int Get(ElementIntegerNames Name)
            {
                return ListInteger.Find(x => x.Name == Name).Value;
            }

            public double Get(ElementDoubleNames Name)
            {
                return ListDouble.Find(x => x.Name == Name).Value;
            }

            public Color Get(ElementColorNames Name)
            {
                return ListColor.Find(x => x.Name == Name).Value;
            }
            #endregion Get

            #region Set
            public void Set(ElementBooleanNames Name, bool value)
            {
                if (ListBoolean.Find(x => x.Name == Name) == null)
                    ListBoolean.Add(new ElementBoolean(Name, value));
                else ListBoolean.Find(x => x.Name == Name).Value = value;
            }

            public void Set(ElementStringNames Name, string value)
            {
                if (ListString.Find(x => x.Name == Name) == null)
                    ListString.Add(new ElementString(Name, value));
                else ListString.Find(x => x.Name == Name).Value = value;
            }

            public void Set(ElementIntegerNames Name, int value)
            {
                if (ListInteger.Find(x => x.Name == Name) == null)
                    ListInteger.Add(new ElementInteger(Name, value));
                else ListInteger.Find(x => x.Name == Name).Value = value;
            }

            public void Set(ElementDoubleNames Name, double value)
            {
                if (ListDouble.Find(x => x.Name == Name) == null)
                    ListDouble.Add(new ElementDouble(Name, value));
                else ListDouble.Find(x => x.Name == Name).Value = value;
            }

            public void Set(ElementColorNames Name, Color value)
            {
                if (ListColor.Find(x => x.Name == Name) == null)
                    ListColor.Add(new ElementColor(Name, value));
                else ListColor.Find(x => x.Name == Name).Value = value;
            }
            #endregion Set

            void Set(string Name, string Value)
            {
                int a = 0;
                int b = 0;
                int c = 0;
                int d = 0;
                int e = 0;

                bool ret = true;

                try { a = (int)Enum.Parse(typeof(ElementBooleanNames), Name); }
                catch { ret = false; }
                finally
                {
                    if (ret)
                        Set((ElementBooleanNames)a, bool.Parse(Value));
                    ret = true;
                }

                try { b = (int)Enum.Parse(typeof(ElementStringNames), Name); }
                catch { ret = false; }
                finally
                {
                    if (ret)
                        Set((ElementStringNames)b, Value);
                    ret = true;
                }

                try { c = (int)Enum.Parse(typeof(ElementIntegerNames), Name); }
                catch { ret = false; }
                finally
                {
                    if (ret)
                        Set((ElementIntegerNames)c, int.Parse(Value));
                    ret = true;
                }

                try { d = (int)Enum.Parse(typeof(ElementDoubleNames), Name); }
                catch { ret = false; }
                finally
                {
                    if (ret)
                        Set((ElementDoubleNames)d, double.Parse(Value));
                    ret = true;
                }

                try { e = (int)Enum.Parse(typeof(ElementColorNames), Name); }
                catch { ret = false; }
                finally
                {
                    if (ret)
                        Set((ElementColorNames)e, (Color)ColorConverter.ConvertFromString(Value));
                    ret = true;
                }
            }

            public void Save(XElement current)
            {
                foreach (var a in ListBoolean)
                    current.Add(new XElement(a.Name.ToString(), a.Value.ToString()));
                foreach (var a in ListString)
                    current.Add(new XElement(a.Name.ToString(), a.Value.ToString()));
                foreach (var a in ListInteger)
                    current.Add(new XElement(a.Name.ToString(), a.Value.ToString()));
                foreach (var a in ListDouble)
                    current.Add(new XElement(a.Name.ToString(), a.Value.ToString()));
                foreach (var a in ListColor)
                    current.Add(new XElement(a.Name.ToString(), a.Value.ToString()));
                foreach (var a in Nest)
                {
                    XElement New = new XElement(a.Name);
                    current.Add(New);
                    a.Save(New);
                }
            }

            public void Load(XElement current)
            {
                if (current.Name == Name)
                {
                    foreach (var a in current.Elements())
                    {
                        if (a.Elements().Count() == 0)
                        {
                            Set(a.Name.LocalName.ToString(), a.Value);
                        }
                        else
                        {
                            Nest.Find(x => x.Name == a.Name.LocalName)?.Load(a);
                        }
                    }
                }
            }
        }

        public void Save(string FileName)
        {
            try
            {
                XDocument XDoc = new XDocument();
                XElement current = new XElement(Current.Name);
                XDoc.Add(current);
                Current.Save(current);

                XDoc.Save(FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void Load(string FileName)
        {
            try
            {
                XDocument XDoc = XDocument.Load(FileName);
                foreach (var a in XDoc.Elements())
                    Current.Load(a);
            }
            catch (FileNotFoundException) { }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public ListOfElements Current = new ListOfElements();
    }

    class Settings : Set
    {
        public class EmptyClass : Set
        {
            public event ColorChangedEventHandler ColorChanged;

            public class TextClass : Set
            {
                public event ColorChangedEventHandler ColorChanged;

                public TextClass(string Name, int X, int Y, Color Color)
                {
                    this.X = X;
                    this.Y = Y;
                    this.Color = Color;
                    Current.Name = Name;
                }

                public int X
                {
                    get { return Current.Get(ElementIntegerNames.X); }
                    set { Current.Set(ElementIntegerNames.X, value); }
                }

                public int Y
                {
                    get { return Current.Get(ElementIntegerNames.Y); }
                    set { Current.Set(ElementIntegerNames.Y, value); }
                }

                public Color Color
                {
                    get { return Current.Get(ElementColorNames.Color); }
                    set
                    {
                        Current.Set(ElementColorNames.Color, value);
                        ColorChanged?.Invoke(value);
                    }
                }
            }

            public TextClass Text { get; set; } = new TextClass("Text", 70, 50, Colors.Black);

            public TextClass Name { get; set; } = new TextClass("Name", 0, 0, Colors.Black);

            public EmptyClass()
            {
                GlyphScale = 1;
                Width = 640;
                Height = 200;
                BackgroundColor = Colors.White;

                Current.Nest.Add(Text.Current);
                Current.Nest.Add(Name.Current);
                Current.Name = "Empty";
            }

            public double GlyphScale
            {
                get { return Current.Get(ElementDoubleNames.GlyphScale); }
                set { Current.Set(ElementDoubleNames.GlyphScale, value); }
            }

            public int Width
            {
                get { return Current.Get(ElementIntegerNames.Width); }
                set { Current.Set(ElementIntegerNames.Width, value); }
            }

            public int Height
            {
                get { return Current.Get(ElementIntegerNames.Height); }
                set { Current.Set(ElementIntegerNames.Height, value); }
            }

            public Color BackgroundColor
            {
                get { return Current.Get(ElementColorNames.Color); }
                set
                {
                    Current.Set(ElementColorNames.Color, value);
                    ColorChanged?.Invoke(value);
                }
            }
        }

        public class MiscClass : Set
        {
            public MiscClass()
            {
                IsLittleEndian = true;

                Current.Name = "Misc";
            }

            public bool IsLittleEndian
            {
                get { return Current.Get(ElementBooleanNames.IsLittleEndian); }
                set { Current.Set(ElementBooleanNames.IsLittleEndian, value); }
            }
        }

        void Init()
        {
            ViewVisualizer = true;
            ViewPrefixPostfix = true;
            SelectedBackground = "Empty";
            SelectedBackgroundVisual = "Empty";

            Current.Nest.Add(Empty.Current);
            Current.Nest.Add(Misc.Current);
            Current.Name = "Settings";
        }

        public void CopyTo(Settings dest)
        {
           
        }

        public Settings()
        {
            Init();
        }

        public Settings(string FileName)
        {
            Init();
            Load(FileName);
        }

        public EmptyClass Empty { get; set; } = new EmptyClass();

        public MiscClass Misc { get; set; } = new MiscClass();

        public bool ViewVisualizer
        {
            get { return Current.Get(ElementBooleanNames.ViewVisualizer); }
            set { Current.Set(ElementBooleanNames.ViewVisualizer, value); }
        }

        public string SelectedBackground
        {
            get { return Current.Get(ElementStringNames.SelectedBackground); }
            set { Current.Set(ElementStringNames.SelectedBackground, value); }
        }

        public string SelectedBackgroundVisual
        {
            get { return Current.Get(ElementStringNames.SelectedBackgroundVisual); }
            set { Current.Set(ElementStringNames.SelectedBackgroundVisual, value); }
        }

        public bool ViewPrefixPostfix
        {
            get { return Current.Get(ElementBooleanNames.ViewPrefixPostfix); }
            set { Current.Set(ElementBooleanNames.ViewPrefixPostfix, value); }
        }
    }
}