using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class AffiliateLoginPage : ContentPage
{
    public AffiliateLoginPage(AffiliateLoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
