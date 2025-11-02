using System.Windows.Input;
using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;

namespace QuanQuenBackOfficeMobile.ViewModels;

public class DataAffiliateInfoViewModel : BaseViewModel
{
    private readonly IAffiliateService _affiliateService;
    private readonly ILogger<DataAffiliateInfoViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DataAffiliateInfoViewModel(IAffiliateService affiliateService, ILogger<DataAffiliateInfoViewModel> logger, IServiceProvider serviceProvider)
    {
        _affiliateService = affiliateService;
        _logger = logger;
        _serviceProvider = serviceProvider;

        SubmitCommand = new Command(async () => await SubmitAsync());
        CancelCommand = new Command(async () => await CancelAsync());
        BackCommand = new Command(async () => await BackAsync());
        ShowReportPopupCommand = new Command(() => ShowReportPopup = true);
        HideReportPopupCommand = new Command(() => ShowReportPopup = false);
        SubmitReportCommand = new Command(async () => await SubmitReportAsync());
    }

    // Prefilled from navigation
    private string _storeId = string.Empty;
    public string StoreId { get => _storeId; set => SetProperty(ref _storeId, value); }

    private string _storeName = string.Empty;
    public string StoreName { get => _storeName; set => SetProperty(ref _storeName, value); }

    private string _address = string.Empty;
    public string Address { get => _address; set => SetProperty(ref _address, value); }

    // User inputs
    private string _phoneNumber = string.Empty;
    public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

    private string _representativeName = string.Empty;
    public string RepresentativeName { get => _representativeName; set => SetProperty(ref _representativeName, value); }

    private string _affiliateName = string.Empty;
    public string AffiliateName { get => _affiliateName; set => SetProperty(ref _affiliateName, value); }

    private string _affiliatePhoneNumber = string.Empty;
    public string AffiliatePhoneNumber { get => _affiliatePhoneNumber; set => SetProperty(ref _affiliatePhoneNumber, value); }

    private string _errorMessage = string.Empty;
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    // Report popup properties
    private bool _showReportPopup = false;
    public bool ShowReportPopup { get => _showReportPopup; set => SetProperty(ref _showReportPopup, value); }

    private string _reportDescription = string.Empty;
    public string ReportDescription { get => _reportDescription; set => SetProperty(ref _reportDescription, value); }

    private string _reportErrorMessage = string.Empty;
    public string ReportErrorMessage { get => _reportErrorMessage; set => SetProperty(ref _reportErrorMessage, value); }

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand ShowReportPopupCommand { get; }
    public ICommand HideReportPopupCommand { get; }
    public ICommand SubmitReportCommand { get; }

    public void InitializeFromParams(string? storeId, string? storeName, string? address)
    {
        StoreId = storeId ?? string.Empty;
        StoreName = storeName ?? string.Empty;
        Address = address ?? string.Empty;
        _logger.LogInformation("DataAffiliateInfoViewModel initialized: {StoreId} - {StoreName}", StoreId, StoreName);
    }

