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

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public partial class PhotoStoreUserViewModel : ObservableObject
    {
        private readonly IStoreMediaService _storeMediaService;
        private readonly Services.ICameraService _cameraService;
        private readonly Services.IPerformanceOptimizationService _performanceService;
        
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

        public PhotoStoreUserViewModel(IStoreMediaService storeMediaService, Services.ICameraService cameraService, Services.IPerformanceOptimizationService performanceService)
        {
            _storeMediaService = storeMediaService;
            _cameraService = cameraService;
            _performanceService = performanceService;
            PickImageCommand = new AsyncRelayCommand(PickImagesAsync, () => CanAddMoreImages);
            CaptureImageCommand = new AsyncRelayCommand(CaptureImageAsync, () => CanAddMoreImages);
            RemoveImageCommand = new RelayCommand<ImageSource>(RemoveImage);
        }

        [RelayCommand]
        public async Task AddCoverImageAsync()
        {
            // Command cũ - giữ lại để tương thích ngược
            await CaptureImageAsync();
        }

        [RelayCommand]
        public async Task AddGalleryImageAtIndexAsync(ImageItem tappedImage)
        {
            // Command cũ - giữ lại để tương thích ngược
            await CaptureImageAsync();
        }

        private async Task<string> CopyFileToAppDirectoryAsync(FileResult file)
        {
            return await _performanceService.RunOnBackgroundThreadAsync(async () =>
            {
                try
                {
                    var appDir = FileSystem.AppDataDirectory;
                    var fileName = Path.GetFileName(file.FullPath);
                    var destPath = Path.Combine(appDir, $"{Guid.NewGuid()}_{fileName}");
                    
                    // Sử dụng buffer nhỏ hơn để giảm memory pressure
                    using (var sourceStream = await file.OpenReadAsync())
                    using (var destStream = File.OpenWrite(destPath))
                    {
                        var buffer = new byte[4096]; // 4KB buffer thay vì 8KB
                        int bytesRead;
                        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await destStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                    
                    // Optimize image nếu cần
                    var optimizedPath = await _cameraService.OptimizeImageAsync(destPath);
                    
                    System.Diagnostics.Debug.WriteLine($"[LOG] Đã copy file sang: {optimizedPath}");
                    return optimizedPath;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi khi copy file: {ex.Message}");
                    throw;
                }
            }, "CopyFileToAppDirectory");
        }

        private async Task PickImagesAsync()
        {
            System.Diagnostics.Debug.WriteLine("[LOG] Bắt đầu chọn ảnh từ thư viện...");
            
            try
            {
                if (!CanAddMoreImages)
                {
                    System.Diagnostics.Debug.WriteLine("[LOG] Không thể thêm ảnh mới, đã đủ 6 ảnh.");
                    return;
                }

                // Sử dụng CameraService với timeout 30 giây
                var result = await _cameraService.PickPhotoWithTimeoutAsync(TimeSpan.FromSeconds(30));
                
                if (result != null)
                {
                    if (StoreImageFiles.Count >= 6)
                    {
                        System.Diagnostics.Debug.WriteLine("[LOG] Đã đủ 6 ảnh, dừng thêm.");
                        return;
                    }
                    if (StoreImageFiles.Any(f => f.FullPath == result.FullPath))
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
                                StoreImageFiles.Add(result);
                                System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm file vào StoreImageFiles: {result.FullPath}");
                                
                                var imgSource = ImageSource.FromFile(copiedPath);
                                StoreImages.Add(new MyImageModel { Images = imgSource, LocalPath = copiedPath });
                                System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào StoreImages (FromFile): {copiedPath}");
                                
                                OnPropertyChanged(nameof(CanAddMoreImages));
                                OnPropertyChanged(nameof(ButtonBackgroundColor));
                                OnPropertyChanged(nameof(ButtonTextColor));
                                OnPropertyChanged(nameof(ImageCounterText));
                                
                                PickImageCommand.NotifyCanExecuteChanged();
                                CaptureImageCommand.NotifyCanExecuteChanged();
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
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][TIMEOUT] Pick photo timeout: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Thông báo", "Chọn ảnh quá lâu đã được tự động hủy. Vui lòng thử lại.", "OK");
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][PERMISSION] Permission denied: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Lỗi", "Ứng dụng cần quyền truy cập thư viện ảnh!", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi trong PickImagesAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra khi chọn ảnh: {ex.Message}", "OK");
            }
            
            System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {StoreImages.Count}");
        }

        private async Task CaptureImageAsync()
        {
            System.Diagnostics.Debug.WriteLine("[LOG] Bắt đầu chụp ảnh...");
            
            try
            {
                if (!CanAddMoreImages)
                {
                    System.Diagnostics.Debug.WriteLine("[LOG] Không thể thêm ảnh mới, đã đủ 6 ảnh.");
                    return;
                }

                // Sử dụng CameraService với timeout 60 giây
                var file = await _cameraService.CapturePhotoWithTimeoutAsync(TimeSpan.FromSeconds(60));
                
                if (file != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOG] Đã chụp ảnh: {file.FullPath}");
                    
                    // Xử lý file trên background thread để tránh block UI
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var copiedPath = await CopyFileToAppDirectoryAsync(file);
                            
                            // Quay lại main thread để update UI
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                StoreImageFiles.Add(file);
                                var imgSource = ImageSource.FromFile(copiedPath);
                                StoreImages.Add(new MyImageModel { Images = imgSource, LocalPath = copiedPath });
                                System.Diagnostics.Debug.WriteLine($"[LOG] Đã thêm vào StoreImages (FromFile): {copiedPath}");
                                
                                OnPropertyChanged(nameof(CanAddMoreImages));
                                OnPropertyChanged(nameof(ButtonBackgroundColor));
                                OnPropertyChanged(nameof(ButtonTextColor));
                                OnPropertyChanged(nameof(ImageCounterText));
                                
                                PickImageCommand.NotifyCanExecuteChanged();
                                CaptureImageCommand.NotifyCanExecuteChanged();
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
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG][TIMEOUT] Camera session timeout: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Thông báo", "Camera session quá lâu đã được tự động hủy để tránh crash. Vui lòng thử lại.", "OK");
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
                System.Diagnostics.Debug.WriteLine($"[LOG][ERROR] Lỗi trong CaptureImageAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra khi chụp ảnh: {ex.Message}", "OK");
            }
            
            System.Diagnostics.Debug.WriteLine($"[LOG] Tổng số ảnh hiện tại: {StoreImages.Count}");
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
        public async Task CaptureCoverImageAsync()
        {
            // Command cũ - giữ lại để tương thích ngược
            await CaptureImageAsync();
        }

        [RelayCommand]
        public async Task PickCoverImageAsync()
        {
            // Command cũ - giữ lại để tương thích ngược
            await PickImagesAsync();
        }

        [RelayCommand]
        public async Task CaptureGalleryImageAsync()
        {
            // Command cũ - giữ lại để tương thích ngược
            await CaptureImageAsync();
        }

        [RelayCommand]
        public async Task PickGalleryImageAsync()
        {
            // Command cũ - giữ lại để tương thích ngược
            await PickImagesAsync();
        }
    }
}