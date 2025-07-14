using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreMenuPage : ContentPage
{
    public StoreMenuPage()
    {
        InitializeComponent();
        BindingContext = new StoreMenuViewModel();
    }
} 