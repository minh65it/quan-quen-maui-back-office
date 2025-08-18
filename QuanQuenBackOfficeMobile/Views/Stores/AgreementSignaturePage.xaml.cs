using QuanQuenBackOfficeMobile.ViewModels.Stores;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Views.Stores
{
    public partial class AgreementSignaturePage : ContentPage
    {
        private AgreementSignatureViewModel _viewModel;
        private AgreementInfoDto _agreementInfo;

        public AgreementSignaturePage(AgreementInfoDto agreementInfo)
        {
            InitializeComponent();
            _agreementInfo = agreementInfo;
            
            // Get services from DI container
            var contractService = App.Services.GetService<IContractService>();
            
            if (contractService != null)
            {
                // Create and set ViewModel
                _viewModel = new AgreementSignatureViewModel(contractService, agreementInfo);
                BindingContext = _viewModel;
            }
            else
            {
                // Fallback if services not available
                DisplayAlert("Lỗi", "Không thể khởi tạo trang. Vui lòng thử lại.", "OK");
            }
        }

        private async void SaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel == null)
                {
                    await DisplayAlert("Lỗi", "Không thể khởi tạo ViewModel", "OK");
                    return;
                }

                // Generate signature image
                var signatureBytes = await GenerateSignatureImage(AgreementDrawingView);

                if (signatureBytes == null)
                {
                    await DisplayAlert("Lỗi", "Vui lòng ký thỏa thuận trước khi lưu!", "OK");
                    return;
                }

                // Call API với signature
                var apiResponse = await _viewModel.SaveAgreementWithSignatureAsync(signatureBytes);

                if (apiResponse.IsSuccess)
                {
                    // Navigate to Agreement page với PDF đã có chữ ký
                    var agreementPage = new AgreementPage(_agreementInfo, apiResponse.Message);
                    await Navigation.PushAsync(agreementPage);
                }
                else
                {
                    await DisplayAlert("Lỗi API", apiResponse.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        private async Task<byte[]?> GenerateSignatureImage(CommunityToolkit.Maui.Views.DrawingView drawingView)
        {
            try
            {
                // Get all points from drawing lines
                var points = drawingView.Lines.SelectMany(x => x.Points).ToList();
                if (!points.Any())
                {
                    await DisplayAlert("Lỗi", "Vui lòng ký thỏa thuận trước khi lưu!", "OK");
                    return null;
                }

                // Calculate dynamic size based on drawing bounds
                var dynamicSize = new Microsoft.Maui.Graphics.Size(
                    points.Max(x => x.X) - points.Min(x => x.X), 
                    points.Max(x => x.Y) - points.Min(x => x.Y)
                );

                // Render stream with transparent background
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
                    
                    // Return as byte array
                    return memoryBuffer.ToArray();
                }
                return null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tạo ảnh chữ ký: {ex.Message}", "OK");
                return null;
            }
        }
    }
}
