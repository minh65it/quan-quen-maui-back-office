using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreDetailPage : ContentPage
{
    private readonly StoreDetailViewModel _viewModel;
    private bool _shouldReloadOnAppearing = false;

    public StoreDetailPage(StoreDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Chỉ reload khi cần thiết (từ map page)
        if (_shouldReloadOnAppearing && _viewModel.StoreDetail?.StoreId != null)
        {
            System.Diagnostics.Debug.WriteLine("[StoreDetailPage] Reloading store detail on appearing");
            _ = _viewModel.ReloadStoreDetailAsync();
            _shouldReloadOnAppearing = false; // Reset flag
        }
    }

    private async void ViewOnMap_Clicked(object sender, EventArgs e)
    {
        try
        {
            var storeService = App.Services.GetService<IStoreService>();
            if (storeService == null)
            {
                await DisplayAlert("Lỗi", "Không thể khởi tạo service", "OK");
                return;
            }

            var viewModel = new StoreMapViewModel(_viewModel.StoreDetail, storeService);
            var mapPage = new StoreMapPage(viewModel);
            
            // Đánh dấu cần reload khi quay về
            _shouldReloadOnAppearing = true;
            
            await Navigation.PushAsync(mapPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể mở bản đồ: {ex.Message}", "OK");
        }
    }

    private async void ScanQRCode_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_viewModel.StoreDetail?.StoreId == null)
            {
                await DisplayAlert("Lỗi", "Không tìm thấy thông tin nhà hàng", "OK");
                return;
            }

            // Navigate đến ScanningQRPage với StoreId
            var scanningPage = new ScanningQRPage(_viewModel.StoreDetail.StoreId);
            await Navigation.PushAsync(scanningPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể mở camera scan QR: {ex.Message}", "OK");
        }
    }

    private async void AddMenuImages_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_viewModel.StoreDetail?.StoreId == null)
            {
                await DisplayAlert("Lỗi", "Không tìm thấy thông tin nhà hàng", "OK");
                return;
            }

            // Navigate đến StoreMenuPage để thêm ảnh menu món ăn
            var storeMenuPage = new StoreMenuPage(_viewModel.StoreDetail.StoreId);
            await Navigation.PushAsync(storeMenuPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể mở trang thêm ảnh menu: {ex.Message}", "OK");
        }
    }
} 