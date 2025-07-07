using System.Globalization;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public static BoolToStatusConverter Instance { get; } = new BoolToStatusConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOpen)
            {
                return isOpen ? "Đang Hoạt Động" : "Đã Đóng Cửa";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 