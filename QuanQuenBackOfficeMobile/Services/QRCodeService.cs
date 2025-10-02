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
                    PropertyNamingPolicy = null, // Sử dụng JsonPropertyName thay vì CamelCase
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

        public async Task<bool> UpdateQRCodeAsync(string qrCode, string qrCodeType, string identityId)
        {
            try
            {
                var baseUrl = _configuration["API:QRCode:BaseUrl"];
                var path = _configuration["API:QRCode:UpdateQRCodeStatus"];
                
                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("QRCode configuration is missing. Please check appsettings.json");
                }
                
                var url = $"{baseUrl}{path}";

                // Tạo request body theo format API yêu cầu
                var request = new UpdateQRCodeRequest
                {
                    IdentityId = identityId,
                    QRCodeType = qrCodeType ?? "Store",
                    QRCode = qrCode,
                    QRFormat = 0,
                    JsonContent = new JsonContentData
                    {
                        ObjectId = new string[] { "" },
                        UserId = new string[] { "" }
                    }
                };

                // Serialize request body
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null, // Sử dụng JsonPropertyName thay vì CamelCase
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                // Lấy token xác thực
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                // Thiết lập headers
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Log request details
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Request URL: {url}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Request Method: PUT");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Request Body: {jsonContent}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Params: qrCode={qrCode}, qrCodeType={qrCodeType}, identityId={identityId}");

                // Tạo HTTP content
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Thực hiện request
                var response = await _httpClient.PutAsync(url, content);
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode HTTP Status: {(int)response.StatusCode} {response.ReasonPhrase}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Response: {responseContent}");
                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Success: true");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Error: {errorContent}");
                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Success: false");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateQRCodeAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
