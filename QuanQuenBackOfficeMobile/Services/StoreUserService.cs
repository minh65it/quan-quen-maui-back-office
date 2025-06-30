using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace QuanQuenBackOfficeMobile.Services;

public class StoreUserService : IStoreUserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public StoreUserService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<StoreUserRegistrationResponse> RegisterUserAsync(StoreUserRegistrationRequest request)
    {
        try
        {
            var baseUrl = _configuration["API:Identity:BaseUrl"];
            var path = _configuration["API:Identity:RegisterPath"];
            var url = $"{baseUrl}{path}";

            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[StoreUserService] Response thành công: {content}");
            
            var result = JsonSerializer.Deserialize<StoreUserRegistrationResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new StoreUserRegistrationResponse { IsSuccess = false, Message = "Failed to deserialize response" };
        }
        catch (Exception ex)
        {
            return new StoreUserRegistrationResponse 
            { 
                IsSuccess = false, 
                Message = $"Registration failed: {ex.Message}" 
            };
        }
    }

    public async Task<ApiResponse> SendCodeEmailAsync(string email)
    {
        try
        {
            var baseUrl = _configuration["API:Identity:BaseUrl"];
            var path = _configuration["API:Identity:SendCodeEmailPath"];
            var url = $"{baseUrl}{path}?email={Uri.EscapeDataString(email)}";

            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse 
            { 
                IsSuccess = false, 
                Message = $"Failed to send verification code: {ex.Message}" 
            };
        }
    }

    public async Task<ApiResponse> VerifyCodeEmailAsync(string email, string code)
    {
        try
        {
            var baseUrl = _configuration["API:Identity:BaseUrl"];
            var path = _configuration["API:Identity:VerifyCodeEmailPath"];
            var url = $"{baseUrl}{path}";

            var request = new { Email = email, Code = code };
            var response = await _httpClient.PutAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = $"Failed to verify code: {ex.Message}"
            };
        }
    }

    public async Task<LevelApiResponse> GetListLevelAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            var baseUrl = _configuration["API:Identity:BaseUrl"];
            var path = _configuration["API:Identity:GetListLevel"];
            var url = $"{baseUrl}{path}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LevelApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? new LevelApiResponse();
        }
        catch (Exception ex)
        {
            return new LevelApiResponse(); 
        }
    }

    public async Task<ApiResponse> UpdateStoreLevelAsync(string storeId, int levelId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:UpdateStoreLevel"];
            var url = $"{baseUrl}{path}";

            var body = new {
                storeId = storeId,
                levelId = levelId,
                startDateLevel = startDate.ToString("o"),
                expirationDateLevel = endDate.ToString("o")
            };
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(body)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Update level failed: {ex.Message}" };
        }
    }

    public async Task<CsrDetailResponse> GetCsrDetailAsync(string url)
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            var baseUrl = _configuration["API:CSR:BaseUrl"];
            var path = _configuration["API:CSR:GetDetailCSR"];
            var apiUrl = $"{baseUrl}{path}?url={Uri.EscapeDataString(url)}";

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CsrDetailResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreUserService] CSR API Error: {ex.Message}");
            throw;
        }
    }
}
