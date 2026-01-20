using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Models
{

    public class ReferralUserLoginRequest
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool IsLoginEmail { get; set; }
    }

    public class ReferralUserResponse
    {
        public string? Message { get; set; }
        public bool Success { get; set; }
        public ReferralUserLoginData? Data { get; set; }
        public string? Caption { get; set; }
        public object? ValidateData { get; set; }
        public object? NoSMS { get; set; }
    }

    public class ReferralUserLoginData
    {
        public bool IsLogin { get; set; }
        public ReferralUserDetail? ReferralUserDetail { get; set; }
        
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        public string? Message { get; set; }
    }

    public class ReferralUserDetail
    {
        public bool EmailIsVerified { get; set; }
        public string? Note { get; set; }
        public string? Address { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public string? Avatar { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? WardId { get; set; }
        public int? DistrictId { get; set; }
        public int? ProvinceId { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PostCode { get; set; }
        public bool IsActive { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? ReferralId { get; set; }
        public string? ReferralName { get; set; }
        public string? PhoneReferrer { get; set; }
        public string? EmailReferrer { get; set; }
        public string? ReferrerType { get; set; }
        public string? ReferrerUserType { get; set; }
        public List<object>? Transactions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }



    public class AffiliateUserRegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AffiliateCreateStoreRequest
    {
        public string StoreName { get; set; }
        public string RepresentativeName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string BusinessType { get; set; }
        public string CreatedBy { get; set; }
        public string ReferralId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Added missing field
        public string StoreTypeMasterId { get; set; }
    }

    public class AffiliateCreateStoreResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public AffiliateStoreResponseData Data { get; set; }
    }

    public class AffiliateStoreResponseData
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string Status { get; set; }
    }

    public class AffiliateOtpResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    // Static class to store affiliate store data between pages
    public static class AffiliateStoreData
    {
        public static string StoreName { get; set; }
        public static string RepresentativeName { get; set; }
        public static string Address { get; set; }
        public static string PhoneNumber { get; set; }
        public static string Email { get; set; }
        public static string ProvinceId { get; set; } = string.Empty;
        public static string DistrictId { get; set; } = string.Empty;
        public static string WardId { get; set; } = string.Empty;
        public static string SelectedBusinessType { get; set; }
        public static string ReferrerId { get; set; }
        public static string RegisterToken { get; set; }
        public static string StoreId { get; set; } = string.Empty;

        // optional: keep StoreTypeMasterId in shared static data if needed by UI flow
        public static string StoreTypeMasterId { get; set; }
    }

    public class AffiliateStoreListResponse
    {
        public PaginationData Pagination { get; set; }
        public List<AffiliateStore> Result { get; set; }
    }

    public class PaginationData
    {
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
    }

    public class AffiliateStore
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string StorePhone { get; set; }
        public string StorePhone2 { get; set; }
        public string StoreLandlinePhone { get; set; }
        public string StoreLandlinePhone2 { get; set; }
        public string StoreEmail { get; set; }
        public string Address { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string NoteAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Hashtag { get; set; }
        public string Avatar { get; set; }
        public string CoverImage { get; set; }
        public double? ReviewPoint { get; set; }
        public string ImageMenu { get; set; }
        public string FrontPicIdentityCard { get; set; }
        public string BackPicIdentityCard { get; set; }
        public bool? IsDelete { get; set; }
        public string Description { get; set; }
        public bool? IsRegisterByCompany { get; set; }
        public bool? IsRegisterByPersonality { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string RepresentativeName { get; set; }
        public string RepresentativeEmail { get; set; }
        public string RepresentativePhone { get; set; }
        public string RepresentativePhoneOther { get; set; }
        public string OtherRepresentativePhone { get; set; }
        public string TaxCode { get; set; }
        public int? Status { get; set; }
        public string ReferralId { get; set; }
        public string ReferralName { get; set; }
        public string PhoneReferrer { get; set; }
        public string EmailReferrer { get; set; }
        public string ReferrerType { get; set; }
        public string Note { get; set; }
        public string Transactions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }

        // Added missing field
        public string StoreTypeMasterId { get; set; }
    }

    // Store Recommendations Models
    public class StoreRecommendationsRequest
    {
        public string LatUser { get; set; }
        public string LonUser { get; set; }
        public double? Radius { get; set; }
        public bool? IsRecently { get; set; }
        public bool? IsMap { get; set; }
        public double? LatPoint1 { get; set; }
        public double? LonPoint1 { get; set; }
        public double? LatPoint2 { get; set; }
        public double? LonPoint2 { get; set; }
        public double? LatPoint3 { get; set; }
        public double? LonPoint3 { get; set; }
        public double? LatPoint4 { get; set; }
        public double? LonPoint4 { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    public class StoreRecommendationsResponse
    {
        public PaginationData Pagination { get; set; }
        public List<StoreRecommendation> Result { get; set; }
    }

    public class StoreRecommendation
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreSlug { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string StoreType { get; set; }
        public double ReviewPoint { get; set; }
        public string Hashtag { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Distance { get; set; }
        public string StoreNameWithoutDiacritics { get; set; }
        public string HashTagWithoutDiacritics { get; set; }
    }

    // Preview Data Agreement Models
    public class PreviewCrawlDataAgreementRequest
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string RepresentativeName { get; set; }
        public string AffiliateName { get; set; }
        public string AffiliatePhoneNumber { get; set; }
        public byte[]? SignatureFile { get; set; } // Store owner signature
        public byte[]? SignatureFileAffilate { get; set; } // Affiliate signature
        public byte[]? SignatureQrFile { get; set; } // QR signature (optional)
    }

    public class PreviewCrawlDataAgreementResponse
    {
        public string Message { get; set; } // PDF file path
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Caption { get; set; }
        public object? ValidateData { get; set; }
        public object? NoSMS { get; set; }
    }

    // Affiliate Statistics Models
    public class AffiliateStatisticsResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public AffiliateStatisticsData Data { get; set; }
        public string? Caption { get; set; }
        public object? ValidateData { get; set; }
        public object? NoSMS { get; set; }
    }

    public class AffiliateStatisticsData
    {
        public int TotalStores { get; set; }
        public int PendingStores { get; set; }
        public int ApprovedStores { get; set; }
        public int InActiveStores { get; set; }
        public int BlockedStores { get; set; }
        public int TotalTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int FinishedTransactions { get; set; }
        public int CanceledTransactions { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal PendingCommission { get; set; }
        public decimal AverageCommission { get; set; }
        public int StoresWithContract { get; set; }
        public int StoresWithoutContract { get; set; }
        public int StoresWithTransaction { get; set; }
        public int StoresWithoutTransaction { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastStoreReferralDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }

    // Assign Store Models
    public class AssignStoreRequest
    {
        public string? ReferralUserId { get; set; }
        public string? StoreId { get; set; }
        public string? AssignedDate { get; set; }
        public string? AssignedBy { get; set; }
    }

    public class AssignStoreResponse
    {
        public string? Message { get; set; }
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Caption { get; set; }
        public object? ValidateData { get; set; }
        public object? NoSMS { get; set; }
    }

    // Report Store Models
    public class ReportStoreRequest
    {
        public string? UserId { get; set; }
        public string? StoreId { get; set; }
        public string? ObjectId { get; set; }
        public string? ReportType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public byte[]? Files { get; set; }
        public string? CreateBy { get; set; }
        public string? UrlContent { get; set; }
    }

    public class ReportStoreResponse
    {
        public string? Message { get; set; }
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Caption { get; set; }
        public object? ValidateData { get; set; }
        public object? NoSMS { get; set; }
    }
}
