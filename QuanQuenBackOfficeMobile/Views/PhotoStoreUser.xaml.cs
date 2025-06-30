using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Views
{
    public partial class PhotoStoreUser : ContentPage
    {
        public PhotoStoreUser()
        {
            InitializeComponent();
            var storeMediaService = App.Services.GetService<IStoreMediaService>();
            BindingContext = new PhotoStoreUserViewModel(storeMediaService);
        }
    }
}