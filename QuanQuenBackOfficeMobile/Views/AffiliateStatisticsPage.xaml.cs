using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views
{
    public partial class AffiliateStatisticsPage : ContentPage
    {
        private readonly AffiliateStatisticsViewModel _viewModel;

        public AffiliateStatisticsPage(AffiliateStatisticsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}

