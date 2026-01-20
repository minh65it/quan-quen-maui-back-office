using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreTableListPage : ContentPage
{
    private readonly StoreTableListViewModel _viewModel;

    public StoreTableListPage(StoreTableListViewModel vm, string storeId, string tableAreaId, string areaName)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
        _viewModel.Initialize(storeId, tableAreaId, areaName);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
