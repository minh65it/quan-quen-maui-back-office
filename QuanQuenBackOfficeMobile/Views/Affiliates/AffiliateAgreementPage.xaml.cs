using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Affiliates;

namespace QuanQuenBackOfficeMobile.Views.Affiliates
{
    public partial class AffiliateAgreementPage : ContentPage
    {
        private AffiliateAgreementPageViewModel _viewModel;

        public AffiliateAgreementPage(AgreementInfoDto agreementInfo, string pdfBase64Data)
        {
            InitializeComponent();

            var contractService = App.Services.GetService<IContractService>();
            var configuration = App.Services.GetService<IConfiguration>();
            if (contractService != null && configuration != null)
            {
                _viewModel = new AffiliateAgreementPageViewModel(contractService, agreementInfo, configuration);
                BindingContext = _viewModel;
                _ = _viewModel.LoadSignedAgreementAsync(pdfBase64Data);
            }
            else
            {
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            try
            {
                // Chuyển sang trang ảnh của Affiliate
                Application.Current.MainPage = new NavigationPage(
                    new QuanQuenBackOfficeMobile.Views.Affiliates.AffiliatePhotoPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể chuyển trang: {ex.Message}", "OK");
            }
        }

        private async Task LogoutFromAffiliate()
        {
            try
            {
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_token");
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_referral_id");
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_first_name");
                Microsoft.Maui.Storage.SecureStorage.Remove("affiliate_email");

                var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateLoginViewModel>();
                Application.Current.MainPage = new AffiliateLoginPage(vm);
            }
            catch
            {
                var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateLoginViewModel>();
                Application.Current.MainPage = new AffiliateLoginPage(vm);
            }
        }

        private async Task NavigateToCreateStore()
        {
            var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateCreateStoreViewModel>();
            Application.Current.MainPage = new AffiliateCreateStorePage(vm);
            await Task.CompletedTask;
        }

        private async Task NavigateToStoreList()
        {
            var vm = IPlatformApplication.Current.Services.GetService<ViewModels.AffiliateStoreListViewModel>();
            Application.Current.MainPage = new AffiliateStoreListPage(vm);
            await Task.CompletedTask;
        }
    }
}


