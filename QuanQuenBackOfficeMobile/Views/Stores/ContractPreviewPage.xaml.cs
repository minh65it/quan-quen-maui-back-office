namespace QuanQuenBackOfficeMobile.Views.Stores;

using QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class ContractPreviewPage : ContentPage
{
    private StoreInformationContractViewModel? _contractData;

    public ContractPreviewPage(ContractPreviewViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            // Dọn WebView để giải phóng tài nguyên render/JS
            if (ContractWebView != null)
            {
                ContractWebView.Source = null;
                ContractWebView.Handler?.DisconnectHandler();
            }
        }
        catch { }
        finally
        {
            // Chủ động GC nhẹ khi rời trang nặng
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    // Constructor với contract data để có thể navigate tới signing page
    public ContractPreviewPage(ContractPreviewViewModel viewModel, StoreInformationContractViewModel contractData)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        _contractData = contractData;
    }

    private async void OnSignContractClicked(object sender, EventArgs e)
    {
        try
        {
            if (_contractData == null)
            {
                await DisplayAlert("Lỗi", "Không tìm thấy thông tin hợp đồng để ký!", "OK");
                return;
            }

            // Navigate to DrawingExamplePage với contract data
            var drawingPage = new QuanQuenBackOfficeMobile.Views.DrawingExamplePage(_contractData);
            await Navigation.PushAsync(drawingPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
    }
} 