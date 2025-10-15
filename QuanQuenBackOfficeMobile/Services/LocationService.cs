using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;

        public LocationService(ILogger<LocationService> logger)
        {
            _logger = logger;
        }

        public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
        {
            try
            {
                _logger.LogInformation("Requesting current location...");

                // Request permission first
                var hasPermission = await RequestLocationPermissionAsync();
                if (!hasPermission)
                {
                    _logger.LogWarning("Location permission denied");
                    return null;
                }

                // Get current location using MAUI Essentials
                var location = await Geolocation.GetLocationAsync(
                    new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(10)
                    });

                if (location != null)
                {
                    _logger.LogInformation($"Current location: Lat={location.Latitude}, Lng={location.Longitude}");
                    return (location.Latitude, location.Longitude);
                }

                _logger.LogWarning("Failed to get current location");
                return null;
            }
            catch (FeatureNotSupportedException ex)
            {
                _logger.LogError(ex, "Location services not supported on this device");
                return null;
            }
            catch (FeatureNotEnabledException ex)
            {
                _logger.LogError(ex, "Location services not enabled");
                return null;
            }
            catch (PermissionException ex)
            {
                _logger.LogError(ex, "Location permission denied");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current location");
                return null;
            }
        }

        public async Task<bool> RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    _logger.LogInformation("Location permission already granted");
                    return true;
                }

                if (status == PermissionStatus.Denied)
                {
                    _logger.LogWarning("Location permission denied");
                    return false;
                }

                // Request permission
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                
                var granted = status == PermissionStatus.Granted;
                _logger.LogInformation($"Location permission request result: {status}");
                
                return granted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting location permission");
                return false;
            }
        }
    }
}
