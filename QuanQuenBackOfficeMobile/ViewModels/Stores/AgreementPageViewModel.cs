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
        private readonly IQRCodeService _qrCodeService;

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

        [ObservableProperty]
        private bool isQrCodeVisible = false;

        [ObservableProperty]
        private string qrCodeImage = string.Empty;

        [ObservableProperty]
        private bool isQrCodeLoading = false;

        [ObservableProperty]
        private string qrCodeErrorMessage = string.Empty;

        public IAsyncRelayCommand RetryCommand { get; }

        public AgreementPageViewModel(IContractService contractService, AgreementInfoDto agreementInfo, IConfiguration configuration, IQRCodeService qrCodeService)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
            _configuration = configuration;
            _qrCodeService = qrCodeService;
            
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
                    
                    // Sau khi PDF load thành công, tự động tạo QR Code
                    await GenerateStoreQRCodeAsync();
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

        /// <summary>
        /// Tạo QR Code cho store (tự động gọi sau khi PDF load thành công)
        /// Sử dụng geocoding service để lấy tọa độ thực tế từ địa chỉ cửa hàng
        /// </summary>
        private async Task GenerateStoreQRCodeAsync()
        {
            try
            {
                IsQrCodeLoading = true;
                IsQrCodeVisible = false;
                QrCodeErrorMessage = string.Empty;

                // Lấy thông tin agent từ storage
                var agentName = await SecureStorage.GetAsync("userName") ?? "Unknown Agent";
                var agentId = await SecureStorage.GetAsync("userId") ?? "Unknown ID";

                // Tạo dữ liệu QR Code từ agreement info
                // Sử dụng geocoding service để lấy tọa độ thực tế từ địa chỉ cửa hàng
                var storeAddress = _agreementInfo?.StoreHeadOfficeAddress ?? "Địa chỉ chưa cập nhật";
                
                // Lấy tọa độ từ địa chỉ cửa hàng bằng MAUI Essentials Geocoding
                (double Latitude, double Longitude)? coordinates = null;
                try
                {
                    var locations = await Geocoding.GetLocationsAsync(storeAddress);
                    var location = locations?.FirstOrDefault();
                    if (location != null)
                    {
                        coordinates = (location.Latitude, location.Longitude);
                    }
                }
                catch (Exception geoEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Essentials Geocoding failed: {geoEx.Message}");
                }
                
                var qrCodeData = new QRCodeData
                {
                    DiaChi = storeAddress,
                    ToaDo = new Coordinates
                    {
                        // Sử dụng tọa độ thực tế nếu có, nếu không thì dùng tọa độ mặc định TP.HCM
                        Latitude = coordinates?.Latitude ?? 10.762622,   // Tọa độ thực tế hoặc mặc định TP.HCM
                        Longitude = coordinates?.Longitude ?? 106.660172  // Tọa độ thực tế hoặc mặc định TP.HCM
                    },
                    TenNguoiDaiDien = _agreementInfo?.RepresentativeName ?? "Tên chưa cập nhật",
                    AgentInfo = new AgentInfo
                    {
                        Ten = agentName,
                        Id = agentId
                    },
                    Timestamp = DateTime.UtcNow
                };
                
                // Log thông tin tọa độ để debug
                if (coordinates.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Geocoding SUCCESS: Address='{storeAddress}' -> Lat={coordinates.Value.Latitude:F6}, Lng={coordinates.Value.Longitude:F6}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Geocoding FAILED: Address='{storeAddress}' -> Using default coordinates: Lat=10.762622, Lng=106.660172");
                }

                // Gọi service tạo QR Code
                var qrCodeResponse = await _qrCodeService.GenerateStoreQRCodeAsync(qrCodeData, qrCodeData.AgentInfo);

                if (qrCodeResponse.Success)
                {
                    // Thay vì sử dụng URL trực tiếp, download image và lưu local
                    try
                    {
                        var localImagePath = await DownloadAndSaveQRCodeImage(qrCodeResponse.QRCodeImage);
                        QrCodeImage = localImagePath;
                        IsQrCodeVisible = true;
                        System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] QR Code generated successfully. Image URL: {qrCodeResponse.QRCodeImage}");
                        System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Local image path: {localImagePath}");
                    }
                    catch (Exception downloadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Failed to download QR image: {downloadEx.Message}");
                        // Fallback: sử dụng URL trực tiếp
                        QrCodeImage = qrCodeResponse.QRCodeImage;
                        IsQrCodeVisible = true;
                    }
                }
                else
                {
                    QrCodeErrorMessage = qrCodeResponse.Message;
                    System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] QR Code generation failed: {qrCodeResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                QrCodeErrorMessage = $"Lỗi tạo QR Code: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Error generating QR Code: {ex.Message}");
                // Không throw exception vì đây là tính năng phụ, không ảnh hưởng đến việc hiển thị PDF
            }
            finally
            {
                IsQrCodeLoading = false;
            }
        }

        /// <summary>
        /// Download và lưu QR Code image vào local storage
        /// </summary>
        private async Task<string> DownloadAndSaveQRCodeImage(string imageUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Downloading QR image from: {imageUrl}");
                
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Không tìm thấy auth_token");
                }

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Không thể tải QR image: {response.StatusCode}");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] QR image downloaded: {imageBytes.Length} bytes");

                // Tạo tên file unique
                var fileName = $"qr_code_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
                var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                
                // Lưu file
                await File.WriteAllBytesAsync(localPath, imageBytes);
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] QR image saved to: {localPath}");
                
                return localPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementPageViewModel] Error downloading QR image: {ex.Message}");
                throw;
            }
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
      margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f5f5f5; overflow-x: hidden;
    }}
    .loading {{ text-align:center; padding:20px; margin-top:20px; }}
    .spinner {{ border:4px solid #f3f3f3; border-top:4px solid #4CAF50; border-radius:50%; width:40px; height:40px; animation:spin 2s linear infinite; margin:0 auto 20px; }}
    @keyframes spin {{ 0% {{transform:rotate(0deg);}} 100% {{transform:rotate(360deg);}} }}
    .progress-bar {{ width:80%; height:4px; background:#ddd; border-radius:2px; margin:20px auto; overflow:hidden; }}
    .progress-fill {{ height:100%; background:#4CAF50; width:0%; transition:width .3s ease; border-radius:2px; }}
    .error {{ color:red; text-align:center; padding:20px; display:none; margin-top:20px; }}
    #pdfPagesContainer {{ margin-top:20px; padding:10px; }}
    .page-wrapper {{ margin-bottom:20px; text-align:center; background:#fff; box-shadow:0 2px 8px rgba(0,0,0,.1); border-radius:8px; padding:10px; }}
    .page-label {{ background:#4CAF50; color:#fff; padding:5px 10px; border-radius:4px; font-size:12px; margin-bottom:10px; display:inline-block; }}
    .page-canvas {{ max-width:100%; height:auto; border-radius:4px; }}
    .scroll-top-btn {{ position:fixed; bottom:20px; right:20px; background:#4CAF50; color:#fff; border:none; border-radius:50%; width:50px; height:50px; font-size:20px; cursor:pointer; display:none; z-index:1000; box-shadow:0 2px 8px rgba(0,0,0,.2); }}
  </style>
</head>
<body>
  <div id='pdfContainer'>
    <div id='loading' class='loading'>
      <div class='spinner'></div>
      <p>Đang tải bản thỏa thuận...</p>
      <div class='progress-bar'><div id='progressFill' class='progress-fill'></div></div>
    </div>
    <div id='error' class='error'>
      <h3>❌ Không thể tải bản thỏa thuận</h3>
      <div id='errMsg' style='white-space:pre-wrap'></div>
    </div>
    <div id='pdfPagesContainer' style='display:none;'></div>
  </div>
  <button id='scrollTopBtn' class='scroll-top-btn' onclick='scrollToTop()'>↑</button>

  <!-- dùng PDF.js CDN -->
  <script src='https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js'></script>
  <script>
    // worker cùng version
    pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';

    const loadingDiv = document.getElementById('loading');
    const errorDiv = document.getElementById('error');
    const errMsg = document.getElementById('errMsg');
    const pagesContainer = document.getElementById('pdfPagesContainer');
    const progressFill = document.getElementById('progressFill');

    function sanitizeBase64(b64) {{
      try {{
        // Nếu là data URL => cắt phần đầu
        if (b64.startsWith('data:')) {{
          const idx = b64.indexOf(',');
          if (idx > -1) b64 = b64.slice(idx + 1);
        }}
        // bỏ whitespace/newline
        b64 = b64.replace(/\\s+/g, '');
        // URL-safe -> chuẩn
        b64 = b64.replace(/-/g, '+').replace(/_/g, '/');
        // padding
        const mod4 = b64.length % 4;
        if (mod4) b64 += '='.repeat(4 - mod4);
        return b64;
      }} catch (e) {{
        return b64; // fallback
      }}
    }}

    async function loadPDF() {{
      try {{
        progressFill.style.width = '20%';

        // KHÔNG dùng atob. Dùng fetch data: + arrayBuffer
        const raw = '{base64Data}';
        const b64 = sanitizeBase64(raw);
        const dataUrl = 'data:application/pdf;base64,' + b64;

        // iOS/Android WebView đều hỗ trợ fetch data: → ArrayBuffer
        const resp = await fetch(dataUrl);
        if (!resp.ok) throw new Error('Fetch data URL thất bại: ' + resp.status);
        const bytes = new Uint8Array(await resp.arrayBuffer());

        progressFill.style.width = '40%';

        const pdf = await pdfjsLib.getDocument({{ data: bytes }}).promise;

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

          await page.render({{ canvasContext: ctx, viewport }}).promise;

          const progress = 60 + (pageNum / pdf.numPages) * 30;
          progressFill.style.width = progress + '%';
        }}

        progressFill.style.width = '100%';
        setTimeout(() => {{
          loadingDiv.style.display = 'none';
          pagesContainer.style.display = 'block';
          setupScrollFeatures();
        }}, 300);

      }} catch (error) {{
        loadingDiv.style.display = 'none';
        errorDiv.style.display = 'block';
        errMsg.textContent = String(error && error.message ? error.message : error);
        console.error('Error loading PDF:', error);
      }}
    }}

    function scrollToTop() {{
      window.scrollTo({{ top: 0, behavior: 'smooth' }});
    }}

    function setupScrollFeatures() {{
      const btn = document.getElementById('scrollTopBtn');
      window.addEventListener('scroll', function() {{
        btn.style.display = (window.pageYOffset > 300) ? 'block' : 'none';
      }});
    }}

    loadPDF();
  </script>
</body>
</html>";
        }
    }
}
