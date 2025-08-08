using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Core;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores;

public partial class ContractStoreViewModel : ObservableObject
{
    private readonly IContractService _contractService;
    private readonly IConfiguration _configuration;

    [ObservableProperty]
    private bool isBusy = false;

    [ObservableProperty]
    private string htmlSource = string.Empty;

    [ObservableProperty]
    private bool isHtmlLoaded = false;

    [ObservableProperty]
    private bool hasError = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private HtmlWebViewSource htmlWebViewSource = new HtmlWebViewSource();

    // Contract data từ StoreInformationContractViewModel
    [ObservableProperty]
    private string selectedBusinessType = string.Empty;

    // BÊN CUNG CẤP DỊCH VỤ (Provider) - THÊM VÀO
    [ObservableProperty]
    private string providerRepresentativeIdNumber = string.Empty;

    [ObservableProperty]
    private string providerRepresentativeIdDate = string.Empty;

    [ObservableProperty]
    private string providerRepresentativeIdPlace = string.Empty;

    [ObservableProperty]
    private string companyName = string.Empty;

    [ObservableProperty]
    private string storeName = string.Empty;

    [ObservableProperty]
    private string headquartersAddress = string.Empty;

    [ObservableProperty]
    private string businessLicenseNumber = string.Empty;

    [ObservableProperty]
    private string businessLicenseDate = string.Empty;

    [ObservableProperty]
    private string businessLicensePlace = string.Empty;

    [ObservableProperty]
    private string representativeName = string.Empty;

    [ObservableProperty]
    private string representativePosition = string.Empty;

    [ObservableProperty]
    private string representativeIdNumber = string.Empty;

    [ObservableProperty]
    private string representativeIdDate = string.Empty;

    [ObservableProperty]
    private string representativeIdPlace = string.Empty;

    [ObservableProperty]
    private string representativePermanentAddress = string.Empty;

    [ObservableProperty]
    private string representativeTemporaryAddress = string.Empty;

    [ObservableProperty]
    private string storeHeadOfficeAddress = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string bankAccountNumber = string.Empty;

    [ObservableProperty]
    private string bankAccountHolder = string.Empty;

    [ObservableProperty]
    private string bankName = string.Empty;

    [ObservableProperty]
    private string ownerName = string.Empty;

    [ObservableProperty]
    private string ownerGender = string.Empty;

    [ObservableProperty]
    private string ownerIdNumber = string.Empty;

    [ObservableProperty]
    private string ownerIdDate = string.Empty;

    [ObservableProperty]
    private string ownerIdPlace = string.Empty;

    [ObservableProperty]
    private string ownerPermanentAddress = string.Empty;

    [ObservableProperty]
    private string ownerTemporaryAddress = string.Empty;

    // Signature data
    [ObservableProperty]
    private ObservableCollection<IDrawingLine> contractLines = new();

    [ObservableProperty]
    private ObservableCollection<IDrawingLine> appendixLines = new();

    [ObservableProperty]
    private byte[]? contractSignatureBytes;

    [ObservableProperty]
    private byte[]? appendixSignatureBytes;

    public IAsyncRelayCommand SaveContractCommand { get; }

    public ContractStoreViewModel(IContractService contractService, IConfiguration configuration)
    {
        _contractService = contractService;
        _configuration = configuration;
        SaveContractCommand = new AsyncRelayCommand(SaveContractAsync);
    }

    // Method để copy data từ StoreInformationContractViewModel
    public void LoadContractData(StoreInformationContractViewModel sourceViewModel)
    {
        CompanyName = sourceViewModel.CompanyName ?? string.Empty;
        StoreName = sourceViewModel.StoreName ?? string.Empty;
        HeadquartersAddress = sourceViewModel.HeadquartersAddress ?? string.Empty;
        BusinessLicenseNumber = sourceViewModel.BusinessLicenseNumber ?? string.Empty;
        BusinessLicenseDate = sourceViewModel.BusinessLicenseDate ?? string.Empty;
        BusinessLicensePlace = sourceViewModel.BusinessLicensePlace ?? string.Empty;
        RepresentativeName = sourceViewModel.RepresentativeName ?? string.Empty;
        RepresentativePosition = sourceViewModel.RepresentativePosition ?? string.Empty;
        RepresentativeIdNumber = sourceViewModel.RepresentativeIdNumber ?? string.Empty;
        RepresentativeIdDate = sourceViewModel.RepresentativeIdDate ?? string.Empty;
        RepresentativeIdPlace = sourceViewModel.RepresentativeIdPlace ?? string.Empty;
        RepresentativePermanentAddress = sourceViewModel.RepresentativePermanentAddress ?? string.Empty;
        RepresentativeTemporaryAddress = sourceViewModel.RepresentativeTemporaryAddress ?? string.Empty;
        StoreHeadOfficeAddress = sourceViewModel.StoreHeadOfficeAddress ?? string.Empty;
        PhoneNumber = sourceViewModel.PhoneNumber ?? string.Empty;
        BankAccountNumber = sourceViewModel.BankAccountNumber ?? string.Empty;
        BankAccountHolder = sourceViewModel.BankAccountHolder ?? string.Empty;
        BankName = sourceViewModel.BankName ?? string.Empty;
        OwnerName = sourceViewModel.OwnerName ?? string.Empty;
        OwnerGender = sourceViewModel.OwnerGender ?? string.Empty;
        OwnerIdNumber = sourceViewModel.OwnerIdNumber ?? string.Empty;
        OwnerIdDate = sourceViewModel.OwnerIdDate ?? string.Empty;
        OwnerIdPlace = sourceViewModel.OwnerIdPlace ?? string.Empty;
        OwnerPermanentAddress = sourceViewModel.OwnerPermanentAddress ?? string.Empty;
        OwnerTemporaryAddress = sourceViewModel.OwnerTemporaryAddress ?? string.Empty;
        ProviderRepresentativeIdNumber = sourceViewModel.ProviderRepresentativeIdNumber ?? string.Empty;
        ProviderRepresentativeIdDate = sourceViewModel.ProviderRepresentativeIdDate ?? string.Empty;
        ProviderRepresentativeIdPlace = sourceViewModel.ProviderRepresentativeIdPlace ?? string.Empty;
    }

