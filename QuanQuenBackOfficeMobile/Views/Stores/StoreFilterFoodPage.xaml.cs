using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class StoreFilterFoodPage : ContentPage
    {
        public StoreFilterFoodPage(StoreFilterFoodViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

