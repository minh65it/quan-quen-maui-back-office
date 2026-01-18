using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Models
{
    public class StoreModel
    {
        [JsonPropertyName("storeId")]
        public string? StoreId { get; set; }

        [JsonPropertyName("storeName")]
        public string? StoreName { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("ward")]
        public string? Ward { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("storeType")]
        public string? StoreType { get; set; }

        [JsonPropertyName("reviewPoint")]
        public double? ReviewPoint { get; set; }

        [JsonPropertyName("hashtag")]
        public string? Hashtag { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("createDate")]
        public DateTime? CreateDate { get; set; }

        [JsonPropertyName("distance")]
        public string? Distance { get; set; }

        [JsonPropertyName("favoriteId")]
        public string? FavoriteId { get; set; }

        [JsonPropertyName("favoritesCount")]
        public int? FavoritesCount { get; set; }

        [JsonPropertyName("promotionId")]
        public string?    PromotionId { get; set; }

        [JsonPropertyName("codePromotion")]
        public string? CodePromotion { get; set; }

        [JsonPropertyName("discount")]
        public double? Discount { get; set; }

        [JsonPropertyName("price")]
        public double? Price { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("storeNameWithoutDiacritics")]
        public string? StoreNameWithoutDiacritics { get; set; }

        [JsonPropertyName("hashTagWithoutDiacritics")]
        public string? HashTagWithoutDiacritics { get; set; }

        [JsonPropertyName("lstFoodToStore")]
        public List<object> LstFoodToStore { get; set; } = new();

        [JsonPropertyName("storePackage")]
        public int? StorePackage { get; set; }

        [JsonPropertyName("infoQRCode")]
        public object? InfoQRCode { get; set; }

        [JsonPropertyName("dayOfWeek")]
        public int? DayOfWeek { get; set; }

        [JsonPropertyName("isOpen")]
        public bool? IsOpen { get; set; }

        [JsonPropertyName("openingTime")]
        public string? OpeningTime { get; set; }

        [JsonPropertyName("closingTime")]
        public string? ClosingTime { get; set; }

        [JsonPropertyName("minPrice")]
        public double? MinPrice { get; set; }

        [JsonPropertyName("maxPrice")]
        public double? MaxPrice { get; set; }
    }

    public class PaginationInfo
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPage")]
        public int TotalPage { get; set; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("skip")]
        public int Skip { get; set; }
    }

    public class StoreApiResponse
    {
        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; }

        [JsonPropertyName("result")]
        public List<StoreModel> Result { get; set; }
    }
    public class StoreCreateRequest
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string StorePhone { get; set; }
        public string StoreEmail { get; set; }
        public string WardId { get; set; }
        public string DistrictId { get; set; }
        public string CityId { get; set; }
        public bool IsRegisterByCompany { get; set; }
        public bool IsRegisterByPersonality { get; set; }
        public string Address { get; set; }
        public string RepresentativeName { get; set; }
        public string ProvinceId { get; set; }
        public string BusinessType { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public string CategoryMaster { get; set; }
    }

    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class CreateStoreResponse
    {
        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
    }

    // Static class to store registration data between pages
    public static class StoreRegistrationData
    {
        public static string StoreName { get; set; } = string.Empty;
        public static string RepresentativeName { get; set; } = string.Empty;
        public static string Province { get; set; } = string.Empty;
        public static string District { get; set; } = string.Empty;
        public static string Ward { get; set; } = string.Empty;
        public static string Address { get; set; } = string.Empty;
        public static string CatergoryMaster { get; set; } = string.Empty;
        public static string PhoneNumber { get; set; } = string.Empty;
        public static string Email { get; set; } = string.Empty;
        public static string BusinessType { get; set; } = string.Empty;
        public static string ProvinceId { get; set; } = string.Empty;
        public static string DistrictId { get; set; } = string.Empty;
        public static string WardId { get; set; } = string.Empty;
        public static string Password { get; set; } = string.Empty;
        public static string StoreId { get; set; } = string.Empty;
        public static string StoreUserId { get; set; } = string.Empty;
        public static string UserToken { get; set; } = string.Empty; // Token từ RegisterUser endpoint
        public static List<FileResult> LicenseImageFiles { get; set; } = new();
        public static List<string> LicenseImagePaths { get; set; } = new();
    }

    public class MyImageModel
    {
        public ImageSource Images { get; set; }
        public string LocalPath { get; set; }
    }

    public class UploadImages
    {
        public byte[] Image { get; set; }
    }

    public class CategoryMediaItem
    {
        [JsonPropertyName("categoryMediaId")]
        public string CategoryMediaId { get; set; } = string.Empty;

        [JsonPropertyName("nameMedia")]
        public string NameMedia { get; set; } = string.Empty;

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; } = string.Empty;

        [JsonPropertyName("countMedia")]
        public int CountMedia { get; set; }

        [JsonPropertyName("createDate")]
        public DateTime? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTime? UpdateDate { get; set; }

        [JsonPropertyName("createBy")]
        public string CreateBy { get; set; } = string.Empty;

        [JsonPropertyName("updateBy")]
        public string UpdateBy { get; set; } = string.Empty;

        [JsonPropertyName("isDelete")]
        public bool? IsDelete { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }
    }

    public class CategoryMediaResponse
    {
        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; } = new();

        [JsonPropertyName("result")]
        public List<CategoryMediaItem> Result { get; set; } = new();
    }
}