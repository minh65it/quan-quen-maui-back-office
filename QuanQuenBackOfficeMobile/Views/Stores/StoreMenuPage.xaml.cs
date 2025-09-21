using QuanQuenBackOfficeMobile.ViewModels.Stores;

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

    private async void OnUploadCompleted(bool success, string message)
    {
        if (message == "skip")
        {
            var confirm = await DisplayAlert("Xác nhận", "Bạn có chắc chắn muốn bỏ qua việc upload ảnh menu?", "Có", "Không");
            if (confirm)
            {
                await Navigation.PopToRootAsync();
            }
            return;
        }

        await DisplayAlert(success ? "Thành công" : "Lỗi", message, "OK");
        if (success)
        {
            await Navigation.PopToRootAsync();
        }
    }
} 