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
using Microsoft.Maui.ApplicationModel;
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


    public ICommand RemoveImageCommand { get; }
    public ICommand UploadImagesCommand { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand SkipCommand { get; }

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
        PickPhotoCommand = new AsyncRelayCommand(OnPickPhoto);
        SkipCommand = new AsyncRelayCommand(OnSkip);
    }

    public void AddImagePaths(IEnumerable<string> imagePaths)
    {
        if (imagePaths == null) return;
        
        // Chỉ cho phép 1 ảnh - xóa ảnh cũ nếu có
        MenuImages.Clear();
        
        foreach (var path in imagePaths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var imageModel = new MyImageModel
            {
                Images = ImageSource.FromFile(path),
                LocalPath = path
            };
            MenuImages.Add(imageModel);
            
            // Chỉ thêm 1 ảnh đầu tiên
            break;
        }
        UploadStatus = MenuImages.Count > 0 ? "Đã chọn 1 ảnh" : "Chưa chọn ảnh";
        ((AsyncRelayCommand)UploadImagesCommand).NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasMenuImage));
    }

    private void OnRemoveImage(MyImageModel image)
    {
        if (image != null)
        {
            MenuImages.Remove(image);
            UploadStatus = MenuImages.Count > 0 ? "Đã chọn 1 ảnh" : "Chưa chọn ảnh";
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

    private async Task OnPickPhoto()
    {
        try
        {
            // Kiểm tra nếu đã có ảnh thì hỏi có muốn thay thế không
            if (MenuImages.Count > 0)
            {
                UploadCompleted?.Invoke(false, "Bạn đã chọn ảnh rồi. Chọn ảnh mới sẽ thay thế ảnh hiện tại.");
                return;
            }

            var status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                UploadCompleted?.Invoke(false, "Cần quyền truy cập thư viện ảnh để chọn ảnh");
                return;
            }

            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                AddImagePaths(new[] { result.FullPath });
            }
        }
        catch (Exception ex)
        {
            UploadCompleted?.Invoke(false, $"Không thể chọn ảnh: {ex.Message}");
        }
    }

    private async Task OnSkip()
    {
        // Event sẽ được xử lý trong code-behind để có thể hiển thị alert và navigation
        UploadCompleted?.Invoke(true, "skip");
    }
} 