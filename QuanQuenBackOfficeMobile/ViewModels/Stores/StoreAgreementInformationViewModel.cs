using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic; // Added missing import
using System.Linq;
using QuanQuenBackOfficeMobile.Views.Stores; // Added for LINQ operations

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreAgreementInformationViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly IConfiguration _configuration;

        [ObservableProperty]
        private bool isBusy = false;





        // Agreement fields
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

        public StoreAgreementInformationViewModel(IContractService contractService, IConfiguration configuration)
        {
            _contractService = contractService;
            _configuration = configuration;
            ContinueCommand = new AsyncRelayCommand(ContinueAsync);

            // Auto-load data từ StoreRegistrationData
            LoadDataFromRegistration();
        }

        private void LoadDataFromRegistration()
        {
            try
            {
                // Kiểm tra null safety
                if (StoreRegistrationData.StoreName != null)
                {
                    StoreName = StoreRegistrationData.StoreName;
                }
                
                if (StoreRegistrationData.RepresentativeName != null)
                {
                    RepresentativeName = StoreRegistrationData.RepresentativeName;
                }
                
                if (StoreRegistrationData.PhoneNumber != null)
                {
                    PhoneNumber = StoreRegistrationData.PhoneNumber;
                }

                // Auto-fill địa chỉ cửa hàng từ StoreRegistrationData (giống như StoreInformationContractViewModel)
                var fullAddress = BuildFullAddress();
                StoreHeadOfficeAddress = fullAddress;

                // Các trường khác để trống để người dùng nhập
                RepresentativePosition = string.Empty;
                RepresentativeIdNumber = string.Empty;

                System.Diagnostics.Debug.WriteLine($"[StoreAgreementInformationViewModel] Loaded default data: StoreName={StoreName}, RepresentativeName={RepresentativeName}, PhoneNumber={PhoneNumber}, StoreHeadOfficeAddress={StoreHeadOfficeAddress}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreAgreementInformationViewModel] Error loading registration data: {ex.Message}");
                // Fallback to empty data
                InitializeFallbackData();
            }
        }

        /// <summary>
        /// Tạo địa chỉ đầy đủ từ StoreRegistrationData (giống như StoreInformationContractViewModel)
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

        /// <summary>
        /// Khởi tạo dữ liệu fallback khi có lỗi
        /// </summary>
        private void InitializeFallbackData()
        {
            try
            {
                StoreName = string.Empty;
                RepresentativeName = string.Empty;
                PhoneNumber = string.Empty;
                RepresentativePosition = string.Empty;
                RepresentativeIdNumber = string.Empty;
                
                // Cũng thử build địa chỉ trong fallback
                try
                {
                    var fullAddress = BuildFullAddress();
                    StoreHeadOfficeAddress = fullAddress;
                }
                catch
                {
                    StoreHeadOfficeAddress = string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreAgreementInformationViewModel] Fallback initialization error: {ex.Message}");
            }
        }



        private async Task ContinueAsync()
        {
            try
            {
                IsBusy = true;

                // 1) Validate required fields
                if (!ValidateRequiredFields())
                {
                    return;
                }



                // 3) Convert ViewModel data to API request
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

                // 4) Call PreviewAgreement API
                var apiResponse = await _contractService.CallPreviewAgreementApiAsync(apiRequest);

                if (apiResponse.IsSuccess)
                {
                    // Tạo ViewModel với PDF content sẵn (giống ContractPreview)
                    var previewViewModel = new AgreementPreviewViewModel(_contractService);
                    previewViewModel.SetApiPdfContent(apiResponse.Message);
                    
                    // Navigate to Agreement Preview page với ViewModel và data
                    var previewPage = new AgreementPreviewPage(previewViewModel, agreementInfo);
                    await Application.Current.MainPage.Navigation.PushAsync(previewPage);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi API", apiResponse.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Validate tất cả các field bắt buộc
        /// </summary>
        private bool ValidateRequiredFields()
        {
            try
            {
                var errors = new List<string>();

                // Kiểm tra null và empty
                if (string.IsNullOrWhiteSpace(StoreName?.Trim()))
                    errors.Add("Vui lòng nhập tên cửa hàng");
                
                if (string.IsNullOrWhiteSpace(RepresentativeName?.Trim()))
                    errors.Add("Vui lòng nhập họ và tên đại diện");
                
                if (string.IsNullOrWhiteSpace(RepresentativePosition?.Trim()))
                    errors.Add("Vui lòng nhập chức vụ đại diện");
                
                if (string.IsNullOrWhiteSpace(RepresentativeIdNumber?.Trim()))
                    errors.Add("Vui lòng nhập số CMND/CCCD");
                

                
                if (string.IsNullOrWhiteSpace(StoreHeadOfficeAddress?.Trim()))
                    errors.Add("Vui lòng nhập địa chỉ cửa hàng");
                
                if (string.IsNullOrWhiteSpace(PhoneNumber?.Trim()))
                    errors.Add("Vui lòng nhập số điện thoại");

                // Validate phone number format (basic)
                if (!string.IsNullOrWhiteSpace(PhoneNumber) && !IsValidPhoneNumber(PhoneNumber))
                    errors.Add("Số điện thoại không đúng định dạng");

                if (errors.Count > 0)
                {
                    var errorMessage = string.Join("\n", errors);
                    System.Diagnostics.Debug.WriteLine($"[StoreAgreementInformationViewModel] Validation errors: {errorMessage}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreAgreementInformationViewModel] Validation error: {ex.Message}");
                return false;
            }
        }



        /// <summary>
        /// Kiểm tra định dạng số điện thoại cơ bản
        /// </summary>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            
            // Loại bỏ khoảng trắng và ký tự đặc biệt
            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Kiểm tra độ dài (10-11 số cho VN)
            return cleanPhone.Length >= 10 && cleanPhone.Length <= 11;
        }
    }
}
