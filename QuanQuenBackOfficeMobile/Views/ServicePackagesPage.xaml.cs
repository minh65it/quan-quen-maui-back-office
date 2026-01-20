using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using QuanQuenBackOfficeMobile.ViewModels;

namespace QuanQuenBackOfficeMobile.Views;

public partial class ServicePackagesPage : ContentPage
{
    private readonly ServicePackagesViewModel _viewModel;

    // Parameterless ctor for XAML/Shell instantiation; resolves VM from App.Services (set in App)
    public ServicePackagesPage() : this(
        QuanQuenBackOfficeMobile.App.Services?.GetService<ServicePackagesViewModel>()
        ?? throw new InvalidOperationException("ServicePackagesViewModel is not registered in DI"))
    {
    }

    public ServicePackagesPage(ServicePackagesViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    // Auto-resize WebView height based on content
    private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (sender is not WebView webView) return;

        try
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS
            var js = @"
                (function(){
                    var body = document.body, html = document.documentElement;
                    var height = Math.max(body.scrollHeight, body.offsetHeight,
                                           html.clientHeight, html.scrollHeight, html.offsetHeight);
                    return height;
                })();";
            var result = await webView.EvaluateJavaScriptAsync(js);
            if (double.TryParse(result, out var contentHeight))
            {
                // Convert px to device independent units (approx). MAUI WebView scales content, so a factor helps.
                var scale = DeviceDisplay.MainDisplayInfo.Density;
                var dp = contentHeight / (scale <= 0 ? 1 : scale);
                if (dp < 40) dp = 40; // minimal height to show heading
                webView.HeightRequest = dp;
            }
#endif
        }
        catch
        {
            // fallback height
            webView.HeightRequest = 200;
        }
    }
}


