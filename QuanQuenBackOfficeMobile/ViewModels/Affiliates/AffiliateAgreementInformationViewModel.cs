using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Views.Affiliates;

namespace QuanQuenBackOfficeMobile.ViewModels.Affiliates
{
    public partial class AffiliateAgreementInformationViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly IConfiguration _configuration;

        [ObservableProperty]
        private bool isBusy = false;

        // Thông tin thỏa thuận cho Affiliate (độc lập với User)
        [ObservableProperty]
        private string storeName = string.Empty;

        [ObservableProperty]
        private string representativeName = string.Empty;

        [ObservableProperty]
        private string representativePosition = string.Empty;

        [ObservableProperty]
        private string representativeIdNumber = string.Empty;

        [ObservableProperty]
        private string storeHeadOfficeAddress = string.Empty;

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        public IAsyncRelayCommand ContinueCommand { get; }

        public AffiliateAgreementInformationViewModel(IContractService contractService, IConfiguration configuration)
        {
            _contractService = contractService;
            _configuration = configuration;
            ContinueCommand = new AsyncRelayCommand(ContinueAsync);

            LoadDefaultData();
        }

        private void LoadDefaultData()
        {
            try
            {
                // Auto-fill từ AffiliateStoreData nếu có
                if (!string.IsNullOrWhiteSpace(AffiliateStoreData.StoreName))
                    StoreName = AffiliateStoreData.StoreName;
                if (!string.IsNullOrWhiteSpace(AffiliateStoreData.RepresentativeName))
                    RepresentativeName = AffiliateStoreData.RepresentativeName;
                if (!string.IsNullOrWhiteSpace(AffiliateStoreData.PhoneNumber))
                    PhoneNumber = AffiliateStoreData.PhoneNumber;

                // Build địa chỉ đầy đủ từ StoreRegistrationData (giống user thường)
                var fullAddress = BuildFullAddress();
                StoreHeadOfficeAddress = fullAddress;

                System.Diagnostics.Debug.WriteLine($"[AffiliateAgreementInformationViewModel] Loaded default data: StoreName={StoreName}, RepresentativeName={RepresentativeName}, PhoneNumber={PhoneNumber}, StoreHeadOfficeAddress={StoreHeadOfficeAddress}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AffiliateAgreementInformationViewModel] Error loading default data: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo địa chỉ đầy đủ từ StoreRegistrationData (giống user thường)
        /// </summary>
        private string BuildFullAddress()
        {
            var addressParts = new List<string>();

            if (!string.IsNullOrEmpty(StoreRegistrationData.Address))
                addressParts.Add(StoreRegistrationData.Address);

            if (!string.IsNullOrEmpty(StoreRegistrationData.Ward))
                addressParts.Add(StoreRegistrationData.Ward);

            if (!string.IsNullOrEmpty(StoreRegistrationData.District))
                addressParts.Add(StoreRegistrationData.District);

            if (!string.IsNullOrEmpty(StoreRegistrationData.Province))
                addressParts.Add(StoreRegistrationData.Province);

            return string.Join(", ", addressParts);
        }

        private async Task ContinueAsync()
        {
            try
            {
                IsBusy = true;

                if (!ValidateRequiredFields())
                {
                    return;
                }

                var agreementInfo = new AgreementInfoDto
                {
                    StoreName = StoreName,
                    RepresentativeName = RepresentativeName,
                    RepresentativePosition = RepresentativePosition,
                    RepresentativeIdNumber = RepresentativeIdNumber,
                    StoreHeadOfficeAddress = StoreHeadOfficeAddress,
                    PhoneNumber = PhoneNumber
                };

                var apiRequest = await _contractService.ConvertToPreviewAgreementRequestAsync(agreementInfo);
                var apiResponse = await _contractService.CallPreviewAgreementApiAsync(apiRequest);

                if (apiResponse.IsSuccess)
                {
                    var previewViewModel = new ViewModels.Affiliates.AffiliateAgreementPreviewViewModel(_contractService);
                    previewViewModel.SetApiPdfContent(apiResponse.Message);

                    var previewPage = new AffiliateAgreementPreviewPage(previewViewModel, agreementInfo);
                    Application.Current.MainPage = new NavigationPage(previewPage);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi API", apiResponse.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateRequiredFields()
        {
            try
            {
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(StoreName?.Trim())) errors.Add("Vui lòng nhập tên cửa hàng");
                if (string.IsNullOrWhiteSpace(RepresentativeName?.Trim())) errors.Add("Vui lòng nhập họ và tên đại diện");
                if (string.IsNullOrWhiteSpace(RepresentativePosition?.Trim())) errors.Add("Vui lòng nhập chức vụ đại diện");
                if (string.IsNullOrWhiteSpace(RepresentativeIdNumber?.Trim())) errors.Add("Vui lòng nhập số CMND/CCCD");
                if (string.IsNullOrWhiteSpace(StoreHeadOfficeAddress?.Trim())) errors.Add("Vui lòng nhập địa chỉ cửa hàng");
                if (string.IsNullOrWhiteSpace(PhoneNumber?.Trim())) errors.Add("Vui lòng nhập số điện thoại");

                if (errors.Any())
                {
                    var message = string.Join("\n", errors);
                    Application.Current?.MainPage?.DisplayAlert("Thiếu thông tin", message, "OK");
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


