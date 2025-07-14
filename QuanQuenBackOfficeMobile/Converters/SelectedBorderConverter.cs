using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class SelectedBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return Color.FromArgb("#FF9900"); // Cam
            return Color.FromArgb("#CCCCCC"); // Xám nhạt
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
} 