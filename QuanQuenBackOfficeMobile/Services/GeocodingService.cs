using System.Text.Json;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeocodingService(IConfigurationService configurationService)
        {
            _httpClient = new HttpClient();
            _apiKey = configurationService.GetGoogleMapsApiKey();
        }

        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    System.Diagnostics.Debug.WriteLine($"[GeocodingService] Address is null or empty");
                    return null;
                }

                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";
                
                System.Diagnostics.Debug.WriteLine($"[GeocodingService] Calling URL: {url}");

                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[GeocodingService] HTTP Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[GeocodingService] Response: {json}");
                
                var result = JsonSerializer.Deserialize<GoogleGeocodingResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Status == "OK" && result.Results?.Count > 0)
                {
                    var location = result.Results[0].Geometry.Location;
                    System.Diagnostics.Debug.WriteLine($"[GeocodingService] Success: Lat={location.Lat}, Lng={location.Lng}");
                    return (location.Lat, location.Lng);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GeocodingService] API returned status: {result?.Status}, results count: {result?.Results?.Count}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeocodingService] Error getting coordinates: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GeocodingService] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        // Google Geocoding API response models
        private class GoogleGeocodingResponse
        {
            public string Status { get; set; } = string.Empty;
            public List<GeocodingResult> Results { get; set; } = new();
        }

        private class GeocodingResult
        {
            public Geometry Geometry { get; set; } = new();
        }

        private class Geometry
        {
            public Location Location { get; set; } = new();
        }

        private class Location
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}
