using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Models
{
    public class StoreModel
    {
        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }

        [JsonPropertyName("storeName")]
        public string StoreName { get; set; }

        [JsonPropertyName("storePhone")]
        public string StorePhone { get; set; }

        [JsonPropertyName("storePhone2")]
        public string StorePhone2 { get; set; }

        [JsonPropertyName("storeLandlinePhone")]
        public string StoreLandlinePhone { get; set; }

        [JsonPropertyName("storeLandlinePhone2")]
        public string StoreLandlinePhone2 { get; set; }

        [JsonPropertyName("storeEmail")]
        public string StoreEmail { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("ward")]
        public string Ward { get; set; }

        [JsonPropertyName("district")]
        public string District { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("noteAddress")]
        public string NoteAddress { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("hashtag")]
        public string Hashtag { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("coverImage")]
        public string CoverImage { get; set; }

        [JsonPropertyName("reviewPoint")]
        public double? ReviewPoint { get; set; }

        [JsonPropertyName("imageMenu")]
        public string ImageMenu { get; set; }

        [JsonPropertyName("frontPicIdentityCard")]
        public string FrontPicIdentityCard { get; set; }

        [JsonPropertyName("backPicIdentityCard")]
        public string BackPicIdentityCard { get; set; }

        [JsonPropertyName("isDelete")]
        public bool? IsDelete { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("isRegisterByCompany")]
        public bool? IsRegisterByCompany { get; set; }

        [JsonPropertyName("isRegisterByPersonality")]
        public bool? IsRegisterByPersonality { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }

        [JsonPropertyName("companyAddress")]
        public string CompanyAddress { get; set; }

        [JsonPropertyName("representativeName")]
        public string RepresentativeName { get; set; }

        [JsonPropertyName("representativeEmail")]
        public string RepresentativeEmail { get; set; }

        [JsonPropertyName("representativePhone")]
        public string RepresentativePhone { get; set; }

        [JsonPropertyName("representativePhoneOther")]
        public string RepresentativePhoneOther { get; set; }

        [JsonPropertyName("otherRepresentativePhone")]
        public string OtherRepresentativePhone { get; set; }

        [JsonPropertyName("taxCode")]
        public string TaxCode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("createBy")]
        public string CreateBy { get; set; }

        [JsonPropertyName("updateBy")]
        public string UpdateBy { get; set; }

        [JsonPropertyName("createDate")]
        public DateTime? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTime? UpdateDate { get; set; }

        [JsonPropertyName("fullAdrress")]
        public string FullAddress { get; set; }

        [JsonPropertyName("wardId")]
        public Guid? WardId { get; set; }

        [JsonPropertyName("districtId")]
        public Guid? DistrictId { get; set; }

        [JsonPropertyName("cityId")]
        public Guid? CityId { get; set; }

        [JsonPropertyName("storeTypeIds")]
        public List<Guid> StoreTypeIds { get; set; }

        [JsonPropertyName("listImageViewDetail")]
        public List<string> ListImageViewDetail { get; set; }

        [JsonPropertyName("listImageViewDetail1")]
        public List<object> ListImageViewDetail1 { get; set; }

        [JsonPropertyName("infoQRCode")]
        public object InfoQRCode { get; set; }

        [JsonPropertyName("companyWard")]
        public string CompanyWard { get; set; }

        [JsonPropertyName("companyDistrict")]
        public string CompanyDistrict { get; set; }

        [JsonPropertyName("companyCity")]
        public string CompanyCity { get; set; }

        [JsonPropertyName("companyWardId")]
        public Guid? CompanyWardId { get; set; }

        [JsonPropertyName("companyDistrictId")]
        public Guid? CompanyDistrictId { get; set; }

        [JsonPropertyName("companyCityId")]
        public Guid? CompanyCityId { get; set; }

        [JsonPropertyName("favoritesCount")]
        public int? FavoritesCount { get; set; }

        [JsonPropertyName("levelId")]
        public int? LevelId { get; set; }

        [JsonPropertyName("createDateLevel")]
        public DateTime? CreateDateLevel { get; set; }

        [JsonPropertyName("startDateLevel")]
        public DateTime? StartDateLevel { get; set; }

        [JsonPropertyName("expirationDateLevel")]
        public DateTime? ExpirationDateLevel { get; set; }

        [JsonPropertyName("distance")]
        public string Distance { get; set; }

        [JsonPropertyName("levelName")]
        public string LevelName { get; set; }
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

    public class StoreTypeModel
    {
        public string StoreTypeId { get; set; }
        public string StoreTypeName { get; set; }
        public string Description { get; set; }
        public string UrlImage { get; set; }
        public int Position { get; set; }
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
    }

    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
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
        public static string PhoneNumber { get; set; } = string.Empty;
        public static string Email { get; set; } = string.Empty;
        public static string BusinessType { get; set; } = string.Empty;
        public static string ProvinceId { get; set; } = string.Empty;
        public static string DistrictId { get; set; } = string.Empty;
        public static string WardId { get; set; } = string.Empty;
        public static string Password { get; set; } = string.Empty;
        public static string StoreId { get; set; } = string.Empty;
        public static List<FileResult> LicenseImageFiles { get; set; } = new();
        public static List<string> LicenseImagePaths { get; set; } = new();
    }

    public class MyImageModel
    {
        public ImageSource LicenseImage { get; set; }
        public string LocalPath { get; set; }
    }

    public class UploadImages
    {
        public byte[] Image { get; set; }
    }
} 