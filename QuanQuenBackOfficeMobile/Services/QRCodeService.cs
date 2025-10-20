using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

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

        /// <summary>
        /// Lấy cả 2 token (auth_token và affiliate_token) nếu có
        /// </summary>
        /// <returns>Dictionary chứa cả 2 token</returns>
        private async Task<Dictionary<string, string?>> GetAllAuthenticationTokensAsync()
        {
            var tokens = new Dictionary<string, string?>();
            
            try
            {
                // Lấy auth_token
                var authToken = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(authToken))
                {
                    authToken = Preferences.Get("auth_token", "");
                }
                tokens["auth_token"] = authToken;
                
                // Lấy affiliate_token
                var affiliateToken = await SecureStorage.GetAsync("affiliate_token");
                if (string.IsNullOrEmpty(affiliateToken))
                {
                    affiliateToken = Preferences.Get("affiliate_token", "");
                }
                tokens["affiliate_token"] = affiliateToken;
                
                // Log thông tin token
                var authStatus = string.IsNullOrEmpty(authToken) ? "NOT_FOUND" : "FOUND";
                var affiliateStatus = string.IsNullOrEmpty(affiliateToken) ? "NOT_FOUND" : "FOUND";
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Token status - auth_token: {authStatus}, affiliate_token: {affiliateStatus}");
                
                return tokens;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Error getting authentication tokens: {ex.Message}");
                return tokens;
            }
        }

        /// <summary>
        /// Lấy token chính để sử dụng (ưu tiên affiliate_token nếu có)
        /// </summary>
        /// <returns>Token string hoặc null nếu không tìm thấy</returns>
        private async Task<string?> GetPrimaryAuthenticationTokenAsync()
        {
            try
            {
                var tokens = await GetAllAuthenticationTokensAsync();
                
                // Ưu tiên affiliate_token nếu có
                if (!string.IsNullOrEmpty(tokens["affiliate_token"]))
                {
                    System.Diagnostics.Debug.WriteLine("[QRCodeService] Using affiliate_token as primary token");
                    return tokens["affiliate_token"];
                }
                
                // Fallback về auth_token
                if (!string.IsNullOrEmpty(tokens["auth_token"]))
                {
                    System.Diagnostics.Debug.WriteLine("[QRCodeService] Using auth_token as primary token");
                    return tokens["auth_token"];
                }
                
                System.Diagnostics.Debug.WriteLine("[QRCodeService] No authentication token found");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Error getting primary authentication token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Thực hiện HTTP request với cả 2 token (thử affiliate_token trước, nếu fail thì thử auth_token)
        /// </summary>
        /// <param name="requestFunc">Function thực hiện HTTP request với token</param>
        /// <returns>Kết quả của request</returns>
        private async Task<T> ExecuteWithBothTokensAsync<T>(Func<string, Task<T>> requestFunc)
        {
            var tokens = await GetAllAuthenticationTokensAsync();
            
            // Thử affiliate_token trước nếu có
            if (!string.IsNullOrEmpty(tokens["affiliate_token"]))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[QRCodeService] Trying request with affiliate_token");
                    return await requestFunc(tokens["affiliate_token"]);
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine("[QRCodeService] affiliate_token failed, trying auth_token");
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
                {
                    System.Diagnostics.Debug.WriteLine("[QRCodeService] affiliate_token returned 401, trying auth_token");
                }
            }
            
            // Thử auth_token nếu affiliate_token không có hoặc fail
            if (!string.IsNullOrEmpty(tokens["auth_token"]))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[QRCodeService] Trying request with auth_token");
                    return await requestFunc(tokens["auth_token"]);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[QRCodeService] auth_token also failed: {ex.Message}");
                    throw;
                }
            }
            
            throw new UnauthorizedAccessException("No valid authentication token found");
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

                // Log request details
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Request URL: {fullUrl}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Request Method: GET");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] QRText: {qrText}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Text: Registration");

                // Thực hiện request với cả 2 token
                var response = await ExecuteWithBothTokensAsync(async token =>
                {
                    // Thiết lập headers
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

                    // Thực hiện request
                    return await _httpClient.GetAsync(fullUrl);
                });
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

                // Log request details
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Request URL: {url}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Request Method: PUT");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Request Body: {jsonContent}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] UpdateQRCode Params: qrCode={qrCode}, qrCodeType={qrCodeType}, identityId={identityId}");

                // Tạo HTTP content
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Thực hiện request với cả 2 token
                var response = await ExecuteWithBothTokensAsync(async token =>
                {
                    // Thiết lập headers
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Thực hiện request
                    return await _httpClient.PutAsync(url, content);
                });
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

        public async Task<bool> ScanQRCodeAndCreateTableAsync(
            string qrCode,
            string storeId,
            string? tableAreaId = null,
            string? tableName = null,
            string? tableNumber = null,
            int? capacity = null,
            string? tableDescription = null,
            string? userId = null,
            string? userName = null)
        {
            try
            {
                // Dùng cấu hình của Store API theo appsettings
                var baseUrl = _configuration["API:Store:BaseUrl"]; // ví dụ: https://api.quanquen.com/store/
                var path = _configuration["API:Store:UpdateTableQRCode"]; // ví dụ: api/Table/ScanQRCodeAndCreateTable

                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("API base url is missing. Please check appsettings.json");
                }
                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("API path is missing. Please check appsettings.json (API:Store:UpdateTableQRCode)");
                }

                var url = $"{baseUrl}{path}";

                var requestBody = new
                {
                    qrCode = qrCode,
                    storeId = storeId,
                    tableAreaId = tableAreaId,
                    tableName = tableName,
                    tableNumber = tableNumber,
                    capacity = capacity,
                    tableDescription = tableDescription,
                    userId = userId,
                    userName = userName
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                var correlationId = Guid.NewGuid().ToString("N");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var maskedStoreId = string.IsNullOrEmpty(storeId) || storeId.Length < 8
                    ? storeId
                    : $"{storeId.Substring(0, 4)}...{storeId.Substring(storeId.Length - 4)}";
                System.Diagnostics.Debug.WriteLine($"[QRCodeService][{correlationId}] POST {url}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService][{correlationId}] BodyLength={jsonContent.Length} chars, storeId={maskedStoreId}");

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Thực hiện request với cả 2 token
                var response = await ExecuteWithBothTokensAsync(async token =>
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    return await _httpClient.PostAsync(url, content);
                });

                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[QRCodeService][{correlationId}] Status {(int)response.StatusCode} {response.ReasonPhrase} in {stopwatch.ElapsedMilliseconds}ms");

                var responseText = await response.Content.ReadAsStringAsync();
                var preview = responseText?.Length > 800 ? responseText.Substring(0, 800) + " ...[truncated]" : responseText;
                System.Diagnostics.Debug.WriteLine($"[QRCodeService][{correlationId}] ResponsePreview: {preview}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] Error in ScanQRCodeAndCreateTableAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QRCodeService] StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
