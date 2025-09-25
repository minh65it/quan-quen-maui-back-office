using Camera.MAUI;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class ScanningQRPage : ContentPage
{
    private readonly IQRCodeService _qrCodeService;
    private string _storeId;
    private string _currentQRCode = string.Empty;
    private int _currentCameraIndex = 0;

    public ScanningQRPage()
    {
        InitializeComponent();
        _qrCodeService = App.Services.GetService<IQRCodeService>();
    }

    public ScanningQRPage(string storeId) : this()
    {
        _storeId = storeId;
    }

    private async void cameraView_CamerasLoaded(object sender, EventArgs e)
    {
        if (cameraView.Cameras.Count > 0)
        {
            _currentCameraIndex = 0;
            cameraView.Camera = cameraView.Cameras[_currentCameraIndex];
            
            // Hiển thị button chuyển đổi nếu có nhiều hơn 1 camera
            switchCameraButton.IsVisible = cameraView.Cameras.Count > 1;
            
            await cameraView.StartCameraAsync();
        }
    }


    private void CameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs e)
    {        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var qrCodeText = e.Result[0].Text;
            
            // Bỏ qua nếu QR Code giống lần trước
            if (_currentQRCode == qrCodeText)
                return;
                
            _currentQRCode = qrCodeText;
            
            // Hiển thị QR Code đã scan
            barcodeResult.Text = $"✅ QR Code: {qrCodeText}";
            barcodeResult.TextColor = Colors.Green;
            
            // Hiển thị nút cập nhật
            confirmUpdateButton.IsVisible = true;
        });
    }

    private async void SwitchCamera_Clicked(object sender, EventArgs e)
    {
        if (cameraView.Cameras.Count < 2)
            return; // chỉ có 1 camera thì không switch được

        // đổi sang camera tiếp theo
        _currentCameraIndex = (_currentCameraIndex + 1) % cameraView.Cameras.Count;
        cameraView.Camera = cameraView.Cameras[_currentCameraIndex];

        await cameraView.StartCameraAsync();
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
}