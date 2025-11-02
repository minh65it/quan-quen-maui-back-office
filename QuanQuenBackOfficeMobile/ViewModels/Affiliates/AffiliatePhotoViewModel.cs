using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Views.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Handlers;

namespace QuanQuenBackOfficeMobile.ViewModels.Affiliates
{
    public partial class AffiliatePhotoViewModel : ObservableObject
    {
        private readonly IStoreMediaService _storeMediaService;
        private readonly IStoreService _storeService;
        
        // Khu 1: Ảnh Nhà hàng (tối đa 3)
        public ObservableCollection<MyImageModel> RestaurantImages { get; } = new();
        // Khu 2: CCCD 2 mặt
        public MyImageModel IdentityFront { get; private set; }
        public MyImageModel IdentityBack { get; private set; }
        // Khu 3: Ảnh Menu (1 ảnh)
        public ObservableCollection<MyImageModel> MenuImages { get; } = new();

        // Khu cũ (giữ tương thích nếu cần)
        public ObservableCollection<MyImageModel> StoreImages { get; } = new();
        public List<FileResult> StoreImageFiles { get; } = new();
        public IAsyncRelayCommand PickImageCommand { get; }
        public IAsyncRelayCommand CaptureImageCommand { get; }
        public bool CanAddMoreImages => StoreImages.Count < 6;
        public bool CanAddMoreRestaurantImages => RestaurantImages.Count < 3;
        public bool CanAddMoreMenuImages => MenuImages.Count < 1;
        public string ButtonBackgroundColor => CanAddMoreImages ? "#F5F5F5" : "Gray";
        public string ButtonTextColor => CanAddMoreImages ? "#F58220" : "Gray";
        public string ImageCounterText => $"Đã chọn {StoreImages.Count}/6 ảnh";

        [ObservableProperty]
        private bool isLoading = false;

        public IRelayCommand<ImageSource> RemoveImageCommand { get; }

        public AffiliatePhotoViewModel(IStoreMediaService storeMediaService, IStoreService storeService)
        {
            _storeMediaService = storeMediaService;
            _storeService = storeService;
            PickImageCommand = new AsyncRelayCommand(DeprecatedPickImagesAsync, () => CanAddMoreImages);
            CaptureImageCommand = new AsyncRelayCommand(DeprecatedCaptureImageAsync, () => CanAddMoreImages);
            RemoveImageCommand = new RelayCommand<ImageSource>(RemoveImage);
        }

