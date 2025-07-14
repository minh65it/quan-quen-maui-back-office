using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Linq;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class StoreMenuViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<MyImageModel> _menuImages = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _uploadStatus = "";

    public bool HasMenuImage => MenuImages != null && MenuImages.Count > 0;


    public ICommand AddImageCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand RemoveImageCommand { get; }
    public ICommand UploadImagesCommand { get; }
    public ICommand SkipCommand { get; }

    private readonly IStoreMediaService _storeMediaService;

    public StoreMenuViewModel()
    {
        _storeMediaService = App.Services.GetService<IStoreMediaService>();
        AddImageCommand = new AsyncRelayCommand(OnAddImage);
        TakePhotoCommand = new AsyncRelayCommand(OnTakePhoto);
        RemoveImageCommand = new RelayCommand<MyImageModel>(OnRemoveImage);
        UploadImagesCommand = new AsyncRelayCommand(OnUploadImages, CanUploadImages);
        SkipCommand = new AsyncRelayCommand(OnSkip);
    }

    private async Task OnAddImage()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            {
                var imageModel = new MyImageModel
                {
                    Images = ImageSource.FromFile(result.FullPath),
                    LocalPath = result.FullPath
                };

                MenuImages.Add(imageModel);
                UploadStatus = $"Đã chọn {MenuImages.Count} ảnh";
                ((AsyncRelayCommand)UploadImagesCommand).NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(HasMenuImage));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMenuViewModel] OnAddImage Exception: {ex}");
            await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể chọn ảnh", "OK");
        }
    }

    private async Task OnTakePhoto()
    {
        try
        {
            if (MediaPicker.IsCaptureSupported)
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    var imageModel = new MyImageModel
                    {
                        Images = ImageSource.FromFile(photo.FullPath),
                        LocalPath = photo.FullPath
                    };

                    MenuImages.Add(imageModel);
                    UploadStatus = $"Đã chọn {MenuImages.Count} ảnh";
                    ((AsyncRelayCommand)UploadImagesCommand).NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(HasMenuImage));
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMenuViewModel] OnTakePhoto Exception: {ex}");
            await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể chụp ảnh", "OK");
        }
    }

    private void OnRemoveImage(MyImageModel image)
    {
        if (image != null)
        {
            MenuImages.Remove(image);
            UploadStatus = $"Đã chọn {MenuImages.Count} ảnh";
            ((AsyncRelayCommand)UploadImagesCommand).NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(HasMenuImage));
        }
    }

    private bool CanUploadImages()
    {
        return MenuImages.Count > 0 && !IsLoading;
    }

    private async Task OnUploadImages()
    {
        if (MenuImages.Count == 0)
        {
            await App.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng chọn ít nhất một ảnh", "OK");
            return;
        }

        IsLoading = true;
        UploadStatus = "Đang upload ảnh...";

        try
        {
            // 1. Lấy categoryMediaId cho "Ảnh nhà hàng"
            var categoryMediaId = await _storeMediaService.GetThumbnailCategoryMediaIdAsync("Ảnh nhà hàng");
            if (string.IsNullOrEmpty(categoryMediaId))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể lấy thông tin category media", "OK");
                return;
            }

            // 2. Convert ảnh thành byte array
            var imageBytesList = new List<byte[]>();
            foreach (var image in MenuImages)
            {
                try
                {
                    var bytes = await File.ReadAllBytesAsync(image.LocalPath);
                    imageBytesList.Add(bytes);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StoreMenuViewModel] Error reading image {image.LocalPath}: {ex}");
                }
            }

            if (imageBytesList.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể đọc ảnh", "OK");
                return;
            }

            // 3. Upload ảnh
            var storeId = StoreRegistrationData.StoreId;
            var createdBy = await SecureStorage.GetAsync("userName");

            var uploadResult = await _storeMediaService.UploadMediasAsync(storeId, categoryMediaId, createdBy,imageBytesList,false,true);

            if (uploadResult.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert("Thành công", "Upload ảnh menu thành công!", "OK");
                await App.Current.MainPage.Navigation.PopToRootAsync();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", uploadResult.Message ?? "Upload ảnh thất bại", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMenuViewModel] OnUploadImages Exception: {ex}");
            await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            UploadStatus = "";
        }
    }

    private async Task OnSkip()
    {
        var result = await App.Current.MainPage.DisplayAlert(
            "Xác nhận", 
            "Bạn có chắc chắn muốn bỏ qua việc upload ảnh menu?", 
            "Có", 
            "Không");

        if (result)
        {
            await App.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
} 