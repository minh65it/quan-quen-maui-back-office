using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class NullOrEmptyToNAConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrWhiteSpace(str))
                return "N/A";
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 