        private async Task<string> CopyFileToAppDirectoryAsync(FileResult file)
        {
            try
            {
                var appDir = FileSystem.AppDataDirectory;
                var fileName = Path.GetFileName(file.FullPath);
                var destPath = Path.Combine(appDir, $"{Guid.NewGuid()}_{fileName}");
                
                using (var sourceStream = await file.OpenReadAsync())
                using (var destStream = File.OpenWrite(destPath))
                {
                    var buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await destStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[LOG] Đã copy file sang: {destPath}");
                return destPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi khi copy file: {ex.Message}");
                throw;
            }
        }

        public async Task AddImageFromFileAsync(FileResult file)
        {
            try
            {
                if (!CanAddMoreImages)
                {
                    System.Diagnostics.Debug.WriteLine("[LOG] Không thể thêm ảnh mới, đã đủ 6 ảnh.");
                    return;
                }

                if (file != null)
                {
                    if (StoreImageFiles.Count >= 6)
                    {
                        System.Diagnostics.Debug.WriteLine("[LOG] Đã đủ 6 ảnh, dừng thêm.");
                        return;
                    }
                    if (StoreImageFiles.Any(f => f.FullPath == file.FullPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOG] Ảnh đã tồn tại: {file.FullPath}");
                        return;
                    }

                    // Xử lý file đơn giản
                    var copiedPath = await CopyFileToAppDirectoryAsync(file);
                    
                    StoreImageFiles.Add(file);
                    System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm file vào StoreImageFiles: {file.FullPath}");
                    
                    var imgSource = ImageSource.FromFile(copiedPath);
                    StoreImages.Add(new MyImageModel { Images = imgSource, LocalPath = copiedPath });
                    System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào StoreImages (FromFile): {copiedPath}");
                    
                    OnPropertyChanged(nameof(CanAddMoreImages));
                    OnPropertyChanged(nameof(ButtonBackgroundColor));
                    OnPropertyChanged(nameof(ButtonTextColor));
                    OnPropertyChanged(nameof(ImageCounterText));
                    
                    PickImageCommand.NotifyCanExecuteChanged();
                    CaptureImageCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LOG] Không có file ảnh hợp lệ để thêm!");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][PERMISSION] Permission denied: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Lỗi", "Ứng dụng cần quyền truy cập ảnh!", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi trong AddImageFromFileAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra khi xử lý ảnh: {ex.Message}", "OK");
            }
            
            System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {StoreImages.Count}");
        }

        // --- API cho từng khu ảnh mới ---
        public async Task AddRestaurantImageFromFileAsync(FileResult file)
        {
            if (!CanAddMoreRestaurantImages || file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            RestaurantImages.Add(new MyImageModel { Images = ImageSource.FromFile(copiedPath), LocalPath = copiedPath });
            OnPropertyChanged(nameof(CanAddMoreRestaurantImages));
        }

        public async Task SetIdentityFrontFromFileAsync(FileResult file)
        {
            if (file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            IdentityFront = new MyImageModel { Images = ImageSource.FromFile(copiedPath), LocalPath = copiedPath };
            OnPropertyChanged(nameof(IdentityFront));
        }

        public async Task SetIdentityBackFromFileAsync(FileResult file)
        {
            if (file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            IdentityBack = new MyImageModel { Images = ImageSource.FromFile(copiedPath), LocalPath = copiedPath };
            OnPropertyChanged(nameof(IdentityBack));
        }

        public async Task AddMenuImageFromFileAsync(FileResult file)
        {
            if (!CanAddMoreMenuImages || file == null) return;
            var copiedPath = await CopyFileToAppDirectoryAsync(file);
            MenuImages.Clear();
            MenuImages.Add(new MyImageModel { Images = ImageSource.FromFile(copiedPath), LocalPath = copiedPath });
            OnPropertyChanged(nameof(CanAddMoreMenuImages));
        }

        // Các phương thức command cũ: hiển thị thông báo, hướng dẫn thao tác từ UI (Code-behind)
        private async Task DeprecatedPickImagesAsync()
        {
            await App.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng dùng nút 'Chọn ảnh nhà hàng' trên giao diện.", "OK");
        }

        private async Task DeprecatedCaptureImageAsync()
        {
            await App.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng dùng nút 'Chụp ảnh nhà hàng' trên giao diện.", "OK");
        }

        private void RemoveImage(ImageSource img)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliatePhotoViewModel] RemoveImage - Bắt đầu xóa ảnh");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliatePhotoViewModel] RemoveImage - StoreImages count trước khi xóa: {StoreImages.Count}");
            
            int idx = StoreImages.IndexOf(StoreImages.FirstOrDefault(i => i.Images == img));
            System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliatePhotoViewModel] RemoveImage - Index cần xóa: {idx}");
            
            if (idx >= 0)
            {
                var removedImage = StoreImages[idx];
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliatePhotoViewModel] RemoveImage - Xóa ảnh: {removedImage.LocalPath}");
                
                StoreImages.RemoveAt(idx);
                if (StoreImageFiles.Count > idx)
                    StoreImageFiles.RemoveAt(idx);
                    
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliatePhotoViewModel] RemoveImage - StoreImages count sau khi xóa: {StoreImages.Count}");
                
