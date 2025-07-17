using QuanQuenBackOfficeMobile.ViewModels.Stores;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class VideoStoreUserPage : ContentPage
    {
        public VideoStoreUserPage(VideoStoreUserViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}