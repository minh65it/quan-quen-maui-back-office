using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views
{
    public partial class AffiliateListingAgentPage : ContentPage
    {
        public AffiliateListingAgentPage(AffiliateListingAgentViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
