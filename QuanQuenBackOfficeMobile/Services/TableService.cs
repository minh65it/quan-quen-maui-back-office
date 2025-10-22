using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Text.Json;

namespace QuanQuenBackOfficeMobile.Services
{
    public class TableService : ITableService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TableService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<TableItem>> GetTablesAsync(string storeId, string tableAreaId, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetTableList"]; // "api/Table/GetLstTable"
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Table configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}?StoreId={Uri.EscapeDataString(storeId)}&TableAreaId={Uri.EscapeDataString(tableAreaId)}";

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
                // API trả về { pagination: {...}, result: [...] }
                var wrapper = JsonSerializer.Deserialize<TableListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return wrapper?.Result ?? new List<TableItem>();
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                System.Diagnostics.Debug.WriteLine($"Error deserializing TableListResponse: {ex.Message}");
                return new List<TableItem>();
            }
        }

        public async Task<ApiResponse> CreateTableAsync(string storeId, string tableAreaId, int tableNumber, string tableName, int tableStatus, int capacity, string? tableDescription, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:CreateTable"]; // "api/Table/CreateTable"
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("CreateTable configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}";

            var requestBody = new
            {
                storeId = storeId,
                tableAreaId = tableAreaId,
                tableNumber = tableNumber,
                tableName = tableName,
                tableStatus = tableStatus,
                capacity = capacity.ToString(),
                userId = (string?)null,
                userName = (string?)null,
                tableDescription = tableDescription
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

        public async Task<ApiResponse> UpdateTableAsync(string tableId, string storeId, string tableAreaId, int tableNumber, string tableName, int tableStatus, int capacity, string? tableDescription, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:UpdateTable"]; // "api/Table/UpdateTable"
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("UpdateTable configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}";

            var requestBody = new
            {
                tableId = tableId,
                storeId = storeId,
                tableAreaId = tableAreaId,
                tableNumber = tableNumber,
                tableName = tableName,
                tableStatus = tableStatus,
                capacity = capacity,
                userId = (string?)null,
                userName = (string?)null,
                tableDescription = tableDescription
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
