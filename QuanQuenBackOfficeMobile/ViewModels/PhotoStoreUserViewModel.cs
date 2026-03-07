using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.ObjectModel;
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

namespace QuanQuenBackOfficeMobile.ViewModels;
    public partial class PhotoStoreUserViewModel : ObservableObject
    {
        private readonly IStoreMediaService _storeMediaService;
        
        public ObservableCollection<MyImageModel> StoreImages { get; } = new();
        public List<FileResult> StoreImageFiles { get; } = new();
        public IAsyncRelayCommand PickImageCommand { get; }
        public IAsyncRelayCommand CaptureImageCommand { get; }
        public bool CanAddMoreImages => StoreImages.Count < 6;
        public string ButtonBackgroundColor => CanAddMoreImages ? "#F5F5F5" : "Gray";
        public string ButtonTextColor => CanAddMoreImages ? "#F58220" : "Gray";
        public string ImageCounterText => $"Đã chọn {StoreImages.Count}/6 ảnh";

        [ObservableProperty]
        private bool isLoading = false;

        public IRelayCommand<ImageSource> RemoveImageCommand { get; }

        public PhotoStoreUserViewModel(IStoreMediaService storeMediaService)
        {
            _storeMediaService = storeMediaService;
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG][PhotoStoreUserViewModel] RemoveImage - Bắt đầu xóa ảnh");
            System.Diagnostics.Debug.WriteLine($"[DEBUG][PhotoStoreUserViewModel] RemoveImage - StoreImages count trước khi xóa: {StoreImages.Count}");
            
            int idx = StoreImages.IndexOf(StoreImages.FirstOrDefault(i => i.Images == img));
            System.Diagnostics.Debug.WriteLine($"[DEBUG][PhotoStoreUserViewModel] RemoveImage - Index cần xóa: {idx}");
            
            if (idx >= 0)
            {
                var removedImage = StoreImages[idx];
                System.Diagnostics.Debug.WriteLine($"[DEBUG][PhotoStoreUserViewModel] RemoveImage - Xóa ảnh: {removedImage.LocalPath}");
                
                StoreImages.RemoveAt(idx);
                if (StoreImageFiles.Count > idx)
                    StoreImageFiles.RemoveAt(idx);
                    
                System.Diagnostics.Debug.WriteLine($"[DEBUG][PhotoStoreUserViewModel] RemoveImage - StoreImages count sau khi xóa: {StoreImages.Count}");
                
                OnPropertyChanged(nameof(CanAddMoreImages));
                OnPropertyChanged(nameof(ButtonBackgroundColor));
                OnPropertyChanged(nameof(ButtonTextColor));
                OnPropertyChanged(nameof(ImageCounterText));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG][PhotoStoreUserViewModel] RemoveImage - Không tìm thấy ảnh để xóa");
            }
            
            PickImageCommand.NotifyCanExecuteChanged();
            CaptureImageCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]  
        public async Task SavePhotoAsync()
        {
            if (StoreImages.Count == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Chưa có ảnh nào!", "OK");
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
            
            var nameMedia = "Ảnh bìa";
            var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);
            if (string.IsNullOrEmpty(categoryMediaId))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy categoryMediaId cho ảnh nhà hàng!", "OK");
                return;
            }

            var imageBytesList = new List<byte[]>();
            foreach (var img in StoreImages)
            {
                if (img == null) continue;
                var bytes = await ImageSourceToBytesAsync(img.Images);
                if (bytes != null) imageBytesList.Add(bytes);
            }

            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            try
            {
                var result = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy, imageBytesList, false, false);
                if (result.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Thêm ảnh nhà hàng thành công!", "OK");
                    // Chuyển đến trang VideoStoreUserPage
                    var serviceProvider = Application.Current.MainPage.Handler.MauiContext.Services;
                    var videoStoreUserPage = serviceProvider.GetRequiredService<VideoStoreUserPage>();
                    await Application.Current.MainPage.Navigation.PushAsync(videoStoreUserPage);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", result.Message, "OK");
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
                // Chuyển trực tiếp đến trang VideoStoreUserPage mà không upload ảnh
                var serviceProvider = Application.Current.MainPage.Handler.MauiContext.Services;
                var videoStoreUserPage = serviceProvider.GetRequiredService<VideoStoreUserPage>();
                await Application.Current.MainPage.Navigation.PushAsync(videoStoreUserPage);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra khi chuyển trang: {ex.Message}", "OK");
            }
        }

    }
