using ZXing.Net.Maui;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.Views;
using System.Linq;
using Microsoft.Maui.Storage;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class ScanningQRPage : ContentPage
{
    private readonly IQRCodeService _qrCodeService;
    private string _storeId;
    private string _currentQRCode = string.Empty;

    public ScanningQRPage()
    {
        InitializeComponent();
        _qrCodeService = App.Services.GetService<IQRCodeService>();

        // Cấu hình ZXing
        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = true
        };
        barcodeReader.CameraLocation = CameraLocation.Rear;
        barcodeReader.IsDetecting = true;
    }

    public ScanningQRPage(string storeId) : this()
    {
        _storeId = storeId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckAndRestrictFeaturesAsync();
    }

    /// <summary>
    /// Kiểm tra và hạn chế chức năng dựa trên token
    /// </summary>
    private async Task CheckAndRestrictFeaturesAsync()
    {
        try
        {
            // Kiểm tra xem user có phải là affiliate không
            var isAffiliate = await SecureStorage.GetAsync("is_affiliate");
            var hasAffiliateToken = !string.IsNullOrEmpty(await SecureStorage.GetAsync("affiliate_token"));
            
            if (!hasAffiliateToken)
            {
                hasAffiliateToken = !string.IsNullOrEmpty(Preferences.Get("affiliate_token", ""));
            }

            System.Diagnostics.Debug.WriteLine($"[ScanningQRPage] is_affiliate: {isAffiliate}, hasAffiliateToken: {hasAffiliateToken}");

            // Nếu là affiliate, ẩn button tạo bàn
            if (isAffiliate == "true" || hasAffiliateToken)
            {
                System.Diagnostics.Debug.WriteLine("[ScanningQRPage] Hiding create table button for affiliate user");
                createTableButton.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScanningQRPage] Error checking user type: {ex.Message}");
        }
    }

    private async void CameraView_BarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {        
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var first = e.Results?.FirstOrDefault();
            if (first is null)
                return;

            var qrCodeText = first.Value;
            
            if (_currentQRCode == qrCodeText)
                return;
                
            _currentQRCode = qrCodeText;
            
            // Hiển thị QR Code đã scan
            barcodeResult.Text = $"✅ QR Code: {qrCodeText}";
            barcodeResult.TextColor = Colors.Green;
            
            // Luôn hiển thị button cập nhật QR
            confirmUpdateButton.IsVisible = true;
            
            // Kiểm tra xem có phải affiliate không để quyết định hiển thị button tạo bàn
            var isAffiliate = await SecureStorage.GetAsync("is_affiliate");
            var hasAffiliateToken = !string.IsNullOrEmpty(await SecureStorage.GetAsync("affiliate_token"));
            
            if (!hasAffiliateToken)
            {
                hasAffiliateToken = !string.IsNullOrEmpty(Preferences.Get("affiliate_token", ""));
            }

            // Chỉ hiển thị button tạo bàn nếu KHÔNG phải affiliate
            createTableButton.IsVisible = !(isAffiliate == "true" || hasAffiliateToken);
            
            System.Diagnostics.Debug.WriteLine($"[ScanningQRPage] QR detected - confirmButton: true, createTableButton: {createTableButton.IsVisible}");
        });
    }

    private async void SwitchCamera_Clicked(object sender, EventArgs e)
    {
        // Toggle giữa camera trước/sau
        barcodeReader.CameraLocation = barcodeReader.CameraLocation == CameraLocation.Rear
            ? CameraLocation.Front
            : CameraLocation.Rear;
        await Task.CompletedTask;
    }

    private async void ConfirmUpdate_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentQRCode) || string.IsNullOrEmpty(_storeId))
            {
                await DisplayAlert("Lỗi", "Thông tin QR Code hoặc Store ID không hợp lệ", "OK");
                return;
            }

            // Hiển thị loading
            barcodeResult.Text = "Đang cập nhật QR Code...";
            barcodeResult.TextColor = Colors.Blue;
            
            // Gọi API UpdateQRCode
            var success = await _qrCodeService.UpdateQRCodeAsync(_currentQRCode, "Store", _storeId);
            
            if (success)
            {
                barcodeResult.Text = "✅ Cập nhật QR Code thành công!";
                barcodeResult.TextColor = Colors.Green;
                
                // Hiển thị thông báo thành công
                await DisplayAlert("Thành công", "Đã cập nhật thông tin nhà hàng vào QR Code thành công!", "OK");
                
                // Quay lại trang trước - kiểm tra xem có phải affiliate không
                await NavigateBackAsync();
            }
            else
            {
                barcodeResult.Text = "❌ Cập nhật QR Code thất bại!";
                barcodeResult.TextColor = Colors.Red;
                
                // Hiển thị thông báo lỗi
                await DisplayAlert("Lỗi", "Có lỗi xảy ra khi cập nhật QR Code. Vui lòng thử lại!", "OK");
            }
        }
        catch (Exception ex)
        {
            barcodeResult.Text = $"❌ Lỗi: {ex.Message}";
            barcodeResult.TextColor = Colors.Red;
            
            await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
    }

    private async void CreateTableAndAssignQR_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentQRCode) || string.IsNullOrEmpty(_storeId))
            {
                await DisplayAlert("Lỗi", "Thông tin QR Code hoặc Store ID không hợp lệ", "OK");
                return;
            }

            barcodeResult.Text = "Đang tạo bàn và gán QR...";
            barcodeResult.TextColor = Colors.Blue;

            var ok = await _qrCodeService.ScanQRCodeAndCreateTableAsync(
                qrCode: _currentQRCode,
                storeId: _storeId,
                tableAreaId: null,
                tableName: null,
                tableNumber: null,
                capacity: null,
                tableDescription: null,
                userId: null,
                userName: null
            );

            if (ok)
            {
                barcodeResult.Text = "✅ Tạo bàn và gán QR thành công!";
                barcodeResult.TextColor = Colors.Green;
                await DisplayAlert("Thành công", "Đã tạo bàn và gán QRCode thành công.", "OK");
                await NavigateBackAsync();
            }
            else
            {
                barcodeResult.Text = "❌ Tạo bàn và gán QR thất bại!";
                barcodeResult.TextColor = Colors.Red;
                await DisplayAlert("Lỗi", "Không thể tạo bàn/gán QR. Vui lòng thử lại.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Quay lại trang trước - xử lý khác nhau cho affiliate và user thường
    /// </summary>
    private async Task NavigateBackAsync()
    {
        try
        {
            // Kiểm tra xem có phải affiliate không
            var isAffiliate = await SecureStorage.GetAsync("is_affiliate");
            var hasAffiliateToken = !string.IsNullOrEmpty(await SecureStorage.GetAsync("affiliate_token"));
            
            if (!hasAffiliateToken)
            {
                hasAffiliateToken = !string.IsNullOrEmpty(Preferences.Get("affiliate_token", ""));
            }

            if (isAffiliate == "true" || hasAffiliateToken)
            {
                // Nếu là affiliate, quay về AffiliateStoreListPage
                System.Diagnostics.Debug.WriteLine("[ScanningQRPage] Navigating back to AffiliateStoreListPage");
                var affiliateStoreListViewModel = IPlatformApplication.Current.Services.GetService<AffiliateStoreListViewModel>();
                Application.Current.MainPage = new AffiliateStoreListPage(affiliateStoreListViewModel);
            }
            else
            {
                // Nếu là user thường, dùng Navigation.PopAsync
                System.Diagnostics.Debug.WriteLine("[ScanningQRPage] Using Navigation.PopAsync for regular user");
                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
                else
                {
                    // Fallback nếu không có navigation stack
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScanningQRPage] Error navigating back: {ex.Message}");
            // Fallback: thử PopAsync
            try
            {
                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
            }
            catch
            {
                // Ignore
            }
        }
    }
}