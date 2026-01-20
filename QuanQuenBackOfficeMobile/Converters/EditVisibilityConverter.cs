using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class EditVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;
            var itemId = values[0]?.ToString();
            var editingId = values[1]?.ToString();

            return !string.IsNullOrEmpty(editingId) && itemId == editingId;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

