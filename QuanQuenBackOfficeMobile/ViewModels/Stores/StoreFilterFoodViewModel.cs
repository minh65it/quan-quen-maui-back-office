using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    [QueryProperty(nameof(StoreId), "storeId")]
    [QueryProperty(nameof(StoreName), "storeName")]
    public partial class StoreFilterFoodViewModel : ObservableObject
    {
        private readonly IFoodFilterService _foodFilterService;

        [ObservableProperty]
        private string storeId = string.Empty;

        [ObservableProperty]
        private string storeName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<FoodFilterItem> items = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string currentEditingItemId = string.Empty;

        [ObservableProperty]
        private string editingNameValue = string.Empty; 

        [ObservableProperty]
        private string newFilterName = string.Empty; // Tên cho filter mới

        public bool HasNoItems => !IsLoading && Items.Count == 0;

        public StoreFilterFoodViewModel(IFoodFilterService foodFilterService)
        {
            _foodFilterService = foodFilterService;
        }

        partial void OnStoreIdChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] StoreId changed to: {value}");
            // Tự động load khi StoreId được set
            if (!string.IsNullOrWhiteSpace(value))
            {
                MainThread.BeginInvokeOnMainThread(async () => await LoadAsync());
            }
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] LoadAsync called");
            System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] StoreId: {StoreId}");
            
            if (string.IsNullOrWhiteSpace(StoreId))
            {
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] StoreId is null or empty, returning");
                return;
            }
            
            try
            {
                IsLoading = true;
                // Reset edit mode khi reload
                CurrentEditingItemId = string.Empty;
                
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] Calling GetFoodFiltersAsync...");
                
                var data = await _foodFilterService.GetFoodFiltersAsync(StoreId);
                
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] Received {data?.Count ?? 0} items");
                
                Items = new ObservableCollection<FoodFilterItem>(data);
                
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] Items.Count: {Items.Count}");
                OnPropertyChanged(nameof(HasNoItems));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] Stack Trace: {ex.StackTrace}");
                await App.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasNoItems));
                System.Diagnostics.Debug.WriteLine($"[StoreFilterFoodViewModel] LoadAsync completed");
            }
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (string.IsNullOrWhiteSpace(StoreId)) return;
            
            try
            {
                IsRefreshing = true;
                var data = await _foodFilterService.GetFoodFiltersAsync(StoreId);
                Items = new ObservableCollection<FoodFilterItem>(data);
                OnPropertyChanged(nameof(HasNoItems));
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public void StartEdit(FoodFilterItem item)
        {
            if (string.IsNullOrEmpty(item.FilterFoodId))
                return;
                
            CurrentEditingItemId = item.FilterFoodId;
            EditingNameValue = item.Name ?? string.Empty;
        }

        [RelayCommand]
        public void CancelEdit()
        {
            CurrentEditingItemId = string.Empty;
            EditingNameValue = string.Empty;
        }

        [RelayCommand]
        public async Task UpdateFoodFilterAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateFoodFilterAsync] CALLED!");
            System.Diagnostics.Debug.WriteLine($"[UpdateFoodFilterAsync] CurrentEditingItemId: {CurrentEditingItemId}");
            System.Diagnostics.Debug.WriteLine($"[UpdateFoodFilterAsync] EditingNameValue: {EditingNameValue}");
            
            if (string.IsNullOrWhiteSpace(CurrentEditingItemId) || string.IsNullOrWhiteSpace(EditingNameValue))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng điền tên bộ lọc", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                
                var success = await _foodFilterService.UpdateFoodFilterAsync(
                    CurrentEditingItemId,
                    StoreId,
                    EditingNameValue, // Name
                    EditingNameValue  // Value
                );

                if (success)
                {
                    // Tìm item trong collection và cập nhật trực tiếp
                    var item = Items.FirstOrDefault(x => x.FilterFoodId == CurrentEditingItemId);
                    if (item != null)
                    {
                        item.Name = EditingNameValue;
                        item.Value = EditingNameValue;
                    }

                    CancelEdit();
                    await App.Current.MainPage.DisplayAlert("Thành công", "Cập nhật bộ lọc món ăn thành công", "OK");
                }
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
        public async Task CreateFoodFilterAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[CreateFoodFilterAsync] CALLED!");
            System.Diagnostics.Debug.WriteLine($"[CreateFoodFilterAsync] NewFilterName: {NewFilterName}");
            
            if (string.IsNullOrWhiteSpace(NewFilterName))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng điền tên bộ lọc", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                
                // Tạo mới với Name và Value giống nhau
                var success = await _foodFilterService.CreateFoodFilterAsync(
                    StoreId,
                    NewFilterName, // Name
                    NewFilterName  // Value (giống Name)
                );

                if (success)
                {
                    NewFilterName = string.Empty; // Reset form
                    
                    // Reload lại danh sách để lấy item mới với ID từ server
                    await LoadAsync();
                    
                    await App.Current.MainPage.DisplayAlert("Thành công", "Tạo bộ lọc món ăn thành công", "OK");
                }
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
        public async Task CreateFoodForFilter(FoodFilterItem filterItem)
        {
            if (filterItem == null || string.IsNullOrEmpty(filterItem.FilterFoodId))
                return;

            // Navigate đến trang StoreCreateFood
            var navigationParameter = new Dictionary<string, object>
            {
                { "storeId", StoreId },
                { "filterFoodId", filterItem.FilterFoodId },
                { "filterName", filterItem.Name ?? "Bộ lọc" }
            };

            await Shell.Current.GoToAsync("StoreCreateFood", navigationParameter);
        }
    }
}

