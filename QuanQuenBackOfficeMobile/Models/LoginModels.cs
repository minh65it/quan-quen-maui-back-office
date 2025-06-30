using System;
using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Models
{
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("isLogin")]
        public bool IsLogin { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("vsiriusUserDto")]
        public VsiriusUserDto VsiriusUserDto { get; set; }
    }
    public class VsiriusUserDto
    {

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }


        [JsonPropertyName("email")]
        public string Email { get; set; }

    }

    //public class LoginV2ApiResponse
    //{
    //    public string Message { get; set; }
    //    public bool Success { get; set; }
    //    public LoginV2Data Data { get; set; }
    //}

    //public class LoginV2Data
    //{
    //    public bool IsLogin { get; set; }
    //    public StoreUserDto StoreUserDto { get; set; }
    //    public string Token { get; set; }
    //    public object[] ListPathPermission { get; set; }
    //    public string Message { get; set; }
    //    public object Menu { get; set; }
    //}

    //public class StoreUserDto
    //{
    //    public string UserId { get; set; }
    //    public string StoreId { get; set; }
    //    public string UserName { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public bool PhoneNumberIsVerified { get; set; }
    //    public string Email { get; set; }
    //    public bool EmailIsVerified { get; set; }
    //    public string Address { get; set; }
    //    public string Ward { get; set; }
    //    public string District { get; set; }
    //    public string Province { get; set; }
    //    public string Country { get; set; }
    //    public string Avatar { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string FullName { get; set; }
    //    public string WardId { get; set; }
    //    public string DistrictId { get; set; }
    //    public string ProvinceId { get; set; }
    //    public string Gender { get; set; }
    //    public DateTime? BirthDate { get; set; }
    //    public string PostCode { get; set; }
    //    public bool IsActive { get; set; }
    //    public bool? IsDelete { get; set; }
    //    public string UpdateBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public DateTime CreatedDate { get; set; }
    //    public bool IsLoginGoogle { get; set; }
    //    public string IdGoogle { get; set; }
    //    public bool IsLoginFacebook { get; set; }
    //    public string IdFacebook { get; set; }
    //    public bool IsLoginZalo { get; set; }
    //    public string IdZalo { get; set; }
    //}



    
} 