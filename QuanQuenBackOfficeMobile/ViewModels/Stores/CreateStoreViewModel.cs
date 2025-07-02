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
    }

    partial void OnSelectedProvinceChanged(ProvinceDto value)
    {
        LoadDistrictsAsync();
    }
    partial void OnSelectedDistrictChanged(DistrictDto value)
    {
        LoadWardsAsync();
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

    private async Task OnNextClicked()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(StoreName))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên cửa hàng", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(RepresentativeName))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên người đại diện", "OK");
                return;
            }

            if (SelectedProvince == null || SelectedDistrict == null || SelectedWard == null)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng chọn đầy đủ thông tin địa chỉ", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập địa chỉ chi tiết", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập số điện thoại", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập email", "OK");
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
        if (!CanAddMoreLicenseImages)
        {
            System.Diagnostics.Debug.WriteLine("[LOG] Không thể thêm ảnh mới, đã đủ 4 ảnh.");
            return;
        }
        var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh giấy phép kinh doanh" });
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
            LicenseImageFiles.Add(result);
            System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm file vào LicenseImageFiles: {result.FullPath}");
            try
            {
                var copiedPath = await CopyFileToAppDirectoryAsync(result);
                var imgSource = ImageSource.FromFile(copiedPath);
                LicenseImages.Add(new MyImageModel { LicenseImage = imgSource, LocalPath = copiedPath });
                System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào LicenseImages (FromFile): {copiedPath}");
                OnPropertyChanged(nameof(CanAddMoreLicenseImages));
                OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
                OnPropertyChanged(nameof(LicenseButtonTextColor));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Không thể copy hoặc tạo ImageSource từ file: {result.FullPath} - {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[LOG] Không chọn được ảnh từ thư viện!");
        }
        System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {LicenseImages.Count}");
        PickLicenseImagesCommand.NotifyCanExecuteChanged();
        CaptureLicenseImageCommand.NotifyCanExecuteChanged();
    }

    private async Task CaptureLicenseImageAsync()
    {
        System.Diagnostics.Debug.WriteLine("[LOG] Bắt đầu chụp ảnh...");
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
            LicenseImageFiles.Add(file);
            try
            {
                var copiedPath = await CopyFileToAppDirectoryAsync(file);
                var imgSource = ImageSource.FromFile(copiedPath);
                LicenseImages.Add(new MyImageModel { LicenseImage = imgSource, LocalPath = copiedPath });
                System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào LicenseImages (FromFile): {copiedPath}");
                OnPropertyChanged(nameof(CanAddMoreLicenseImages));
                OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
                OnPropertyChanged(nameof(LicenseButtonTextColor));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Không thể copy hoặc tạo ImageSource từ file: {file.FullPath} - {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[LOG] Không lấy được file ảnh từ camera!");
        }
        System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {LicenseImages.Count}");
        PickLicenseImagesCommand.NotifyCanExecuteChanged();
        CaptureLicenseImageCommand.NotifyCanExecuteChanged();
    }

    private void RemoveLicenseImage(ImageSource img)
    {
        int idx = LicenseImages.IndexOf(LicenseImages.FirstOrDefault(i => i.LicenseImage == img));
        if (idx >= 0)
        {
            LicenseImages.RemoveAt(idx);
            if (LicenseImageFiles.Count > idx)
                LicenseImageFiles.RemoveAt(idx);
            OnPropertyChanged(nameof(CanAddMoreLicenseImages));
            OnPropertyChanged(nameof(LicenseButtonBackgroundColor));
            OnPropertyChanged(nameof(LicenseButtonTextColor));
        }
        PickLicenseImagesCommand.NotifyCanExecuteChanged();
        CaptureLicenseImageCommand.NotifyCanExecuteChanged();
    }
}
