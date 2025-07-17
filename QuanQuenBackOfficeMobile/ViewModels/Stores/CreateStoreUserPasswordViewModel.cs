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

            // 2. Lấy storeId từ response
            if (string.IsNullOrEmpty(response.StoreId))
            {
                PasswordError = "StoreId bắt buộc có";
                return;
            }
            var storeId = response.StoreId.StartsWith("U") ? ("S" + response.StoreId.Substring(1)) : response.StoreId;

            // 3. Gọi API tạo cửa hàng với file giấy phép kinh doanh
            var storeRequest = new StoreCreateRequest
            {
                StoreId = storeId,
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

           

            var storeResponse = await _storeService.CreateStoreAsync(storeRequest);
            if (storeResponse == null || !storeResponse.IsSuccess)
            {
                PasswordError = storeResponse?.Message ?? "Tạo cửa hàng thất bại";
                return;
            }
            var nameMedia = "Hồ sơ nhà hàng";
            var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);
            
            if (string.IsNullOrEmpty(categoryMediaId))
            {
                PasswordError = "Không tìm thấy categoryMediaId cho hồ sơ nhà hàng!";
                return;
            }

            // Lấy userName để truyền vào createdBy
            var createdBy = await SecureStorage.GetAsync("userName");

            // Lấy danh sách file ảnh từ StoreRegistrationData
            List<string> licenseImagePaths = StoreRegistrationData.LicenseImagePaths ?? new List<string>();

            // Kiểm tra xem có ảnh nào để upload không
            if (licenseImagePaths.Count > 0)
            {
                // Đọc file thành mảng byte[]
                var imageBytesList = new List<byte[]>();
                foreach (var path in licenseImagePaths)
                {
                    if (File.Exists(path))
                    {
                        var bytes = await File.ReadAllBytesAsync(path);
                        imageBytesList.Add(bytes);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] File không tồn tại: {path}");
                    }
                }

                // Chỉ upload khi có ảnh thực sự
                if (imageBytesList.Count > 0)
                {
                    // Gọi upload ảnh
                    var licenseResponse = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy!, imageBytesList, false, false);

                    // Xử lý kết quả upload
                    if (!licenseResponse.IsSuccess)
                    {
                        PasswordError = licenseResponse.Message ?? "Upload ảnh giấy phép kinh doanh thất bại";
                        return;
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
            }
            // --- Chỉ khi cả hai API đều thành công mới chuyển trang ---
            if (response.IsSuccess && storeResponse.IsSuccess)
            {
                // Lưu StoreId thực tế để dùng cho upload media
                StoreRegistrationData.StoreId = storeId;
                // 4. Thành công cả hai, chuyển sang trang vẽ chữ ký
                await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.DrawingExamplePage());
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