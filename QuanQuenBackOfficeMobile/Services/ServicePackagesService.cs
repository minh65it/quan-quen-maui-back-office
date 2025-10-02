using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Net.Http.Headers;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Services;

public class ServicePackagesService : IServicePackagesService
{
    private readonly ISystemConfigService _systemConfigService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ServicePackagesService(ISystemConfigService systemConfigService, HttpClient httpClient, IConfiguration configuration)
    {
        _systemConfigService = systemConfigService;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ServicePackagesDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var keys = new[] { "quan-moi", "quan-quen" };
        var resp = await _systemConfigService.GetValuesAsListAsync(keys, cancellationToken);
        var dto = new ServicePackagesDto();
        if (!resp.Success) return dto;
        if (resp.Data.TryGetValue("quan-moi", out var qm)) dto.QuanMoi = qm;
        if (resp.Data.TryGetValue("quan-quen", out var qq)) dto.QuanQuen = qq;
        return dto;
    }

    public async Task<ApiResponse> UpdateStorePackageAsync(string storeId, int storePackage, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(storeId))
            {
                return new ApiResponse { IsSuccess = false, Message = "storeId rỗng" };
            }

            var token = await SecureStorage.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:UpdateStorePackage"];
            var url = $"{baseUrl}{path}?storeId={Uri.EscapeDataString(storeId)}&storePackage={storePackage}";

            Debug.WriteLine($"[ServicePackagesService] PUT {url}");

            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            Debug.WriteLine($"[ServicePackagesService] Response {(int)response.StatusCode}: {content}");

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse { IsSuccess = true, Message = "Cập nhật gói dịch vụ thành công" };
            }

            return new ApiResponse { IsSuccess = false, Message = $"Cập nhật thất bại: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = ex.Message };
        }
    }
}


