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


    public ICommand RemoveImageCommand { get; }
    public ICommand UploadImagesCommand { get; }

    private readonly IStoreMediaService _storeMediaService;
    private readonly IStoreService _storeService;
    private readonly string _storeId;
    public event Action<bool, string> UploadCompleted;

    public StoreMenuViewModel(string storeId)
    {
        _storeMediaService = App.Services.GetService<IStoreMediaService>();
        _storeService = App.Services.GetService<IStoreService>();
        _storeId = storeId;
        RemoveImageCommand = new RelayCommand<MyImageModel>(OnRemoveImage);
        UploadImagesCommand = new AsyncRelayCommand(OnUploadImages, CanUploadImages);
    }

    public void AddImagePaths(IEnumerable<string> imagePaths)
    {
        if (imagePaths == null) return;
        foreach (var path in imagePaths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var imageModel = new MyImageModel
            {
                Images = ImageSource.FromFile(path),
                LocalPath = path
            };
            MenuImages.Add(imageModel);
        }
        UploadStatus = $"Đã chọn {MenuImages.Count} ảnh";
        ((AsyncRelayCommand)UploadImagesCommand).NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasMenuImage));
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
            // Không show alert ở VM; chỉ cập nhật trạng thái để UI xử lý
            UploadCompleted?.Invoke(false, "Vui lòng chọn ít nhất một ảnh");
            return;
        }

        IsLoading = true;
        UploadStatus = "Đang gửi ảnh nhận diện món...";

        try
        {
            // Gọi API GenerateFoodFromImage với storeId + danh sách file ảnh
            var storeId = _storeId;
            var imagePaths = MenuImages.Select(m => m.LocalPath).ToList();

            var result = await _storeService.GenerateFoodFromImageAsync(storeId, imagePaths);
            if (result.IsSuccess)
                UploadCompleted?.Invoke(true, "Đã gửi ảnh menu. Hệ thống sẽ tự động sinh món ăn.");
            else
                UploadCompleted?.Invoke(false, result.Message ?? "Gửi ảnh thất bại");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StoreMenuViewModel] OnUploadImages Exception: {ex}");
            UploadCompleted?.Invoke(false, $"Có lỗi xảy ra: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            UploadStatus = "";
        }
    }
} 