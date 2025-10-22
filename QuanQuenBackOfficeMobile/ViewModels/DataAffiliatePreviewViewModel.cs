using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QuanQuenBackOfficeMobile.ViewModels;

public class DataAffiliatePreviewViewModel : BaseViewModel
{
    private readonly IAffiliateService _affiliateService;
    private readonly ILogger<DataAffiliatePreviewViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DataAffiliatePreviewViewModel(
        IAffiliateService affiliateService,
        ILogger<DataAffiliatePreviewViewModel> logger,
        IServiceProvider serviceProvider)
    {
        _affiliateService = affiliateService;
        _logger = logger;
        _serviceProvider = serviceProvider;

    }

    #region Properties

    private string _pdfBase64 = string.Empty;
    public string PdfBase64
    {
        get => _pdfBase64;
        set => SetProperty(ref _pdfBase64, value);
    }

    private string _pdfDataUrl = string.Empty;
    public string PdfDataUrl
    {
        get => _pdfDataUrl;
        set => SetProperty(ref _pdfDataUrl, value);
    }

    private HtmlWebViewSource _pdfHtmlSource = new HtmlWebViewSource();
    public HtmlWebViewSource PdfHtmlSource
    {
        get => _pdfHtmlSource;
        set => SetProperty(ref _pdfHtmlSource, value);
    }

    private string _storeName = string.Empty;
    public string StoreName
    {
        get => _storeName;
        set => SetProperty(ref _storeName, value);
    }

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _hasError = false;
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    #endregion

    #region Commands


    #endregion

    #region Methods

    public async Task LoadPdfAsync(string pdfBase64, string storeName)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            StoreName = storeName;
            PdfBase64 = pdfBase64;

            _logger.LogInformation("Loading PDF for store: {StoreName}", storeName);

            if (string.IsNullOrEmpty(pdfBase64))
            {
                HasError = true;
                ErrorMessage = "Không có dữ liệu PDF để hiển thị";
                return;
            }

