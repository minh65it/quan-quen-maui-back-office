using QuanQuenBackOfficeMobile.ViewModels.Affiliates;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace QuanQuenBackOfficeMobile.Views.Affiliates
{
    public partial class AffiliatePhotoPage : ContentPage
    {
        public AffiliatePhotoPage()
        {
            InitializeComponent();
            var storeMediaService = App.Services.GetService<IStoreMediaService>();
            BindingContext = new AffiliatePhotoViewModel(storeMediaService);
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                // Giải phóng memory khi rời khỏi trang
                if (Vm != null)
                {
                    // Clear image sources để giải phóng memory
                    foreach (var img in Vm.StoreImages.ToList())
                    {
                        if (img.Images is FileImageSource fileSource)
                        {
                            // Dispose image source
                            img.Images = null;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private AffiliatePhotoViewModel Vm => BindingContext as AffiliatePhotoViewModel;

        private async void OnCapturePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Lỗi", "Ứng dụng cần quyền Camera để chụp ảnh.", "OK");
                        return;
                    }
                }

                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlert("Lỗi", "Thiết bị không hỗ trợ chụp ảnh.", "OK");
                    return;
                }

                var file = await MediaPicker.Default.CapturePhotoAsync();
                if (file != null && Vm != null)
                {
                    await Vm.AddImageFromFileAsync(file);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể chụp ảnh: {ex.Message}", "OK");
            }
        }

        private async void OnPickPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // Android 13+: READ_MEDIA_IMAGES; <=12: READ_EXTERNAL_STORAGE
#if ANDROID
                PermissionStatus mediaStatus;
#if ANDROID33_0_OR_GREATER
                mediaStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (mediaStatus != PermissionStatus.Granted)
                {
                    mediaStatus = await Permissions.RequestAsync<Permissions.Photos>();
                    if (mediaStatus != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Lỗi", "Ứng dụng cần quyền Ảnh để chọn ảnh.", "OK");
                        return;
                    }
                }
#else
                mediaStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (mediaStatus != PermissionStatus.Granted)
                {
                    mediaStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                    if (mediaStatus != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Lỗi", "Ứng dụng cần quyền đọc bộ nhớ để chọn ảnh.", "OK");
                        return;
                    }
                }
#endif
#endif

                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh" });
                if (result != null && Vm != null)
                {
                    await Vm.AddImageFromFileAsync(result);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
            }
        }
    }
}


