using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class AgreementPageViewModel : ObservableObject
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

        public AgreementPageViewModel(IContractService contractService, AgreementInfoDto agreementInfo, IConfiguration configuration)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
            _configuration = configuration;
            
            RetryCommand = new AsyncRelayCommand(RetryAsync);
        }

        /// <summary>
        /// Load PDF đã có chữ ký từ URL
        /// </summary>
        public async Task LoadSignedAgreementAsync(string pdfUrl)
        {
            try
            {
                IsBusy = true;
                HasError = false;
                IsSuccess = false;
                ErrorMessage = string.Empty;

                if (!string.IsNullOrEmpty(pdfUrl))
                {
                    // Gắn Asset.ImageUrl vào URL nếu cần
                    var fullUrl = GetFullUrl(pdfUrl);
                    
                    // Download PDF từ URL và hiển thị bằng PDF.js
                    await DisplaySignedAgreementAsync(fullUrl);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Không thể tải PDF đã ký - URL trống";
                    IsPdfLoaded = false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi khi tải PDF: {ex.Message}";
                IsPdfLoaded = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Download PDF từ URL và hiển thị bằng PDF.js
        /// </summary>
        private async Task DisplaySignedAgreementAsync(string pdfUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Loading signed agreement from: {pdfUrl}");

                                 // Tạo HTML với PDF.js để hiển thị PDF
                 var htmlContent = $@"
 <!DOCTYPE html>
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
         // Cấu hình PDF.js
         pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
         
         // Load PDF
         pdfjsLib.getDocument('{pdfUrl}').promise.then(function(pdf) {{
             console.log('PDF loaded successfully');
             const container = document.getElementById('pdf-container');
             container.innerHTML = '';
             
             // Render từng trang tuần tự
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
                 
                 // Tất cả trang đã render xong
                 console.log('All pages rendered successfully');
                 
                 // Thêm delay nhỏ để đảm bảo UI đã update
                 setTimeout(() => {{
                     console.log('PDF display completed');
                 }}, 100);
             }}
             
             // Bắt đầu render
             renderAllPages();
         }}).catch(function(error) {{
             console.error('Error loading PDF:', error);
             const container = document.getElementById('pdf-container');
             container.innerHTML = '<div class=""error"">Không thể tải thỏa thuận. Vui lòng thử lại.</div>';
         }});
     </script>
 </body>
 </html>";

                PdfSource.Html = htmlContent;
                IsPdfLoaded = true;
                IsSuccess = true;
                HasError = false;

                System.Diagnostics.Debug.WriteLine("[AgreementPageViewModel] Signed agreement HTML loaded successfully");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi hiển thị thỏa thuận: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Error displaying signed agreement: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy full URL với Asset.ImageUrl
        /// </summary>
        private string GetFullUrl(string pdfUrl)
        {
            // Nếu URL đã là full URL thì giữ nguyên
            if (pdfUrl.StartsWith("http://") || pdfUrl.StartsWith("https://"))
            {
                return pdfUrl;
            }

            var assetBaseUrl = _configuration?["API:Asset:ImageUrl"] ?? string.Empty;
            return $"{assetBaseUrl}/{pdfUrl.TrimStart('/')}";
        }

        /// <summary>
        /// Retry khi có lỗi
        /// </summary>
        private async Task RetryAsync()
        {
            // Có thể implement retry logic ở đây
            await Task.CompletedTask;
        }
    }
}
