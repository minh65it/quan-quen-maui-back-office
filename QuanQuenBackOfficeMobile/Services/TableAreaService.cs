using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Text.Json;

namespace QuanQuenBackOfficeMobile.Services
{
    public class TableAreaService : ITableAreaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TableAreaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<TableAreaItem>> GetTableAreasAsync(string storeId, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetTableAreaList"]; // "api/TableArea/GetLstTableArea"
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("TableArea configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}?StoreId={Uri.EscapeDataString(storeId)}";

            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            try
            {
                // API có thể trả về { pagination: {...}, result: [...] }
                var wrapper = JsonSerializer.Deserialize<TableAreaListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return wrapper?.Result ?? new List<TableAreaItem>();
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                System.Diagnostics.Debug.WriteLine($"Error deserializing TableAreaListResponse: {ex.Message}");
                return new List<TableAreaItem>();
            }
        }

        public async Task<ApiResponse> CreateTableAreaAsync(string storeId, string areaName, string? areaDescription, int order, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:CreateTableArea"]; // "api/TableArea/CreateTableArea"
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("CreateTableArea configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}";

            var requestBody = new
            {
                storeId = storeId,
                areaName = areaName,
                areaDescription = areaDescription,
                order = order
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch
            {
                return new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
            }
        }

        public async Task<ApiResponse> UpdateTableAreaAsync(string storeId, string tableAreaId, string areaName, string? areaDescription, int order, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:UpdateTableArea"]; // "api/TableArea/UpdateTableArea"
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("UpdateTableArea configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}";

            var requestBody = new
            {
                storeId = storeId,
                tableAreaId = tableAreaId,
                areaName = areaName,
                areaDescription = areaDescription,
                order = order
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            using var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch
            {
                return new ApiResponse { IsSuccess = false, Message = "Failed to parse response" };
            }
        }
    }
}


