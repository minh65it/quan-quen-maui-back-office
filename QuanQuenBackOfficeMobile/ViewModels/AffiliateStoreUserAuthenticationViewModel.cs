using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public partial class AffiliateStoreUserAuthenticationViewModel : ObservableObject
    {
        private readonly IAffiliateService _affiliateService;

        [ObservableProperty]
        private string otpCode;

        [ObservableProperty]
        private string countdownText = "Gửi lại OTP";

        [ObservableProperty]
        private bool canResend = true;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string email;

        public ICommand VerifyOtpCommand { get; }
        public ICommand ResendOtpCommand { get; }

        private int countdown = 25;
        private System.Timers.Timer timer;
        private static HashSet<string> _sentOtpEmails = new();

        public AffiliateStoreUserAuthenticationViewModel(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
            Email = AffiliateStoreData.Email;
            VerifyOtpCommand = new AsyncRelayCommand(VerifyOtpAsync);
            ResendOtpCommand = new AsyncRelayCommand(ResendOtpAsync, () => CanResend);
            // Không start countdown ngay, chỉ khi user bấm resend
        }

        private void StartCountdown()
        {
            CanResend = false;
            countdown = 25;
            CountdownText = $"{countdown}s";
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, e) => {
                countdown--;
                CountdownText = $"{countdown}s";
                if (countdown <= 0)
                {
                    timer.Stop();
                    CanResend = true;
                    CountdownText = "0s";
                    (ResendOtpCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            };
            timer.Start();
        }

        private async Task VerifyOtpAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(OtpCode) || string.IsNullOrWhiteSpace(Email))
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập mã OTP", "OK");
                    return;
                }

                // Không gửi token cho verify OTP, giống user thường
                var response = await _affiliateService.VerifyAffiliateOtpAsync(Email, OtpCode);
                if (response.IsSuccess)
                {
                    // OTP xác thực thành công, hỏi user muốn làm gì tiếp theo
                    await Application.Current.MainPage.DisplayAlert("Thành công", "Xác thực OTP thành công! Store đã được tạo.", "OK");
                    
                    await ShowNextActionOptions();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Mã xác thực không chính xác", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowNextActionOptions()
        {
            var action = await Application.Current.MainPage.DisplayActionSheet(
                "Bạn muốn làm gì tiếp theo?", 
                "Hủy", 
                "Đăng xuất", 
                "Tạo store mới");

            switch (action)
            {
                case "Đăng xuất":
                    await LogoutFromAffiliate();
                    break;
                case "Tạo store mới":
                    await NavigateToCreateStore();
                    break;
                default:
                    // User cancelled, stay on current page
                    break;
            }
        }

        private async Task LogoutFromAffiliate()
        {
            try
            {
                // Clear affiliate data
                await ClearAffiliateData();
                
                // Navigate về affiliate login
                var affiliateLoginViewModel = IPlatformApplication.Current.Services.GetService<AffiliateLoginViewModel>();
                Application.Current.MainPage = new AffiliateLoginPage(affiliateLoginViewModel);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể đăng xuất: {ex.Message}", "OK");
            }
        }

        private async Task NavigateToCreateStore()
        {
            try
            {
                // Navigate về affiliate create store để tạo store mới
                var affiliateCreateStoreViewModel = IPlatformApplication.Current.Services.GetService<AffiliateCreateStoreViewModel>();
                Application.Current.MainPage = new AffiliateCreateStorePage(affiliateCreateStoreViewModel);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chuyển trang: {ex.Message}", "OK");
            }
        }

        private async Task ClearAffiliateData()
        {
            // Clear affiliate token và các dữ liệu đã lưu
            try
            {
                 SecureStorage.Remove("affiliate_token");
                 SecureStorage.Remove("affiliate_referral_id");
                 SecureStorage.Remove("affiliate_first_name");
                 SecureStorage.Remove("affiliate_email");
            }
            catch (Exception ex)
            {
                // Nếu không xóa được SecureStorage, xóa Preferences
                Preferences.Remove("affiliate_token");
                Preferences.Remove("affiliate_referral_id");
                Preferences.Remove("affiliate_first_name");
                Preferences.Remove("affiliate_email");
            }
        }


        private async Task ResendOtpAsync()
        {
            if (!CanResend || IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await SendOtpAsync();
                StartCountdown();
                await Application.Current.MainPage.DisplayAlert("OTP", "Đã gửi lại mã OTP!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SendOtpAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return;
            }

            var response = await _affiliateService.SendAffiliateOtpAsync(Email);
            if (!response.IsSuccess)
            {
                await Application.Current.MainPage.DisplayAlert("Thông Báo", response.Message ?? "Không thể gửi OTP", "OK");
            }
        }
    }
}
