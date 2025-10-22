using System.Globalization;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                var hasItems = count > 0;
                if (parameter?.ToString() == "Inverse")
                {
                    return !hasItems;
                }
                return hasItems;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
