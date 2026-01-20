using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Views.Stores;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Maui.ApplicationModel;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class CreateStoreViewModel : ObservableObject
{
    private readonly GeographicApiService _geoService;
    public ObservableCollection<ProvinceDto> ProvinceList { get; } = new();
    public ObservableCollection<DistrictDto> DistrictList { get; } = new();
    public ObservableCollection<WardDto> WardList { get; } = new();
    public ObservableCollection<string> BusinessTypeList { get; } = new();
    public ObservableCollection<MyImageModel> LicenseImages { get; } = new();
    public List<FileResult> LicenseImageFiles { get; } = new();
    public IAsyncRelayCommand PickLicenseImagesCommand { get; }
    public IAsyncRelayCommand CaptureLicenseImageCommand { get; }
    public bool CanAddMoreLicenseImages => LicenseImages.Count < 4;
    public string LicenseButtonBackgroundColor => CanAddMoreLicenseImages ? "#F5F5F5" : "Gray";
    public string LicenseButtonTextColor => CanAddMoreLicenseImages ? "#F58220" : "Gray";

    // New: Identity card front/back and business registration single images
    [ObservableProperty]
    private ImageSource frontIdImage;
    [ObservableProperty]
    private ImageSource backIdImage;
    [ObservableProperty]
    private ImageSource businessRegistrationImage;

    public bool HasFrontIdImage => FrontIdImage != null;
    public bool HasBackIdImage => BackIdImage != null;
    public bool HasBusinessRegistrationImage => BusinessRegistrationImage != null;

    public bool CanSetFrontIdImage => !HasFrontIdImage;
    public bool CanSetBackIdImage => !HasBackIdImage;
    public bool CanSetBusinessRegistrationImage => !HasBusinessRegistrationImage;

    public IAsyncRelayCommand PickFrontIdImageCommand { get; }
    public IAsyncRelayCommand CaptureFrontIdImageCommand { get; }
    public IRelayCommand RemoveFrontIdImageCommand { get; }

    public IAsyncRelayCommand PickBackIdImageCommand { get; }
    public IAsyncRelayCommand CaptureBackIdImageCommand { get; }
    public IRelayCommand RemoveBackIdImageCommand { get; }

    public IAsyncRelayCommand PickBusinessRegistrationImageCommand { get; }
    public IAsyncRelayCommand CaptureBusinessRegistrationImageCommand { get; }
    public IRelayCommand RemoveBusinessRegistrationImageCommand { get; }

    private string frontIdLocalPath;
    private string backIdLocalPath;
    private string businessRegistrationLocalPath;


    [ObservableProperty]
    private ProvinceDto selectedProvince;
    [ObservableProperty]
    private DistrictDto selectedDistrict;
    [ObservableProperty]
    private WardDto selectedWard;
    [ObservableProperty]
    private string selectedBusinessType;

    [ObservableProperty]
    private string storeName;
    [ObservableProperty]
    private string representativeName;
    [ObservableProperty]
    private string address;
    [ObservableProperty]
    private string phoneNumber;
    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string phoneError = "";

    [ObservableProperty]
    private string emailError = "";

    [ObservableProperty]
    private string storeNameError = "";

    [ObservableProperty]
    private string representativeNameError = "";

    [ObservableProperty]
    private string addressError = "";

    [ObservableProperty]
    private string businessTypeError = "";

    [ObservableProperty]
    private bool isBusy;

    public ICommand NextCommand { get; }
    public IRelayCommand<ImageSource> RemoveLicenseImageCommand { get; }

    private List<UploadImages> images = new();

    public CreateStoreViewModel(GeographicApiService geoService)
    {
        _geoService = geoService;
        LoadProvincesAsync();
        BusinessTypeList.Add("Tư Nhân");
        BusinessTypeList.Add("Công Ty");
        BusinessTypeList.Add("Hộ Kinh Doanh");
        NextCommand = new Command(async () => await OnNextClicked());
        PickLicenseImagesCommand = new AsyncRelayCommand(PickLicenseImagesAsync, () => CanAddMoreLicenseImages);
        CaptureLicenseImageCommand = new AsyncRelayCommand(CaptureLicenseImageAsync, () => CanAddMoreLicenseImages);
        RemoveLicenseImageCommand = new RelayCommand<ImageSource>(RemoveLicenseImage);

        // New: init commands for three single-image sections
        PickFrontIdImageCommand = new AsyncRelayCommand(PickFrontIdImageAsync, () => CanSetFrontIdImage);
        CaptureFrontIdImageCommand = new AsyncRelayCommand(CaptureFrontIdImageAsync, () => CanSetFrontIdImage);
        RemoveFrontIdImageCommand = new RelayCommand(RemoveFrontIdImage);

        PickBackIdImageCommand = new AsyncRelayCommand(PickBackIdImageAsync, () => CanSetBackIdImage);
        CaptureBackIdImageCommand = new AsyncRelayCommand(CaptureBackIdImageAsync, () => CanSetBackIdImage);
        RemoveBackIdImageCommand = new RelayCommand(RemoveBackIdImage);

        PickBusinessRegistrationImageCommand = new AsyncRelayCommand(PickBusinessRegistrationImageAsync, () => CanSetBusinessRegistrationImage);
        CaptureBusinessRegistrationImageCommand = new AsyncRelayCommand(CaptureBusinessRegistrationImageAsync, () => CanSetBusinessRegistrationImage);
        RemoveBusinessRegistrationImageCommand = new RelayCommand(RemoveBusinessRegistrationImage);
        
        // Debug log để kiểm tra trạng thái ban đầu
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] Constructor - LicenseImages count: {LicenseImages.Count}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] Constructor - StoreRegistrationData.LicenseImagePaths count: {StoreRegistrationData.LicenseImagePaths.Count}");
    }

    partial void OnSelectedProvinceChanged(ProvinceDto value)
    {
        LoadDistrictsAsync();
    }
    partial void OnSelectedDistrictChanged(DistrictDto value)
    {
        LoadWardsAsync();
    }

    partial void OnPhoneNumberChanged(string value)
    {
        ValidatePhoneNumber();
    }

    partial void OnEmailChanged(string value)
    {
        ValidateEmail();
    }

    partial void OnStoreNameChanged(string value)
    {
        ValidateStoreName();
    }

    partial void OnRepresentativeNameChanged(string value)
    {
        ValidateRepresentativeName();
    }

    partial void OnAddressChanged(string value)
    {
        ValidateAddress();
    }

    partial void OnSelectedBusinessTypeChanged(string value)
    {
        ValidateBusinessType();
    }

    private async void LoadProvincesAsync()
    {
        ProvinceList.Clear();
        var provinces = await _geoService.GetProvincesAsync();
        foreach (var p in provinces)
            ProvinceList.Add(p);
    }
    private async void LoadDistrictsAsync()
    {
        DistrictList.Clear();
        WardList.Clear();
        if (SelectedProvince?.zoneId == null) return;
        var districts = await _geoService.GetDistrictsAsync(SelectedProvince.zoneId);
        foreach (var d in districts)
            DistrictList.Add(d);
    }
    private async void LoadWardsAsync()
    {
        WardList.Clear();
        if (SelectedDistrict?.territoryId == null) return;
        var wards = await _geoService.GetWardsAsync(SelectedDistrict.territoryId);
        foreach (var w in wards)
            WardList.Add(w);
    }

    /// <summary>
    /// Validate số điện thoại Việt Nam
    /// </summary>
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Loại bỏ khoảng trắng và ký tự đặc biệt
        var cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");
        
        // Kiểm tra độ dài
        if (cleanPhone.Length != 10)
            return false;
        
        // Kiểm tra format số điện thoại Việt Nam
        // Bắt đầu bằng 0, tiếp theo là 3, 5, 7, 8, 9 và có 9 chữ số tiếp theo
        var phonePattern = @"^0[3|5|7|8|9][0-9]{8}$";
        return Regex.IsMatch(cleanPhone, phonePattern);
    }

  

    /// <summary>
    /// Validate email
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate và cập nhật lỗi cho số điện thoại
    /// </summary>
    private void ValidatePhoneNumber()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            PhoneError = "Vui lòng nhập số điện thoại";
        }
        else if (!IsValidPhoneNumber(PhoneNumber))
        {
            PhoneError = "Số điện thoại không hợp lệ. Vui lòng nhập đúng định dạng số điện thoại Việt Nam";
        }
        else
        {
            PhoneError = "";
        }
    }

    /// <summary>
    /// Validate và cập nhật lỗi cho email
    /// </summary>
    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Vui lòng nhập email";
        }
        else if (!IsValidEmail(Email))
        {
            EmailError = "Email không hợp lệ. Vui lòng nhập đúng định dạng email (VD: example@email.com)";
        }
        else
        {
            EmailError = "";
        }
    }

    /// <summary>
    /// Validate tên cửa hàng
    /// </summary>
    private void ValidateStoreName()
    {
        if (string.IsNullOrWhiteSpace(StoreName))
        {
            StoreNameError = "Vui lòng nhập tên cửa hàng";
        }
        else
        {
            StoreNameError = "";
        }
    }

    /// <summary>
    /// Validate tên người đại diện
    /// </summary>
    private void ValidateRepresentativeName()
    {
        if (string.IsNullOrWhiteSpace(RepresentativeName))
        {
            RepresentativeNameError = "Vui lòng nhập tên người đại diện";
        }
        else if (RepresentativeName.Length < 2)
        {
            RepresentativeNameError = "Tên người đại diện phải có ít nhất 2 ký tự";
        }
        else
        {
            RepresentativeNameError = "";
        }
    }

    /// <summary>
    /// Validate địa chỉ
    /// </summary>
    private void ValidateAddress()
    {
        if (string.IsNullOrWhiteSpace(Address))
        {
            AddressError = "Vui lòng nhập địa chỉ chi tiết";
        }
        else
        {
            AddressError = "";
        }
    }

    /// <summary>
    /// Validate loại hình kinh doanh
    /// </summary>
    private void ValidateBusinessType()
    {
        if (string.IsNullOrWhiteSpace(SelectedBusinessType))
        {
            BusinessTypeError = "Vui lòng chọn loại hình kinh doanh";
        }
        else
        {
            BusinessTypeError = "";
        }
    }

    /// <summary>
    /// Validate địa chỉ (tỉnh/thành, quận/huyện, phường/xã)
    /// </summary>
    private bool ValidateLocation()
    {
        return SelectedProvince != null && SelectedDistrict != null && SelectedWard != null;
    }

    /// <summary>
    /// Validate tất cả các trường
    /// </summary>
    private bool ValidateAllFields()
    {
        ValidateStoreName();
        ValidateRepresentativeName();
        ValidatePhoneNumber();
        ValidateEmail();
        ValidateAddress();
        ValidateBusinessType();

        return string.IsNullOrWhiteSpace(StoreNameError) &&
               string.IsNullOrWhiteSpace(RepresentativeNameError) &&
               string.IsNullOrWhiteSpace(PhoneError) &&
               string.IsNullOrWhiteSpace(EmailError) &&
               string.IsNullOrWhiteSpace(AddressError) &&
               string.IsNullOrWhiteSpace(BusinessTypeError) &&
               ValidateLocation();
    }

    private async Task OnNextClicked()
    {
        try
        {
            if (!ValidateAllFields())
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng khắc phục các lỗi đã nêu", "OK");
                return;
            }

            // Lưu thông tin vào StoreRegistrationData
            StoreRegistrationData.StoreName = StoreName;
            StoreRegistrationData.RepresentativeName = RepresentativeName;
            StoreRegistrationData.Province = SelectedProvince.Name;
            StoreRegistrationData.District = SelectedDistrict.Name;
            StoreRegistrationData.Ward = SelectedWard.Name;
            StoreRegistrationData.Address = Address;
            StoreRegistrationData.PhoneNumber = PhoneNumber;
            StoreRegistrationData.Email = Email;
            StoreRegistrationData.BusinessType = SelectedBusinessType;
            StoreRegistrationData.ProvinceId = SelectedProvince.zoneId;
            StoreRegistrationData.DistrictId = SelectedDistrict.territoryId;
            StoreRegistrationData.WardId = SelectedWard.stateId;
            StoreRegistrationData.LicenseImagePaths = LicenseImages.Select(img => img.LocalPath).ToList();
            // New: Save three single image paths
            StoreRegistrationData.FileFrontPicIdentityCardPath = frontIdLocalPath;
            StoreRegistrationData.FileBackPicIdentityCardPath = backIdLocalPath;
            StoreRegistrationData.FileImageBusinessRegistrationPath = businessRegistrationLocalPath;
            
            // Debug log để kiểm tra dữ liệu ảnh
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] Số lượng ảnh đã chọn: {LicenseImages.Count}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] LicenseImagePaths count: {StoreRegistrationData.LicenseImagePaths.Count}");
            foreach (var path in StoreRegistrationData.LicenseImagePaths)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] LicenseImagePath: {path}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] File exists: {File.Exists(path)}");
            }
            
            // Debug log để kiểm tra dữ liệu cuối cùng trước khi chuyển trang
            System.Diagnostics.Debug.WriteLine($"[DEBUG][OnNextClicked] Dữ liệu cuối cùng:");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][OnNextClicked] StoreName: {StoreRegistrationData.StoreName}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][OnNextClicked] Email: {StoreRegistrationData.Email}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][OnNextClicked] LicenseImagePaths count: {StoreRegistrationData.LicenseImagePaths.Count}");
            
            await App.Current.MainPage.Navigation.PushAsync(new CreateStoreUserPasswordPage(Email));
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
    }

    private async Task<string> CopyFileToAppDirectoryAsync(FileResult file)
    {
        var appDir = FileSystem.AppDataDirectory;
        var fileName = Path.GetFileName(file.FullPath);
        var destPath = Path.Combine(appDir, $"{Guid.NewGuid()}_{fileName}");
        using (var sourceStream = await file.OpenReadAsync())
        using (var destStream = File.OpenWrite(destPath))
        {
            await sourceStream.CopyToAsync(destStream);
        }
        System.Diagnostics.Debug.WriteLine($"[LOG] Đã copy file sang: {destPath}");
        return destPath;
    }

    private async Task PickLicenseImagesAsync()
    {
        System.Diagnostics.Debug.WriteLine("[LOG] Bắt đầu chọn ảnh từ thư viện...");
        
        try
        {
            if (!CanAddMoreLicenseImages)
            {
                System.Diagnostics.Debug.WriteLine("[LOG] Không thể thêm ảnh mới, đã đủ 4 ảnh.");
                return;
            }

            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh giấy phép" });
            
            if (result != null)
            {
                if (LicenseImageFiles.Count >= 4)
                {
                    System.Diagnostics.Debug.WriteLine("[LOG] Đã đủ 4 ảnh, dừng thêm.");
                    return;
                }
                if (LicenseImageFiles.Any(f => f.FullPath == result.FullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[LOG] Ảnh đã tồn tại: {result.FullPath}");
                    return;
                }

                // Xử lý file trên background thread
                await Task.Run(async () =>
                {
                    try
                    {
                        var copiedPath = await CopyFileToAppDirectoryAsync(result);
                        
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LicenseImageFiles.Add(result);
                            System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm file vào LicenseImageFiles: {result.FullPath}");
                            
                            var imgSource = ImageSource.FromFile(copiedPath);
                            LicenseImages.Add(new MyImageModel { Images = imgSource, LocalPath = copiedPath });
                            System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào LicenseImages (FromFile): {copiedPath}");
                            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] PickLicenseImagesAsync - LicenseImages count sau khi thêm: {LicenseImages.Count}");
                            
                            OnPropertyChanged(nameof(CanAddMoreLicenseImages));
                            OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
                            OnPropertyChanged(nameof(LicenseButtonTextColor));
                            
                            PickLicenseImagesCommand.NotifyCanExecuteChanged();
                            CaptureLicenseImageCommand.NotifyCanExecuteChanged();
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Không thể copy hoặc tạo ImageSource từ file: {result.FullPath} - {ex.Message}");
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể xử lý ảnh: {ex.Message}", "OK");
                        });
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LOG] Không chọn được ảnh từ thư viện!");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG][PERMISSION] Permission denied: {ex.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", "Ứng dụng cần quyền truy cập thư viện ảnh!", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi trong PickLicenseImagesAsync: {ex.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra khi chọn ảnh: {ex.Message}", "OK");
        }
        
        System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {LicenseImages.Count}");
    }

    private async Task CaptureLicenseImageAsync()
    {
        System.Diagnostics.Debug.WriteLine("[LOG] Bắt đầu chụp ảnh...");
        
        try
        {
            if (!CanAddMoreLicenseImages)
            {
                System.Diagnostics.Debug.WriteLine("[LOG] Không thể thêm ảnh mới, đã đủ 4 ảnh.");
                return;
            }
            
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                System.Diagnostics.Debug.WriteLine("[LOG] Thiết bị không hỗ trợ chụp ảnh!");
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh!", "OK");
                return;
            }

            var file = await MediaPicker.Default.CapturePhotoAsync();
            if (file != null)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG] Đã chụp ảnh: {file.FullPath}");
                
                // Xử lý file trên background thread
                await Task.Run(async () =>
                {
                    try
                    {
                        var copiedPath = await CopyFileToAppDirectoryAsync(file);
                        
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LicenseImageFiles.Add(file);
                            var imgSource = ImageSource.FromFile(copiedPath);
                            LicenseImages.Add(new MyImageModel { Images = imgSource, LocalPath = copiedPath });
                            System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào LicenseImages (FromFile): {copiedPath}");
                            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] CaptureLicenseImageAsync - LicenseImages count sau khi thêm: {LicenseImages.Count}");
                            
                            OnPropertyChanged(nameof(CanAddMoreLicenseImages));
                            OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
                            OnPropertyChanged(nameof(LicenseButtonTextColor));
                            
                            PickLicenseImagesCommand.NotifyCanExecuteChanged();
                            CaptureLicenseImageCommand.NotifyCanExecuteChanged();
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Không thể copy hoặc tạo ImageSource từ file: {file.FullPath} - {ex.Message}");
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể xử lý ảnh: {ex.Message}", "OK");
                        });
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LOG] Không lấy được file ảnh từ camera!");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG][PERMISSION] Permission denied: {ex.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", "Ứng dụng cần quyền truy cập camera để chụp ảnh!", "OK");
        }
        catch (NotSupportedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG][NOT_SUPPORTED] Camera not supported: {ex.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh!", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi trong CaptureLicenseImageAsync: {ex.Message}");
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra khi chụp ảnh: {ex.Message}", "OK");
        }
        
        System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {LicenseImages.Count}");
    }

    private void RemoveLicenseImage(ImageSource img)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] RemoveLicenseImage - Bắt đầu xóa ảnh");
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] RemoveLicenseImage - LicenseImages count trước khi xóa: {LicenseImages.Count}");
        
        int idx = LicenseImages.IndexOf(LicenseImages.FirstOrDefault(i => i.Images == img));
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] RemoveLicenseImage - Index cần xóa: {idx}");
        
        if (idx >= 0)
        {
            var removedImage = LicenseImages[idx];
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] RemoveLicenseImage - Xóa ảnh: {removedImage.LocalPath}");
            
            LicenseImages.RemoveAt(idx);
            if (LicenseImageFiles.Count > idx)
                LicenseImageFiles.RemoveAt(idx);
                
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] RemoveLicenseImage - LicenseImages count sau khi xóa: {LicenseImages.Count}");
            
            OnPropertyChanged(nameof(CanAddMoreLicenseImages));
            OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
            OnPropertyChanged(nameof(LicenseButtonTextColor));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateStoreViewModel] RemoveLicenseImage - Không tìm thấy ảnh để xóa");
        }
        
        PickLicenseImagesCommand.NotifyCanExecuteChanged();
        CaptureLicenseImageCommand.NotifyCanExecuteChanged();
    }
}

