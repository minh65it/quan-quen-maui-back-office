using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using System.Linq;

namespace QuanQuenBackOfficeMobile.ViewModels;

public partial class DataAffiliateSignatureViewModel : BaseViewModel
{
    private readonly IAffiliateService _affiliateService;
    private readonly ILogger<DataAffiliateSignatureViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DataAffiliateSignatureViewModel(
        IAffiliateService affiliateService,
        ILogger<DataAffiliateSignatureViewModel> logger,
        IServiceProvider serviceProvider)
    {
        _affiliateService = affiliateService;
        _logger = logger;
        _serviceProvider = serviceProvider;

        CompleteSignatureCommand = new Command(async () => await CompleteSignatureAsync());
        GoBackCommand = new Command(async () => await GoBackAsync());
    }

    #region Properties

    private string _pdfBase64 = string.Empty;
    public string PdfBase64
    {
        get => _pdfBase64;
        set => SetProperty(ref _pdfBase64, value);
    }

    private string _storeName = string.Empty;
    public string StoreName
    {
        get => _storeName;
        set => SetProperty(ref _storeName, value);
    }

    private string _contractDate = string.Empty;
    public string ContractDate
    {
        get => _contractDate;
        set => SetProperty(ref _contractDate, value);
    }

    // Affiliate Info
    private string _affiliateName = string.Empty;
    public string AffiliateName
    {
        get => _affiliateName;
        set => SetProperty(ref _affiliateName, value);
    }

    private string _affiliatePhoneNumber = string.Empty;
    public string AffiliatePhoneNumber
    {
        get => _affiliatePhoneNumber;
        set => SetProperty(ref _affiliatePhoneNumber, value);
    }

    private string _affiliateSignatureDate = string.Empty;
    public string AffiliateSignatureDate
    {
        get => _affiliateSignatureDate;
        set => SetProperty(ref _affiliateSignatureDate, value);
    }

    private bool _hasAffiliateSignature = false;
    public bool HasAffiliateSignature
    {
        get => _hasAffiliateSignature;
        set => SetProperty(ref _hasAffiliateSignature, value);
    }

    // Store Owner Info
    private string _representativeName = string.Empty;
    public string RepresentativeName
    {
        get => _representativeName;
        set => SetProperty(ref _representativeName, value);
    }

    private string _storePhoneNumber = string.Empty;
    public string StorePhoneNumber
    {
        get => _storePhoneNumber;
        set => SetProperty(ref _storePhoneNumber, value);
    }

    private string _storeOwnerSignatureDate = string.Empty;
    public string StoreOwnerSignatureDate
    {
        get => _storeOwnerSignatureDate;
        set => SetProperty(ref _storeOwnerSignatureDate, value);
    }

    private bool _hasStoreOwnerSignature = false;
    public bool HasStoreOwnerSignature
    {
        get => _hasStoreOwnerSignature;
        set => SetProperty(ref _hasStoreOwnerSignature, value);
    }

    // Signature Data
    private byte[]? _affiliateSignatureBytes = null;
    public byte[]? AffiliateSignatureBytes
    {
        get => _affiliateSignatureBytes;
        set => SetProperty(ref _affiliateSignatureBytes, value);
    }

    private byte[]? _storeOwnerSignatureBytes = null;
    public byte[]? StoreOwnerSignatureBytes
    {
        get => _storeOwnerSignatureBytes;
        set => SetProperty(ref _storeOwnerSignatureBytes, value);
    }

    // Contract Data
    private string _storeId = string.Empty;
    public string StoreId
    {
        get => _storeId;
        set => SetProperty(ref _storeId, value);
    }

    private string _storeAddress = string.Empty;
    public string StoreAddress
    {
        get => _storeAddress;
        set => SetProperty(ref _storeAddress, value);
    }

    private bool _isBusy = false;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // Lines collections for DrawingView bindings (like DrawingExamplePage)
    [ObservableProperty]
    public ObservableCollection<IDrawingLine> affiliateLines = new();

    [ObservableProperty]
    public ObservableCollection<IDrawingLine> storeOwnerLines = new();

    #endregion

    #region Commands

    public ICommand CompleteSignatureCommand { get; }
    public ICommand GoBackCommand { get; }

    #endregion

    #region Methods

