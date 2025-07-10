using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Converters
{
    public class ImageUrlConverter : IValueConverter
    {
        private readonly string _baseUrl;
        public ImageUrlConverter()
        {
            // Lấy cấu hình từ App.Services
            var config = App.Services.GetService(typeof(IConfiguration)) as IConfiguration;
            _baseUrl = config?["API:Asset:ImageUrl"] ?? "https://quanquenapi-dev.azurewebsites.net/asset";
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;
            if (string.IsNullOrEmpty(url)) return null;
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
            return _baseUrl + url;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 