// ------------- New methods for single-image pick/capture/remove -------------
partial class CreateStoreViewModel
{
    private async Task PickFrontIdImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh CMND/CCCD - mặt trước" });
            if (result == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(result);
            frontIdLocalPath = copiedPath;
            FrontIdImage = ImageSource.FromFile(copiedPath);
            OnPropertyChanged(nameof(HasFrontIdImage));
            OnPropertyChanged(nameof(CanSetFrontIdImage));
            PickFrontIdImageCommand.NotifyCanExecuteChanged();
            CaptureFrontIdImageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
        }
    }

    private async Task CaptureFrontIdImageAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh!", "OK");
                return;
            }
            var file = await MediaPicker.Default.CapturePhotoAsync();
            if (file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            frontIdLocalPath = copiedPath;
            FrontIdImage = ImageSource.FromFile(copiedPath);
            OnPropertyChanged(nameof(HasFrontIdImage));
            OnPropertyChanged(nameof(CanSetFrontIdImage));
            PickFrontIdImageCommand.NotifyCanExecuteChanged();
            CaptureFrontIdImageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chụp ảnh: {ex.Message}", "OK");
        }
    }

    private void RemoveFrontIdImage()
    {
        FrontIdImage = null;
        frontIdLocalPath = null;
        OnPropertyChanged(nameof(HasFrontIdImage));
        OnPropertyChanged(nameof(CanSetFrontIdImage));
        PickFrontIdImageCommand.NotifyCanExecuteChanged();
        CaptureFrontIdImageCommand.NotifyCanExecuteChanged();
    }

    private async Task PickBackIdImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh CMND/CCCD - mặt sau" });
            if (result == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(result);
            backIdLocalPath = copiedPath;
            BackIdImage = ImageSource.FromFile(copiedPath);
            OnPropertyChanged(nameof(HasBackIdImage));
            OnPropertyChanged(nameof(CanSetBackIdImage));
            PickBackIdImageCommand.NotifyCanExecuteChanged();
            CaptureBackIdImageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
        }
    }

    private async Task CaptureBackIdImageAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh!", "OK");
                return;
            }
            var file = await MediaPicker.Default.CapturePhotoAsync();
            if (file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            backIdLocalPath = copiedPath;
            BackIdImage = ImageSource.FromFile(copiedPath);
            OnPropertyChanged(nameof(HasBackIdImage));
            OnPropertyChanged(nameof(CanSetBackIdImage));
            PickBackIdImageCommand.NotifyCanExecuteChanged();
            CaptureBackIdImageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chụp ảnh: {ex.Message}", "OK");
        }
    }

    private void RemoveBackIdImage()
    {
        BackIdImage = null;
        backIdLocalPath = null;
        OnPropertyChanged(nameof(HasBackIdImage));
        OnPropertyChanged(nameof(CanSetBackIdImage));
        PickBackIdImageCommand.NotifyCanExecuteChanged();
        CaptureBackIdImageCommand.NotifyCanExecuteChanged();
    }

    private async Task PickBusinessRegistrationImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh Giấy phép ĐKKD" });
            if (result == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(result);
            businessRegistrationLocalPath = copiedPath;
            BusinessRegistrationImage = ImageSource.FromFile(copiedPath);
            OnPropertyChanged(nameof(HasBusinessRegistrationImage));
            OnPropertyChanged(nameof(CanSetBusinessRegistrationImage));
            PickBusinessRegistrationImageCommand.NotifyCanExecuteChanged();
            CaptureBusinessRegistrationImageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
        }
    }

    private async Task CaptureBusinessRegistrationImageAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh!", "OK");
                return;
            }
            var file = await MediaPicker.Default.CapturePhotoAsync();
            if (file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            businessRegistrationLocalPath = copiedPath;
            BusinessRegistrationImage = ImageSource.FromFile(copiedPath);
            OnPropertyChanged(nameof(HasBusinessRegistrationImage));
            OnPropertyChanged(nameof(CanSetBusinessRegistrationImage));
            PickBusinessRegistrationImageCommand.NotifyCanExecuteChanged();
            CaptureBusinessRegistrationImageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chụp ảnh: {ex.Message}", "OK");
        }
    }

    private void RemoveBusinessRegistrationImage()
    {
        BusinessRegistrationImage = null;
        businessRegistrationLocalPath = null;
        OnPropertyChanged(nameof(HasBusinessRegistrationImage));
        OnPropertyChanged(nameof(CanSetBusinessRegistrationImage));
        PickBusinessRegistrationImageCommand.NotifyCanExecuteChanged();
        CaptureBusinessRegistrationImageCommand.NotifyCanExecuteChanged();
    }
}
