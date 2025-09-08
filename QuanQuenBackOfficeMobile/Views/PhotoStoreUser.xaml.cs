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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                // Dọn bớt các trang nặng khỏi back stack để tránh giữ tài nguyên khi mở camera
                var nav = Navigation;
                if (nav?.NavigationStack?.Count > 1)
                {
                    // Giữ lại trang hiện tại, loại các trang trước là các trang nặng nếu có
                    var heavyPageTypes = new Type[]
                    {
                        typeof(Views.Stores.StoreMapPage),
                        typeof(Views.Stores.ContractPreviewPage),
                        typeof(Views.DrawingExamplePage)
                    };

                    // Sao chép danh sách để tránh sửa khi đang duyệt
                    var pages = nav.NavigationStack.ToList();
                    foreach (var p in pages)
                    {
                        if (p != this && heavyPageTypes.Contains(p.GetType()))
                        {
                            nav.RemovePage(p);
                        }
                    }
                }
            }
            catch { }
        }
    }
}