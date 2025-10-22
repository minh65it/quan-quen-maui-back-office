using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services
{
    public class FoodFilterService : IFoodFilterService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public FoodFilterService(HttpClient httpClient, IConfiguration configuration, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task<List<FoodFilterItem>> GetFoodFiltersAsync(string storeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var endpoint = _configuration["API:Store:GetListFoodFilter"];
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("Token không tồn tại");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl}{endpoint}?StoreId={storeId}";
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Full URL: {url}");
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API returned status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Response Content: {responseContent}");

                var wrapper = JsonSerializer.Deserialize<FoodFilterListResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Wrapper Result Count: {wrapper?.Result?.Count ?? 0}");

                return wrapper?.Result ?? new List<FoodFilterItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Stack Trace: {ex.StackTrace}");
                throw new Exception($"Lỗi khi lấy danh sách bộ lọc món ăn: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateFoodFilterAsync(string filterFoodId, string storeId, string name, string value, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var endpoint = _configuration["API:Store:UpdateFoodFilter"];
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("Token không tồn tại");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl}{endpoint}";

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(filterFoodId), "FilterFoodId");
                formData.Add(new StringContent(storeId), "StoreId");
                formData.Add(new StringContent(name), "Name");
                formData.Add(new StringContent(value), "Value");

                var response = await _httpClient.PutAsync(url, formData, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"API returned status code: {response.StatusCode}. Error: {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Update ERROR: {ex.Message}");
                throw new Exception($"Lỗi khi cập nhật bộ lọc món ăn: {ex.Message}", ex);
            }
        }

        public async Task<bool> CreateFoodFilterAsync(string storeId, string name, string value, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var endpoint = _configuration["API:Store:AddFoodFilter"];
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("Token không tồn tại");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl}{endpoint}";

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(storeId), "StoreId");
                formData.Add(new StringContent(name), "Name");
                formData.Add(new StringContent(value), "Value");

                var response = await _httpClient.PostAsync(url, formData, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"API returned status code: {response.StatusCode}. Error: {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Create ERROR: {ex.Message}");
                throw new Exception($"Lỗi khi tạo bộ lọc món ăn: {ex.Message}", ex);
            }
        }

        public async Task<string> CreateFoodAsync(string storeId, string filterFoodId, string name, string description, decimal price, decimal priceSale, string foodUnitId, bool isCombo = false, bool isOutOfStock = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var endpoint = _configuration["API:Store:CreateFood"];
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("Token không tồn tại");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl}{endpoint}";

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(storeId), "StoreId");
                formData.Add(new StringContent(filterFoodId), "FilterFoodId");
                formData.Add(new StringContent(name), "Name");
                formData.Add(new StringContent(description), "Description");
                formData.Add(new StringContent(price.ToString()), "Price");
                formData.Add(new StringContent(priceSale.ToString()), "PriceSale");
                formData.Add(new StringContent(foodUnitId), "FoodUnitId");
                formData.Add(new StringContent(isCombo.ToString().ToLower()), "IsCombo");
                formData.Add(new StringContent(isOutOfStock.ToString().ToLower()), "IsOutOfStock");

                var response = await _httpClient.PostAsync(url, formData, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"API returned status code: {response.StatusCode}. Error: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonDoc = JsonSerializer.Deserialize<JsonDocument>(responseContent);
                var message = jsonDoc?.RootElement.GetProperty("message").GetString() ?? "Tạo món ăn thành công!";

                return message;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FoodFilterService] Create Food ERROR: {ex.Message}");
                throw new Exception($"Lỗi khi tạo món ăn: {ex.Message}", ex);
            }
        }
    }
}

