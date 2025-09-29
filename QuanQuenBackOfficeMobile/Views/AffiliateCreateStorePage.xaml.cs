using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class AffiliateCreateStorePage : ContentPage
{
    public AffiliateCreateStorePage(AffiliateCreateStoreViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
