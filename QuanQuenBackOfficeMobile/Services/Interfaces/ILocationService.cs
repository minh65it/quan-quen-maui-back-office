namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface ILocationService
    {
        Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
        Task<bool> RequestLocationPermissionAsync();
    }
}
