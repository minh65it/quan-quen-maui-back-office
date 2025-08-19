using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class AgreementPageViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly AgreementInfoDto _agreementInfo;

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

        public AgreementPageViewModel(IContractService contractService, AgreementInfoDto agreementInfo)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
            
            RetryCommand = new AsyncRelayCommand(RetryAsync);
        }

        /// <summary>
        /// Load PDF đã có chữ ký từ URL (giống ContractStoreViewModel)
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
        /// Download PDF từ URL và hiển thị bằng PDF.js (giống ContractStoreViewModel)
        /// </summary>
        private async Task DisplaySignedAgreementAsync(string pdfUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Downloading PDF from: {pdfUrl}");
                
                // Download PDF với authentication
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Không tìm thấy auth_token");
                }

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await httpClient.GetAsync(pdfUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Không thể tải PDF: {response.StatusCode}");
                }

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var base64Pdf = Convert.ToBase64String(pdfBytes);
                
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] PDF downloaded: {pdfBytes.Length} bytes");

                // Sử dụng PDF.js để hiển thị
                var html = GeneratePdfJsHtml(base64Pdf, "Bản Thỏa Thuận Đã Ký");

                PdfSource = new HtmlWebViewSource { Html = html };
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

            // Nếu là relative path thì gắn Asset.ImageUrl
            var assetBaseUrl = "https://quanquenapi-dev.azurewebsites.net/asset";
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

        /// <summary>
        /// Tạo PDF.js HTML content cho agreement đã ký (giống ContractStoreViewModel)
        /// </summary>
        private string GeneratePdfJsHtml(string base64Data, string title)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
    <title>{title}</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
            overflow-x: hidden;
        }}

        .success-message {{
            background: #d4edda;
            border: 1px solid #c3e6cb;
            color: #155724;
            padding: 10px;
            margin: 5px;
            border-radius: 4px;
            font-size: 14px;
        }}
        .loading {{
            text-align: center;
            padding: 20px;
            margin-top: 20px;
        }}
        .spinner {{
            border: 4px solid #f3f3f3;
            border-top: 4px solid #4CAF50;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 2s linear infinite;
            margin: 0 auto 20px;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        .progress-bar {{
            width: 80%;
            height: 4px;
            background-color: #ddd;
            border-radius: 2px;
            margin: 20px auto;
            overflow: hidden;
        }}
        .progress-fill {{
            height: 100%;
            background-color: #4CAF50;
            width: 0%;
            transition: width 0.3s ease;
            border-radius: 2px;
        }}
        .error {{
            color: red;
            text-align: center;
            padding: 20px;
            display: none;
            margin-top: 20px;
        }}
        #pdfPagesContainer {{
            margin-top: 20px;
            padding: 10px;
        }}
        .page-wrapper {{
            margin-bottom: 20px;
            text-align: center;
            background: white;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            border-radius: 8px;
            padding: 10px;
        }}
        .page-label {{
            background: #4CAF50;
            color: white;
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 12px;
            margin-bottom: 10px;
            display: inline-block;
        }}
        .page-canvas {{
            max-width: 100%;
            height: auto;
            border-radius: 4px;
        }}
        .scroll-top-btn {{
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: #4CAF50;
            color: white;
            border: none;
            border-radius: 50%;
            width: 50px;
            height: 50px;
            font-size: 20px;
            cursor: pointer;
            display: none;
            z-index: 1000;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        }}
    </style>
</head>
<body>


    <div id='pdfContainer'>
        <div id='loading' class='loading'>
            <div class='spinner'></div>
            <p>Đang tải bản thỏa thuận...</p>
            <div class='progress-bar'>
                <div id='progressFill' class='progress-fill'></div>
            </div>
        </div>
        <div id='error' class='error'>
            <h3>❌ Không thể tải bản thỏa thuận</h3>
        </div>
        <div id='pdfPagesContainer' style='display: none;'></div>
    </div>

    <button id='scrollTopBtn' class='scroll-top-btn' onclick='scrollToTop()'>↑</button>

    <script src='https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.11.338/pdf.min.js'></script>
    <script>
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.11.338/pdf.worker.min.js';

        const loadingDiv = document.getElementById('loading');
        const errorDiv = document.getElementById('error');
        const pagesContainer = document.getElementById('pdfPagesContainer');
        const progressFill = document.getElementById('progressFill');

        async function loadPDF() {{
            try {{
                progressFill.style.width = '20%';

                const base64Data = '{base64Data}';
                console.log('PDF base64 length:', base64Data.length);

                const binaryString = atob(base64Data);
                const bytes = new Uint8Array(binaryString.length);
                for (let i = 0; i < binaryString.length; i++) {{
                    bytes[i] = binaryString.charCodeAt(i);
                }}

                progressFill.style.width = '40%';

                const pdf = await pdfjsLib.getDocument({{ data: bytes }}).promise;
                console.log('PDF loaded successfully, pages:', pdf.numPages);

                progressFill.style.width = '60%';

                for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {{
                    const page = await pdf.getPage(pageNum);
                    const viewport = page.getViewport({{ scale: 1.2 }});

                    const canvas = document.createElement('canvas');
                    const ctx = canvas.getContext('2d');
                    canvas.width = viewport.width;
                    canvas.height = viewport.height;
                    canvas.className = 'page-canvas';

                    const pageLabel = document.createElement('div');
                    pageLabel.className = 'page-label';
                    pageLabel.textContent = `Trang ${{pageNum}}/${{pdf.numPages}}`;

                    const pageWrapper = document.createElement('div');
                    pageWrapper.className = 'page-wrapper';
                    pageWrapper.appendChild(pageLabel);
                    pageWrapper.appendChild(canvas);

                    pagesContainer.appendChild(pageWrapper);

                    await page.render({{ canvasContext: ctx, viewport: viewport }}).promise;
                    
                    const progress = 60 + (pageNum / pdf.numPages) * 30;
                    progressFill.style.width = progress + '%';
                }}

                progressFill.style.width = '100%';

                setTimeout(() => {{
                    loadingDiv.style.display = 'none';
                    pagesContainer.style.display = 'block';
                    setupScrollFeatures();
                }}, 500);

            }} catch (error) {{
                console.error('Error loading PDF:', error);
                loadingDiv.style.display = 'none';
                errorDiv.style.display = 'block';
            }}
        }}

        function scrollToTop() {{
            window.scrollTo({{ top: 0, behavior: 'smooth' }});
        }}

        function setupScrollFeatures() {{
            const scrollTopBtn = document.getElementById('scrollTopBtn');
            window.addEventListener('scroll', function() {{
                if (window.pageYOffset > 300) {{
                    scrollTopBtn.style.display = 'block';
                }} else {{
                    scrollTopBtn.style.display = 'none';
                }}
            }});
        }}

        // Start loading PDF
        loadPDF();
    </script>
</body>
</html>";
        }
    }
}
