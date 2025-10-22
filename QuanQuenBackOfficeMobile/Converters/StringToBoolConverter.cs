using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.Converters
{
    // Converter để kiểm tra xem item có đang được edit không
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var editingId = value as string;  // ID của item đang edit
            var itemId = parameter as string;  // ID của item hiện tại
            
            // Trả về true nếu item hiện tại ĐANG được edit
            return !string.IsNullOrEmpty(editingId) && editingId == itemId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Converter để kiểm tra xem item có KHÔNG đang được edit không
    public class InverseStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var editingId = value as string;  // ID của item đang edit
            var itemId = parameter as string;  // ID của item hiện tại
            
            // Trả về true nếu:
            // - Không có item nào đang edit (editingId empty), HOẶC
            // - Item hiện tại KHÔNG phải item đang edit
            return string.IsNullOrEmpty(editingId) || editingId != itemId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}