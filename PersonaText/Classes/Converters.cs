using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersonaText
{

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
            double pos = 0;
            double heigth = 0;
            if (value != null)
            {
                pos = (double)value[0];
                heigth = (double)value[1];
            }

            if (parameter != null)
            {
                if (parameter.ToString() == "Name")
                {
                    heigth = heigth * 0.9375;
                }
                else
                {
                    heigth = heigth * 0.36364 * 0.9375;
                }
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
            double pos = 0;
            double heigth = 0;
            if (value != null)
            {
                pos = (double)value[0];
                heigth = (double)value[1];
            }

            if (parameter != null)
            {
                if (parameter.ToString() == "Name")
                {
                }
                else
                {
                    heigth = heigth * 0.36364;
                }
            }
            return (pos / 32) * heigth;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CharacterIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int Index = (int)value;
            if (MSG.name.Count > Index)
            { return MSG.name[Index].Old_Name; }
            else
            { return "<NO_NAME>"; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}