            // Tạo HTML dùng PDF.js hiển thị PDF base64 (tránh nội suy C# gây lỗi format)
            var htmlTemplate = @"<!DOCTYPE html>
<html dir='ltr'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
  <title>Xem trước hợp đồng</title>
  <style>
    * { margin:0; padding:0; box-sizing:border-box; }
    html, body { height:100%; font-family: Arial, sans-serif; background:#f0f0f0; }
    #pdfContainer { width:100%; height:100vh; display:flex; flex-direction:column; }
    #pdfPagesContainer { flex:1; width:100%; overflow-y:auto; padding:10px; background:#f8f9fa; }
    .page-wrapper { margin-bottom:20px; padding:10px; background:#fff; border-radius:8px; box-shadow:0 2px 4px rgba(0,0,0,.1); }
    .page-label { text-align:center; padding:8px; background:#f5f5f5; color:#666; font-size:12px; border-radius:4px; margin-bottom:5px; font-weight:bold; }
    .page-canvas { width:100%; height:auto; border:1px solid #ddd; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,.1); }
    .loading { display:flex; justify-content:center; align-items:center; height:100vh; flex-direction:column; background:#f8f9fa; }
    .spinner { border:4px solid #f3f3f3; border-top:4px solid #2196F3; border-radius:50%; width:50px; height:50px; animation:spin 1s linear infinite; margin-bottom:20px; }
    @keyframes spin { 0% { transform:rotate(0deg); } 100% { transform:rotate(360deg); } }
    .error { display:none; }
    .progress-bar { width:250px; height:6px; background:#f0f0f0; border-radius:3px; margin:15px auto; overflow:hidden; }
    .progress-fill { height:100%; background:linear-gradient(90deg,#2196F3,#21CBF3); border-radius:3px; transition:width .3s ease; }
  </style>
</head>
<body>
  <div id='pdfContainer'>
    <div id='loading' class='loading'>
      <div class='spinner'></div>
      <div class='progress-bar'><div id='progressFill' class='progress-fill'></div></div>
    </div>
    <div id='error' class='error'></div>
    <div id='pdfPagesContainer' style='display:none; width:100%; overflow-y:auto; height:100%;'></div>
  </div>

  <script>
    const script = document.createElement('script');
    script.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
    script.onload = function() {
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
        loadPDF();
    };
    document.head.appendChild(script);

    const loadingDiv = document.getElementById('loading');
    const errorDiv = document.getElementById('error');
    const errMsg = document.getElementById('errMsg');
    const pagesContainer = document.getElementById('pdfPagesContainer');
    const progressFill = document.getElementById('progressFill');

    function sanitizeBase64(b64) {
      try {
        if (!b64) return '';
        if (b64.startsWith('data:')) {
          const i = b64.indexOf(',');
          if (i > -1) b64 = b64.slice(i + 1);
        }
        b64 = b64.replace(/\s+/g, '');
        b64 = b64.replace(/-/g, '+').replace(/_/g, '/');
        const mod4 = b64.length % 4;
        if (mod4) b64 += '='.repeat(4 - mod4);
        return b64;
      } catch (e) { return b64 || ''; }
    }

    async function loadPDF() {
      try {
        progressFill.style.width = '15%';
        const raw = '__PDF_BASE64__';
        const b64 = sanitizeBase64(raw);
        if (!b64) throw new Error('Base64 rỗng hoặc không hợp lệ');

        const dataUrl = 'data:application/pdf;base64,' + b64;
        const resp = await fetch(dataUrl);
        if (!resp.ok) throw new Error('Fetch data URL thất bại: ' + resp.status);
        const bytes = new Uint8Array(await resp.arrayBuffer());

        progressFill.style.width = '35%';
        const pdf = await pdfjsLib.getDocument({ data: bytes }).promise;
        progressFill.style.width = '55%';

        for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {
          const page = await pdf.getPage(pageNum);
          // Fit-to-width scaling
          const containerWidth = pagesContainer.clientWidth || window.innerWidth;
          const unscaled = page.getViewport({ scale: 1 });
          const fitScale = containerWidth / unscaled.width;
          const viewport = page.getViewport({ scale: fitScale });

          const canvas = document.createElement('canvas');
          const ctx = canvas.getContext('2d');
          canvas.width = viewport.width;
          canvas.height = viewport.height;
          canvas.className = 'page-canvas';

          const pageLabel = document.createElement('div');
          pageLabel.className = 'page-label';
          pageLabel.textContent = 'Trang ' + pageNum + '/' + pdf.numPages;

          const wrapper = document.createElement('div');
          wrapper.className = 'page-wrapper';
          wrapper.appendChild(pageLabel);
          wrapper.appendChild(canvas);
          pagesContainer.appendChild(wrapper);

          await page.render({ canvasContext: ctx, viewport }).promise;

          const progress = 55 + (pageNum / pdf.numPages) * 40;
          progressFill.style.width = progress + '%';
        }

        progressFill.style.width = '100%';
        setTimeout(() => {
          loadingDiv.style.display = 'none';
          pagesContainer.style.display = 'block';
        }, 250);

      } catch (error) {
        loadingDiv.style.display = 'none';
        errorDiv.style.display = 'block';
        // Hide error banner; only log error to console
        console.error('❌ Error loading PDF:', error);
      }
    }
  </script>
</body>
</html>";

            var htmlContent = htmlTemplate.Replace("__PDF_BASE64__", pdfBase64);
            PdfHtmlSource = new HtmlWebViewSource { Html = htmlContent };
            _logger.LogInformation("PDF HTML prepared for store: {StoreName}", storeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading PDF for store: {StoreName}", storeName);
            HasError = true;
            ErrorMessage = "Có lỗi xảy ra khi tải tài liệu";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadPdfFromPathAsync(string pdfPath, string storeName)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            StoreName = storeName;

            _logger.LogInformation("Loading PDF from path for store: {StoreName}, Path: {PdfPath}", storeName, pdfPath);

            if (string.IsNullOrEmpty(pdfPath))
            {
                HasError = true;
                ErrorMessage = "Không có đường dẫn PDF để hiển thị";
                return;
            }

            // Convert path to URL for WebView (files served under /asset)
            var pdfUrl = $"https://quanquenapi-dev.azurewebsites.net/asset{pdfPath}";
            PdfDataUrl = pdfUrl;
            _logger.LogInformation("PDF loaded successfully from path for store: {StoreName}", storeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading PDF from path for store: {StoreName}", storeName);
            HasError = true;
            ErrorMessage = "Có lỗi xảy ra khi tải tài liệu";
        }
        finally
        {
            IsLoading = false;
        }
    }


    public async Task NavigateToSignatureAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(PdfBase64))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có tài liệu để ký", "OK");
                return;
            }

            _logger.LogInformation("Navigating to signature page for store: {StoreName}", StoreName);

            // Lưu dữ liệu cần thiết sang Preferences để tránh URI dài và đảm bảo ổn định
            Preferences.Set("data_affiliate_preview_pdf_base64_for_sign", PdfBase64);
            Preferences.Set("data_affiliate_preview_store_name_for_sign", StoreName);

            // Resolve page và điều hướng bằng Navigation
            var page = _serviceProvider.GetRequiredService<DataAffiliateSignaturePage>();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await App.Current.MainPage.Navigation.PushAsync(page);
                }
                catch (Exception exPush)
                {
                    _logger.LogWarning(exPush, "PushAsync to signature failed, trying PushModalAsync");
                    try
                    {
                        await App.Current.MainPage.Navigation.PushModalAsync(page);
                    }
                    catch (Exception exModal)
                    {
                        _logger.LogError(exModal, "Navigation to signature failed");
                        await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể điều hướng sang trang ký hợp đồng.", "OK");
                    }
                }
            });

            _logger.LogInformation("Successfully navigated to signature page for store: {StoreName}", StoreName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to signature page for store: {StoreName}", StoreName);
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể chuyển đến trang ký hợp đồng", "OK");
        }
    }

    #endregion
}
