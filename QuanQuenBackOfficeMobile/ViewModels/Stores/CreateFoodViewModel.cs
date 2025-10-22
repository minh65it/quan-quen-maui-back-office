using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    [QueryProperty(nameof(StoreId), "storeId")]
    [QueryProperty(nameof(FilterFoodId), "filterFoodId")]
    [QueryProperty(nameof(FilterName), "filterName")]
    public partial class CreateFoodViewModel : ObservableObject
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

        public CreateFoodViewModel(IFoodFilterService foodFilterService)
        {
            _foodFilterService = foodFilterService;
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

                var message = await _foodFilterService.CreateFoodAsync(
                    StoreId,
                    FilterFoodId,
                    FoodName,
                    Description,
                    priceValue,
                    priceSaleValue,
                    FoodName // FoodUnitId = Name
                );

                await App.Current.MainPage.DisplayAlert("Thành công", message, "OK");
                
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
            await Shell.Current.GoToAsync("..");
        }
    }
}

