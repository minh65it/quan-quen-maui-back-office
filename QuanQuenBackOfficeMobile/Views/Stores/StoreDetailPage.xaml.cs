using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores;

public partial class StoreDetailPage : ContentPage
{
    private readonly StoreDetailViewModel _viewModel;

    public StoreDetailPage(StoreDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
} 