    // Method để set signature data từ DrawingExamplePage
    public void SetSignatureData(ObservableCollection<IDrawingLine> contractLines, ObservableCollection<IDrawingLine> appendixLines, 
                                byte[]? contractSignatureBytes, byte[]? appendixSignatureBytes)
    {
        ContractLines = contractLines;
        AppendixLines = appendixLines;
        ContractSignatureBytes = contractSignatureBytes;
        AppendixSignatureBytes = appendixSignatureBytes;
    }

    private async Task SaveContractAsync()
    {
        try
        {
            IsBusy = true;
            HasError = false;

            // Validate signatures
            if (ContractSignatureBytes == null || ContractSignatureBytes.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng ký hợp đồng chính", "OK");
                return;
            }

            if (AppendixSignatureBytes == null || AppendixSignatureBytes.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng ký phụ lục hợp đồng", "OK");
                return;
            }

            // Convert ViewModel data to PreviewContractRequest
            var apiRequest = await ConvertToPreviewContractRequestAsync();

            // Call API với signatures
            var apiResponse = await _contractService.CallPreviewContractApiWithSignaturesAsync(apiRequest, ContractSignatureBytes, AppendixSignatureBytes);

            if (apiResponse.IsSuccess)
            {
                // Tạo PDF URL từ response
                var assetBaseUrl = _configuration["API:Asset:ImageUrl"];
                var pdfPath = apiResponse.Message;
                
                var pdfUrl = $"{assetBaseUrl.TrimEnd('/')}/{pdfPath.TrimStart('/')}";
                
                System.Diagnostics.Debug.WriteLine($"[ContractStoreViewModel] Asset Base URL: {assetBaseUrl}");
                System.Diagnostics.Debug.WriteLine($"[ContractStoreViewModel] PDF Path: {pdfPath}");
                System.Diagnostics.Debug.WriteLine($"[ContractStoreViewModel] Final PDF URL: {pdfUrl}");

                // Display PDF trong WebView
                await DisplayFinalContractAsync(pdfUrl);
                
                await App.Current.MainPage.DisplayAlert("Thành công", "Hợp đồng đã được tạo thành công!", "OK");
            }
            else
            {
                HasError = true;
                ErrorMessage = apiResponse.Message ?? "Có lỗi xảy ra khi tạo hợp đồng";
                await App.Current.MainPage.DisplayAlert("Lỗi API", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
            await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<PreviewContractRequest> ConvertToPreviewContractRequestAsync()
    {
        var storeId = StoreRegistrationData.StoreId ; // Get from registration data
        var userId = StoreRegistrationData.StoreUserId ; // Get from registration data
        
        if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(userId))
        {
            throw new Exception("Thiếu thông tin StoreId hoặc UserId. Vui lòng hoàn tất đăng ký trước.");
        }

        return await Task.FromResult(new PreviewContractRequest
        {
            StoreId = storeId,
            UserId = userId,
            MainContract = new MainContractDto
            {
                // Bên Cung Cấp Dịch Vụ - THÊM VÀO
                ProviderRepresentativeIdNumber = ProviderRepresentativeIdNumber ?? "",
                ProviderRepresentativeIdDate = ProviderRepresentativeIdDate ?? "",
                ProviderRepresentativeIdPlace = ProviderRepresentativeIdPlace ?? "",
                
                // Bên Thuê Dịch Vụ
                BusinessType = StoreRegistrationData.BusinessType,  
                CompanyName = CompanyName,
                StoreName = StoreRegistrationData.StoreName,
                HeadquartersAddress = HeadquartersAddress,
                BusinessLicenseNumber = BusinessLicenseNumber,
                BusinessLicenseDate = BusinessLicenseDate,
                BusinessLicensePlace = BusinessLicensePlace,
                RepresentativeName = StoreRegistrationData.RepresentativeName,
                RepresentativePosition = RepresentativePosition,
                RepresentativeIdNumber = RepresentativeIdNumber,
                RepresentativeIdDate = RepresentativeIdDate,
                RepresentativeIdPlace = RepresentativeIdPlace,
                RepresentativePermanentAddress = RepresentativePermanentAddress,
                RepresentativeTemporaryAddress = RepresentativeTemporaryAddress,
                StoreHeadOfficeAddress = StoreHeadOfficeAddress,
                PhoneNumber = PhoneNumber,
                BankAccountNumber = BankAccountNumber,
                BankAccountHolder = BankAccountHolder,
                BankName = BankName,
                OwnerName = OwnerName,
                OwnerGender = OwnerGender,
                OwnerIdNumber = OwnerIdNumber,
                OwnerIdDate = OwnerIdDate,
                OwnerIdPlace = OwnerIdPlace,
                OwnerPermanentAddress = OwnerPermanentAddress,
                OwnerTemporaryAddress = OwnerTemporaryAddress
            },
            AppendixContract = new AppendixContractDto
            {
                // CHỈ 2 FIELDS NHƯ CURL EXAMPLE
                AppendixSignDate = DateTime.Now.ToString("dd/MM/yyyy"),
                AppendixNumber = $"PL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}"
            },
            CreatedDate = DateTime.Now,
            File = null, 
            File2 = null 
        });
    }

    private async Task DisplayFinalContractAsync(string pdfUrl)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ContractStoreViewModel] Downloading PDF from: {pdfUrl}");
            
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
            
            System.Diagnostics.Debug.WriteLine($"[ContractStoreViewModel] PDF downloaded successfully: {pdfBytes.Length} bytes");

            // Sử dụng PDF.js để hiển thị như ContractPreviewPage
            var html = GeneratePdfJsHtml(base64Pdf, "Hợp Đồng Đã Hoàn Thành");

            HtmlWebViewSource = new HtmlWebViewSource { Html = html };
            IsHtmlLoaded = true;
            HasError = false;

            System.Diagnostics.Debug.WriteLine("[ContractStoreViewModel] Final contract HTML loaded successfully");
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Lỗi hiển thị hợp đồng: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ContractStoreViewModel] Error displaying final contract: {ex.Message}");
        }
    }

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
        .header {{
            background: linear-gradient(135deg, #4CAF50 0%, #2E7D32 100%);
            color: white;
            padding: 15px;
            text-align: center;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            z-index: 1000;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header h1 {{
            margin: 0;
            font-size: 18px;
            font-weight: bold;
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
            padding: 60px 20px;
            margin-top: 80px;
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
            margin-top: 80px;
        }}
        #pdfPagesContainer {{
            margin-top: 80px;
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
    <div class='header'>
        <h1>{title}</h1>
        <div class='success-message'>
            ✅ Hợp đồng đã được tạo thành công với chữ ký điện tử!
        </div>
    </div>

    <div id='pdfContainer'>
        <div id='loading' class='loading'>
            <div class='spinner'></div>
            <p>Đang tải hợp đồng cuối cùng...</p>
            <div class='progress-bar'>
                <div id='progressFill' class='progress-fill'></div>
            </div>
            <p id='loadingText'>Khởi tạo...</p>
        </div>
        <div id='error' class='error'>
            <h3>❌ Không thể tải hợp đồng</h3>
            <p>Vui lòng thử lại sau hoặc liên hệ hỗ trợ.</p>
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
        const loadingText = document.getElementById('loadingText');

        async function loadPDF() {{
            try {{
                loadingText.textContent = 'Đang xử lý dữ liệu PDF...';
                progressFill.style.width = '20%';

                const base64Data = '{base64Data}';
                console.log('PDF base64 length:', base64Data.length);

                const binaryString = atob(base64Data);
                const bytes = new Uint8Array(binaryString.length);
                for (let i = 0; i < binaryString.length; i++) {{
                    bytes[i] = binaryString.charCodeAt(i);
                }}

                loadingText.textContent = 'Đang tải document...';
                progressFill.style.width = '40%';

                const pdf = await pdfjsLib.getDocument({{ data: bytes }}).promise;
                console.log('PDF loaded successfully, pages:', pdf.numPages);

                loadingText.textContent = `Đang render ${{pdf.numPages}} trang...`;
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
                    loadingText.textContent = `Đã render ${{pageNum}}/${{pdf.numPages}} trang`;
                }}

                progressFill.style.width = '100%';
                loadingText.textContent = 'Hoàn tất!';

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