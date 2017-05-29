using System;
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
    public class CharacterIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int Index = (int)value[0];
            MainWindow MW = (MainWindow)value[1];
            if (MW.MSG1.name.Count > Index)
            {
                return MW.MSG1.name[Index].Old_Name;
            }
            else { return "<NO_NAME>"; }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterHeigth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double Height = (double)value;
            return Height * 0.36364;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double a = (double)value;
            return a * 0.9375;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterXpos : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            double pos = (double)value[0];
            double heigth = (double)value[1];
            if (parameter.ToString() == "Name")
            {
                heigth = heigth * 0.9375;
            }
            else
            {
                heigth = heigth * 0.36364 * 0.9375;
            }

            return (pos / 32) * heigth;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterYpos : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double pos = (double)value[0];
            double heigth = (double)value[1];
            if (parameter.ToString() == "Name")
            {
            }
            else
            {
                heigth = heigth * 0.36364;
            }
            return (pos / 32) * heigth;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}