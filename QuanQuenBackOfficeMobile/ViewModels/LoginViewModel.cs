using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _emailOrPhone;
        private string _password;
        private bool _isBusy;
        private bool _showEmptyEmailError;
        private bool _showEmptyPasswordError;
        private bool _showLengthPasswordError;
        private string _authError;
        private bool _isPasswordHidden = true;
        private string _passwordVisibilityIcon = "🙈";

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await LoginAsync());
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
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
        public string EmailOrPhone
        {
            get => _emailOrPhone;
            set
            {
                if (SetProperty(ref _emailOrPhone, value))
                {
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
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool ShowEmptyEmailError
        {
            get => _showEmptyEmailError;
            set => SetProperty(ref _showEmptyEmailError, value);
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

        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
            PasswordVisibilityIcon = IsPasswordHidden ? "🙈" : "🐵";
        }

        

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ShowEmptyEmailError = ShowEmptyPasswordError = ShowLengthPasswordError = false;

                if (string.IsNullOrWhiteSpace(EmailOrPhone))
                {
                    ShowEmptyEmailError = true;
                }
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowEmptyPasswordError = true;
                }
                if (ShowEmptyEmailError || ShowEmptyPasswordError)
                {
                    IsBusy = false;
                    return;
                }

                // 2) Kiểm tra độ dài mật khẩu
                if (Password.Length < 6)
                {
                    ShowLengthPasswordError = true;
                    IsBusy = false;
                    return;
                }               
                var response = await _authService.LoginAsync(EmailOrPhone, Password);

                if (response.IsLogin && !string.IsNullOrEmpty(response.Token))
                {
                    try
                    {
                        await SecureStorage.SetAsync("auth_token", response.Token);
                        await SecureStorage.SetAsync("userName", response.VsiriusUserDto.UserName);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Preferences.Set("auth_token", response.Token);
                        Preferences.Set("userName", response.VsiriusUserDto.UserName);
#endif
                    }
                    var mainViewModel = IPlatformApplication.Current.Services.GetService<MainViewModel>();
                    Application.Current.MainPage = new MainPage(mainViewModel);
                }
                else
                {
                    AuthError = "Vui lòng kiểm tra lại tài khoản và mật khẩu";
                }
            }
            catch (Exception)
            {
                // Xử lý lỗi
                ShowEmptyEmailError = true;
                ShowEmptyPasswordError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 