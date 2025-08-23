using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreDetailViewModel : ObservableObject
    {
        private readonly IStoreService _storeService;

        [ObservableProperty]
        private StoreDetailModel storeDetail;

        public StoreDetailViewModel(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public async Task LoadStoreDetailAsync(string storeId)
        {
            try
            {
                StoreDetail = await _storeService.GetStoreDetailAsync(storeId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tải chi tiết nhà hàng: " + ex.Message, "OK");
            }
        }

        public async Task ReloadStoreDetailAsync()
        {
            if (StoreDetail?.StoreId != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[StoreDetailViewModel] Reloading store detail for store: {StoreDetail.StoreId}");
                    var newStoreDetail = await _storeService.GetStoreDetailAsync(StoreDetail.StoreId);
                    
                    if (newStoreDetail != null)
                    {
                        // Cập nhật StoreDetail với data mới
                        StoreDetail = newStoreDetail;
                        System.Diagnostics.Debug.WriteLine($"[StoreDetailViewModel] Store detail reloaded successfully");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[StoreDetailViewModel] Error reloading store detail: {ex.Message}");
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tải lại thông tin nhà hàng: " + ex.Message, "OK");
                }
            }
        }

        [RelayCommand]
        private async Task ViewOnMap()
        {
            System.Diagnostics.Debug.WriteLine("[StoreDetailViewModel] ViewOnMap called");
            if (StoreDetail == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[StoreDetailViewModel] Creating StoreMapViewModel for store: {StoreDetail.StoreName}");
                // Tạo ViewModel cho trang map
                var mapViewModel = new QuanQuenBackOfficeMobile.ViewModels.Stores.StoreMapViewModel(StoreDetail, _storeService);
                
                System.Diagnostics.Debug.WriteLine("[StoreDetailViewModel] Creating StoreMapPage");
                // Tạo và điều hướng đến trang map
                var mapPage = new QuanQuenBackOfficeMobile.Views.Stores.StoreMapPage(mapViewModel);
                
                System.Diagnostics.Debug.WriteLine("[StoreDetailViewModel] Navigating to StoreMapPage");
                await App.Current.MainPage.Navigation.PushAsync(mapPage);
                System.Diagnostics.Debug.WriteLine("[StoreDetailViewModel] Navigation completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreDetailViewModel] Error: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể mở bản đồ: " + ex.Message, "OK");
            }
        }
    }
} 