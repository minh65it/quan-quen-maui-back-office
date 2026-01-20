using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class DataAffiliateInfoPage : ContentPage
{
    public DataAffiliateInfoPage(DataAffiliateInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DataAffiliateInfoViewModel vm)
        {
            // Lấy thông tin store từ Preferences (đã được lưu khi chọn store)
            var storeId = Preferences.Get("selected_store_id", string.Empty);
            var storeName = Preferences.Get("selected_store_name", string.Empty);
            var address = Preferences.Get("selected_store_address", string.Empty);
            
            if (!string.IsNullOrEmpty(storeId))
            {
                vm.InitializeFromParams(storeId, storeName, address);
                
                // Xóa sau khi dùng
                Preferences.Remove("selected_store_id");
                Preferences.Remove("selected_store_name");
                Preferences.Remove("selected_store_address");
            }
        }
    }
}


