using System.Globalization;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public static BoolToColorConverter Instance { get; } = new BoolToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOpen)
            {
                return isOpen ? Color.FromHex("#4CAF50") : Color.FromHex("#F44336"); // Green for open, Red for closed
            }
            return Color.FromHex("#9E9E9E"); // Gray for unknown
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 