using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace QuanQuenBackOfficeMobile.Services
{
    /// <summary>
    /// Service xử lý các thao tác liên quan đến xác thực
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private const string TokenKey = "auth_token";

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(string emailOrPhone, string password)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:LoginPath"];
                var url = $"{baseUrl}{path}";

                var loginRequest = new LoginRequest
                {
                    Email = emailOrPhone,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync(url, loginRequest);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null && result.IsLogin && !string.IsNullOrEmpty(result.Token))
                    await SecureStorage.SetAsync(TokenKey, result.Token);

                return result ?? new LoginResponse { IsLogin = false };
            }
            catch (Exception)
            {
                return new LoginResponse { IsLogin = false };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                SecureStorage.Default.Remove(TokenKey);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 