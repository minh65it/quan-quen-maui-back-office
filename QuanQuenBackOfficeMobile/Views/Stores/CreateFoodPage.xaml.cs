using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class CreateFoodPage : ContentPage
    {
        public CreateFoodPage(CreateFoodViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

