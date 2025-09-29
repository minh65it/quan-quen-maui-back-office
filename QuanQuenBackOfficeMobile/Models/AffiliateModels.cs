using System.ComponentModel.DataAnnotations;

namespace QuanQuenBackOfficeMobile.Models
{
    public class ReferralUserRequest
    {
        public string? ReferralName { get; set; }
        public string? PhoneReferrer { get; set; }
        public string? EmailReferrer { get; set; }
        public string? Password { get; set; }
        public string? ReferrerType { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public string? CreatedBy { get; set; }
    }

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
        public List<object>? Transactions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }

    public enum ReferrerType
    {
        [Display(Name = "Cá Nhân")]
        Personal = 1,
        
        [Display(Name = "Công Ty")]
        Company = 2
    }

    public class AffiliateUserRegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AffiliateUserRegisterResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public AffiliateUserData Data { get; set; }
    }

    public class AffiliateUserData
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string ReferrerId { get; set; }
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
        public List<Microsoft.Maui.Storage.FileResult> LicenseImages { get; set; }
        public string CreatedBy { get; set; }
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
        public static List<Microsoft.Maui.Storage.FileResult> LicenseImageFiles { get; set; } = new();
        public static string ReferrerId { get; set; }
        public static string RegisterToken { get; set; }
        public static string StoreId { get; set; } = string.Empty;
    }
}