                OnPropertyChanged(nameof(CanAddMoreImages));
                OnPropertyChanged(nameof(ButtonBackgroundColor));
                OnPropertyChanged(nameof(ButtonTextColor));
                OnPropertyChanged(nameof(ImageCounterText));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG][AffiliatePhotoViewModel] RemoveImage - Không tìm thấy ảnh để xóa");
            }
            
            PickImageCommand.NotifyCanExecuteChanged();
            CaptureImageCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]  
        public async Task SavePhotoAsync()
        {
            // Kiểm tra có ít nhất một loại ảnh nào đó
            if (RestaurantImages.Count == 0 && IdentityFront == null && IdentityBack == null && MenuImages.Count == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Chưa có ảnh nào để lưu!", "OK");
                return;
            }
            
            var token = await SecureStorage.GetAsync("auth_token");
            var createdBy = await SecureStorage.GetAsync("userName");
            var storeId = StoreRegistrationData.StoreId;
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(createdBy) || string.IsNullOrEmpty(storeId))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Thiếu thông tin xác thực hoặc storeId!", "OK");
                return;
            }

            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            try
            {
                // Lưu từng loại ảnh riêng biệt
                var successCount = 0;
                var totalTasks = 0;

                // 1. Lưu ảnh nhà hàng (vẫn qua StoreMediaService)
                if (RestaurantImages.Count > 0)
                {
                    totalTasks++;
                    var restaurantCategoryId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync("Ảnh bìa");
                    if (!string.IsNullOrEmpty(restaurantCategoryId))
                    {
                        var restaurantBytes = new List<byte[]>();
                        foreach (var img in RestaurantImages)
                        {
                            if (img?.Images != null)
                            {
                                var bytes = await ImageSourceToBytesAsync(img.Images);
                                if (bytes != null) restaurantBytes.Add(bytes);
                            }
                        }
                        if (restaurantBytes.Count > 0)
                        {
                            var result = await _storeMediaService.UploadMediasAsync(storeId, restaurantCategoryId, createdBy, restaurantBytes, false, false);
                            if (result.IsSuccess) successCount++;
                        }
                    }
                }

                // 2, 3, 4. Lưu CCCD và menu qua API UpdateStore
                byte[]? frontIdentityBytes = null;
                byte[]? backIdentityBytes = null;
                byte[]? menuBytes = null;

                // Lấy CCCD mặt trước
                if (IdentityFront?.Images != null)
                {
                    totalTasks++;
                    frontIdentityBytes = await ImageSourceToBytesAsync(IdentityFront.Images);
                }

                // Lấy CCCD mặt sau
                if (IdentityBack?.Images != null)
                {
                    totalTasks++;
                    backIdentityBytes = await ImageSourceToBytesAsync(IdentityBack.Images);
                }

                // Lấy ảnh menu (chỉ lấy ảnh đầu tiên)
                if (MenuImages.Count > 0 && MenuImages[0]?.Images != null)
                {
                    totalTasks++;
                    menuBytes = await ImageSourceToBytesAsync(MenuImages[0].Images);
                }

                // Gửi CCCD và menu qua API UpdateStore
                if (frontIdentityBytes != null || backIdentityBytes != null || menuBytes != null)
                {
                    var updateResult = await _storeService.UpdateStoreWithImagesAsync(
                        storeId,
                        frontIdentityBytes,
                        backIdentityBytes,
                        menuBytes);
                    
                    if (updateResult.IsSuccess)
                    {
                        // Đếm số lượng ảnh đã gửi thành công
                        if (frontIdentityBytes != null) successCount++;
                        if (backIdentityBytes != null) successCount++;
                        if (menuBytes != null) successCount++;
                    }
                }

                // Hiển thị kết quả
                if (successCount == totalTasks && totalTasks > 0)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", $"Đã lưu thành công {successCount}/{totalTasks} loại ảnh!", "OK");
                    await ShowNextActionOptions();
                }
                else if (successCount > 0)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công một phần", $"Đã lưu thành công {successCount}/{totalTasks} loại ảnh!", "OK");
                    await ShowNextActionOptions();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể lưu ảnh nào!", "OK");
                }
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private async Task<byte[]> ImageSourceToBytesAsync(ImageSource imageSource)
        {
            // Chỉ hỗ trợ FileImageSource và StreamImageSource
            if (imageSource is FileImageSource fileImageSource)
            {
                var file = fileImageSource.File;
                if (File.Exists(file))
                {
                    return await File.ReadAllBytesAsync(file);
                }
            }
            else if (imageSource is StreamImageSource streamImageSource)
            {
                var streamTask = streamImageSource.Stream(CancellationToken.None);
                if (streamTask != null)
                {
                    using var stream = await streamTask;
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        [RelayCommand]
        public async Task SkipAsync()
        {
            try
            {
                // Hiển thị ActionSheet thay vì chuyển trực tiếp
                await ShowNextActionOptions();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        private async Task ShowNextActionOptions()
        {
            try
            {
                var action = await Application.Current.MainPage.DisplayActionSheet(
                    "Bạn muốn làm gì tiếp theo?",
                    "Hoàn Tất",
                    "Đăng xuất",
                    "Tạo store mới");

                switch (action)
                {
                    case "Hoàn Tất":
                        await NavigateToStoreList();
                        break;
                    case "Đăng xuất":
                        await LogoutFromAffiliate();
                        break;
                    case "Tạo store mới":
                        await NavigateToCreateStore();
                        break;
                    default:
                        await NavigateToStoreList();
                        break;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        private async Task LogoutFromAffiliate()
        {
            try
            {
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_token");
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_referral_id");
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_first_name");
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_email");

                var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateLoginViewModel>();
                Application.Current.MainPage = new AffiliateLoginPage(vm);
            }
            catch
            {
                var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateLoginViewModel>();
                Application.Current.MainPage = new AffiliateLoginPage(vm);
            }
        }

        private async Task NavigateToCreateStore()
        {
            var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateCreateStoreViewModel>();
            Application.Current.MainPage = new NavigationPage(new AffiliateCreateStorePage(vm));
            await Task.CompletedTask;
        }

        private async Task NavigateToStoreList()
        {
            var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateStoreListViewModel>();
            Application.Current.MainPage = new NavigationPage(new AffiliateStoreListPage(vm));
            await Task.CompletedTask;
        }
    }
}


