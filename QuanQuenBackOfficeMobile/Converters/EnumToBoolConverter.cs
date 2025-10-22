using System.Globalization;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var enumValue = value.ToString();
            var parameterValue = parameter.ToString();

            return enumValue == parameterValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                if (Enum.TryParse(targetType, parameter.ToString(), out var result))
                {
                    return result;
                }
            }

            return value;
        }
    }
}
