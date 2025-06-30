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
            var token = await SecureStorage.GetAsync("auth_token");
            var baseUrl = _configuration["API:Store:BaseUrl"];
            var path = _configuration["API:Store:GetCategoryMedia"];
            var apiUrl = $"{baseUrl}/{path}?storeId=default";

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            if (root.TryGetProperty("result", out var resultArray) && resultArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in resultArray.EnumerateArray())
                {
                    if (item.TryGetProperty("nameMedia", out var nameProp) && nameProp.GetString() == nameMedia)
                    {
                        return item.GetProperty("categoryMediaId").GetString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMediaService] GetThumbnailCategoryMediaIdAsync Exception: {ex}");
        }
        return null;
    }

    public async Task<ApiResponse> UploadMediasAsync(string storeId, string categoryMediaId, string createdBy, List<byte[]> imageBytesList, bool isSecurity = false)
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
            content.Add(new StringContent("false"), "IsMenu");

            foreach (var bytes in imageBytesList)
            {
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
} 