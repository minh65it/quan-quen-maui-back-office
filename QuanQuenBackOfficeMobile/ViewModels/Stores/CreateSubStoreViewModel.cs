using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class CreateSubStoreViewModel : ObservableObject
{
    private readonly GeographicApiService _geoService;
    private readonly IStoreService _storeService;
    private readonly IStoreMediaService _storeMediaService;
    private readonly IContractService _contractService;
    private readonly IConfiguration _configuration;
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

    [ObservableProperty]
    private ProvinceDto selectedProvince;
    [ObservableProperty]
    private DistrictDto selectedDistrict;
    [ObservableProperty]
    private WardDto selectedWard;

    [ObservableProperty]
    private string storeName;
    [ObservableProperty]
    private string address;

    [ObservableProperty]
    private string storeNameError = "";
    [ObservableProperty]
    private string addressError = "";

    [ObservableProperty]
    private bool isBusy;

    public IAsyncRelayCommand CreateSubStoreCommand { get; }
    public IRelayCommand<ImageSource> RemoveLicenseImageCommand { get; }

    private List<UploadImages> images = new();

    public CreateSubStoreViewModel(GeographicApiService geoService, IStoreService storeService, IStoreMediaService storeMediaService, IContractService contractService, IConfiguration configuration)
    {
        _geoService = geoService;
        _storeService = storeService;
        _storeMediaService = storeMediaService;
        _contractService = contractService;
        _configuration = configuration;
        LoadProvincesAsync();
        CreateSubStoreCommand = new AsyncRelayCommand(CreateSubStoreAsync);
        PickLicenseImagesCommand = new AsyncRelayCommand(PickLicenseImagesAsync, () => CanAddMoreLicenseImages);
        CaptureLicenseImageCommand = new AsyncRelayCommand(CaptureLicenseImageAsync, () => CanAddMoreLicenseImages);
        RemoveLicenseImageCommand = new RelayCommand<ImageSource>(RemoveLicenseImage);
    }

    partial void OnSelectedProvinceChanged(ProvinceDto value)
    {
        LoadDistrictsAsync();
    }
    
    partial void OnSelectedDistrictChanged(DistrictDto value)
    {
        LoadWardsAsync();
    }

    partial void OnStoreNameChanged(string value)
    {
        ValidateStoreName();
    }

    partial void OnAddressChanged(string value)
    {
        ValidateAddress();
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
        ValidateAddress();

        return string.IsNullOrWhiteSpace(StoreNameError) &&
               string.IsNullOrWhiteSpace(AddressError) &&
               ValidateLocation();
    }

    private async Task CreateSubStoreAsync()
    {
        try
        {
            if (!ValidateAllFields())
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng khắc phục các lỗi đã nêu", "OK");
                return;
            }

            IsBusy = true;

            var userToken = StoreRegistrationData.UserToken;
            if (string.IsNullOrEmpty(userToken))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy userToken. Vui lòng đăng nhập lại.", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Sử dụng userToken: {userToken?.Substring(0, Math.Min(20, userToken?.Length ?? 0))}...");

            // Lấy userId từ SecureStorage để làm CreatedBy
            var userId = await SecureStorage.GetAsync("userId");
            var storePIC = userId ?? "Unknown";

            // Tạo request để gọi createStore API - chỉ truyền thông tin đã nhập
            var storeRequest = new StoreCreateRequest
            {
                StoreName = StoreName,
                WardId = SelectedWard.stateId,
                DistrictId = SelectedDistrict.territoryId,
                CityId = SelectedProvince.zoneId,
                Address = Address,
                ProvinceId = SelectedProvince.zoneId,
                CreatedBy = storePIC
            };

            // Gọi API tạo chi nhánh với userToken
            var storeResponse = await _storeService.CreateStoreAsync(storeRequest, userToken);
            if (storeResponse.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert("Thành công", "Tạo chi nhánh thành công!", "OK");                
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", storeResponse.Message ?? "Có lỗi xảy ra khi tạo chi nhánh", "OK");
            }
            var storeId = storeResponse.Data;

            // Upload license images nếu có
            if (storeResponse.IsSuccess && !string.IsNullOrEmpty(storeId) && LicenseImages.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Bắt đầu upload {LicenseImages.Count} ảnh license cho storeId: {storeId}");
                
                try
                {
                    // Lấy categoryMediaId cho "Hồ sơ nhà hàng"
                    var nameMedia = "Hồ sơ nhà hàng";
                    var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);
                    
                    if (string.IsNullOrEmpty(categoryMediaId))
                    {
                        System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Không tìm thấy categoryMediaId cho {nameMedia}");
                        await App.Current.MainPage.DisplayAlert("Cảnh báo", "Không thể upload ảnh license. Chi nhánh đã được tạo thành công.", "OK");
                        await App.Current.MainPage.Navigation.PopToRootAsync();
                        return;
                    }

                    // Lấy userName để làm createdBy
                    var createdBy = await SecureStorage.GetAsync("userName");
                    if (string.IsNullOrEmpty(createdBy))
                    {
                        createdBy = await SecureStorage.GetAsync("email");
                    }

                    // Đọc file thành mảng byte[]
                    var imageBytesList = new List<byte[]>();
                    foreach (var licenseImage in LicenseImages)
                    {
                        if (!string.IsNullOrEmpty(licenseImage.LocalPath) && File.Exists(licenseImage.LocalPath))
                        {
                            try
                            {
                                var bytes = await File.ReadAllBytesAsync(licenseImage.LocalPath);
                                imageBytesList.Add(bytes);
                                System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Đọc file thành công: {licenseImage.LocalPath}, size: {bytes.Length} bytes");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Lỗi đọc file {licenseImage.LocalPath}: {ex.Message}");
                            }
                        }
                    }

                    // Upload ảnh nếu có
                    if (imageBytesList.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Upload {imageBytesList.Count} ảnh license");
                        var licenseResponse = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy!, imageBytesList, false, false);
                        
                        if (licenseResponse.IsSuccess)
                        {
                            System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Upload license thành công");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Upload license thất bại: {licenseResponse.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Lỗi upload license: {ex.Message}");
                }
            }

            // Navigate to StoreAgreementInformationPage (bỏ qua CreateStoreUserPassword và Verify OTP)
            if (storeResponse.IsSuccess && !string.IsNullOrEmpty(storeId))
            {
                System.Diagnostics.Debug.WriteLine($"[CreateSubStoreViewModel] Chuyển đến StoreAgreementInformationPage với storeId: {storeId}");
                
                // Cập nhật StoreRegistrationData với thông tin chi nhánh mới để Agreement sử dụng
                StoreRegistrationData.StoreId = storeId;
                StoreRegistrationData.StoreName = StoreName; // Tên chi nhánh mới
                StoreRegistrationData.Address = Address; // Địa chỉ chi nhánh mới
                StoreRegistrationData.Province = SelectedProvince?.Name ?? "";
                StoreRegistrationData.District = SelectedDistrict?.Name ?? "";
                StoreRegistrationData.Ward = SelectedWard?.Name ?? "";
                StoreRegistrationData.ProvinceId = SelectedProvince?.zoneId ?? "";
                StoreRegistrationData.DistrictId = SelectedDistrict?.territoryId ?? "";
                StoreRegistrationData.WardId = SelectedWard?.stateId ?? "";
                
                // Giữ nguyên thông tin từ cửa hàng chính (PhoneNumber, Email, RepresentativeName, BusinessType)
                // Vì Agreement cần thông tin này để tạo hợp đồng
                
                // Chuyển đến trang Agreement Information
                await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.Stores.StoreAgreementInformationPage());
            }
            else
            {
                // Nếu tạo store thất bại, quay về root page
                await App.Current.MainPage.Navigation.PopToRootAsync();
            }

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
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
                            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] PickLicenseImagesAsync - LicenseImages count sau khi thêm: {LicenseImages.Count}");
                            
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
                            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] CaptureLicenseImageAsync - LicenseImages count sau khi thêm: {LicenseImages.Count}");
                            
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
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] RemoveLicenseImage - Bắt đầu xóa ảnh");
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] RemoveLicenseImage - LicenseImages count trước khi xóa: {LicenseImages.Count}");
        
        int idx = LicenseImages.IndexOf(LicenseImages.FirstOrDefault(i => i.Images == img));
        System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] RemoveLicenseImage - Index cần xóa: {idx}");
        
        if (idx >= 0)
        {
            var removedImage = LicenseImages[idx];
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] RemoveLicenseImage - Xóa ảnh: {removedImage.LocalPath}");
            
            LicenseImages.RemoveAt(idx);
            if (LicenseImageFiles.Count > idx)
                LicenseImageFiles.RemoveAt(idx);
                
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] RemoveLicenseImage - LicenseImages count sau khi xóa: {LicenseImages.Count}");
            
            OnPropertyChanged(nameof(CanAddMoreLicenseImages));
            OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
            OnPropertyChanged(nameof(LicenseButtonTextColor));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG][CreateSubStoreViewModel] RemoveLicenseImage - Không tìm thấy ảnh để xóa");
        }
        
        PickLicenseImagesCommand.NotifyCanExecuteChanged();
        CaptureLicenseImageCommand.NotifyCanExecuteChanged();
    }
}
