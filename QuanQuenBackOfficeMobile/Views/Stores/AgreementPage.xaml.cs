using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class AgreementPage : ContentPage
    {
        private AgreementPageViewModel _viewModel;

        public AgreementPage(AgreementInfoDto agreementInfo, string pdfBase64Data)
        {
            InitializeComponent();
            
            // Get services from DI container
            var contractService = App.Services.GetService<IContractService>();
            var configuration = App.Services.GetService<IConfiguration>();
            
            if (contractService != null && configuration != null)
            {
                // Create and set ViewModel
                _viewModel = new AgreementPageViewModel(contractService, agreementInfo, configuration);
                BindingContext = _viewModel;
                
                // Load PDF với chữ ký từ URL
                _ = _viewModel.LoadSignedAgreementAsync(pdfBase64Data);
            }
            else
            {
                // Fallback if services not available
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to PhotoStoreUser page without animation để giảm CPU/GPU load
                await Application.Current.MainPage.Navigation.PushAsync(
                    new QuanQuenBackOfficeMobile.Views.PhotoStoreUser(), 
                    animated: false);
                System.Diagnostics.Debug.WriteLine("[AgreementPage] Navigated to PhotoStoreUser without animation");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể chuyển trang: {ex.Message}", "OK");
            }
        }
    }
}
