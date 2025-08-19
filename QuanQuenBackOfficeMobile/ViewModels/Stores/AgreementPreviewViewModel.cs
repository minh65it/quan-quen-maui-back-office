using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Controls;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class AgreementPreviewViewModel : ObservableObject
    {
        private readonly IContractService _contractService;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isPdfLoaded = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private HtmlWebViewSource pdfSource = new HtmlWebViewSource();

        public IAsyncRelayCommand RetryCommand { get; }
        public IAsyncRelayCommand SignAgreementCommand { get; }

        // Data từ Agreement Information
        public AgreementInfoDto AgreementInfo { get; set; } = new AgreementInfoDto();

        public AgreementPreviewViewModel(IContractService contractService)
        {
            _contractService = contractService;
            RetryCommand = new AsyncRelayCommand(RetryAsync);
            SignAgreementCommand = new AsyncRelayCommand(SignAgreementAsync);
        }

        /// <summary>
        /// Set PDF content từ API response (giống ContractPreviewViewModel)
        /// </summary>
        public void SetApiPdfContent(string pdfBase64Data)
        {
            try
            {
                if (!string.IsNullOrEmpty(pdfBase64Data))
                {
                    // Tạo PDF.js HTML content từ base64 data
                    var pdfHtmlContent = CreatePdfJsHtml(pdfBase64Data);
                    
                    // Set PDF source
                    PdfSource.Html = pdfHtmlContent;
                    IsPdfLoaded = true;
                    HasError = false;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Không thể tải PDF từ API - Data trống";
                    IsPdfLoaded = false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi hiển thị PDF: {ex.Message}";
                IsPdfLoaded = false;
            }
        }

        /// <summary>
        /// Load agreement preview từ API (giữ lại để tương thích)
        /// </summary>
        public async Task LoadAgreementPreviewAsync(AgreementInfoDto agreementInfo)
        {
            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;
                AgreementInfo = agreementInfo;

                // Convert to API request
                var apiRequest = await _contractService.ConvertToPreviewAgreementRequestAsync(agreementInfo);

                // Call API để lấy preview
                var apiResponse = await _contractService.CallPreviewAgreementApiAsync(apiRequest);

                if (apiResponse.IsSuccess)
                {
                    // Sử dụng method mới
                    SetApiPdfContent(apiResponse.Message);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = apiResponse.Message;
                    IsPdfLoaded = false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi khi tải preview: {ex.Message}";
                IsPdfLoaded = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Retry khi có lỗi
        /// </summary>
        private async Task RetryAsync()
        {
            if (AgreementInfo != null)
            {
                await LoadAgreementPreviewAsync(AgreementInfo);
            }
        }

        /// <summary>
        /// Ký thỏa thuận
        /// </summary>
        private async Task SignAgreementAsync()
        {
            try
            {
                // TODO: Navigate to signature page
                await App.Current.MainPage.DisplayAlert("Thông báo", "Chuyển đến trang ký thỏa thuận", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Tạo PDF.js HTML content cho agreement preview (giống ContractPreviewViewModel)
        /// </summary>
        private string CreatePdfJsHtml(string pdfBase64Data)
        {
            return $@"
<!DOCTYPE html>
<html dir='ltr'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1'>
    <title>Xem trước thỏa thuận</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        html, body {{
            height: 100%;
            font-family: Arial, sans-serif;
            background: #f0f0f0;
        }}
        #pdfContainer {{
            width: 100%;
            height: 100vh;
            display: flex;
            flex-direction: column;
        }}
        #pdfPagesContainer {{
            flex: 1;
            width: 100%;
            overflow-y: auto;
            padding: 10px;
            background: #f8f9fa;
        }}
        .page-wrapper {{
            margin-bottom: 20px;
            padding: 10px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .page-label {{
            text-align: center;
            padding: 8px;
            background: #f5f5f5;
            color: #666;
            font-size: 12px;
            border-radius: 4px;
            margin-bottom: 5px;
            font-weight: bold;
        }}
        .page-canvas {{
            width: 100%;
            height: auto;
            border: 1px solid #ddd;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }}
        .loading {{
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            flex-direction: column;
            background: #f8f9fa;
        }}
        .spinner {{
            border: 4px solid #f3f3f3;
            border-top: 4px solid #2196F3;
            border-radius: 50%;
            width: 50px;
            height: 50px;
            animation: spin 1s linear infinite;
            margin-bottom: 20px;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        .error {{
            display: none;
            text-align: center;
            padding: 20px;
            color: #d32f2f;
            background: #ffebee;
            border-radius: 8px;
            margin: 20px;
        }}
        .progress-bar {{
            width: 250px;
            height: 6px;
            background: #f0f0f0;
            border-radius: 3px;
            margin: 15px auto;
            overflow: hidden;
        }}
        .progress-fill {{
            height: 100%;
            background: linear-gradient(90deg, #2196F3, #21CBF3);
            border-radius: 3px;
            transition: width 0.3s ease;
        }}
        .scroll-top-btn {{
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 50px;
            height: 50px;
            background: #2196F3;
            color: white;
            border: none;
            border-radius: 50%;
            font-size: 20px;
            cursor: pointer;
            box-shadow: 0 4px 12px rgba(33, 150, 243, 0.3);
            display: none;
            z-index: 1000;
        }}
        .scroll-top-btn:hover {{
            background: #1976D2;
            transform: scale(1.1);
        }}
    </style>
</head>
<body>
    <div id='pdfContainer'>
        <div id='loading' class='loading'>
            <div class='spinner'></div>
        </div>
        
        <div id='error' class='error'>
            <h3>❌ Không thể tải PDF</h3>
        </div>
        
        <div id='pdfPagesContainer' style='display: none; width: 100%; overflow-y: auto; height: 100%;'></div>
    </div>
    
    <button id='scrollTopBtn' class='scroll-top-btn' onclick='scrollToTop()'>↑</button>

    <script>
        console.log('🚀 Starting PDF.js viewer for Agreement...');
        
        // Sử dụng PDF.js từ CDN để tránh vấn đề path trong MAUI WebView
        const script = document.createElement('script');
        script.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
        script.onload = function() {{
            // Cấu hình PDF.js worker từ CDN
            pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
            loadPDF();
        }};
        document.head.appendChild(script);
        
        async function loadPDF() {{
        
        const base64Data = '{pdfBase64Data}';
        const loadingDiv = document.getElementById('loading');
        const errorDiv = document.getElementById('error');
        const pagesContainer = document.getElementById('pdfPagesContainer');
        
        try {{
            console.log('📄 Converting base64 to PDF...');
            
            // Convert base64 to Uint8Array
            const binaryString = atob(base64Data);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {{
                bytes[i] = binaryString.charCodeAt(i);
            }}
            
            console.log('📖 Loading PDF document...');
            
            // Load PDF document
            const pdf = await pdfjsLib.getDocument({{ data: bytes }}).promise;
            console.log(`PDF loaded successfully! Pages: ${{pdf.numPages}}`);
            
            // Update loading message
            loadingDiv.innerHTML = `
                <div class='spinner'></div>
            `;
            
            // Render all pages
            for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {{
                console.log(`📄 Rendering page ${{pageNum}}/${{pdf.numPages}}...`);
                
                const page = await pdf.getPage(pageNum);
                const viewport = page.getViewport({{ scale: 1.2 }});
                
                // Create canvas for this page
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                canvas.width = viewport.width;
                canvas.height = viewport.height;
                canvas.className = 'page-canvas';
                
                // Add page number label
                const pageLabel = document.createElement('div');
                pageLabel.className = 'page-label';
                pageLabel.textContent = `Trang ${{pageNum}}/${{pdf.numPages}}`;
                
                // Create page wrapper
                const pageWrapper = document.createElement('div');
                pageWrapper.className = 'page-wrapper';
                pageWrapper.appendChild(pageLabel);
                pageWrapper.appendChild(canvas);
                
                // Add to container
                pagesContainer.appendChild(pageWrapper);
                
                // Render page
                const renderContext = {{
                    canvasContext: ctx,
                    viewport: viewport
                }};
                
                await page.render(renderContext).promise;
                
                // Update loading progress
                const progressPercent = (pageNum / pdf.numPages) * 100;
                loadingDiv.innerHTML = `
                    <div class='spinner'></div>
                    <div class='progress-bar'>
                        <div class='progress-fill' style='width: ${{progressPercent}}%;'></div>
                    </div>
                `;
            }}
            
            // Hide loading, show all pages
            loadingDiv.style.display = 'none';
            pagesContainer.style.display = 'block';
            
            console.log('All PDF pages rendered successfully!');
            
            // Setup scroll functionality
            setupScrollFeatures();
            
        }} catch (error) {{
            console.error('❌ Error loading PDF:', error);
            loadingDiv.style.display = 'none';
            errorDiv.style.display = 'block';
            errorDiv.innerHTML = `
                <h3>❌ Không thể tải PDF</h3>
                <p>Lỗi: ${{error.message}}</p>
            `;
        }}
        }}
        
        // Scroll functionality
        function scrollToTop() {{
            const container = document.getElementById('pdfPagesContainer');
            container.scrollTo({{ top: 0, behavior: 'smooth' }});
        }}
        
        function setupScrollFeatures() {{
            const container = document.getElementById('pdfPagesContainer');
            const scrollBtn = document.getElementById('scrollTopBtn');
            
            // Show/hide scroll to top button
            container.addEventListener('scroll', function() {{
                if (container.scrollTop > 300) {{
                    scrollBtn.style.display = 'block';
                }} else {{
                    scrollBtn.style.display = 'none';
                }}
            }});
            
            // Add smooth scrolling behavior
            container.style.scrollBehavior = 'smooth';
            
            console.log('📱 Scroll features setup complete');
        }}
    </script>
</body>
</html>";
        }
    }
}
