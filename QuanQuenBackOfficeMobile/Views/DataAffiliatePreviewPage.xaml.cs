using QuanQuenBackOfficeMobile.ViewModels;
using System.Web;

namespace QuanQuenBackOfficeMobile.Views;

public partial class DataAffiliatePreviewPage : ContentPage
{
    public DataAffiliatePreviewPage(DataAffiliatePreviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is DataAffiliatePreviewViewModel viewModel)
        {
            // Prefer reading large payloads from Preferences to avoid long URIs
            var pdfBase64 = Preferences.Get("data_affiliate_preview_pdf_base64", string.Empty);
            var storeName = Preferences.Get("data_affiliate_preview_store_name", "Nhà hàng");

            if (!string.IsNullOrEmpty(pdfBase64))
            {
                await viewModel.LoadPdfAsync(pdfBase64, storeName);
                // Clear after use to avoid stale data
                Preferences.Remove("data_affiliate_preview_pdf_base64");
                Preferences.Remove("data_affiliate_preview_store_name");
                return;
            }

            // Fallback to URL params if provided
            var route = Shell.Current.CurrentState.Location.ToString();
            var routePdfBase64 = ExtractParameter(route, "pdfBase64");
            var routePdfPath = ExtractParameter(route, "pdfPath");
            var routeStoreName = ExtractParameter(route, "storeName") ?? storeName;

            if (!string.IsNullOrEmpty(routePdfBase64))
            {
                await viewModel.LoadPdfAsync(routePdfBase64, routeStoreName);
            }
            else if (!string.IsNullOrEmpty(routePdfPath))
            {
                await viewModel.LoadPdfFromPathAsync(routePdfPath, routeStoreName);
            }
        }
    }

    private async void OnSignContractClicked(object sender, EventArgs e)
    {
        if (BindingContext is DataAffiliatePreviewViewModel viewModel)
        {
            await viewModel.NavigateToSignatureAsync();
        }
    }

    private string? ExtractParameter(string route, string parameterName)
    {
        try
        {
            var uri = new Uri(route);
            var query = uri.Query;
            if (string.IsNullOrEmpty(query)) return null;

            var parameters = System.Web.HttpUtility.ParseQueryString(query);
            return parameters[parameterName];
        }
        catch
        {
            return null;
        }
    }
}
