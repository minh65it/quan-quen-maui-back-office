using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class AffiliateStoreUserAuthenticationPage : ContentPage
{
    public AffiliateStoreUserAuthenticationPage(AffiliateStoreUserAuthenticationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
