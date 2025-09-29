using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class AffiliateStoreUserPasswordPage : ContentPage
{
    public AffiliateStoreUserPasswordPage(AffiliateStoreUserPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
