using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class DataAffiliateFinalPage : ContentPage
{
    public DataAffiliateFinalPage(DataAffiliateFinalViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DataAffiliateFinalViewModel vm)
        {
            await vm.LoadFromPreferencesAsync();
        }
    }
}


