using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    [QueryProperty(nameof(StoreId), "storeId")]
    [QueryProperty(nameof(FilterFoodId), "filterFoodId")]
    [QueryProperty(nameof(FilterName), "filterName")]
    public partial class StoreCreateFoodViewModel : ObservableObject
    {
        private readonly IFoodFilterService _foodFilterService;

        [ObservableProperty]
        private string storeId = string.Empty;

        [ObservableProperty]
        private string filterFoodId = string.Empty;

        [ObservableProperty]
        private string filterName = string.Empty;

        [ObservableProperty]
        private string foodName = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string price = "0";

        [ObservableProperty]
        private string priceSale = "0";

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private ImageSource? selectedImage;

        [ObservableProperty]
        private bool hasSelectedImage;

        [ObservableProperty]
        private ObservableCollection<FoodAttribute> attributes = new();

        private Stream? _imageStream;
        private string? _imageFileName;

        public StoreCreateFoodViewModel(IFoodFilterService foodFilterService)
        {
            _foodFilterService = foodFilterService;
        }

        public async Task InitializeAsync()
        {
            await LoadAttributesAsync();
        }

        private async Task LoadAttributesAsync()
        {
            try
            {
                IsLoading = true;
                var attributeList = await _foodFilterService.GetAttributesAsync();
                
                Attributes.Clear();
                foreach (var attr in attributeList)
                {
                    Attributes.Add(attr);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải danh sách thuộc tính: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task PickImageAsync()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Chọn ảnh món ăn"
                });

                if (result != null)
                {
                    _imageStream = await result.OpenReadAsync();
                    _imageFileName = result.FileName;
                    SelectedImage = ImageSource.FromStream(() => result.OpenReadAsync().Result);
                    HasSelectedImage = true;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task TakePhotoAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh", "OK");
                    return;
                }

                var result = await MediaPicker.Default.CapturePhotoAsync();

                if (result != null)
                {
                    _imageStream = await result.OpenReadAsync();
                    _imageFileName = result.FileName;
                    SelectedImage = ImageSource.FromStream(() => result.OpenReadAsync().Result);
                    HasSelectedImage = true;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chụp ảnh: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public void RemoveImage()
        {
            _imageStream?.Dispose();
            _imageStream = null;
            _imageFileName = null;
            SelectedImage = null;
            HasSelectedImage = false;
        }

        [RelayCommand]
        public async Task CreateFoodAsync()
        {
            if (string.IsNullOrWhiteSpace(FoodName))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập tên món ăn", "OK");
                return;
            }

            if (!decimal.TryParse(Price, out var priceValue))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Giá không hợp lệ", "OK");
                return;
            }

            if (!decimal.TryParse(PriceSale, out var priceSaleValue))
            {
                priceSaleValue = priceValue; // Mặc định bằng giá gốc
            }

            try
            {
                IsLoading = true;

                // Lấy danh sách attribute IDs đã chọn
                var selectedAttributeIds = Attributes
                    .Where(a => a.IsSelected && !string.IsNullOrEmpty(a.Id))
                    .Select(a => a.Id!)
                    .ToList();

                var message = await _foodFilterService.CreateFoodAsync(
                    StoreId,
                    FilterFoodId,
                    FoodName,
                    Description,
                    priceValue,
                    priceSaleValue,
                    FoodName, // FoodUnitId = Name
                    false, // IsCombo = false
                    false, // IsOutOfStock = false
                    _imageStream,
                    _imageFileName,
                    selectedAttributeIds.Count > 0 ? selectedAttributeIds : null
                );

                await App.Current.MainPage.DisplayAlert("Thành công", message, "OK");
                
                // Cleanup
                RemoveImage();
                
                // Quay lại trang trước
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            RemoveImage();
            await Shell.Current.GoToAsync("..");
        }
    }
}

