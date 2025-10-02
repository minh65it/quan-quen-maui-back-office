using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels
{
    public partial class AffiliateStoreListViewModel : ObservableObject
    {
        private readonly IAffiliateService _affiliateService;

        [ObservableProperty]
        private ObservableCollection<AffiliateStore> stores = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string referralId;

        [ObservableProperty]
        private string referralName;

        [ObservableProperty]
        private string totalCount;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage;

        public ICommand LoadStoresCommand { get; }
        public ICommand RefreshStoresCommand { get; }
        public ICommand BackToCreateStoreCommand { get; }

        public AffiliateStoreListViewModel(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
            LoadStoresCommand = new AsyncRelayCommand(LoadStoresAsync);
            RefreshStoresCommand = new AsyncRelayCommand(RefreshStoresAsync);
            BackToCreateStoreCommand = new AsyncRelayCommand(BackToCreateStoreAsync);
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Lấy ReferralId từ SecureStorage/Preferences
                ReferralId = await SecureStorage.GetAsync("affiliate_referral_id") ?? "";
                ReferralName = await SecureStorage.GetAsync("affiliate_referral_name") ?? "";
                
                if (string.IsNullOrEmpty(ReferralId))
                {
                    ReferralId = Preferences.Get("affiliate_referral_id", "");
                    ReferralName = Preferences.Get("affiliate_referral_name", "");
                }

                System.Diagnostics.Debug.WriteLine($"[AffiliateStoreList] ReferralId: {ReferralId}, ReferralName: {ReferralName}");
                
                await LoadStoresAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AffiliateStoreList] InitializeAsync error: {ex.Message}");
                HasError = true;
                ErrorMessage = $"Lỗi khởi tạo: {ex.Message}";
            }
        }

        private async Task LoadStoresAsync()
        {
            if (IsLoading || string.IsNullOrEmpty(ReferralId))
                return;

            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = "";

                // Get affiliate token
                var token = await SecureStorage.GetAsync("affiliate_token") ?? 
                           Preferences.Get("affiliate_token", "");

                System.Diagnostics.Debug.WriteLine($"[AffiliateStoreList] Calling API with ReferralId: {ReferralId}, Token: {(string.IsNullOrEmpty(token) ? "EMPTY" : "EXISTS")}");
                
                var response = await _affiliateService.GetStoresByReferralIdAsync(ReferralId, token);
                
                if (response?.Result != null)
                {
                    Stores.Clear();
                    foreach (var store in response.Result)
                    {
                        Stores.Add(store);
                    }
                    
                    TotalCount = $"{response.Pagination?.TotalCount ?? 0} nhà hàng";
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Không có dữ liệu nhà hàng";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshStoresAsync()
        {
            // Refresh lại data khi user quay về từ các trang khác
            if (!string.IsNullOrEmpty(ReferralId))
            {
                await LoadStoresAsync();
            }
        }

        private async Task BackToCreateStoreAsync()
        {
            try
            {
                var affiliateCreateStoreViewModel = IPlatformApplication.Current.Services.GetService<AffiliateCreateStoreViewModel>();
                Application.Current.MainPage = new AffiliateCreateStorePage(affiliateCreateStoreViewModel);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể chuyển trang: {ex.Message}", "OK");
            }
        }

        private string GetFormattedPhone(string phone)
        {
            return string.IsNullOrEmpty(phone) ? "Chưa cập nhật" : phone;
        }

        private string GetFormattedAddress(string address, string city)
        {
            var addressParts = new[] { address, city }.Where(s => !string.IsNullOrEmpty(s));
            return addressParts.Any() ? string.Join(", ", addressParts) : "Chưa cập nhật";
        }

        private string GetFormattedDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }
    }
}
