using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Views;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class CreateStoreUserPasswordViewModel : ObservableObject
{
    private readonly IStoreUserService _storeUserService;
    private readonly IStoreService _storeService;
    private readonly IStoreMediaService _storeMediaService;
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

    public CreateStoreUserPasswordViewModel(IStoreUserService storeUserService, IStoreService storeService, IStoreMediaService storeMediaService)
    {
        _storeMediaService = storeMediaService;
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
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - LicenseImagePaths count: {StoreRegistrationData.LicenseImagePaths.Count}");
        foreach (var path in StoreRegistrationData.LicenseImagePaths)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - LicenseImagePath: {path}");
        }
        
        // Kiểm tra xem dữ liệu có bị mất không
        if (string.IsNullOrEmpty(StoreRegistrationData.StoreName) || StoreRegistrationData.LicenseImagePaths.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - DỮ LIỆU BỊ MẤT!");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - StoreName empty: {string.IsNullOrEmpty(StoreRegistrationData.StoreName)}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Constructor - LicenseImagePaths empty: {StoreRegistrationData.LicenseImagePaths.Count == 0}");
        }
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG][Register] LicenseImagePaths count: {StoreRegistrationData.LicenseImagePaths.Count}");
            foreach (var path in StoreRegistrationData.LicenseImagePaths)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG][Register] LicenseImagePath: {path}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][Register] File exists: {File.Exists(path)}");
            }

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
                BusinessType = StoreRegistrationData.BusinessType
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

            // Lấy danh sách file ảnh từ StoreRegistrationData
            List<string> licenseImagePaths = StoreRegistrationData.LicenseImagePaths ?? new List<string>();
            
            // Debug log để kiểm tra dữ liệu ảnh
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] LicenseImagePaths count: {licenseImagePaths.Count}");
            foreach (var path in licenseImagePaths)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] LicenseImagePath: {path}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] File exists: {File.Exists(path)}");
            }

            // Kiểm tra xem có ảnh nào để upload không
            if (licenseImagePaths.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Có {licenseImagePaths.Count} ảnh để upload");
                
                // Lấy categoryMediaId chỉ khi cần upload ảnh
                var nameMedia = "Hồ sơ nhà hàng";
                var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);
                
                if (string.IsNullOrEmpty(categoryMediaId))
                {
                    PasswordError = "Không tìm thấy categoryMediaId cho hồ sơ nhà hàng!";
                    return;
                }

                // Lấy userName để truyền vào createdBy
                var createdBy = await SecureStorage.GetAsync("userName");
                
                // Đọc file thành mảng byte[]
                var imageBytesList = new List<byte[]>();
                foreach (var path in licenseImagePaths)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Đang xử lý file: {path}");
                    if (File.Exists(path))
                    {
                        try
                        {
                            var bytes = await File.ReadAllBytesAsync(path);
                            imageBytesList.Add(bytes);
                            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Đọc file thành công: {path}, size: {bytes.Length} bytes");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Lỗi đọc file {path}: {ex.Message}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] File không tồn tại: {path}");
                    }
                }

                // Chỉ upload khi có ảnh thực sự
                if (imageBytesList.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Bắt đầu upload {imageBytesList.Count} ảnh");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] StoreId: {storeId}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] CategoryMediaId: {categoryMediaId}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] CreatedBy: {createdBy}");
                    
                    // Gọi upload ảnh
                    var licenseResponse = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy!, imageBytesList, false, false);

                    // Xử lý kết quả upload
                    if (!licenseResponse.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Upload thất bại: {licenseResponse.Message}");
                        PasswordError = licenseResponse.Message ?? "Upload ảnh giấy phép kinh doanh thất bại";
                        return;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] Upload thành công: {licenseResponse.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LOG][INFO] Không có ảnh hợp lệ để upload, bỏ qua bước upload");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LOG][INFO] Không có ảnh giấy phép kinh doanh để upload");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] KHÔNG CÓ ẢNH NÀO ĐỂ UPLOAD!");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreUserPasswordViewModel] StoreRegistrationData.LicenseImagePaths: {StoreRegistrationData.LicenseImagePaths?.Count ?? 0}");
            }
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