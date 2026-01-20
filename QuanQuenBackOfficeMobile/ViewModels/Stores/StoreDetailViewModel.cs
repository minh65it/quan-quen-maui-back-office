using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.IO;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreDetailViewModel : ObservableObject
    {
        private readonly IStoreService _storeService;
        private readonly IServicePackagesService _servicePackagesService;

        [ObservableProperty]
        private StoreDetailModel storeDetail;

        [ObservableProperty]
        private ObservableCollection<string> storePackageOptions = new(new[] { "None", "QuanQuen", "QuanMoi", "DongHanh" });

        [ObservableProperty]
        private int selectedStorePackageIndex = 0;

        [ObservableProperty]
        private ObservableCollection<string> monthOptions = new(new[] { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" });

        [ObservableProperty]
        private int selectedMonthIndex = 0;

        [ObservableProperty]
        private bool isUploadingInvoice = false;

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

        [RelayCommand]
        private async Task UpdateStorePackage()
        {
            if (StoreDetail?.StoreId is null)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy StoreId", "OK");
                return;
            }

            try
            {
                var svc = App.Services.GetService<IServicePackagesService>();
                var response = await svc.UpdateStorePackageAsync(StoreDetail.StoreId, SelectedStorePackageIndex);
                if (response.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã cập nhật gói dịch vụ", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Cập nhật thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task UploadInvoiceImage()
        {
            if (StoreDetail?.StoreId is null)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy StoreId", "OK");
                return;
            }

            try
            {
                IsUploadingInvoice = true;

                // Chọn ảnh từ thư viện
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo == null)
                {
                    IsUploadingInvoice = false;
                    return;
                }

                // Đọc file ảnh
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Lấy tháng được chọn (từ 1-12)
                var selectedMonth = SelectedMonthIndex + 1;

                // Upload ảnh
                var storeMediaService = App.Services.GetService<IStoreMediaService>();
                var response = await storeMediaService.UploadInvoiceImageAsync(
                    StoreDetail.StoreId, 
                    selectedMonth, 
                    imageBytes, 
                    photo.FileName ?? "invoice.jpg"
                );

                if (response.IsSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã upload ảnh hóa đơn thành công", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", response.Message ?? "Upload thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi khi upload ảnh hóa đơn: {ex.Message}", "OK");
            }
            finally
            {
                IsUploadingInvoice = false;
            }
        }
    }
} 