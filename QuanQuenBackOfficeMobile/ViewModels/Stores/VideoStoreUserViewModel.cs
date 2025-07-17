using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class VideoStoreUserViewModel : ObservableObject
    {
        private readonly IStoreMediaService _storeMediaService;

        [ObservableProperty]
        private ImageSource videoThumbnail;
        
        [ObservableProperty]
        private bool hasVideo = false;
        
        [ObservableProperty]
        private bool isLoading = false;
        
        [ObservableProperty]
        private string videoFileName = "";
        
        [ObservableProperty]
        private string videoDuration = "";
        
        [ObservableProperty]
        private string videoSize = "";
        
        [ObservableProperty]
        private FileResult recordedVideo;

        public VideoStoreUserViewModel(IStoreMediaService storeMediaService)
        {
            _storeMediaService = storeMediaService;
        }

        [RelayCommand]
        public async Task StartRecordingAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ quay video.", "OK");
                    return;
                }

                var video = await MediaPicker.Default.CaptureVideoAsync(new MediaPickerOptions 
                { 
                    Title = "Quay video nhà hàng" 
                });

                if (video != null)
                {
                    RecordedVideo = video;
                    HasVideo = true;
                    VideoThumbnail = ImageSource.FromFile("iconmediaelement.svg");
                    var fileInfo = new FileInfo(video.FullPath);
                    VideoFileName = video.FileName;
                    VideoSize = FormatFileSize(fileInfo.Length);
                    VideoDuration = "Đang xử lý...";
                    // Bỏ popup thành công ở đây
                }
                else
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không có video nào được quay.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể quay video: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public void DeleteVideo()
        {
            RecordedVideo = null;
            HasVideo = false;
            VideoThumbnail = null;
            VideoFileName = string.Empty;
            VideoDuration = string.Empty;
            VideoSize = string.Empty;
        }

        [RelayCommand]
        public async Task SaveVideoAsync()
        {
            if (RecordedVideo == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không có video nào để lưu!", "OK");
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
            try
            {
                using var stream = await RecordedVideo.OpenReadAsync();
                var videoBytes = ReadFully(stream);
                var nameMedia = "Video nhà hàng";
                var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync(nameMedia);
                if (string.IsNullOrEmpty(categoryMediaId))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy categoryMediaId cho video!", "OK");
                    return;
                }
                var videoBytesList = new List<byte[]> { videoBytes };
                var result = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy, videoBytesList, false, false);
                if (result.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Video đã được lưu thành công!", "OK");
                    await NavigateToNextPage();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu video: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SkipVideoAsync()
        {
            var result = await Shell.Current.DisplayAlert(
                "Xác nhận", 
                "Bạn có muốn bỏ qua việc quay video và chuyển đến bước tiếp theo không?", 
                "Có", "Không");
            if (result)
            {
                await NavigateToNextPage();
            }
        }

        private async Task NavigateToNextPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.Stores.StoreLevelPage());
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private byte[] ReadFully(Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
} 