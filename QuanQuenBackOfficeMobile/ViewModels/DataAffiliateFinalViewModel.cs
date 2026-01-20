using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace QuanQuenBackOfficeMobile.ViewModels;

public class DataAffiliateFinalViewModel : BaseViewModel
{
    private readonly ILogger<DataAffiliateFinalViewModel> _logger;

    public DataAffiliateFinalViewModel(ILogger<DataAffiliateFinalViewModel> logger)
    {
        _logger = logger;
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
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(CanGoHome));
            }
        }
    }

    private bool _hasError = false;
    public bool HasError
    {
        get => _hasError;
        set
        {
            if (SetProperty(ref _hasError, value))
            {
                OnPropertyChanged(nameof(CanGoHome));
            }
        }
    }

    public bool CanGoHome => !_isLoading && !_hasError;

    public ICommand BackToRootCommand => new Command(async () =>
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Đơn giản: reset MainPage về trang danh sách Data Affiliate
                try
                {
                    var listingPage = App.Services.GetService<QuanQuenBackOfficeMobile.Views.AffiliateListingAgentPage>();
                    if (listingPage != null)
                    {
                        Application.Current.MainPage = new NavigationPage(listingPage);
                        return;
                    }
                }
                catch { }

                // Fallback: Shell về MainPage
                if (Shell.Current != null)
                {
                    try { await Shell.Current.GoToAsync("///MainPage"); return; } catch { }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back to root");
        }
    });

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public async Task LoadFromPreferencesAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;

            var base64 = Preferences.Get("affiliate_agreement_final_pdf_base64", string.Empty);
            var pdfPath = Preferences.Get("affiliate_agreement_final_pdf_path", string.Empty);
            StoreName = Preferences.Get("affiliate_agreement_store_name", "Nhà hàng");

            if (!string.IsNullOrEmpty(base64))
            {
                // Dùng PDF.js render base64 (ổn định hơn data URL trực tiếp)
                PdfHtmlSource = new HtmlWebViewSource { Html = CreatePdfJsHtmlFromBase64(base64) };
                _logger.LogInformation("Loaded final PDF (base64) into HTML for store: {StoreName}", StoreName);
            }
            else if (!string.IsNullOrEmpty(pdfPath))
            {
                var fullUrl = $"https://quanquenapi-dev.azurewebsites.net/asset{pdfPath}";
                // Dùng PDF.js render từ URL trực tiếp
                PdfHtmlSource = new HtmlWebViewSource { Html = CreatePdfJsHtmlFromUrl(fullUrl) };
                _logger.LogInformation("Loaded final PDF from url into HTML for store: {StoreName}", StoreName);
            }
            else
            {
                HasError = true;
                ErrorMessage = "Không có tài liệu để hiển thị";
            }

            // Clear after use
            Preferences.Remove("affiliate_agreement_final_pdf_base64");
            Preferences.Remove("affiliate_agreement_final_pdf_path");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading final PDF");
            HasError = true;
            ErrorMessage = "Có lỗi xảy ra khi tải tài liệu";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string CreatePdfJsHtmlFromBase64(string pdfBase64Data)
    {
        var htmlTemplate = @"<!DOCTYPE html>
<html dir='ltr'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
  <title>Biên bản đã ký</title>
  <style>
    * {{ margin:0; padding:0; box-sizing:border-box; }}
    html, body {{ height:100%; font-family: Arial, sans-serif; background:#f0f0f0; }}
    #pdfContainer {{ width:100%; height:100vh; display:flex; flex-direction:column; }}
    #pdfPagesContainer {{ flex:1; width:100%; overflow-y:auto; padding:10px; background:#f8f9fa; }}
    .page-wrapper {{ margin-bottom:20px; padding:10px; background:#fff; border-radius:8px; box-shadow:0 2px 4px rgba(0,0,0,.1); }}
    .page-label {{ text-align:center; padding:8px; background:#f5f5f5; color:#666; font-size:12px; border-radius:4px; margin-bottom:5px; font-weight:bold; }}
    .page-canvas {{ width:100%; height:auto; border:1px solid #ddd; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,.1); }}
    .loading {{ display:flex; justify-content:center; align-items:center; height:100vh; flex-direction:column; background:#f8f9fa; }}
    .spinner {{ border:4px solid #f3f3f3; border-top:4px solid #2196F3; border-radius:50%; width:50px; height:50px; animation:spin 1s linear infinite; margin-bottom:20px; }}
    @keyframes spin {{ 0% {{ transform:rotate(0deg); }} 100% {{ transform:rotate(360deg); }} }}
    .error {{ display:none; }}
  </style>
</head>
<body>
  <div id='pdfContainer'>
    <div id='loading' class='loading'>
      <div class='spinner'></div>
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
        const raw = '__PDF_BASE64__';
        const b64 = sanitizeBase64(raw);
        if (!b64) throw new Error('Base64 rỗng hoặc không hợp lệ');

        const dataUrl = 'data:application/pdf;base64,' + b64;
        const resp = await fetch(dataUrl);
        if (!resp.ok) throw new Error('Fetch data URL thất bại: ' + resp.status);
        const bytes = new Uint8Array(await resp.arrayBuffer());

        const pdf = await pdfjsLib.getDocument({ data: bytes }).promise;

        for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {
          const page = await pdf.getPage(pageNum);
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
        }

        loadingDiv.style.display = 'none';
        pagesContainer.style.display = 'block';

      } catch (error) {
        loadingDiv.style.display = 'none';
        errorDiv.style.display = 'block';
        // Hide error banner; only log error
        console.error('❌ Error loading PDF:', error);
      }
    }
  </script>
</body>
</html>";
        return htmlTemplate.Replace("__PDF_BASE64__", pdfBase64Data);
    }

    private string CreatePdfJsHtmlFromUrl(string pdfUrl)
    {
        var htmlTemplate = @"<!DOCTYPE html>
<html dir='ltr'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
  <title>Biên bản đã ký</title>
  <style>
    * {{ margin:0; padding:0; box-sizing:border-box; }}
    html, body {{ height:100%; font-family: Arial, sans-serif; background:#f0f0f0; }}
    #pdfContainer {{ width:100%; height:100vh; display:flex; flex-direction:column; }}
    #pdfPagesContainer {{ flex:1; width:100%; overflow-y:auto; padding:10px; background:#f8f9fa; }}
    .page-wrapper {{ margin-bottom:20px; padding:10px; background:#fff; border-radius:8px; box-shadow:0 2px 4px rgba(0,0,0,.1); }}
    .page-label {{ text-align:center; padding:8px; background:#f5f5f5; color:#666; font-size:12px; border-radius:4px; margin-bottom:5px; font-weight:bold; }}
    .page-canvas {{ width:100%; height:auto; border:1px solid #ddd; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,.1); }}
    .loading {{ display:flex; justify-content:center; align-items:center; height:100vh; flex-direction:column; background:#f8f9fa; }}
    .spinner {{ border:4px solid #f3f3f3; border-top:4px solid #2196F3; border-radius:50%; width:50px; height:50px; animation:spin 1s linear infinite; margin-bottom:20px; }}
    @keyframes spin {{ 0% {{ transform:rotate(0deg); }} 100% {{ transform:rotate(360deg); }} }}
    .error {{ display:none; text-align:center; padding:20px; color:#d32f2f; background:#ffebee; border-radius:8px; margin:20px; }}
  </style>
</head>
<body>
  <div id='pdfContainer'>
    <div id='loading' class='loading'>
      <div class='spinner'></div>
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

    async function loadPDF() {
      try {
        const url = '__PDF_URL__';
        const pdf = await pdfjsLib.getDocument({ url }).promise;

        for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {
          const page = await pdf.getPage(pageNum);
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
        }

        loadingDiv.style.display = 'none';
        pagesContainer.style.display = 'block';

      } catch (error) {
        loadingDiv.style.display = 'none';
        errorDiv.style.display = 'block';
        // Hide error banner; only log error
        console.error('❌ Error loading PDF:', error);
      }
    }
  </script>
</body>
    </html>";
        return htmlTemplate.Replace("__PDF_URL__", pdfUrl);
    }
}


