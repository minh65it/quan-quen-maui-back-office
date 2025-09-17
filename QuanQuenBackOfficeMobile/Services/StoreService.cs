using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Storage;

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

                // Tạo request body
                var requestBody = new
                {
                    contentSearch = searchText ?? ""
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var token = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Token: {token}");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No authentication token found");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Log request details
                System.Diagnostics.Debug.WriteLine($"[StoreService] Request URL: {url}");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Request Method: POST");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Request Headers: Authorization=Bearer {token.Substring(0, Math.Min(20, token.Length))}...");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Request Body: {jsonContent}");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] Response: {responseContent}");

                var result = JsonSerializer.Deserialize<StoreApiResponse>(responseContent, new JsonSerializerOptions
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

        public async Task<CreateStoreResponse> CreateStoreAsync(StoreCreateRequest request, string token = null)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:CreateStores"];
                var url = $"{baseUrl}{path}";

                using var form = new MultipartFormDataContent();

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

                // Sử dụng token được truyền vào hoặc lấy từ SecureStorage
                var authToken = token ?? await SecureStorage.GetAsync("auth_token");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                var response = await _httpClient.PostAsync(url, form);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] Response thành công: {content}");
                var result = JsonSerializer.Deserialize<CreateStoreResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Fix: Nếu message là 'Create Success' nhưng IsSuccess là false, thì vẫn trả về IsSuccess = true
                if (result != null && !result.IsSuccess && result.Message != null && result.Message.Contains("Create Success"))
                {
                    result.IsSuccess = true;
                }

                return result ?? new CreateStoreResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                return new CreateStoreResponse
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
            return JsonSerializer.Deserialize<StoreDetailModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        public async Task<List<FilterStoreModel>> GetUtilitiesAsync()
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetUtilities"];
            var url = $"{baseUrl}{path}?filterType=1";
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("No authentication token found");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var result = root.GetProperty("result");
            var utilities = new List<FilterStoreModel>();
            foreach (var item in result.EnumerateArray())
            {
                utilities.Add(new FilterStoreModel
                {
                    FilterStoreId = item.GetProperty("filterStoreId").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    Value = item.GetProperty("value").GetString(),
                    UrlImage = item.GetProperty("urlImage").GetString(),
                    FilterTypeStore = item.GetProperty("filterTypeStore").GetInt32()
                });
            }
            return utilities;
        }
        public async Task<List<FilterStoreModel>> GetSuitabilitiesAsync()
        {
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetUtilities"];
            var url = $"{baseUrl}{path}?filterType=2";
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("No authentication token found");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var result = root.GetProperty("result");
            var utilities = new List<FilterStoreModel>();
            foreach (var item in result.EnumerateArray())
            {
                utilities.Add(new FilterStoreModel
                {
                    FilterStoreId = item.GetProperty("filterStoreId").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    Value = item.GetProperty("value").GetString(),
                    UrlImage = item.GetProperty("urlImage").GetString(),
                    FilterTypeStore = item.GetProperty("filterTypeStore").GetInt32()
                });
            }
            return utilities;
        }

        public async Task<ApiResponse> CreateStoreTypeAsync(List<string> filterStoreIds, string storeId)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:CreateStoreFilter"];
                var url = $"{baseUrl}{path}";

                var requestBody = new
                {
                    filterStoreIds = filterStoreIds,
                    storeId = storeId
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedAccessException("No authentication token found");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] CreateStoreTypeAsync response: {responseContent}");

                var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] CreateStoreTypeAsync error: {ex.Message}");
                return new ApiResponse { IsSuccess = false, Message = $"Create store type failed: {ex.Message}" };
            }
        }

        public async Task<List<StoreTypeModel>> GetStoreTypeAsync()
        {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:GetStoreType"];
                var url = $"{baseUrl}{path}";
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedAccessException("No authentication token found");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var result = root.GetProperty("result");
                var storeTypeList = new List<StoreTypeModel>();
                foreach (var item in result.EnumerateArray())
                {
                    storeTypeList.Add(new StoreTypeModel
                    {
                        storeTypeId = item.GetProperty("storeTypeId").GetString(),
                        storeTypeName = item.GetProperty("storeTypeName").GetString(),
                        description = item.GetProperty("description").GetString(),
                        urlImage = item.GetProperty("urlImage").GetString(),
                    });
                }
                return storeTypeList;
        }

        public async Task<ApiResponse> GenerateFoodFromImageAsync(string storeId, List<string> imagePaths)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedAccessException("No authentication token found");

                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:GenerateFoodFromImage"];
                var url = $"{baseUrl}{path}";

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // API mẫu yêu cầu 1 file mỗi request với field name 'File' và có 'StoreId', 'Url'
                // Nếu có nhiều ảnh, gửi tuần tự từng ảnh và tổng hợp kết quả
                bool allSuccess = true;
                string lastMessage = string.Empty;

                foreach (var imagePath in imagePaths)
                {
                    if (!System.IO.File.Exists(imagePath))
                    {
                        allSuccess = false;
                        lastMessage = $"File not found: {imagePath}";
                        continue;
                    }

                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(storeId), "StoreId");
                    form.Add(new StringContent(string.Empty), "Url");

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                    var byteArrayContent = new ByteArrayContent(fileBytes);

                    var ext = System.IO.Path.GetExtension(imagePath).ToLowerInvariant();
                    var mediaType = ext switch
                    {
                        ".png" => "image/png",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".webp" => "image/webp",
                        _ => "application/octet-stream"
                    };
                    byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
                    form.Add(byteArrayContent, "File", System.IO.Path.GetFileName(imagePath));

                    System.Diagnostics.Debug.WriteLine($"[GenerateFoodFromImage] POST {url}");
                    System.Diagnostics.Debug.WriteLine($"[GenerateFoodFromImage] StoreId={storeId}, File={System.IO.Path.GetFileName(imagePath)}, ContentType={mediaType}");
                    var response = await _httpClient.PostAsync(url, form);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[GenerateFoodFromImage] Status={(int)response.StatusCode} {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[GenerateFoodFromImage] RawResponse={responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        allSuccess = false;
                        lastMessage = responseContent;
                        continue;
                    }

                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(responseContent);
                        var root = doc.RootElement;
                        bool success = false;
                        string? message = null;
                        if (root.TryGetProperty("success", out var successProp)) success = successProp.GetBoolean();
                        if (root.TryGetProperty("message", out var messageProp)) message = messageProp.GetString();
                        System.Diagnostics.Debug.WriteLine($"[GenerateFoodFromImage] Parsed success={success}, message={message}");
                        if (!success)
                        {
                            allSuccess = false;
                            lastMessage = message ?? "Unknown error";
                        }
                        else
                        {
                            lastMessage = message ?? "Tạo món ăn thành công";
                        }
                    }
                    catch
                    {
                        // Fallback deserialize sang ApiResponse nếu có đúng schema
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        System.Diagnostics.Debug.WriteLine($"[GenerateFoodFromImage] Fallback parsed: isSuccess={parsed?.IsSuccess}, message={parsed?.Message}");
                        if (parsed == null || (!parsed.IsSuccess))
                        {
                            allSuccess = false;
                            lastMessage = parsed?.Message ?? "Deserialize error";
                        }
                        else
                        {
                            lastMessage = parsed.Message ?? "Tạo món ăn thành công";
                        }
                    }
                }

                return new ApiResponse
                {
                    IsSuccess = allSuccess,
                    Message = string.IsNullOrWhiteSpace(lastMessage) ? (allSuccess ? "Tạo món ăn thành công" : "Có ảnh thất bại") : lastMessage
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> CreateStoreTypeToStoreAsync(List<string> storeTypeIds, string storeId)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = "api/StoreTypeToStore/CreateStoreTypeToStore";
                var url = $"{baseUrl}{path}";

                var requestBody = new
                {
                    storeTypeIds = storeTypeIds,
                    storeId = storeId
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedAccessException("No authentication token found");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] CreateStoreTypeToStoreAsync response: {responseContent}");
                
                var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] CreateStoreTypeToStoreAsync error: {ex.Message}");
                return new ApiResponse { IsSuccess = false, Message = $"Create store type to store failed: {ex.Message}" };
            }
        }

        public async Task<bool> UpdateStoreLocationAsync(string storeId, double latitude, double longitude, string address, string ward = null, string district = null, string city = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] UpdateStoreLocationAsync started for store: {storeId}");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Request parameters - Lat: {latitude}, Lng: {longitude}, Address: {address}, Ward: {ward}, District: {district}, City: {city}");

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("[StoreService] No auth token found");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"[StoreService] Auth token found: {token.Substring(0, Math.Min(20, token.Length))}...");

                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:UpdateStore"];
                var url = $"{baseUrl}{path}";
                
                System.Diagnostics.Debug.WriteLine($"[StoreService] API URL: {url}");

                using var form = new MultipartFormDataContent();
                
                // Thêm các field cần thiết
                form.Add(new StringContent(storeId), "StoreId");
                form.Add(new StringContent(address ?? string.Empty), "Address");
                form.Add(new StringContent(ward ?? string.Empty), "Ward");
                form.Add(new StringContent(district ?? string.Empty), "District");
                form.Add(new StringContent(city ?? string.Empty), "City");
                form.Add(new StringContent(latitude.ToString("F6")), "Latitude");
                form.Add(new StringContent(longitude.ToString("F6")), "Longitude");

                System.Diagnostics.Debug.WriteLine($"[StoreService] Form data prepared - StoreId: {storeId}, Address: {address}, Ward: {ward}, District: {district}, City: {city}, Lat: {latitude:F6}, Lng: {longitude:F6}");

                using var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = form
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                
                System.Diagnostics.Debug.WriteLine($"[StoreService] Response received - Status: {response.StatusCode}, IsSuccess: {response.IsSuccessStatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[StoreService] Response content: {responseContent}");
                    
                    // Parse response theo format thực tế của API
                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(responseContent);
                        var root = jsonDoc.RootElement;
                        bool isSuccess = false;
                        string message = "";
                        if (root.TryGetProperty("success", out var successProp))
                        {
                            isSuccess = successProp.GetBoolean();
                        }
                        else if (root.TryGetProperty("isSuccess", out var isSuccessProp))
                        {
                            isSuccess = isSuccessProp.GetBoolean();
                        }
                        if (root.TryGetProperty("message", out var messageProp))
                        {
                            message = messageProp.GetString() ?? "";
                        }
                        System.Diagnostics.Debug.WriteLine($"[StoreService] Final result - isSuccess: {isSuccess}, message: {message}");
                        return isSuccess;
                    }
                    catch (JsonException jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[StoreService] JSON parsing error: {jsonEx.Message}");
                        var containsSuccess = responseContent.Contains("\"success\":true") || responseContent.Contains("\"isSuccess\":true");
                        return containsSuccess;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[StoreService] UpdateStoreLocationAsync failed with status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Error response content: {errorContent}");
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreService] UpdateStoreLocationAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[StoreService] Exception type: {ex.GetType().Name}");
                return false;
            }
        }
    }
} 