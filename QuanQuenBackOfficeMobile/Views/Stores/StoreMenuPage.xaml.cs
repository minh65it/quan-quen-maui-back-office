using QuanQuenBackOfficeMobile.ViewModels.Stores;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreMenuPage : ContentPage
{
    public StoreMenuPage(string storeId)
    {
        InitializeComponent();
        var vm = new StoreMenuViewModel(storeId);
        vm.UploadCompleted += OnUploadCompleted;
        BindingContext = vm;
    }

    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Quyền truy cập", "Cần quyền truy cập thư viện ảnh để chọn ảnh", "OK");
                return;
            }

            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                (BindingContext as StoreMenuViewModel)?.AddImagePaths(new[] { result.FullPath });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
        }
    }

    private async void OnCapturePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Quyền truy cập", "Cần quyền Camera để chụp ảnh", "OK");
                return;
            }

            if (!MediaPicker.IsCaptureSupported)
            {
                await DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh", "OK");
                return;
            }

            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                (BindingContext as StoreMenuViewModel)?.AddImagePaths(new[] { photo.FullPath });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể chụp ảnh: {ex.Message}", "OK");
        }
    }

    private async void OnSkipClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Xác nhận", "Bạn có chắc chắn muốn bỏ qua việc upload ảnh menu?", "Có", "Không");
        if (confirm)
        {
            await Navigation.PopToRootAsync();
        }
    }

    private async void OnUploadCompleted(bool success, string message)
    {
        await DisplayAlert(success ? "Thành công" : "Lỗi", message, "OK");
        if (success)
        {
            await Navigation.PopToRootAsync();
        }
    }
} 