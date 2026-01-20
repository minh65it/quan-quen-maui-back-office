using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreMapViewModel : ObservableObject
    {
        [ObservableProperty]
        private StoreDetailModel _storeDetail;

        [ObservableProperty]
        private string _storeName;

        [ObservableProperty]
        private string _storeAddress;

        [ObservableProperty]
        private Microsoft.Maui.Controls.Maps.Map _mapControl;

        private readonly IStoreService _storeService;

        public StoreMapViewModel(StoreDetailModel storeDetail, IStoreService storeService)
        {
            _storeDetail = storeDetail;
            _storeService = storeService;
            _storeName = storeDetail?.StoreName ?? "Nhà hàng";
            _storeAddress = storeDetail?.FullAdrress ?? "Không có địa chỉ";
            
            System.Diagnostics.Debug.WriteLine($"[StoreMapViewModel] Created for store: {_storeName}");
        }

        [RelayCommand]
        private async Task SaveLocationAsync()
        {
            try
            {
                if (_storeDetail == null || string.IsNullOrEmpty(_storeDetail.StoreId))
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có thông tin nhà hàng để lưu", "OK");
                    return;
                }

                var success = await _storeService.UpdateStoreLocationAsync(
                    _storeDetail.StoreId,
                    _storeDetail.Latitude ?? 0,
                    _storeDetail.Longitude ?? 0,
                    _storeDetail.FullAdrress ?? ""
                );

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Thành công", 
                        "Đã cập nhật vị trí nhà hàng lên server", 
                        "OK"
                    );
                    
                    System.Diagnostics.Debug.WriteLine($"[StoreMapViewModel] Location saved successfully for store: {_storeDetail.StoreId}");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Lỗi", 
                        "Không thể cập nhật vị trí lên server", 
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Lỗi", 
                    $"Lỗi khi lưu vị trí: {ex.Message}", 
                    "OK"
                );
                System.Diagnostics.Debug.WriteLine($"[StoreMapViewModel] SaveLocationAsync error: {ex.Message}");
            }
        }
    }
}
