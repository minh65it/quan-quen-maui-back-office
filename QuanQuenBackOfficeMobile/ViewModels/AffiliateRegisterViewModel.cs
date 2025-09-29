using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public class AffiliateRegisterViewModel : BaseViewModel
    {
        private readonly IAffiliateService _affiliateService;
        private string _fullName;
        private string _phoneNumber;
        private string _email;
        private string _address;
        private string _password;
        private ReferrerType _selectedReferrerType = ReferrerType.Personal;
        private bool _isBusy;
        private bool _showEmptyFullNameError;
        private bool _showEmptyPhoneError;
        private bool _showEmptyEmailError;
        private bool _showEmptyAddressError;
        private bool _showEmptyPasswordError;
        private bool _showLengthPasswordError;
        private string _authError;
        private bool _isPasswordHidden = true;
        private string _passwordVisibilityIcon = "🙈";

        public AffiliateRegisterViewModel(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
            RegisterCommand = new Command(async () => await RegisterAsync());
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
            BackToAffiliateLoginCommand = new Command(async () => await BackToAffiliateLoginAsync());
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

        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    if (!string.IsNullOrEmpty(value))
                        ShowEmptyFullNameError = false;
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (SetProperty(ref _phoneNumber, value))
                {
                    if (!string.IsNullOrEmpty(value))
                        ShowEmptyPhoneError = false;
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    if (!string.IsNullOrEmpty(value))
                        ShowEmptyEmailError = false;
                }
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    if (!string.IsNullOrEmpty(value))
                        ShowEmptyAddressError = false;
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
                    if (!string.IsNullOrEmpty(value))
                    {
                        ShowEmptyPasswordError = false;
                        ShowLengthPasswordError = false;
                    }
                }
            }
        }


        public ReferrerType SelectedReferrerType
        {
            get => _selectedReferrerType;
            set => SetProperty(ref _selectedReferrerType, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool ShowEmptyFullNameError
        {
            get => _showEmptyFullNameError;
            set => SetProperty(ref _showEmptyFullNameError, value);
        }

        public bool ShowEmptyPhoneError
        {
            get => _showEmptyPhoneError;
            set => SetProperty(ref _showEmptyPhoneError, value);
        }

        public bool ShowEmptyEmailError
        {
            get => _showEmptyEmailError;
            set => SetProperty(ref _showEmptyEmailError, value);
        }

        public bool ShowEmptyAddressError
        {
            get => _showEmptyAddressError;
            set => SetProperty(ref _showEmptyAddressError, value);
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

        public ICommand RegisterCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand BackToAffiliateLoginCommand { get; }

        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
            PasswordVisibilityIcon = IsPasswordHidden ? "🙈" : "🐵";
        }


        private async Task BackToAffiliateLoginAsync()
        {
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//AffiliateLogin");
                }
                else
                {
                    // Fallback: Navigate using Application.Current.MainPage
                    var affiliateLoginViewModel = IPlatformApplication.Current.Services.GetService<AffiliateLoginViewModel>();
                    Application.Current.MainPage = new AffiliateLoginPage(affiliateLoginViewModel);
                }
            }
            catch (Exception ex)
            {
                // Fallback navigation
                var affiliateLoginViewModel = IPlatformApplication.Current.Services.GetService<AffiliateLoginViewModel>();
                Application.Current.MainPage = new AffiliateLoginPage(affiliateLoginViewModel);
            }
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearAllErrors();
                AuthError = string.Empty;

                // Validate all fields
                bool hasError = false;

                if (string.IsNullOrWhiteSpace(FullName))
                {
                    ShowEmptyFullNameError = true;
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    ShowEmptyPhoneError = true;
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ShowEmptyEmailError = true;
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(Address))
                {
                    ShowEmptyAddressError = true;
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowEmptyPasswordError = true;
                    hasError = true;
                }

                if (hasError)
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


                // Call affiliate registration API
                var request = new ReferralUserRequest
                {
                    ReferralName = FullName,
                    PhoneReferrer = PhoneNumber,
                    EmailReferrer = Email,
                    Password = Password,
                    ReferrerType = ((int)SelectedReferrerType).ToString(),
                    Address = Address,
                    CreatedBy = "MobileApp"
                };

                var response = await _affiliateService.RegisterReferralUserAsync(request);

                if (response.Success && response.Data?.ReferralUserDetail != null)
                {
                    AuthError = $"Đăng ký thành công! Mã Affiliate: {response.Data.ReferralUserDetail.ReferralId}";
                    await Task.Delay(3000);
                    
                    // Navigate back to affiliate login
                    await BackToAffiliateLoginAsync();
                }
                else
                {
                    AuthError = response.Message ?? "Đăng ký thất bại. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                AuthError = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearAllErrors()
        {
            ShowEmptyFullNameError = false;
            ShowEmptyPhoneError = false;
            ShowEmptyEmailError = false;
            ShowEmptyAddressError = false;
            ShowEmptyPasswordError = false;
            ShowLengthPasswordError = false;
        }
    }
}
