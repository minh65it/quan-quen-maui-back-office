using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Affiliates;
using System.Linq;

namespace QuanQuenBackOfficeMobile.Views.Affiliates
{
    public partial class AffiliateAgreementSignaturePage : ContentPage
    {
        private AffiliateAgreementSignatureViewModel _viewModel;
        private AgreementInfoDto _agreementInfo;

        public AffiliateAgreementSignaturePage(AgreementInfoDto agreementInfo)
        {
            InitializeComponent();
            _agreementInfo = agreementInfo;

            var contractService = App.Services.GetService<IContractService>();
            if (contractService != null)
            {
                _viewModel = new AffiliateAgreementSignatureViewModel(contractService, agreementInfo);
                BindingContext = _viewModel;
            }
            else
            {
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }

        private async void SaveClicked(object sender, EventArgs e)
        {
            if (_viewModel == null)
            {
                await DisplayAlert("Lỗi", "Không thể khởi tạo ViewModel", "OK");
                return;
            }

            var signatureBytes = await GenerateSignatureImage(AgreementDrawingView);
            if (signatureBytes == null)
            {
                await DisplayAlert("Lỗi", "Vui lòng ký thỏa thuận trước khi lưu!", "OK");
                return;
            }

            var apiResponse = await _viewModel.SaveAgreementWithSignatureAsync(signatureBytes);
            if (apiResponse.IsSuccess)
            {
                var agreementPage = new AffiliateAgreementPage(_agreementInfo, apiResponse.Message);
                Application.Current.MainPage = new NavigationPage(agreementPage);
            }
            else
            {
                await DisplayAlert("Lỗi API", apiResponse.Message, "OK");
            }
        }

        private async Task<byte[]?> GenerateSignatureImage(CommunityToolkit.Maui.Views.DrawingView drawingView)
        {
            var points = drawingView.Lines.SelectMany(x => x.Points).ToList();
            if (!points.Any())
            {
                await DisplayAlert("Lỗi", "Vui lòng ký thỏa thuận trước khi lưu!", "OK");
                return null;
            }

            var dynamicSize = new Microsoft.Maui.Graphics.Size(
                points.Max(x => x.X) - points.Min(x => x.X),
                points.Max(x => x.Y) - points.Min(x => x.Y)
            );

            var imageStream = await CommunityToolkit.Maui.Views.DrawingView.GetImageStream(
                drawingView.Lines,
                dynamicSize,
                Colors.Transparent
            );

            if (imageStream != null)
            {
                using var memoryBuffer = new MemoryStream();
                await imageStream.CopyToAsync(memoryBuffer);
                memoryBuffer.Position = 0;
                return memoryBuffer.ToArray();
            }
            return null;
        }
    }
}


