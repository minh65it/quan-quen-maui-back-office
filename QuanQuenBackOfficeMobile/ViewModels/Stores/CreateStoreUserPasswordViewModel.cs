using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Views;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class CreateStoreUserPasswordViewModel : ObservableObject
{
    private readonly IStoreUserService _storeUserService;
    private readonly IStoreService _storeService;
    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string confirmPassword;

    [ObservableProperty]
    private bool isAccepted;

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

    [ObservableProperty]
    private bool isLoading;

    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand BackCommand { get; }
    public IAsyncRelayCommand ShowPolicyCommand { get; }
    public IAsyncRelayCommand ShowTermsCommand { get; }

    public CreateStoreUserPasswordViewModel(IStoreUserService storeUserService, IStoreService storeService)
    {
        _storeUserService = storeUserService;
        _storeService = storeService;
        TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
        ToggleConfirmPasswordVisibilityCommand = new Command(ToggleConfirmPasswordVisibility);
        RegisterCommand = new Command(async () => await Register());
        BackCommand = new Command(async () => await GoBack());
        ShowPolicyCommand = new AsyncRelayCommand(() => ShowCsrDetailAsync("policy"));
        ShowTermsCommand = new AsyncRelayCommand(() => ShowCsrDetailAsync("terms-of-service"));
        // Set email from previous page
        Email = StoreRegistrationData.Email;
        
        // Debug log để kiểm tra dữ liệu nhận được
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - Email: {StoreRegistrationData.Email}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - StoreName: {StoreRegistrationData.StoreName}");
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

    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task Register()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            IsLoading = true;
            PasswordError = string.Empty;
            
            // Debug log để kiểm tra dữ liệu khi bắt đầu đăng ký
            System.Diagnostics.Debug.WriteLine($"[DEBUG][Register] Bắt đầu đăng ký");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][Register] StoreName: {StoreRegistrationData.StoreName}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][Register] Email: {StoreRegistrationData.Email}");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Vui lòng nhập mật khẩu";
                return;
            }

            if (Password.Length < 6)
            {
                PasswordError = "Mật khẩu phải có ít nhất 6 ký tự";
                return;
            }

            if (!(Password.Any(char.IsLetter) && Password.Any(char.IsDigit)))
            {
                PasswordError = "Mật khẩu phải chứa cả chữ và số";
                return;
            }

            if (Password != ConfirmPassword)
            {
                PasswordError = "Mật khẩu xác nhận không khớp";
                return;
            }

            if (!IsAccepted)
            {
                PasswordError = "Vui lòng chấp nhận điều khoản dịch vụ";
                return;
            }

            // Create registration request
            var request = new StoreUserRegistrationRequest
            {
                StoreName = StoreRegistrationData.StoreName,
                RepresentativeName = StoreRegistrationData.RepresentativeName,
                Province = StoreRegistrationData.Province,
                District = StoreRegistrationData.District,
                Ward = StoreRegistrationData.Ward,
                Address = StoreRegistrationData.Address,
                PhoneNumber = StoreRegistrationData.PhoneNumber,
                Email = StoreRegistrationData.Email,
                BusinessType = StoreRegistrationData.BusinessType,
                ProvinceId = StoreRegistrationData.ProvinceId,
                DistrictId = StoreRegistrationData.DistrictId,
                WardId = StoreRegistrationData.WardId,
                Password = Password,
                UserName = StoreRegistrationData.RepresentativeName
            };

            // Lưu password để dùng cho bước sau
            StoreRegistrationData.Password = Password;

            // 1. Gọi API tạo user
            var response = await _storeUserService.RegisterUserAsync(request);
            if (!response.IsSuccess)
            {
                PasswordError = response.Message ?? "Tạo tài khoản user thất bại";
                return;
            }

            // Lấy token từ response của RegisterUser
            var userToken = response.StoreId; // StoreId field chứa token từ data
            if (string.IsNullOrEmpty(userToken))
            {
                PasswordError = "Token bắt buộc có";
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Token từ RegisterUser: {userToken}");

            // Lưu token vào StoreRegistrationData để sử dụng sau này
            StoreRegistrationData.UserToken = userToken;

            // Lấy userId từ SecureStorage để làm CreatedBy
            var userId = await SecureStorage.GetAsync("userId");
            
            // Debug để kiểm tra giá trị
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] UserId từ SecureStorage: {userId}");
            
            var storePIC = userId ?? "Unknown";
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Final storePIC: {storePIC}");

            // 2. Gọi API tạo cửa hàng với token từ RegisterUser và không cần StoreId
            var storeRequest = new StoreCreateRequest
            {
                StoreName = StoreRegistrationData.StoreName,
                StorePhone = StoreRegistrationData.PhoneNumber,
                StoreEmail = StoreRegistrationData.Email,
                WardId = StoreRegistrationData.WardId,
                DistrictId = StoreRegistrationData.DistrictId,
                CityId = StoreRegistrationData.ProvinceId,
                IsRegisterByCompany = StoreRegistrationData.BusinessType == "Công Ty",
                IsRegisterByPersonality = StoreRegistrationData.BusinessType == "Tư Nhân",
                Address = StoreRegistrationData.Address,
                RepresentativeName = StoreRegistrationData.RepresentativeName,
                ProvinceId = StoreRegistrationData.ProvinceId,
                BusinessType = StoreRegistrationData.BusinessType,
                CreatedBy = storePIC,
                // Pass file paths directly in request
                FileFrontPicIdentityCardPath = StoreRegistrationData.FileFrontPicIdentityCardPath,
                FileBackPicIdentityCardPath = StoreRegistrationData.FileBackPicIdentityCardPath,
                FileImageBusinessRegistrationPath = StoreRegistrationData.FileImageBusinessRegistrationPath
            };

            var storeResponse = await _storeService.CreateStoreAsync(storeRequest, userToken);
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
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] StoreId từ CreateStore: {storeId}");

            // --- Chỉ khi cả hai API đều thành công mới chuyển trang ---
            if (response.IsSuccess && storeResponse.IsSuccess)
            {
                // Lưu StoreId từ response của CreateStore endpoint để sử dụng cho các API khác
                StoreRegistrationData.StoreId = storeId;
                // 4. Thành công cả hai, chuyển sang trang xác thực OTP
                await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.Stores.StoreUserAuthentication(Email));
            }
            // Nếu một trong hai không thành công thì không chuyển trang (đã return ở trên)
        }
        catch (Exception ex)
        {
            PasswordError = $"Có lỗi xảy ra: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
        }
    }

    private async Task ShowCsrDetailAsync(string url)
    {
        try
        {
            var detail = await _storeUserService.GetCsrDetailAsync(url);
            await App.Current.MainPage.Navigation.PushAsync(
                new QuanQuenBackOfficeMobile.Views.CsrDetailPage(
                    detail?.title ?? "Chi tiết",
                    detail?.description ?? "Không có nội dung"
                )
            );
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải nội dung: {ex.Message}", "OK");
        }
    }
}