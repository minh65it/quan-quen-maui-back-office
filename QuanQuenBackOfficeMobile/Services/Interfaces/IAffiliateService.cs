using QuanQuenBackOfficeMobile.Models;

namespace QuanQuenBackOfficeMobile.Services.Interfaces
{
    public interface IAffiliateService
    {
        Task<ReferralUserResponse> RegisterReferralUserAsync(ReferralUserRequest request);
        Task<ReferralUserResponse> LoginReferralUserAsync(ReferralUserLoginRequest request);
        Task<StoreUserRegistrationResponse> RegisterAffiliateUserAsync(AffiliateUserRegisterRequest request);
        Task<CreateStoreResponse> CreateAffiliateStoreAsync(AffiliateCreateStoreRequest request, string token);
        Task<ApiResponse> SendAffiliateOtpAsync(string email);
        Task<ApiResponse> VerifyAffiliateOtpAsync(string email, string otpCode);
        Task<AffiliateStoreListResponse> GetStoresByReferralIdAsync(string referralId, string token);
        Task<StoreRecommendationsResponse> GetStoreRecommendationsByLocationAsync(StoreRecommendationsRequest request, string token);
        Task<PreviewCrawlDataAgreementResponse> PreviewCrawlDataAgreementAsync(PreviewCrawlDataAgreementRequest request, string token);
    }
}
