using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.Maui.Storage;

namespace QuanQuenBackOfficeMobile.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public QRCodeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<QRCodeResponse> GenerateStoreQRCodeAsync(QRCodeData storeData, AgentInfo agentInfo)
        {
            try
            {
                // Cập nhật thông tin agent
                storeData.AgentInfo = agentInfo;
                storeData.Timestamp = DateTime.UtcNow;

                // Serialize dữ liệu store thành JSON với Unicode support
                var qrText = JsonSerializer.Serialize(storeData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                // Gọi API tạo QR Code với text cố định là "Registration"
                return await GenerateQRCodeFromAPI(qrText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateStoreQRCodeAsync: {ex.Message}");
                throw;
            }
        }

        private async Task<QRCodeResponse> GenerateQRCodeFromAPI(string qrText)
        {
            try
            {
                var baseUrl = _configuration["API:QRCode:BaseUrl"];
                var path = _configuration["API:QRCode:GetQRCode"];
                
                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("QRCode configuration is missing. Please check appsettings.json");
                }
                
                var url = $"{baseUrl}{path}";

                // Tạo query parameters với text cố định là "Registration"
                var queryParams = new List<string>
                {
                    $"qrText={Uri.EscapeDataString(qrText)}",
                    $"text=Registration"
                };

                var fullUrl = $"{url}?{string.Join("&", queryParams)}";

                // Lấy token xác thực
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                // Thiết lập headers
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

                // Log request details
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Request URL: {fullUrl}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Request Method: GET");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] QRText: {qrText}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Text: Registration");

                // Thực hiện request
                var response = await _httpClient.GetAsync(fullUrl);
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] HTTP Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Content-Type: {response.Content.Headers.ContentType}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Content-Length: {response.Content.Headers.ContentLength?.ToString() ?? "(unknown)"}");

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Raw Response (as string): {responseContent}");

                // Xử lý response - API trả về trực tiếp image file
                if (response.Content.Headers.ContentType?.MediaType?.Contains("image") == true)
                {
                    // Nếu response là hình ảnh, lưu URL của image
                    var imageUrl = fullUrl; // Sử dụng URL đã gọi để lấy image
                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] Detected image response. Using request URL as image source.");
                    
                    return new QRCodeResponse
                    {
                        Success = true,
                        Message = "QR Code generated successfully",
                        QRCodeImage = imageUrl
                    };
                }
                else
                {
                    // Nếu response là JSON
                    var result = JsonSerializer.Deserialize<QRCodeResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] Parsed JSON -> Success: {result?.Success}, Message: {result?.Message}");

                    return result ?? new QRCodeResponse
                    {
                        Success = false,
                        Message = "Failed to parse response",
                        QRCodeImage = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateQRCodeFromAPI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
