using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Diagnostics;
using System.Linq;

namespace QuanQuenBackOfficeMobile.Services;

public class StoreMediaService : IStoreMediaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public StoreMediaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetThumbnailCategoryMediaIdAsync(string nameMedia)
    {
        try
        {
            if (string.IsNullOrEmpty(nameMedia))
            {
                Debug.WriteLine("[StoreMediaService] GetThumbnailCategoryMediaIdAsync - nameMedia is null or empty");
                return null;
            }

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("[StoreMediaService] GetThumbnailCategoryMediaIdAsync - No auth token found");
                return null;
            }

            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetCategoryMedia"];
            var apiUrl = $"{baseUrl}/{path}?storeId=default";

            Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync - API URL: {apiUrl}");
            Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync - Looking for nameMedia: {nameMedia}");

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync - Response: {result}");

            // Deserialize response thành object
            var categoryMediaResponse = JsonSerializer.Deserialize<CategoryMediaResponse>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (categoryMediaResponse?.Result != null)
            {
                Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync - Found {categoryMediaResponse.Result.Count} categories");
                
                // Sử dụng LINQ để tìm categoryMediaId
                var category = categoryMediaResponse.Result.FirstOrDefault(item => item.NameMedia == nameMedia);

                if (category != null)
                {
                    Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync - Found categoryMediaId: {category.CategoryMediaId} for nameMedia: {nameMedia}");
                    return category.CategoryMediaId;
                }
                else
                {
                    Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync - No categoryMediaId found for nameMedia: {nameMedia}");
                }
            }
            else
            {
                Debug.WriteLine("[StoreMediaService] GetThumbnailCategoryMediaIdAsync - Response is null or does not contain result");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync Exception: {ex}");
        }
        return null;
    }

    public async Task<ApiResponse> UploadMediasAsync(string storeId, string categoryMediaId, string createdBy, List<byte[]> imageBytesList, bool isSecurity = false, bool isMenu = false)
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:CreateMedia"];
            var apiUrl = $"{baseUrl}{path}";

            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Start upload for storeId: {storeId}");
            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - API URL: {apiUrl}");
            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - CategoryMediaId: {categoryMediaId}");
            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - CreatedBy: {createdBy}");
            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Total images to upload: {imageBytesList.Count}");

            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(storeId), "StoreId");
            content.Add(new StringContent(""), "ObjectId");
            content.Add(new StringContent(categoryMediaId ?? ""), "CategoryMediaId");
            content.Add(new StringContent(isSecurity ? "true" : "false"), "IsSecurity");
            content.Add(new StringContent(createdBy), "CreateBy");
            content.Add(new StringContent(isMenu ? "true":"false"), "IsMenu");

            foreach (var bytes in imageBytesList)
            {
                // Sử dụng tên file đơn giản để tránh lỗi Azure Storage
                var fileName = $"{Guid.NewGuid()}.jpg";
                Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Adding image: {fileName}, Size: {bytes.Length} bytes");
                content.Add(new ByteArrayContent(bytes), "FileMedia", fileName);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Debug.WriteLine("[StoreMediaService] UploadStoreImagesAsync - Sending request...");
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Response Content: {result}");

            string message = "Không rõ kết quả!";
            bool isSuccess = false;

            try
            {
                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var msgProp))
                {
                    message = msgProp.GetString() ?? message;
                    Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Response Message: {message}");
                }
                if (root.TryGetProperty("success", out var successProp))
                {
                    isSuccess = successProp.GetBoolean();
                    Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Success: {isSuccess}");
                }
            }
            catch (Exception ex)
            {
                message = $"Không parse được response: {ex.Message}";
                Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Parse Response Error: {ex}");
            }

            return new ApiResponse { IsSuccess = isSuccess, Message = message };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMediaService] UploadStoreImagesAsync - Exception: {ex}");
            return new ApiResponse { IsSuccess = false, Message = $"Có lỗi khi upload ảnh: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> UploadInvoiceImageAsync(string storeId, int month, byte[] imageBytes, string fileName)
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("[StoreMediaService] UploadInvoiceImageAsync - No auth token found");
                return new ApiResponse { IsSuccess = false, Message = "Không tìm thấy token xác thực" };
            }

            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Token found: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");

            var url = $"/private/InvoiceImage/{month}/{storeId}.png";
            var baseUrl = _configuration["API:Asset:ImageUrl"];
            var uploadPath = _configuration["API:Asset:UploadFile"];
            var apiUrl = $"{baseUrl}/api/{uploadPath}?Url={Uri.EscapeDataString(url)}";
            
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Base URL from config: {baseUrl}");
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Upload path from config: {uploadPath}");

            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - API URL: {apiUrl}");
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - StoreId: {storeId}");
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Month: {month}");
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Image size: {imageBytes.Length} bytes");

            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(imageBytes), "File", fileName);

            using var assetHttpClient = new HttpClient();
            assetHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };

            Debug.WriteLine("[StoreMediaService] UploadInvoiceImageAsync - Sending request...");
            var response = await assetHttpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Response Content: {result}");

            string message = "Không rõ kết quả!";
            bool isSuccess = false;

            // Kiểm tra status code trước
            if (response.IsSuccessStatusCode)
            {
                // Nếu thành công, thử parse JSON
                try
                {
                    if (!string.IsNullOrEmpty(result) && result.Trim().StartsWith("{"))
                    {
                        using var doc = JsonDocument.Parse(result);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("message", out var msgProp))
                        {
                            message = msgProp.GetString() ?? message;
                            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Response Message: {message}");
                        }
                        if (root.TryGetProperty("success", out var successProp))
                        {
                            isSuccess = successProp.GetBoolean();
                            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Success: {isSuccess}");
                        }
                    }
                    else
                    {
                        // Nếu không phải JSON, coi như thành công
                        isSuccess = true;
                        message = "Upload thành công";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Parse Response Error: {ex}");
                    // Nếu parse lỗi nhưng status code thành công, coi như thành công
                    isSuccess = true;
                    message = "Upload thành công";
                }
            }
            else
            {
                // Nếu status code không thành công, sử dụng response content làm message
                isSuccess = false;
                message = string.IsNullOrEmpty(result) ? $"HTTP {(int)response.StatusCode} {response.StatusCode}" : result;
                Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Error Response: {message}");
            }

            return new ApiResponse { IsSuccess = isSuccess, Message = message };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMediaService] UploadInvoiceImageAsync - Exception: {ex}");
            return new ApiResponse { IsSuccess = false, Message = $"Có lỗi khi upload ảnh hóa đơn: {ex.Message}" };
        }
    }
} 
