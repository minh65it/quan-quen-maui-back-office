using System;
using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Models;

public class StoreUserRegistrationRequest
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("storeId")]
    public string StoreId { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("ward")]
    public string Ward { get; set; } = string.Empty;

    [JsonPropertyName("district")]
    public string District { get; set; } = string.Empty;

    [JsonPropertyName("province")]
    public string Province { get; set; } = string.Empty;

    [JsonPropertyName("wardId")]
    public string WardId { get; set; } = string.Empty;

    [JsonPropertyName("districtId")]
    public string DistrictId { get; set; } = string.Empty;

    [JsonPropertyName("provinceId")]
    public string ProvinceId { get; set; } = string.Empty;

    [JsonPropertyName("storeName")]
    public string StoreName { get; set; } = string.Empty;

    [JsonPropertyName("representativeName")]
    public string RepresentativeName { get; set; } = string.Empty;

    [JsonPropertyName("businessType")]
    public string BusinessType { get; set; } = string.Empty;
}

public class StoreUserRegistrationResponse
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string StoreId { get; set; } = string.Empty;
}

public class CsrDetailResponse
{
    public string title { get; set; }
    public string description { get; set; }
} 