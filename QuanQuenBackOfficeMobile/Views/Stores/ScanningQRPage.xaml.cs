using ZXing.Net.Maui;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System.Linq;

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

    private void CameraView_BarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {        
        MainThread.BeginInvokeOnMainThread(() =>
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
            
            // QR trống (chưa liên kết) => vẫn cho cập nhật
            confirmUpdateButton.IsVisible = true;
            createTableButton.IsVisible = true;
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
                
                // Quay lại trang trước
                await Navigation.PopAsync();
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
                await Navigation.PopAsync();
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
}