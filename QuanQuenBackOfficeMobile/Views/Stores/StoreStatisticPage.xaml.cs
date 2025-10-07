using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreStatisticPage : ContentPage
{
    private readonly StoreStatisticViewModel _viewModel;

    public StoreStatisticPage(StoreStatisticViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        await _viewModel.LoadOrdersAsync();
        await _viewModel.LoadUsersInOrdersAsync();
        await _viewModel.LoadLoyalCustomersAsync();
        await _viewModel.LoadAverageRevenueAsync();
    }
}


