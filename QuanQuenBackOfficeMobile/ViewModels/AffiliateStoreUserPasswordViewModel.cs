using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Views;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class AffiliateStoreUserPasswordViewModel : ObservableObject
{
    private readonly IAffiliateService _affiliateService;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string confirmPassword;

        [ObservableProperty]
    private bool isPasswordHidden = true;

    [ObservableProperty]
    private bool isConfirmPasswordHidden = true;

    [ObservableProperty]
    private string passwordEyeIcon = "🙈";

    [ObservableProperty]
    private string confirmPasswordEyeIcon = "🙈";

    [ObservableProperty]
    private string passwordError;

    [ObservableProperty]
    private bool isBusy;

    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand CreateAccountCommand { get; }
        public ICommand BackCommand { get; }

    public AffiliateStoreUserPasswordViewModel(IAffiliateService affiliateService)
    {
        _affiliateService = affiliateService;
        TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
        ToggleConfirmPasswordVisibilityCommand = new Command(ToggleConfirmPasswordVisibility);
        CreateAccountCommand = new Command(async () => await CreateAccountAsync());
        BackCommand = new Command(async () => await GoBackAsync());
        
        // Set email from affiliate store data
        Email = AffiliateStoreData.Email;
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        PasswordEyeIcon = IsPasswordHidden ? "🙈" : "🐵";
    }

    private void ToggleConfirmPasswordVisibility()
    {
        IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
        ConfirmPasswordEyeIcon = IsConfirmPasswordHidden ? "🙈" : "🐵";
    }

    private async Task GoBackAsync()
    {
        try
        {
            var affiliateCreateStoreViewModel = IPlatformApplication.Current.Services.GetService<AffiliateCreateStoreViewModel>();
            Application.Current.MainPage = new AffiliateCreateStorePage(affiliateCreateStoreViewModel);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi navigation: {ex.Message}", "OK");
        }
    }


    private async Task CreateAccountAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            PasswordError = "";

            // Validate fields
            if (string.IsNullOrWhiteSpace(Email))
            {
                PasswordError = "Vui lòng nhập email";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Vui lòng nhập mật khẩu";
                return;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                PasswordError = "Vui lòng xác nhận mật khẩu";
                return;
            }

            if (Password != ConfirmPassword)
            {
                PasswordError = "Mật khẩu xác nhận không khớp";
                return;
            }


            // Register user and create store
            await RegisterUserAndCreateStoreAsync();
        }
        catch (Exception ex)
        {
            PasswordError = $"Có lỗi xảy ra: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

        private async Task RegisterUserAndCreateStoreAsync()
        {
            try
            {
                // 1. Gọi API tạo user
                var registerRequest = new AffiliateUserRegisterRequest
                {
                    Email = Email,
                    Password = Password
                };

                var registerResponse = await _affiliateService.RegisterAffiliateUserAsync(registerRequest);
                
                if (!registerResponse.IsSuccess)
                {
                    PasswordError = registerResponse.Message ?? "Tạo tài khoản user thất bại";
                    return;
                }

                // Lấy token từ response của RegisterUser
                var userToken = registerResponse.StoreId; // StoreId field chứa token từ data
                if (string.IsNullOrEmpty(userToken))
                {
                    PasswordError = "Token bắt buộc có";
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Token từ RegisterUser: {userToken?.Substring(0, Math.Min(20, userToken?.Length ?? 0))}... (total length: {userToken?.Length ?? 0})");

                // Lưu token vào AffiliateStoreData để sử dụng sau này
                AffiliateStoreData.RegisterToken = userToken;

                // Lấy ReferrerId từ AffiliateStoreData hoặc SecureStorage để thuần nhất
                var referrerId =await SecureStorage.GetAsync("affiliate_referral_id");
                
                // Debug để kiểm tra giá trị
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] ReferrerId từ AffiliateStoreData: {AffiliateStoreData.ReferrerId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] SecureStorage fallback: {await SecureStorage.GetAsync("affiliate_referral_id")}");
                
                var storePIC = referrerId ?? "Unknown";
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Final storePIC: {storePIC}");

                // 2. Lấy tọa độ từ địa chỉ bằng geocoding (giống user thường)
                (double Latitude, double Longitude)? coordinates = null;
                try
                {
                    var fullAddress = $"{AffiliateStoreData.Address}";
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Geocoding address: {fullAddress}");
                    
                    var locations = await Microsoft.Maui.Devices.Sensors.Geocoding.GetLocationsAsync(fullAddress);
                    var location = locations?.FirstOrDefault();
                    if (location != null)
                    {
                        coordinates = (location.Latitude, location.Longitude);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Geocoding success: Lat={coordinates.Value.Latitude}, Lng={coordinates.Value.Longitude}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Geocoding failed: No location found");
                    }
                }
                catch (Exception geoEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Geocoding error: {geoEx.Message}");
                }

                // 3. Gọi API tạo cửa hàng với token từ RegisterUser
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Bắt đầu tạo store request");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] StoreName: {AffiliateStoreData.StoreName}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] RepresentativeName: {AffiliateStoreData.RepresentativeName}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Address: {AffiliateStoreData.Address}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] PhoneNumber: {AffiliateStoreData.PhoneNumber}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Email: {AffiliateStoreData.Email}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] ProvinceId: {AffiliateStoreData.ProvinceId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] DistrictId: {AffiliateStoreData.DistrictId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] WardId: {AffiliateStoreData.WardId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] BusinessType: {AffiliateStoreData.SelectedBusinessType}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] CreatedBy: {storePIC}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] ReferralId sẽ set: '{storePIC}' (length: {storePIC?.Length ?? 0})");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Coordinates: Lat={coordinates?.Latitude}, Lng={coordinates?.Longitude}");

                var storeRequest = new AffiliateCreateStoreRequest
                {
                    StoreName = AffiliateStoreData.StoreName,
                    RepresentativeName = AffiliateStoreData.RepresentativeName,
                    Address = AffiliateStoreData.Address,
                    PhoneNumber = AffiliateStoreData.PhoneNumber,
                    Email = AffiliateStoreData.Email,
                    ProvinceId = AffiliateStoreData.ProvinceId,
                    DistrictId = AffiliateStoreData.DistrictId,
                    WardId = AffiliateStoreData.WardId,
                    BusinessType = AffiliateStoreData.SelectedBusinessType,
                    CreatedBy = storePIC,
                    ReferralId = storePIC, // Sử dụng cùng referrer ID
                    Latitude = coordinates?.Latitude,
                    Longitude = coordinates?.Longitude,
                    StoreTypeMasterId = AffiliateStoreData.StoreTypeMasterId
                };

                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] Gọi CreateAffiliateStoreAsync với token: {userToken?.Substring(0, Math.Min(20, userToken?.Length ?? 0))}...");
                var storeResponse = await _affiliateService.CreateAffiliateStoreAsync(storeRequest, userToken);
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] CreateStore response: IsSuccess={storeResponse?.IsSuccess}, Message={storeResponse?.Message}");
                if (storeResponse == null || !storeResponse.IsSuccess)
                {
                    PasswordError = storeResponse?.Message ?? "Tạo cửa hàng thất bại";
                    return;
                }

                // Lấy StoreId từ response của CreateStore
                var storeId = storeResponse.Data;
                if (string.IsNullOrEmpty(storeId))
                {
                    PasswordError = "StoreId bắt buộc có từ response CreateStore";
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliateStoreUserPasswordViewModel] StoreId từ CreateStore: {storeId}");

                // --- Chỉ khi cả hai API đều thành công mới chuyển trang ---
                if (registerResponse.IsSuccess && storeResponse.IsSuccess)
                {
                    // Lưu StoreId từ response của CreateStore endpoint để sử dụng cho các API khác
                    AffiliateStoreData.StoreId = storeId;
                    // Đồng bộ sang StoreRegistrationData để các service (ContractService) dùng chung
                    StoreRegistrationData.StoreId = storeId;
                    // 3. Thành công cả hai, chuyển sang trang xác thực OTP
                    var affiliateStoreUserAuthenticationViewModel = IPlatformApplication.Current.Services.GetService<AffiliateStoreUserAuthenticationViewModel>();
                    Application.Current.MainPage = new AffiliateStoreUserAuthenticationPage(affiliateStoreUserAuthenticationViewModel);
                }
                // Nếu một trong hai không thành công thì không chuyển trang (đã return ở trên)
            }
            catch (Exception ex)
            {
                PasswordError = $"Có lỗi xảy ra: {ex.Message}";
            }
        }

}
