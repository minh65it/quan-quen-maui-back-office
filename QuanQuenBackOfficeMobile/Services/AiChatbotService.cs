using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Net.Http;
using System.Text.Json;

namespace QuanQuenBackOfficeMobile.Services
{
    public class AiChatbotService : IAiChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiChatbotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default)
        {
            var baseUrl = _configuration["API:AiChatbot:BaseUrl"];
            var path = _configuration["API:AiChatbot:GetResponse"];
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("AiChatbot configuration is missing. Please check appsettings.json");

            var url = $"{baseUrl}{path}?message={Uri.EscapeDataString(message ?? string.Empty)}";

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
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString() ?? string.Empty;
                }
            }
            catch
            {
                // ignore parsing error, just return raw content
            }
            return content;
        }
    }
}


