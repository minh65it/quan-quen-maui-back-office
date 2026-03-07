using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views;

namespace QuanQuenBackOfficeMobile.ViewModels;
    public class AffiliateListingAgentViewModel : INotifyPropertyChanged
    {
        private readonly IAffiliateService _affiliateService;
        private readonly IGeocodingService _geocodingService;
        private readonly ILocationService _locationService;
        private readonly ILogger<AffiliateListingAgentViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;

        private bool _isBusy;
        private string _userAddress = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _showLocationPopup = true;
        private double _userLatitude;
        private double _userLongitude;
        private ObservableCollection<StoreRecommendation> _stores = new();
        private PaginationData _pagination;
        private int _currentPage = 1;
        private bool _canLoadMore = true;
        
        // Form properties
        private bool _showStoreFormPopup = false;
        private PreviewCrawlDataAgreementRequest _storeForm = new();
        private string _formErrorMessage = string.Empty;
        private string _selectedStoreId = string.Empty;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string UserAddress
        {
            get => _userAddress;
            set => SetProperty(ref _userAddress, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool ShowLocationPopup
        {
            get => _showLocationPopup;
            set => SetProperty(ref _showLocationPopup, value);
        }

        public double UserLatitude
        {
            get => _userLatitude;
            set => SetProperty(ref _userLatitude, value);
        }

        public double UserLongitude
        {
            get => _userLongitude;
            set => SetProperty(ref _userLongitude, value);
        }

        public ObservableCollection<StoreRecommendation> Stores
        {
            get => _stores;
            set => SetProperty(ref _stores, value);
        }

        public PaginationData Pagination
        {
            get => _pagination;
            set => SetProperty(ref _pagination, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public bool CanLoadMore
        {
            get => _canLoadMore;
            set => SetProperty(ref _canLoadMore, value);
        }

        public string PageInfo
        {
            get
            {
                if (Pagination == null) return string.Empty;
                return $"Trang {CurrentPage}/{Pagination.TotalPage}";
            }
        }

        // Form properties
        public bool ShowStoreFormPopup
        {
            get => _showStoreFormPopup;
            set => SetProperty(ref _showStoreFormPopup, value);
        }

        public PreviewCrawlDataAgreementRequest StoreForm
        {
            get => _storeForm;
            set => SetProperty(ref _storeForm, value);
        }

        public string FormErrorMessage
        {
            get => _formErrorMessage;
            set => SetProperty(ref _formErrorMessage, value);
        }

        public ICommand ConfirmLocationCommand { get; }
        public ICommand LoadStoresCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        
        public ICommand SubmitFormCommand { get; }
        public ICommand CancelFormCommand { get; }
        public ICommand GoToDataAffiliatePreviewCommand { get; }
        public ICommand SelectStoreCommand { get; }

        public AffiliateListingAgentViewModel(
            IAffiliateService affiliateService,
            IGeocodingService geocodingService,
            ILocationService locationService,
            ILogger<AffiliateListingAgentViewModel> logger,
            IServiceProvider serviceProvider)
        {
            _affiliateService = affiliateService;
            _geocodingService = geocodingService;
            _locationService = locationService;
            _logger = logger;
            _serviceProvider = serviceProvider;

            ConfirmLocationCommand = new Command(async () => await ConfirmLocationAsync());
            LoadStoresCommand = new Command(async () => await LoadStoresAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            LoadMoreCommand = new Command(async () => await LoadMoreAsync());
            SubmitFormCommand = new Command(async () => await SubmitFormAsync());
            CancelFormCommand = new Command(async () => await CancelFormAsync());
            GoToDataAffiliatePreviewCommand = new Command(async () => await GoToDataAffiliatePreviewAsync());
            SelectStoreCommand = new Command<StoreRecommendation>(async (store) => await SelectStoreAsync(store));
        }

        private async Task ConfirmLocationAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Get current location using GPS
                var coordinates = await _locationService.GetCurrentLocationAsync();
                if (coordinates.HasValue)
                {
                    UserLatitude = coordinates.Value.Latitude;
                    UserLongitude = coordinates.Value.Longitude;
                    ShowLocationPopup = false;
                    
                    _logger.LogInformation($"Location obtained: Lat={UserLatitude}, Lng={UserLongitude}. Calling API directly...");
                    
                    // Call API directly with coordinates
                    await CallStoreRecommendationsAPIAsync(UserLatitude, UserLongitude);
                }
                else
                {
                    ErrorMessage = "Không thể lấy vị trí hiện tại. Vui lòng kiểm tra quyền truy cập vị trí và thử lại.";
                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current location");
                ErrorMessage = $"Có lỗi xảy ra khi lấy vị trí: {ex.Message}";
                IsBusy = false;
            }
        }

        private async Task CallStoreRecommendationsAPIAsync(double latitude, double longitude, int pageNumber = 1, bool isLoadMore = false)
        {
            try
            {
                _logger.LogInformation($"Calling API with Lat: {latitude}, Lng: {longitude}");

                var token = await SecureStorage.GetAsync("affiliate_token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Affiliate token is null or empty");
                    ErrorMessage = "Token không hợp lệ";
                    IsBusy = false;
                    return;
                }

                var request = new StoreRecommendationsRequest
                {
                    LatUser = latitude.ToString("F7", System.Globalization.CultureInfo.InvariantCulture),
                    LonUser = longitude.ToString("F7", System.Globalization.CultureInfo.InvariantCulture),
                    Radius = null,
                    IsRecently = null,
                    IsMap = null,
                    LatPoint1 = null,
                    LonPoint1 = null,
                    LatPoint2 = null,
                    LonPoint2 = null,
                    LatPoint3 = null,
                    LonPoint3 = null,
                    LatPoint4 = null,
                    LonPoint4 = null,
                    PageSize = 5,
                    PageNumber = pageNumber
                };

                _logger.LogInformation($"Calling GetStoreRecommendationsByLocationAsync with LatUser: {request.LatUser}, LonUser: {request.LonUser}");
                var response = await _affiliateService.GetStoreRecommendationsByLocationAsync(request, token);
                _logger.LogInformation($"API response received. Success: {response != null}, Result count: {response?.Result?.Count ?? 0}");
                
                if (response?.Result != null)
                {
                    _logger.LogInformation($"Adding {response.Result.Count} stores to UI (Page {pageNumber})");
                    
                    if (!isLoadMore)
                    {
                        // Reset stores for first page or refresh
                        Stores.Clear();
                        CurrentPage = 1;
                    }
                    
                    foreach (var store in response.Result)
                    {
                        Stores.Add(store);
                        _logger.LogInformation($"Added store: {store.StoreName} at {store.Distance}");
                    }
                    
                    Pagination = response.Pagination;
                    CurrentPage = pageNumber;
                    
                    // Check if can load more
                    CanLoadMore = Pagination != null && CurrentPage < Pagination.TotalPage;
                    
                    // Trigger PageInfo update
                    OnPropertyChanged(nameof(PageInfo));
                    
                    _logger.LogInformation($"UI updated with {Stores.Count} stores total. Page {CurrentPage}/{Pagination?.TotalPage ?? 0}");
                }
                else
                {
                    ErrorMessage = "Không thể tải danh sách nhà hàng";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling store recommendations API");
                ErrorMessage = $"Có lỗi xảy ra khi tải danh sách nhà hàng: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadStoresAsync()
        {
            await CallStoreRecommendationsAPIAsync(UserLatitude, UserLongitude, 1, false);
        }

        private async Task RefreshAsync()
        {
            await LoadStoresAsync();
        }

        private async Task LoadMoreAsync()
        {
            if (IsBusy || !CanLoadMore) return;

            try
            {
                IsBusy = true;
                var nextPage = CurrentPage + 1;
                _logger.LogInformation($"Loading more stores - Page {nextPage}");
                
                await CallStoreRecommendationsAPIAsync(UserLatitude, UserLongitude, nextPage, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading more stores");
                ErrorMessage = $"Có lỗi xảy ra khi tải thêm nhà hàng: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        

        private async Task SelectStoreAsync(StoreRecommendation store)
        {
            if (store == null) return;
            try
            {
                _logger.LogInformation("SelectStoreAsync tapped. StoreId: {StoreId}, StoreName: {StoreName}", store.StoreId, store.StoreName);
                
                _ = AssignStoreInBackgroundAsync(store.StoreId);
                
                // Lưu thông tin store vào Preferences để DataAffiliateInfoPage lấy
                Preferences.Set("selected_store_id", store.StoreId);
                Preferences.Set("selected_store_name", store.StoreName);
                Preferences.Set("selected_store_address", store.Address);
                
                // Resolve page từ DI
                var page = _serviceProvider.GetRequiredService<DataAffiliateInfoPage>();
                
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.Navigation.PushAsync(page);
                    _logger.LogInformation("Navigated to DataAffiliateInfoPage for store: {StoreName}", store.StoreName);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting store for navigation. StoreId: {StoreId}", store?.StoreId);
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể điều hướng sang trang nhập thông tin: {ex.Message}", "OK");
            }
        }

        private async Task AssignStoreInBackgroundAsync(string storeId)
        {
            try
            {
                _logger.LogInformation("Starting background assign store for StoreId: {StoreId}", storeId);
                
                // Lấy token từ SecureStorage với fallback Preferences
                var token = await SecureStorage.GetAsync("affiliate_token");
                if (string.IsNullOrEmpty(token))
                {
                    token = Preferences.Get("affiliate_token", "");
                }
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Cannot assign store - token is empty");
                    return;
                }

                // Lấy referralUserId từ SecureStorage với fallback Preferences
                var referralUserId = await SecureStorage.GetAsync("affiliate_referral_id");
                if (string.IsNullOrEmpty(referralUserId))
                {
                    referralUserId = Preferences.Get("affiliate_referral_id", "");
                }
                
                if (string.IsNullOrEmpty(referralUserId))
                {
                    _logger.LogWarning("Cannot assign store - referralUserId is empty");
                    return;
                }

                var request = new AssignStoreRequest
                {
                    ReferralUserId = referralUserId,
                    StoreId = storeId,
                    AssignedDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    AssignedBy = referralUserId
                };

                var response = await _affiliateService.AssignStoreForReferralUserDataAsync(request, token);
                
                if (response?.Success == true)
                {
                    _logger.LogInformation("Store assigned successfully in background. StoreId: {StoreId}", storeId);
                }
                else
                {
                    _logger.LogWarning("Failed to assign store in background. StoreId: {StoreId}, Message: {Message}", 
                        storeId, response?.Message);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không hiển thị cho user vì đây là background task
                _logger.LogError(ex, "Error in background assign store. StoreId: {StoreId}", storeId);
            }
        }

        private async Task SubmitFormAsync()
        {
            if (IsBusy) return;

            try
            {
                // Validate form
                if (string.IsNullOrWhiteSpace(StoreForm.StoreName))
                {
                    FormErrorMessage = "Vui lòng nhập tên nhà hàng";
                    return;
                }

                if (string.IsNullOrWhiteSpace(StoreForm.PhoneNumber))
                {
                    FormErrorMessage = "Vui lòng nhập số điện thoại nhà hàng";
                    return;
                }

                if (string.IsNullOrWhiteSpace(StoreForm.Address))
                {
                    FormErrorMessage = "Vui lòng nhập địa chỉ nhà hàng";
                    return;
                }

                if (string.IsNullOrWhiteSpace(StoreForm.RepresentativeName))
                {
                    FormErrorMessage = "Vui lòng nhập tên người đại diện";
                    return;
                }

                if (string.IsNullOrWhiteSpace(StoreForm.AffiliateName))
                {
                    FormErrorMessage = "Vui lòng nhập tên affiliate";
                    return;
                }

                if (string.IsNullOrWhiteSpace(StoreForm.AffiliatePhoneNumber))
                {
                    FormErrorMessage = "Vui lòng nhập số điện thoại affiliate";
                    return;
                }

                IsBusy = true;
                FormErrorMessage = string.Empty;

                _logger.LogInformation("Submitting form for StoreId: {StoreId}", StoreForm.StoreId);

                // Get token
                var token = await SecureStorage.GetAsync("affiliate_token");
                if (string.IsNullOrEmpty(token))
                {
                    FormErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return;
                }

                // Call API to get PDF (message: base64 string per current server behavior)
                var response = await _affiliateService.PreviewCrawlDataAgreementAsync(StoreForm, token);

                if (response == null || string.IsNullOrEmpty(response.Message))
                {
                    FormErrorMessage = "Không thể tạo hợp đồng. Vui lòng thử lại.";
                    return;
                }

                // Hide form popup
                ShowStoreFormPopup = false;

                // Avoid passing huge base64 via URL → store in Preferences and navigate without heavy params
                // Lưu theo chuẩn chung của trang preview thỏa thuận
                Preferences.Set("affiliate_agreement_pdf_base64", response.Message);
                Preferences.Set("affiliate_agreement_store_name", StoreForm.StoreName);

                // Đồng thời giữ key cũ để tương thích nếu cần
                Preferences.Set("data_affiliate_preview_pdf_base64", response.Message);
                Preferences.Set("data_affiliate_preview_store_name", StoreForm.StoreName);

                // Điều hướng sang trang preview thông qua Command chuyên biệt
                await GoToDataAffiliatePreviewAsync();

                _logger.LogInformation("Successfully created PDF for StoreId: {StoreId}", StoreForm.StoreId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting form for StoreId: {StoreId}", StoreForm.StoreId);
                FormErrorMessage = "Có lỗi xảy ra khi tạo hợp đồng. Vui lòng thử lại.";
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async Task GoToDataAffiliatePreviewAsync()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("DataAffiliatePreviewPage");
                    }
                    else
                    {
                        _logger.LogError("Shell.Current is null; cannot navigate to DataAffiliatePreviewPage");
                        await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể điều hướng sang trang xem trước.", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to DataAffiliatePreviewPage");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể điều hướng sang trang xem trước.", "OK");
            }
        }

        private async Task CancelFormAsync()
        {
            try
            {
                ShowStoreFormPopup = false;
                FormErrorMessage = string.Empty;
                StoreForm = new PreviewCrawlDataAgreementRequest();
                _selectedStoreId = string.Empty;

                _logger.LogInformation("Form cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling form");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

