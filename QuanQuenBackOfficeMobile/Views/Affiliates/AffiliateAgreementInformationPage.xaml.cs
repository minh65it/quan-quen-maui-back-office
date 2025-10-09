using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Affiliates;

namespace QuanQuenBackOfficeMobile.Views.Affiliates
{
    public partial class AffiliateAgreementInformationPage : ContentPage
    {
        public AffiliateAgreementInformationPage()
        {
            InitializeComponent();

            var contractService = App.Services.GetService<IContractService>();
            var configuration = App.Services.GetService<IConfiguration>();
            if (contractService != null && configuration != null)
            {
                var vm = new AffiliateAgreementInformationViewModel(contractService, configuration);
                BindingContext = vm;
            }
            else
            {
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }
    }
}


