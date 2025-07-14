using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class AvatarUrlConverter : IValueConverter
    {
        private readonly string _baseUrl;

        public AvatarUrlConverter()
        {
            var config = App.Services.GetService(typeof(IConfiguration)) as IConfiguration;
            _baseUrl = config?["API:Asset:ImageUrl"] ?? "https://quanquenapi-dev.azurewebsites.net/asset";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var avatar = value as string;
            if (string.IsNullOrEmpty(avatar))
                return "default_ava.png";
            if (avatar.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return avatar;
            return _baseUrl + avatar;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 