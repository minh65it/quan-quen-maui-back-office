using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                var member = enumValue.GetType().GetMember(enumValue.ToString());
                if (member.Length > 0)
                {
                    var displayAttr = member[0].GetCustomAttribute<DisplayAttribute>();
                    if (displayAttr != null)
                        return displayAttr.Name;
                }
                return enumValue.ToString();
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
} 