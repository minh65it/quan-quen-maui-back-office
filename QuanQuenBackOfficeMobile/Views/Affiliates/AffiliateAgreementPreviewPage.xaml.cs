using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Affiliates;

namespace QuanQuenBackOfficeMobile.Views.Affiliates
{
    public partial class AffiliateAgreementPreviewPage : ContentPage
    {
        private AffiliateAgreementPreviewViewModel _viewModel;
        private AgreementInfoDto _agreementData;

        public AffiliateAgreementPreviewPage()
        {
            InitializeComponent();
            var contractService = App.Services.GetService<IContractService>();
            if (contractService != null)
            {
                _viewModel = new AffiliateAgreementPreviewViewModel(contractService);
                BindingContext = _viewModel;
            }
            else
            {
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }

        public AffiliateAgreementPreviewPage(AffiliateAgreementPreviewViewModel viewModel, AgreementInfoDto agreementInfo) : this()
        {
            _viewModel = viewModel;
            _agreementData = agreementInfo;
            BindingContext = _viewModel;
        }

        public AffiliateAgreementPreviewPage(AgreementInfoDto agreementInfo) : this()
        {
            if (_viewModel != null)
            {
                _ = _viewModel.LoadAgreementPreviewAsync(agreementInfo);
            }
        }

        private async void OnSignAgreementClicked(object sender, EventArgs e)
        {
            if (_agreementData == null)
            {
                await DisplayAlert("Lỗi", "Không tìm thấy thông tin thỏa thuận để ký!", "OK");
                return;
            }

            var signaturePage = new AffiliateAgreementSignaturePage(_agreementData);
            Application.Current.MainPage = new NavigationPage(signaturePage);
        }
    }
}


