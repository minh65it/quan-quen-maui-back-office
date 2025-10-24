using System;
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
    public partial class AffiliateStatisticsViewModel : ObservableObject
    {
        private readonly IAffiliateService _affiliateService;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string referralId;

        [ObservableProperty]
        private string referralName;

        // Store Statistics
        [ObservableProperty]
        private int totalStores;

        [ObservableProperty]
        private int pendingStores;

        [ObservableProperty]
        private int approvedStores;

        [ObservableProperty]
        private int inActiveStores;

        [ObservableProperty]
        private int blockedStores;

        [ObservableProperty]
        private int storesWithContract;

        [ObservableProperty]
        private int storesWithoutContract;

        // Transaction Statistics
        [ObservableProperty]
        private int totalTransactions;

        [ObservableProperty]
        private int pendingTransactions;

        [ObservableProperty]
        private int finishedTransactions;

        [ObservableProperty]
        private int canceledTransactions;

        [ObservableProperty]
        private int storesWithTransaction;

        [ObservableProperty]
        private int storesWithoutTransaction;

        // Commission Statistics
        [ObservableProperty]
        private string totalCommissionEarned;

        [ObservableProperty]
        private string pendingCommission;

        [ObservableProperty]
        private string averageCommission;

        // Other Statistics
        [ObservableProperty]
        private string successRate;

        [ObservableProperty]
        private string lastStoreReferralDate;

        [ObservableProperty]
        private string lastTransactionDate;

        public ICommand LoadStatisticsCommand { get; }
        public ICommand BackCommand { get; }

        public AffiliateStatisticsViewModel(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
            LoadStatisticsCommand = new AsyncRelayCommand(LoadStatisticsAsync);
            BackCommand = new AsyncRelayCommand(BackAsync);
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

                System.Diagnostics.Debug.WriteLine($"[AffiliateStatistics] ReferralId: {ReferralId}, ReferralName: {ReferralName}");
                
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AffiliateStatistics] InitializeAsync error: {ex.Message}");
                HasError = true;
                ErrorMessage = $"Lỗi khởi tạo: {ex.Message}";
            }
        }

        private async Task LoadStatisticsAsync()
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

                System.Diagnostics.Debug.WriteLine($"[AffiliateStatistics] Calling API with ReferralId: {ReferralId}");
                
                var response = await _affiliateService.GetReferralStatisticsAsync(ReferralId, token);
                
                if (response?.Success == true && response.Data != null)
                {
                    var data = response.Data;
                    
                    // Store statistics
                    TotalStores = data.TotalStores;
                    PendingStores = data.PendingStores;
                    ApprovedStores = data.ApprovedStores;
                    InActiveStores = data.InActiveStores;
                    BlockedStores = data.BlockedStores;
                    StoresWithContract = data.StoresWithContract;
                    StoresWithoutContract = data.StoresWithoutContract;

                    // Transaction statistics
                    TotalTransactions = data.TotalTransactions;
                    PendingTransactions = data.PendingTransactions;
                    FinishedTransactions = data.FinishedTransactions;
                    CanceledTransactions = data.CanceledTransactions;
                    StoresWithTransaction = data.StoresWithTransaction;
                    StoresWithoutTransaction = data.StoresWithoutTransaction;

                    // Commission statistics
                    TotalCommissionEarned = FormatCurrency(data.TotalCommissionEarned);
                    PendingCommission = FormatCurrency(data.PendingCommission);
                    AverageCommission = FormatCurrency(data.AverageCommission);

                    // Other statistics
                    SuccessRate = $"{data.SuccessRate:F1}%";
                    LastStoreReferralDate = FormatDate(data.LastStoreReferralDate);
                    LastTransactionDate = FormatDate(data.LastTransactionDate);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Không thể tải dữ liệu thống kê";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[AffiliateStatistics] Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BackAsync()
        {
            try
            {
                // Navigate back to AffiliateStoreListPage
                var affiliateStoreListViewModel = IPlatformApplication.Current.Services.GetService<AffiliateStoreListViewModel>();
                var affiliateStoreListPage = new Views.AffiliateStoreListPage(affiliateStoreListViewModel);
                Application.Current.MainPage = affiliateStoreListPage;
                
                // Refresh the store list
                await affiliateStoreListViewModel.RefreshStoresAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AffiliateStatistics] Back error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể quay lại: {ex.Message}", "OK");
            }
        }

        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("N0") + " VNĐ";
        }

        private string FormatDate(DateTime? date)
        {
            if (!date.HasValue)
                return "Chưa có";
            
            return date.Value.ToString("dd/MM/yyyy HH:mm");
        }
    }
}

