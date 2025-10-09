using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels.Affiliates
{
    public partial class AffiliateAgreementPageViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly AgreementInfoDto _agreementInfo;
        private readonly IConfiguration _configuration;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isPdfLoaded = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private bool isSuccess = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private HtmlWebViewSource pdfSource = new HtmlWebViewSource();

        public IAsyncRelayCommand RetryCommand { get; }

        public AffiliateAgreementPageViewModel(IContractService contractService, AgreementInfoDto agreementInfo, IConfiguration configuration)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
            _configuration = configuration;
            RetryCommand = new AsyncRelayCommand(RetryAsync);
        }

        public async Task LoadSignedAgreementAsync(string pdfUrlOrBase64)
        {
            try
            {
                IsBusy = true;
                HasError = false;
                IsSuccess = false;
                ErrorMessage = string.Empty;

                if (!string.IsNullOrEmpty(pdfUrlOrBase64))
                {
                    // Quyết định URL hay base64: nếu bắt đầu bằng http/https hoặc '/' hoặc có đuôi .pdf => URL
                    var looksLikeUrl = pdfUrlOrBase64.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                       || pdfUrlOrBase64.StartsWith("/")
                                       || pdfUrlOrBase64.Contains(".pdf", StringComparison.OrdinalIgnoreCase);

                    if (looksLikeUrl)
                    {
                        var fullUrl = GetFullUrl(pdfUrlOrBase64);
                        PdfSource.Html = CreatePdfJsHtmlFromUrl(fullUrl);
                    }
                    else
                    {
                        PdfSource.Html = CreatePdfJsHtml(pdfUrlOrBase64);
                    }
                    IsPdfLoaded = true;
                    IsSuccess = true;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Không tìm thấy dữ liệu PDF";
                    IsPdfLoaded = false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
                IsPdfLoaded = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RetryAsync()
        {
            await Task.CompletedTask;
        }

        private string CreatePdfJsHtml(string pdfBase64Data)
        {
            return $"<html><body><iframe src='data:application/pdf;base64,{pdfBase64Data}' style='width:100%;height:100vh;border:none;'></iframe></body></html>";
        }

        private string CreatePdfJsHtmlFromUrl(string pdfUrl)
        {
            // Dùng PDF.js để tải và render PDF từ URL, tương tự user flow
            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
  <title>Signed Agreement</title>
  <script src='https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js'></script>
  <style>
    body {{
      margin: 0;
      padding: 0;
      background-color: #f5f5f5;
      font-family: Arial, sans-serif;
      -webkit-user-select: none;
      -moz-user-select: none;
      -ms-user-select: none;
      user-select: none;
      text-align: center;
    }}
    #pdf-container {{
      width: 100%;
      height: 100vh;
      overflow: auto;
      background-color: white;
      -webkit-user-select: none;
      -moz-user-select: none;
      -ms-user-select: none;
      user-select: none;
    }}
    .pdf-page {{
      margin: 10px auto;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      background-color: white;
      text-align: center;
    }}
    .pdf-page canvas {{
      max-width: 100%;
      height: auto;
      display: block;
      margin: 0 auto;
    }}
    .loading {{
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
      font-size: 16px;
      color: #666;
    }}
    .error {{
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
      font-size: 16px;
      color: #d32f2f;
      text-align: center;
      padding: 20px;
    }}
  </style>
  </head>
  <body>
    <div id='pdf-container'>
      <div class='loading'>Đang tải thỏa thuận...</div>
    </div>
    <script>
      pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
      const url = '{pdfUrl}';
      pdfjsLib.getDocument(url).promise.then(function(pdf) {{
        console.log('PDF loaded successfully');
        const container = document.getElementById('pdf-container');
        container.innerHTML = '';
        
        async function renderAllPages() {{
          for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {{
            try {{
              const page = await pdf.getPage(pageNum);
              const canvas = document.createElement('canvas');
              const context = canvas.getContext('2d');
              const viewport = page.getViewport({{scale: 1.0}});
              
              canvas.height = viewport.height;
              canvas.width = viewport.width;
              
              const renderContext = {{
                canvasContext: context,
                viewport: viewport
              }};
              
              await page.render(renderContext).promise;
              
              const pageDiv = document.createElement('div');
              pageDiv.className = 'pdf-page';
              pageDiv.appendChild(canvas);
              container.appendChild(pageDiv);
              
              console.log(`Page ${{pageNum}} rendered successfully`);
            }} catch (error) {{
              console.error(`Error rendering page ${{pageNum}}:`, error);
            }}
          }}
          console.log('All pages rendered successfully');
        }}
        
        renderAllPages();
      }}).catch(function(error) {{
        console.error('Error loading PDF:', error);
        const container = document.getElementById('pdf-container');
        container.innerHTML = '<div class=""error"">Không thể tải thỏa thuận. Vui lòng thử lại.</div>';
      }});
    </script>
  </body>
</html>";
        }

        private string GetFullUrl(string pdfUrl)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl)) return pdfUrl;
            if (pdfUrl.StartsWith("http://") || pdfUrl.StartsWith("https://")) return pdfUrl;
            var assetBaseUrl = _configuration?["API:Asset:ImageUrl"] ?? string.Empty;
            return string.IsNullOrEmpty(assetBaseUrl) ? pdfUrl : $"{assetBaseUrl}/{pdfUrl.TrimStart('/')}";
        }
    }
}


