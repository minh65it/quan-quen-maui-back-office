using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views
{
    public partial class AffiliateStoreListPage : ContentPage
    {
        public AffiliateStoreListPage(AffiliateStoreListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AffiliateStoreListViewModel viewModel)
            {
                // Đảm bảo ReferralId được load từ SecureStorage trước khi refresh
                await viewModel.InitializeAsync();
            }
        }
    }
}
