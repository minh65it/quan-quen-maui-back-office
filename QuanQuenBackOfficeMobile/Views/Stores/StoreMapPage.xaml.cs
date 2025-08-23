using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Linq;
using System.Net.Http;
using Microsoft.Maui.Maps;
using System.Text.Json;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreMapPage : ContentPage
{
    private readonly StoreMapViewModel _viewModel;

    public StoreMapPage(StoreMapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Truyền reference của map control xuống ViewModel
        _viewModel.MapControl = storeMap;

        // Thêm event handler cho tap trên map
        storeMap.MapClicked += OnMapClicked;

        await InitializeMapAsync();
    }

    private async Task InitializeMapAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[StoreMapPage] InitializeMapAsync started");
            var detail = _viewModel.StoreDetail;
            if (detail == null) return;

            double? latitude = detail.Latitude;
            double? longitude = detail.Longitude;

            // Nếu không có tọa độ, dùng geocoding
            if ((!latitude.HasValue || !longitude.HasValue) && !string.IsNullOrWhiteSpace(detail.FullAdrress))
            {
                var geocodingService = App.Services.GetService<IGeocodingService>();
                if (geocodingService != null)
                {
                    var coords = await geocodingService.GetCoordinatesAsync(detail.FullAdrress);
                    if (coords.HasValue)
                    {
                        latitude = coords.Value.Latitude;
                        longitude = coords.Value.Longitude;
                        // Cập nhật vào model
                        detail.Latitude = latitude;
                        detail.Longitude = longitude;
                    }
                }
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                var location = new Location(latitude.Value, longitude.Value);

                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Adding store pin at Lat: {latitude.Value}, Lng: {longitude.Value}");

                // Xóa tất cả pin cũ
                storeMap.Pins.Clear();

                // Thêm pin cho nhà hàng (vị trí hiện tại)
                var storePin = new Pin
                {
                    Label = detail.StoreName ?? "Nhà hàng",
                    Address = detail.FullAdrress,
                    Type = PinType.Place,
                    Location = location
                };
                storeMap.Pins.Add(storePin);

                // Di chuyển camera đến vị trí nhà hàng
                var mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(0.5));
                storeMap.MoveToRegion(mapSpan);

                System.Diagnostics.Debug.WriteLine("[StoreMapPage] Store pin added and camera moved");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể hiển thị bản đồ: " + ex.Message, "OK");
        }
    }

    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        try
        {
            var tappedLocation = e.Location;
            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Map tapped at Lat: {tappedLocation.Latitude}, Lng: {tappedLocation.Longitude}");

            // Reverse geocoding để lấy địa chỉ từ tọa độ
            string newAddress = await GetAddressFromCoordinatesAsync(tappedLocation.Latitude, tappedLocation.Longitude);
            
            // Cố gắng lấy thêm ward/district/city nếu có
            var (ward, district, city) = await GetAdministrativePartsAsync(tappedLocation.Latitude, tappedLocation.Longitude);
            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Admin parts - Ward: {ward}, District: {district}, City: {city}");

            // Hiển thị popup với thông tin vị trí mới
            var result = await DisplayAlert(
                "Vị trí mới", 
                $"Tọa độ: {tappedLocation.Latitude:F6}, {tappedLocation.Longitude:F6}\n\nĐịa chỉ: {newAddress}\n\nBạn có muốn lưu vị trí mới này không?",
                "Lưu vị trí mới",
                "Hủy"
            );

            if (result)
            {
                // User chọn lưu vị trí mới
                await SaveNewLocationAsync(tappedLocation, newAddress, ward, district, city);
            }
            // Nếu user bấm "Hủy" thì không làm gì cả, popup tự đóng
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể xử lý vị trí mới: " + ex.Message, "OK");
        }
    }

    private async Task SaveNewLocationAsync(Location newLocation, string newAddress, string ward, string district, string city)
    {
        try
        {
            // Tạo pin mới tại vị trí được tap
            var newPin = new Pin
            {
                Label = "Vị trí mới",
                Address = newAddress,
                Type = PinType.SavedPin,
                Location = newLocation
            };

            // Thêm pin mới vào map (giữ nguyên pin nhà hàng gốc)
            storeMap.Pins.Add(newPin);

            // Cập nhật vị trí mới và địa chỉ vào StoreDetail
            if (_viewModel.StoreDetail != null)
            {
                // Lưu tọa độ mới
                _viewModel.StoreDetail.Latitude = newLocation.Latitude;
                _viewModel.StoreDetail.Longitude = newLocation.Longitude;
                
                // Lưu địa chỉ đã reverse geocoding
                _viewModel.StoreDetail.FullAdrress = newAddress;
                
                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Updated store location to Lat: {newLocation.Latitude}, Lng: {newLocation.Longitude}");
                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] New address: {newAddress}");
                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] StoreDetail updated with new location data");
            }

            // Gọi endpoint UpdateStoreLocationAsync để lưu lên backend
            var storeService = App.Services.GetService<IStoreService>();
            if (storeService != null)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Calling UpdateStoreLocationAsync for store: {_viewModel.StoreDetail?.StoreId}");
                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Request data - StoreId: {_viewModel.StoreDetail?.StoreId}, Lat: {newLocation.Latitude}, Lng: {newLocation.Longitude}, Address: {newAddress}");
                
                var success = await storeService.UpdateStoreLocationAsync(
                    _viewModel.StoreDetail?.StoreId ?? "",
                    newLocation.Latitude,
                    newLocation.Longitude,
                    newAddress,
                    ward,
                    district,
                    city
                );

                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] UpdateStoreLocationAsync response: success = {success}");

                if (success)
                {
                    await DisplayAlert("Thành công", "Đã cập nhật vị trí nhà hàng lên server thành công! Bạn sẽ được chuyển về trang chi tiết.", "OK");
                    System.Diagnostics.Debug.WriteLine($"[StoreMapPage] UpdateStoreLocationAsync success - Location saved to backend");
                }
                else
                {
                    await DisplayAlert("Lỗi", "Đã cập nhật vị trí mới nhưng không thể lưu lên server. Vui lòng thử lại.", "OK");
                    System.Diagnostics.Debug.WriteLine($"[StoreMapPage] UpdateStoreLocationAsync failed - Could not save to backend");
                }
            }
            else
            {
                await DisplayAlert("Lỗi", "Không thể khởi tạo service để lưu vị trí.", "OK");
                System.Diagnostics.Debug.WriteLine($"[StoreMapPage] StoreService not found - Cannot save location to backend");
            }

            // Redirect về trang StoreDetail
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể cập nhật vị trí mới: " + ex.Message, "OK");
            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] SaveNewLocationAsync error: {ex.Message}");
        }
    }

    private async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            // Thử dùng .NET MAUI Geocoding trước
            var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
            if (placemarks?.Any() == true)
            {
                var placemark = placemarks.First();
                
                // Tạo địa chỉ chi tiết và đầy đủ
                var addressParts = new List<string>();
                
                if (!string.IsNullOrEmpty(placemark.Thoroughfare))
                    addressParts.Add(placemark.Thoroughfare);
                    
                if (!string.IsNullOrEmpty(placemark.SubThoroughfare))
                    addressParts.Add(placemark.SubThoroughfare);
                    
                if (!string.IsNullOrEmpty(placemark.Locality))
                    addressParts.Add(placemark.Locality);
                    
                if (!string.IsNullOrEmpty(placemark.SubLocality))
                    addressParts.Add(placemark.SubLocality);
                    
                if (!string.IsNullOrEmpty(placemark.AdminArea))
                    addressParts.Add(placemark.AdminArea);
                    
                if (!string.IsNullOrEmpty(placemark.CountryName))
                    addressParts.Add(placemark.CountryName);

                var address = string.Join(", ", addressParts.Where(p => !string.IsNullOrEmpty(p)));
                
                if (!string.IsNullOrEmpty(address))
                {
                    System.Diagnostics.Debug.WriteLine($"[StoreMapPage] MAUI Geocoding result: {address}");
                    return address;
                }
            }

            // Nếu MAUI Geocoding không có kết quả, dùng Google Geocoding API
            var geocodingService = App.Services.GetService<IGeocodingService>();
            if (geocodingService != null)
            {
                // Gọi Google Reverse Geocoding API
                var address = await GetGoogleReverseGeocodingAsync(latitude, longitude);
                if (!string.IsNullOrEmpty(address))
                {
                    System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Google Reverse Geocoding result: {address}");
                    return address;
                }
            }

            // Fallback: trả về tọa độ nếu không lấy được địa chỉ
            return $"Tọa độ: {latitude:F6}, {longitude:F6}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Geocoding error: {ex.Message}");
            return $"Tọa độ: {latitude:F6}, {longitude:F6}";
        }
    }

    private async Task<string> GetGoogleReverseGeocodingAsync(double latitude, double longitude)
    {
        try
        {
            var configurationService = App.Services.GetService<IConfigurationService>();
            if (configurationService == null) return null;

            var apiKey = configurationService.GetGoogleMapsApiKey();
            if (string.IsNullOrEmpty(apiKey)) return null;

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={apiKey}&language=vi&result_type=street_address|route|premise";
            
            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            
            // Parse JSON response để lấy formatted_address
            // Đây là implementation đơn giản, bạn có thể dùng Newtonsoft.Json để parse đầy đủ hơn
            if (response.Contains("\"formatted_address\""))
            {
                var startIndex = response.IndexOf("\"formatted_address\"") + "\"formatted_address\"".Length;
                var endIndex = response.IndexOf("\"", startIndex + 2);
                if (endIndex > startIndex)
                {
                    var address = response.Substring(startIndex + 2, endIndex - startIndex - 2);
                    
                    // Làm sạch địa chỉ (loại bỏ dấu phẩy thừa)
                    address = address.Replace(",,", ",").Replace(" ,", ",").Replace(", ", ",").Trim(',', ' ');
                    
                    return address;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Google Reverse Geocoding error: {ex.Message}");
            return null;
        }
    }

    private async Task<(string Ward, string District, string City)> GetAdministrativePartsAsync(double latitude, double longitude)
    {
        try
        {
            // Ưu tiên MAUI Geocoding (nhanh, offline). Dùng làm fallback nếu Google không rõ ràng
            string wardFromMaui = null, districtFromMaui = null, cityFromMaui = null;
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    wardFromMaui = placemark.SubLocality;                // Phường/Xã
                    districtFromMaui = placemark.Locality ?? placemark.SubAdminArea; // Quận/Huyện
                    cityFromMaui = placemark.AdminArea ?? placemark.CountryName;      // Tỉnh/Thành phố
                }
            }
            catch { /* bỏ qua, sẽ dùng Google */ }

            // Google Reverse Geocoding (ưu tiên chất lượng dữ liệu)
            var configurationService = App.Services.GetService<IConfigurationService>();
            var apiKey = configurationService?.GetGoogleMapsApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                return (wardFromMaui, districtFromMaui, cityFromMaui);
            }

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={apiKey}&language=vi";
            using var client = new HttpClient();
            var json = await client.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return (wardFromMaui, districtFromMaui, cityFromMaui);
            }

            string wardVal = null, districtVal = null, cityVal = null;
            foreach (var result in results.EnumerateArray())
            {
                if (!result.TryGetProperty("address_components", out var components)) continue;
                foreach (var comp in components.EnumerateArray())
                {
                    var longName = comp.GetProperty("long_name").GetString();
                    var types = comp.GetProperty("types").EnumerateArray().Select(t => t.GetString()).ToList();

                    // Heuristic theo thứ tự ưu tiên phổ biến tại VN
                    if (wardVal == null && (types.Contains("sublocality_level_2") || types.Contains("sublocality") || types.Contains("administrative_area_level_3") || types.Contains("neighborhood")))
                        wardVal = longName;

                    if (districtVal == null && (types.Contains("administrative_area_level_2") || types.Contains("sublocality_level_1") || types.Contains("locality")))
                        districtVal = longName;

                    if (cityVal == null && (types.Contains("administrative_area_level_1") || types.Contains("locality") || types.Contains("postal_town")))
                        cityVal = longName;
                }

                if (wardVal != null && districtVal != null && cityVal != null) break;
            }

            // Fallback về MAUI nếu thiếu
            wardVal ??= wardFromMaui;
            districtVal ??= districtFromMaui;
            cityVal ??= cityFromMaui;

            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] Parsed Ward/District/City => {wardVal} / {districtVal} / {cityVal}");
            return (wardVal, districtVal, cityVal);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreMapPage] GetAdministrativePartsAsync error: {ex.Message}");
            return (null, null, null);
        }
    }
}