    public async Task InitializeAsync(string pdfBase64, string storeName, string affiliateName, string affiliatePhone, string representativeName, string storePhone, string storeId, string storeAddress)
    {
        try
        {
            PdfBase64 = pdfBase64;
            StoreName = storeName;
            AffiliateName = affiliateName;
            AffiliatePhoneNumber = affiliatePhone;
            RepresentativeName = representativeName;
            StorePhoneNumber = storePhone;
            StoreId = storeId;
            StoreAddress = storeAddress;
            ContractDate = DateTime.Now.ToString("dd/MM/yyyy");

            _logger.LogInformation("DataAffiliateSignatureViewModel initialized for store: {StoreName}, StoreId: {StoreId}", storeName, storeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing DataAffiliateSignatureViewModel");
        }
    }

    public void SetAffiliateSignature(byte[]? signatureBytes)
    {
        AffiliateSignatureBytes = signatureBytes;
        HasAffiliateSignature = signatureBytes != null && signatureBytes.Length > 0;
        AffiliateSignatureDate = HasAffiliateSignature ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : string.Empty;
    }

    public void SetStoreOwnerSignature(byte[]? signatureBytes)
    {
        StoreOwnerSignatureBytes = signatureBytes;
        HasStoreOwnerSignature = signatureBytes != null && signatureBytes.Length > 0;
        StoreOwnerSignatureDate = HasStoreOwnerSignature ? DateTime.Now.ToString("dd/MM/yyyy HH:mm") : string.Empty;
    }

    private async Task<byte[]> CreateSampleSignatureAsync(string signatureText)
    {
        var signatureData = System.Text.Encoding.UTF8.GetBytes($"Sample signature: {signatureText} - {DateTime.Now}");
        return signatureData;
    }

    private async Task<byte[]?> GenerateBytesFromLinesAsync(ObservableCollection<IDrawingLine> lines)
    {
        try
        {
            if (lines == null || lines.Count == 0) return null;
            var points = lines.SelectMany(x => x.Points).ToList();
            if (!points.Any()) return null;
            var dynamicSize = new Microsoft.Maui.Graphics.Size(
                points.Max(p => p.X) - points.Min(p => p.X),
                points.Max(p => p.Y) - points.Min(p => p.Y)
            );
            var stream = await CommunityToolkit.Maui.Views.DrawingView.GetImageStream(lines, dynamicSize, Colors.Transparent);
            if (stream == null) return null;
            using var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Position = 0;
            return mem.ToArray();
        }
        catch
        {
            return null;
        }
    }

    [RelayCommand]
    public void ClearAffiliateSignature()
    {
        AffiliateLines?.Clear();
        AffiliateSignatureBytes = null;
        HasAffiliateSignature = false;
        AffiliateSignatureDate = string.Empty;
    }

    [RelayCommand]
    public void ClearStoreOwnerSignature()
    {
        StoreOwnerLines?.Clear();
        StoreOwnerSignatureBytes = null;
        HasStoreOwnerSignature = false;
        StoreOwnerSignatureDate = string.Empty;
    }

    private async Task CompleteSignatureAsync()
    {
        if (IsBusy) return;

        try
        {
            // Validate signatures with fallback generate from Lines
            if (!HasAffiliateSignature)
            {
                var gen = await GenerateBytesFromLinesAsync(AffiliateLines);
                if (gen != null && gen.Length > 0) SetAffiliateSignature(gen);
                if (!HasAffiliateSignature)
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng ký chữ ký Cộng tác viên", "OK");
                    return;
                }
            }

            if (!HasStoreOwnerSignature)
            {
                var gen = await GenerateBytesFromLinesAsync(StoreOwnerLines);
                if (gen != null && gen.Length > 0) SetStoreOwnerSignature(gen);
                if (!HasStoreOwnerSignature)
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng ký chữ ký Chủ nhà hàng", "OK");
                    return;
                }
            }

            IsBusy = true;

            _logger.LogInformation("Completing signature process for store: {StoreName}", StoreName);

            // Get token
            var token = await SecureStorage.GetAsync("affiliate_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "OK");
                return;
            }

            // Create request with signature files
            var request = new PreviewCrawlDataAgreementRequest
            {
                StoreId = StoreId,
                StoreName = StoreName,
                PhoneNumber = StorePhoneNumber,
                Address = StoreAddress,
                RepresentativeName = RepresentativeName,
                AffiliateName = AffiliateName,
                AffiliatePhoneNumber = AffiliatePhoneNumber,
                SignatureFile = StoreOwnerSignatureBytes, // Store owner signature
                SignatureFileAffilate = AffiliateSignatureBytes, // Affiliate signature
                SignatureQrFile = null // Optional QR signature
            };

            // Call API to upload signatures and get final PDF
            var response = await _affiliateService.PreviewCrawlDataAgreementAsync(request, token);
            
            if (!response.Success || string.IsNullOrEmpty(response.Message))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể hoàn tất hợp đồng. Vui lòng thử lại.", "OK");
                return;
            }

            await Application.Current.MainPage.DisplayAlert(
                "Thành công", 
                "Hợp đồng đã được ký thành công!", 
                "OK");

            // Save final PDF info and navigate to final page
            Preferences.Set("affiliate_agreement_final_pdf_path", response.Message);
            Preferences.Set("affiliate_agreement_store_name", StoreName);

            // Điều hướng FinalPage bằng Navigation
            var page = _serviceProvider.GetRequiredService<DataAffiliateFinalPage>();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await App.Current.MainPage.Navigation.PushAsync(page);
            });

            _logger.LogInformation("Signature process completed successfully for store: {StoreName}", StoreName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing signature process for store: {StoreName}", StoreName);
            await Application.Current.MainPage.DisplayAlert("Lỗi", "Có lỗi xảy ra khi hoàn tất hợp đồng", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error going back");
        }
    }

    #endregion
}
