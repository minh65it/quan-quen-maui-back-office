using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class AvatarUrlConverter : IValueConverter
    {
        private const string BaseUrl = "https://quanquenapi-dev.azurewebsites.net/asset";
        private const string DefaultAva = "default_ava.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var avatar = value as string;
            if (string.IsNullOrEmpty(avatar))
                return DefaultAva;
            if (avatar.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return avatar;
            return BaseUrl + avatar;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 