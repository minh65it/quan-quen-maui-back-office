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
            var cameraService = App.Services.GetService<Services.ICameraService>();
            var performanceService = App.Services.GetService<Services.IPerformanceOptimizationService>();
            BindingContext = new PhotoStoreUserViewModel(storeMediaService, cameraService, performanceService);
        }
    }
}