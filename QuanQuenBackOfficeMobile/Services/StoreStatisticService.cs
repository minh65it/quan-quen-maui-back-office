using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Services;

public class StoreStatisticService : IStoreStatisticService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public StoreStatisticService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ApiResponseInvoiceListModel> GetInvoicesAsync(string storeName, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeName))
        {
            throw new ArgumentException("storeName is required", nameof(storeName));
        }

        var token = await SecureStorage.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration["API:Payment:BaseUrl"];
        var path = _configuration["API:Payment:GetAllInvoice"];
        var query = $"StoreName={Uri.EscapeDataString(storeName)}&PageNumber={pageNumber}&PageSize={pageSize}";
        var url = $"{baseUrl}{path}?{query}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch invoices: {(int)response.StatusCode} - {content}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = JsonSerializer.Deserialize<ApiResponseInvoiceListModel>(content, options);
        return result ?? new ApiResponseInvoiceListModel { success = false, data = new InvoiceListDataModel { result = new() } };
    }

    public async Task<ApiResponseMerchantOrderSuccess> GetMerchantOrdersAsync(string storeId, DateTime fromDate, DateTime toDate, string cityId, string storeTypeId = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId)) throw new ArgumentException("storeId is required", nameof(storeId));
        if (string.IsNullOrWhiteSpace(cityId)) throw new ArgumentException("cityId is required", nameof(cityId));

        var token = await SecureStorage.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration["API:Statistics:BaseUrl"];
        var path = _configuration["API:Statistics:GetOrderCreatedSuccess"];

        var queryParts = new List<string>
        {
            $"StoreId={Uri.EscapeDataString(storeId)}",
            $"FromDate={Uri.EscapeDataString(fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}",
            $"ToDate={Uri.EscapeDataString(toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}",
            $"CityId={Uri.EscapeDataString(cityId)}",
            $"PageNumber={pageNumber}",
            $"PageSize={pageSize}"
        };
        if (!string.IsNullOrWhiteSpace(storeTypeId))
        {
            queryParts.Add($"StoreType={Uri.EscapeDataString(storeTypeId)}");
        }
        var url = $"{baseUrl}{path}?{string.Join("&", queryParts)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch orders: {(int)response.StatusCode} - {content}");
        }
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ApiResponseMerchantOrderSuccess>(content, options);
    }

    public async Task<ApiResponseOrderList> GetNumberOfUserInOrderAsync(string storeId, DateTime fromDate, DateTime toDate, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId)) throw new ArgumentException("storeId is required", nameof(storeId));

        var token = await SecureStorage.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration["API:Statistics:BaseUrl"];
        var path = _configuration["API:Statistics:GetNumberOfUserInOrder"];
        var query = string.Join('&', new[]
        {
            $"StoreId={Uri.EscapeDataString(storeId)}",
            $"FromDate={Uri.EscapeDataString(fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}",
            $"ToDate={Uri.EscapeDataString(toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}",
            $"PageNumber={pageNumber}",
            $"PageSize={pageSize}"
        });
        var url = $"{baseUrl}{path}?{query}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch users-in-order: {(int)response.StatusCode} - {content}");
        }
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ApiResponseOrderList>(content, options);
    }

    public async Task<ApiResponseLoyalCustomer> GetTotalCustomerCombackAsync(string storeId, int frequency = 2, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId)) throw new ArgumentException("storeId is required", nameof(storeId));

        var token = await SecureStorage.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration["API:Statistics:BaseUrl"];
        var path = _configuration["API:Statistics:GetCusComeBack"];
        var url = $"{baseUrl}{path}?Frequency={frequency}&StoreId={Uri.EscapeDataString(storeId)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch loyal customers: {(int)response.StatusCode} - {content}");
        }
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ApiResponseLoyalCustomer>(content, options);
    }

    public async Task<ApiResponseAverageRevenue> GetAverageRevenueAsync(string storeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId)) throw new ArgumentException("storeId is required", nameof(storeId));

        var token = await SecureStorage.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration["API:Statistics:BaseUrl"];
        var path = _configuration["API:Statistics:CalculateAverageRevenue"];
        var url = $"{baseUrl}{path}?storeId={Uri.EscapeDataString(storeId)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch average revenue: {(int)response.StatusCode} - {content}");
        }
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ApiResponseAverageRevenue>(content, options);
    }

    // Dummy endpoints placeholders: you mentioned there are endpoints to get lists.
    // Here we try to read from known services if available, otherwise return empty.
    public Task<IReadOnlyList<CityItemModel>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        // If later you provide exact endpoints, we can wire them here.
        return Task.FromResult((IReadOnlyList<CityItemModel>)new List<CityItemModel>());
    }

    public Task<IReadOnlyList<StoreTypeItemModel>> GetStoreTypesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<StoreTypeItemModel>)new List<StoreTypeItemModel>());
    }
}


