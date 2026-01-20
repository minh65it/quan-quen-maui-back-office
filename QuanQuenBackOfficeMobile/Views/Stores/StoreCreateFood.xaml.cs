using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class StoreCreateFood : ContentPage
    {
        private readonly StoreCreateFoodViewModel _viewModel;

        public StoreCreateFood(StoreCreateFoodViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}