    // Busy state
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private async Task SubmitAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(StoreName)) { ErrorMessage = "Vui lòng nhập tên nhà hàng"; return; }
            if (string.IsNullOrWhiteSpace(PhoneNumber)) { ErrorMessage = "Vui lòng nhập số điện thoại"; return; }
            if (string.IsNullOrWhiteSpace(Address)) { ErrorMessage = "Vui lòng nhập địa chỉ"; return; }
            if (string.IsNullOrWhiteSpace(RepresentativeName)) { ErrorMessage = "Vui lòng nhập tên người đại diện"; return; }
            if (string.IsNullOrWhiteSpace(AffiliateName)) { ErrorMessage = "Vui lòng nhập tên cộng tác viên"; return; }
            if (string.IsNullOrWhiteSpace(AffiliatePhoneNumber)) { ErrorMessage = "Vui lòng nhập SĐT cộng tác viên"; return; }

            var token = await SecureStorage.GetAsync("affiliate_token");
            if (string.IsNullOrEmpty(token)) { ErrorMessage = "Phiên đăng nhập đã hết hạn"; return; }

            var request = new PreviewCrawlDataAgreementRequest
            {
                StoreId = StoreId,
                StoreName = StoreName,
                PhoneNumber = PhoneNumber,
                Address = Address,
                RepresentativeName = RepresentativeName,
                AffiliateName = AffiliateName,
                AffiliatePhoneNumber = AffiliatePhoneNumber,
                SignatureFile = null,
                SignatureFileAffilate = null,
                SignatureQrFile = null
            };

            _logger.LogInformation("Submitting preview for {StoreId}", StoreId);
            var response = await _affiliateService.PreviewCrawlDataAgreementAsync(request, token);
            if (response == null || string.IsNullOrEmpty(response.Message))
            {
                ErrorMessage = "Không thể tạo hợp đồng. Vui lòng thử lại.";
                return;
            }

            // Save to preferences for preview page (đồng bộ key dùng bởi PreviewPage)
            Preferences.Set("affiliate_agreement_pdf_base64", response.Message);
            Preferences.Set("affiliate_agreement_store_name", StoreName);
            Preferences.Set("data_affiliate_preview_pdf_base64", response.Message);
            Preferences.Set("data_affiliate_preview_store_name", StoreName);

            // Lưu sẵn thông tin cần cho bước Signature
            Preferences.Set("data_affiliate_signature_affiliate_name", AffiliateName);
            Preferences.Set("data_affiliate_signature_affiliate_phone", AffiliatePhoneNumber);
            Preferences.Set("data_affiliate_signature_representative_name", RepresentativeName);
            Preferences.Set("data_affiliate_signature_store_phone", PhoneNumber);
            Preferences.Set("data_affiliate_signature_store_id", StoreId);
            Preferences.Set("data_affiliate_signature_store_address", Address);

            // Điều hướng bằng Navigation.PushAsync
            var page = _serviceProvider.GetRequiredService<DataAffiliatePreviewPage>();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await App.Current.MainPage.Navigation.PushAsync(page);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting data affiliate info");
            ErrorMessage = "Có lỗi xảy ra khi tạo hợp đồng";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling info page");
        }
    }

    private async Task BackAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else if (App.Current.MainPage.Navigation != null)
                {
                    await App.Current.MainPage.Navigation.PopAsync();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back from info page");
        }
    }

    private async Task SubmitReportAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ReportErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ReportDescription))
            {
                ReportErrorMessage = "Vui lòng nhập nội dung báo cáo";
                return;
            }

            var token = await SecureStorage.GetAsync("affiliate_token");
            if (string.IsNullOrEmpty(token))
            {
                ReportErrorMessage = "Phiên đăng nhập đã hết hạn";
                return;
            }

            // Lấy referralUserId từ SecureStorage
            var referralUserId = await SecureStorage.GetAsync("affiliate_referral_id");
            if (string.IsNullOrEmpty(referralUserId))
            {
                referralUserId = Preferences.Get("affiliate_referral_id", "");
            }

            if (string.IsNullOrEmpty(referralUserId))
            {
                ReportErrorMessage = "Không tìm thấy thông tin người dùng";
                return;
            }

            var request = new ReportStoreRequest
            {
                UserId = referralUserId,
                StoreId = StoreId,
                ObjectId = StoreId,
                ReportType = "2", // Type 2 for data affiliate report
                Title = string.Empty,
                Description = ReportDescription,
                Files = null,
                CreateBy = referralUserId,
                UrlContent = string.Empty
            };

            _logger.LogInformation("Submitting report for store: {StoreId}", StoreId);
            var response = await _affiliateService.ReportStoreToQQAsync(request, token);

            if (response?.Success == true)
            {
                await Application.Current.MainPage.DisplayAlert("Thành công", "Báo cáo đã được gửi thành công", "OK");
                ShowReportPopup = false;
                ReportDescription = string.Empty;
            }
            else
            {
                ReportErrorMessage = response?.Message ?? "Không thể gửi báo cáo. Vui lòng thử lại.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting report");
            ReportErrorMessage = "Có lỗi xảy ra khi gửi báo cáo";
        }
        finally
        {
            IsBusy = false;
        }
    }
}


