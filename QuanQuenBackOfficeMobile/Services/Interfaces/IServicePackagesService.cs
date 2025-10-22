namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IServicePackagesService
{
    Task<ServicePackagesDto> GetAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateStorePackageAsync(string storeId, int storePackage, CancellationToken cancellationToken = default);
}

public sealed class ServicePackagesDto
{
    public List<string> QuanMoi { get; set; } = new();
    public List<string> QuanQuen { get; set; } = new();
}


