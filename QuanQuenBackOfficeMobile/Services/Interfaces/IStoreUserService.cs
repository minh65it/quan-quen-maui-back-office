using System;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface IStoreUserService
{
    /// <summary>
    /// Register a new store user
    /// </summary>
    /// <param name="request">Store user registration request</param>
    /// <returns>Registration response</returns>
    Task<StoreUserRegistrationResponse> RegisterUserAsync(StoreUserRegistrationRequest request);

    /// <summary>
    /// Send verification code to user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Response indicating success or failure</returns>
    Task<ApiResponse> SendCodeEmailAsync(string email);

    /// <summary>
    /// Verify OTP code sent to user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="code">OTP code</param>
    /// <returns>Response indicating success or failure</returns>
    Task<ApiResponse> VerifyCodeEmailAsync(string email, string code);

    /// <summary>
    /// LoginV2 for store user (email or phone)
    /// </summary>
    //Task<LoginV2ApiResponse> LoginV2Async(string emailOrPhoneNumber, string password, bool isEmail);

    /// <summary>
    /// Lấy danh sách level
    /// </summary>
    Task<LevelApiResponse> GetListLevelAsync();

    /// <summary>
    /// Cập nhật level cho nhà hàng
    /// </summary>
    Task<ApiResponse> UpdateStoreLevelAsync(string storeId, int levelId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Lấy chi tiết CSR theo URL
    /// </summary>
    /// <param name="url">URL của CSR (policy hoặc terms-of-service)</param>
    /// <returns>Chi tiết CSR</returns>
    Task<CsrDetailResponse> GetCsrDetailAsync(string url);
} 