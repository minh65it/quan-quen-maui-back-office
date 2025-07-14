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

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public partial class PhotoStoreUserViewModel : ObservableObject
    {
        private readonly IStoreMediaService _storeMediaService;

        [ObservableProperty]
        private ImageSource coverImageSource;
        [ObservableProperty]
        private ObservableCollection<ImageItem> galleryImages = new(Enumerable.Range(0, 8).Select(_ => new ImageItem()).ToList());
        public bool CanAddMoreImages => GalleryImages.Count(img => img.Source == null) > 0;
        [ObservableProperty]
        private bool isLoading = false;

        public PhotoStoreUserViewModel(IStoreMediaService storeMediaService)
        {
            _storeMediaService = storeMediaService;
        }

        [RelayCommand]
        public async Task AddCoverImageAsync()
        {
            string action = await Shell.Current.DisplayActionSheet("Chọn ảnh bìa", "Hủy", null, "Chụp ảnh mới", "Chọn từ thư viện");
            FileResult fileResult = null;
            if (action == "Chụp ảnh mới")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                    fileResult = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (action == "Chọn từ thư viện")
            {
                fileResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh bìa" });
            }
            if (fileResult != null)
            {
                using var stream = await fileResult.OpenReadAsync();
                var bytes = ReadFully(stream);
                CoverImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }

        [RelayCommand]
        public async Task AddGalleryImageAtIndexAsync(ImageItem tappedImage)
        {
            int index = GalleryImages.IndexOf(tappedImage);
            if (index < 0 || index >= 8) return;
            string action = await Shell.Current.DisplayActionSheet("Thêm ảnh", "Hủy", null, "Chụp ảnh mới", "Chọn từ thư viện");
            FileResult fileResult = null;
            if (action == "Chụp ảnh mới")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                    fileResult = await MediaPicker.Default.CapturePhotoAsync();  
            }
            else if (action == "Chọn từ thư viện")
            {
                fileResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh" });
            }
            if (fileResult != null)
            {
                using var stream = await fileResult.OpenReadAsync();
                var bytes = ReadFully(stream);
                tappedImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                OnPropertyChanged(nameof(CanAddMoreImages));
            }
        }

        [RelayCommand]  
        public async Task SavePhotoAsync()
        {
            if (CoverImageSource == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Chưa có ảnh bìa!", "OK");
                return;
            }
            if (GalleryImages.All(img => img == null))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Chưa có ảnh bìa !", "OK");
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
                await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy categoryMediaId cho ảnh bìa!", "OK");
                return;
            }

            var imageBytesList = new List<byte[]>();
            var coverBytes = await ImageSourceToBytesAsync(CoverImageSource);
            if (coverBytes != null) imageBytesList.Add(coverBytes);
            foreach (var img in GalleryImages)
            {
                if (img == null) continue;
                var bytes = await ImageSourceToBytesAsync(img.Source);
                if (bytes != null) imageBytesList.Add(bytes);
            }

            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            try
            {
                var result = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy, imageBytesList, false, false);
                if (result.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Thêm ảnh bìa nhà hàng thành công!", "OK");
                    await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.Stores.StoreLevelPage());
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

        [RelayCommand]
        public async Task RecordVideoAsync()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var video = await MediaPicker.Default.CaptureVideoAsync();
                    if (video != null)
                    {
                        await Shell.Current.DisplayAlert("Thành công", $"Đã quay video: {video.FullPath}", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ quay video.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể quay video: {ex.Message}", "OK");
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

        private byte[] ReadFully(Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}