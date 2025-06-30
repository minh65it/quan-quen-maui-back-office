using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services
{
    public class StoreService : IStoreService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public StoreService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<StoreApiResponse> GetAllStoresAsync(int page = 1, int pageSize = 20, string searchText = null)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:GetAllPath"];
                var url = $"{baseUrl}{path}?PageNumber={page}&PageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    url += $"&searchKey={Uri.EscapeDataString(searchText)}";
                }

                var token = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Token: {token}");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StoreApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new StoreApiResponse 
                { 
                    Pagination = new PaginationInfo 
                    { 
                        TotalCount = 0,
                        TotalPage = 0,
                        PageNumber = page,
                        PageSize = pageSize,
                        Skip = (page - 1) * pageSize
                    },
                    Result = new List<StoreModel>() 
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAllStoresAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<ApiResponse> CreateStoreAsync(StoreCreateRequest request)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:CreateStores"];
                var url = $"{baseUrl}{path}";

                using var form = new MultipartFormDataContent();

                form.Add(new StringContent(request.StoreId ?? ""), "StoreId");
                form.Add(new StringContent(request.StoreName ?? ""), "StoreName");
                form.Add(new StringContent(request.StorePhone ?? ""), "StorePhone");
                form.Add(new StringContent(request.StoreEmail ?? ""), "StoreEmail");
                form.Add(new StringContent(request.WardId ?? ""), "WardId");
                form.Add(new StringContent(request.DistrictId ?? ""), "DistrictId");
                form.Add(new StringContent(request.CityId ?? ""), "CityId");
                form.Add(new StringContent(request.IsRegisterByCompany.ToString()), "IsRegisterByCompany");
                form.Add(new StringContent(request.IsRegisterByPersonality.ToString()), "IsRegisterByPersonality");
                form.Add(new StringContent(request.Address ?? ""), "Address");
                form.Add(new StringContent(request.RepresentativeName ?? ""), "RepresentativeName");
                form.Add(new StringContent(request.ProvinceId ?? ""), "ProvinceId");
                form.Add(new StringContent(request.BusinessType ?? ""), "BusinessType");

                var token = await SecureStorage.GetAsync("auth_token");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(url, form);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] Response thành công: {content}");
                var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Fix: Nếu message là 'Create Success' nhưng IsSuccess là false, thì vẫn trả về IsSuccess = true
                if (result != null && !result.IsSuccess && result.Message != null && result.Message.Contains("Create Success"))
                {
                    result.IsSuccess = true;
                }

                return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Failed to create store: {ex.Message}"
                };
            }
        }

        public async Task<StoreDetailModel> GetStoreDetailAsync(string storeId)
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetStoreById"];
            var url = $"{baseUrl}{path}?storeId={storeId}";

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("No authentication token found");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<StoreDetailModel>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<ApiResponse> UpdateStore(string storeId, Status status)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:UpdateStore"];
                var url = $"{baseUrl}{path}";

                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(storeId), "StoreId");
                form.Add(new StringContent(((int)status).ToString()), "Status");
                // Thêm các trường khác với giá trị null
                form.Add(new StringContent(""), "Avatar");
                form.Add(new StringContent(""), "CoverImage");
                form.Add(new StringContent(""), "FrontPicIdentityCard");
                form.Add(new StringContent(""), "BackPicIdentityCard");
                form.Add(new StringContent(""), "ImageMenu");
                form.Add(new StringContent(""), "StoreName");
                form.Add(new StringContent(""), "StorePhone");
                form.Add(new StringContent(""), "StorePhone2");
                form.Add(new StringContent(""), "StoreLandlinePhone");
                form.Add(new StringContent(""), "StoreLandlinePhone2");
                form.Add(new StringContent(""), "StoreEmail");
                form.Add(new StringContent(""), "Address");
                form.Add(new StringContent(""), "Ward");
                form.Add(new StringContent(""), "District");
                form.Add(new StringContent(""), "City");
                form.Add(new StringContent(""), "NoteAddress");
                form.Add(new StringContent(""), "Latitude");
                form.Add(new StringContent(""), "Longitude");
                form.Add(new StringContent(""), "Hashtag");
                form.Add(new StringContent(""), "Description");
                form.Add(new StringContent("false"), "IsRegisterByCompany");
                form.Add(new StringContent("false"), "IsRegisterByPersonality");
                form.Add(new StringContent(""), "CompanyName");
                form.Add(new StringContent(""), "CompanyAddress");
                form.Add(new StringContent(""), "RepresentativeName");
                form.Add(new StringContent(""), "RepresentativeEmail");
                form.Add(new StringContent(""), "RepresentativePhone");
                form.Add(new StringContent(""), "RepresentativePhoneOther");
                form.Add(new StringContent(""), "WardId");
                form.Add(new StringContent(""), "DistrictId");
                form.Add(new StringContent(""), "CityId");
                form.Add(new StringContent(""), "CompanyWard");
                form.Add(new StringContent(""), "CompanyDistrict");
                form.Add(new StringContent(""), "CompanyCity");
                form.Add(new StringContent(""), "CompanyWardId");
                form.Add(new StringContent(""), "CompanyDistrictId");
                form.Add(new StringContent(""), "CompanyCityId");
                form.Add(new StringContent(""), "TaxCode");
                form.Add(new StringContent(""), "ReferralId");

                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = form
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] UpdateStore response: {content}");
                var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] UpdateStore error: {ex.Message}");
                return new ApiResponse { IsSuccess = false, Message = $"Update store failed: {ex.Message}" };
            }
        }
    }

    public class StoreApiResponse
    {
        [JsonPropertyName("result")]
        public List<StoreModel> Result { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; }
    }
} 