using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreTableAreaPage : ContentPage
{
    private readonly StoreTableAreaViewModel _viewModel;

    public StoreTableAreaPage(StoreTableAreaViewModel vm, string storeId)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
        _viewModel.Initialize(storeId);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}


