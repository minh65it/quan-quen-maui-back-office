using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class AgreementPreviewPage : ContentPage
    {
        private AgreementPreviewViewModel _viewModel;
        private AgreementInfoDto _agreementData;

        public AgreementPreviewPage()
        {
            InitializeComponent();
            
            // Get services from DI container
            var contractService = App.Services.GetService<IContractService>();
            var configuration = App.Services.GetService<IConfiguration>();
            
            if (contractService != null)
            {
                // Create and set ViewModel
                _viewModel = new AgreementPreviewViewModel(contractService);
                BindingContext = _viewModel;
            }
            else
            {
                // Fallback if services not available
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }

        /// <summary>
        /// Constructor với ViewModel và AgreementInfo (giống ContractPreviewPage)
        /// </summary>
        public AgreementPreviewPage(AgreementPreviewViewModel viewModel, AgreementInfoDto agreementInfo) : this()
        {
            // Set ViewModel với PDF content sẵn
            _viewModel = viewModel;
            _agreementData = agreementInfo;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Constructor với AgreementInfo để load preview (giữ lại để tương thích)
        /// </summary>
        public AgreementPreviewPage(AgreementInfoDto agreementInfo) : this()
        {
            // Load preview khi page được khởi tạo với data
            if (_viewModel != null)
            {
                _ = _viewModel.LoadAgreementPreviewAsync(agreementInfo);
            }
        }

        /// <summary>
        /// Xử lý khi click nút ký thỏa thuận
        /// </summary>
        private async void OnSignAgreementClicked(object sender, EventArgs e)
        {
            try
            {
                if (_agreementData == null)
                {
                    await DisplayAlert("Lỗi", "Không tìm thấy thông tin thỏa thuận để ký!", "OK");
                    return;
                }

                // Navigate to signature page với agreement data
                var signaturePage = new AgreementSignaturePage(_agreementData);
                await Navigation.PushAsync(signaturePage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }
    }
}
