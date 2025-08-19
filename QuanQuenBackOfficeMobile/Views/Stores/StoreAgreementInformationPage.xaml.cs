using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class StoreAgreementInformationPage : ContentPage
    {
        public StoreAgreementInformationPage()
        {
            InitializeComponent();
            
            // Get services from DI container
            var contractService = App.Services.GetService<IContractService>();
            var configuration = App.Services.GetService<IConfiguration>();
            
            if (contractService != null && configuration != null)
            {
                // Create and set ViewModel
                var viewModel = new StoreAgreementInformationViewModel(contractService, configuration);
                BindingContext = viewModel;
            }
            else
            {
                // Fallback if services not available
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }
    }
}