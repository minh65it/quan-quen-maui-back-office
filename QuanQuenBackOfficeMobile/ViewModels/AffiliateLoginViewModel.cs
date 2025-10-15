using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Views.Stores;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public class AffiliateLoginViewModel : BaseViewModel
    {
        private readonly IAffiliateService _affiliateService;
        private string _affiliateCode;
        private string _password;
        private bool _isBusy;
        private bool _showEmptyAffiliateCodeError;
        private bool _showEmptyPasswordError;
        private bool _showLengthPasswordError;
        private string _authError;
        private bool _isPasswordHidden = true;
        private string _passwordVisibilityIcon = "🙈";

        public AffiliateLoginViewModel(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
            LoginCommand = new Command(async () => await LoginAsync());
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
            BackToNormalLoginCommand = new Command(async () => await BackToNormalLoginAsync());
            GoToAffiliateRegisterCommand = new Command(async () => await GoToAffiliateRegisterAsync());
        }

        public string AuthError
        {
            get => _authError;
            set
            {
                if (SetProperty(ref _authError, value))
                    OnPropertyChanged(nameof(ShowAuthError));
            }
        }

        public bool ShowAuthError => !string.IsNullOrEmpty(AuthError);

        public string AffiliateCode
        {
            get => _affiliateCode;
            set
            {
                if (SetProperty(ref _affiliateCode, value))
                {
                    // Clear error when user starts typing
                    if (!string.IsNullOrEmpty(value))
                        ShowEmptyAffiliateCodeError = false;
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    // Clear error when user starts typing
                    if (!string.IsNullOrEmpty(value))
                    {
                        ShowEmptyPasswordError = false;
                        ShowLengthPasswordError = false;
                    }
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool ShowEmptyAffiliateCodeError
        {
            get => _showEmptyAffiliateCodeError;
            set => SetProperty(ref _showEmptyAffiliateCodeError, value);
        }

        public bool ShowEmptyPasswordError
        {
            get => _showEmptyPasswordError;
            set => SetProperty(ref _showEmptyPasswordError, value);
        }

        public bool ShowLengthPasswordError
        {
            get => _showLengthPasswordError;
            set => SetProperty(ref _showLengthPasswordError, value);
        }

        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set => SetProperty(ref _isPasswordHidden, value);
        }

        public string PasswordVisibilityIcon
        {
            get => _passwordVisibilityIcon;
            set => SetProperty(ref _passwordVisibilityIcon, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand BackToNormalLoginCommand { get; }
        public ICommand GoToAffiliateRegisterCommand { get; }

        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
            PasswordVisibilityIcon = IsPasswordHidden ? "🙈" : "🐵";
        }

        private async Task GoToAffiliateRegisterAsync()
        {
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//AffiliateRegister");
                }
                else
                {
                    // Fallback: Navigate using Application.Current.MainPage
                    var affiliateRegisterViewModel = IPlatformApplication.Current.Services.GetService<AffiliateRegisterViewModel>();
                    Application.Current.MainPage = new AffiliateRegisterPage(affiliateRegisterViewModel);
                }
            }
            catch (Exception ex)
            {
                // Fallback navigation
                var affiliateRegisterViewModel = IPlatformApplication.Current.Services.GetService<AffiliateRegisterViewModel>();
                Application.Current.MainPage = new AffiliateRegisterPage(affiliateRegisterViewModel);
            }
        }

        private async Task BackToNormalLoginAsync()
        {
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//Login");
                }
                else
                {
                    // Fallback: Navigate using Application.Current.MainPage
                    var loginViewModel = IPlatformApplication.Current.Services.GetService<LoginViewModel>();
                    Application.Current.MainPage = new LoginPage(loginViewModel);
                }
            }
            catch (Exception ex)
            {
                // Fallback navigation
                var loginViewModel = IPlatformApplication.Current.Services.GetService<LoginViewModel>();
                Application.Current.MainPage = new LoginPage(loginViewModel);
            }
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ShowEmptyAffiliateCodeError = ShowEmptyPasswordError = ShowLengthPasswordError = false;
                AuthError = string.Empty;

                // Validate affiliate code
                if (string.IsNullOrWhiteSpace(AffiliateCode))
                {
                    ShowEmptyAffiliateCodeError = true;
                }

                // Validate password
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowEmptyPasswordError = true;
                }

                if (ShowEmptyAffiliateCodeError || ShowEmptyPasswordError)
                {
                    IsBusy = false;
                    return;
                }

                // Check password length
                if (Password.Length < 6)
                {
                    ShowLengthPasswordError = true;
                    IsBusy = false;
                    return;
                }

                // Call affiliate login API
                var request = new ReferralUserLoginRequest
                {
                    PhoneNumber = null,
                    Email = AffiliateCode,
                    Password = Password,
                    IsLoginEmail = true
                };

                var response = await _affiliateService.LoginReferralUserAsync(request);

                if (response.Success && response.Data?.ReferralUserDetail != null)
                {
                    var userDetail = response.Data.ReferralUserDetail;
                    
                    try
                    {
                        await SecureStorage.SetAsync("affiliate_token", response.Data.Token ?? "");
                        await SecureStorage.SetAsync("affiliate_id", userDetail.ReferralId ?? "");
                        await SecureStorage.SetAsync("affiliate_referral_name", userDetail.ReferralName ?? "");
                        await SecureStorage.SetAsync("affiliate_email", userDetail.EmailReferrer ?? "");
                        await SecureStorage.SetAsync("affiliate_phone", userDetail.PhoneReferrer ?? "");
                        await SecureStorage.SetAsync("affiliate_referral_id", userDetail.ReferralId ?? "");
                        await SecureStorage.SetAsync("affiliate_referrer_user_type", userDetail.ReferrerUserType ?? "");
                        await SecureStorage.SetAsync("is_affiliate", "true");
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Preferences.Set("affiliate_token", response.Data.Token ?? "");
                        Preferences.Set("affiliate_id", userDetail.ReferralId ?? "");
                        Preferences.Set("affiliate_referral_name", userDetail.ReferralName ?? "");
                        Preferences.Set("affiliate_email", userDetail.EmailReferrer ?? "");
                        Preferences.Set("affiliate_phone", userDetail.PhoneReferrer ?? "");
                        Preferences.Set("affiliate_referral_id", userDetail.ReferralId ?? "");
                        Preferences.Set("affiliate_referrer_user_type", userDetail.ReferrerUserType ?? "");
                        Preferences.Set("is_affiliate", "true");
#endif
                    }
                    
                    // Navigate based on affiliate role
                    if (userDetail.ReferrerUserType == "ListingAgent")
                    {
                        // DataAffiliate - Navigate to AffiliateListingAgentPage
                        var affiliateListingAgentViewModel = IPlatformApplication.Current.Services.GetService<AffiliateListingAgentViewModel>();
                        Application.Current.MainPage = new AffiliateListingAgentPage(affiliateListingAgentViewModel);
                    }
                    else
                    {
                        // ContractAffiliate - Navigate to AffiliateStoreListPage
                        var affiliateStoreListViewModel = IPlatformApplication.Current.Services.GetService<AffiliateStoreListViewModel>();
                        Application.Current.MainPage = new AffiliateStoreListPage(affiliateStoreListViewModel);
                    }
                }
                else
                {
                    AuthError = response.Message ?? "Mã Affiliate hoặc mật khẩu không đúng";
                }
            }
            catch (Exception ex)
            {
                AuthError = $"Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại. Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
