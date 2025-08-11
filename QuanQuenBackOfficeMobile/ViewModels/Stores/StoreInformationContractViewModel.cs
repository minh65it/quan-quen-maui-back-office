using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuanQuenBackOfficeMobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class StoreInformationContractViewModel : ObservableObject
    {
        private readonly IContractService _contractService;

        // Danh sách giới tính để chọn
        public List<string> GenderList { get; } = new List<string> { "Nam", "Nữ" };

        [ObservableProperty]
        private bool isBusy;

        // Error properties cho validation
        [ObservableProperty]
        private string phoneError = string.Empty;

        [ObservableProperty]
        private string validationError = string.Empty;

        // ---- Error cho các ngày (thêm mới) ----
        [ObservableProperty] private string providerRepresentativeIdDateError = string.Empty;
        [ObservableProperty] private string businessLicenseDateError = string.Empty;
        [ObservableProperty] private string representativeIdDateError = string.Empty;
        [ObservableProperty] private string ownerIdDateError = string.Empty;

        // BÊN CUNG CẤP DỊCH VỤ (Provider) - Common for both contracts
        [ObservableProperty]
        private string providerRepresentativeIdNumber = string.Empty;

        [ObservableProperty]
        private string providerRepresentativeIdDate = string.Empty;

        [ObservableProperty]
        private string providerRepresentativeIdPlace = string.Empty;

        // Main Contract - Tổ chức
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

        // Main Contract - Hộ kinh doanh
        [ObservableProperty]
        private string ownerName;

        [ObservableProperty]
        private string ownerGender;

        [ObservableProperty]
        private string ownerIdNumber;

        [ObservableProperty]
        private string ownerIdDate;

        [ObservableProperty]
        private string ownerIdPlace;

        [ObservableProperty]
        private string ownerPermanentAddress;

        [ObservableProperty]
        private string ownerTemporaryAddress;

        public ICommand NextCommand { get; }
        public ICommand PreviewCommand { get; }

        public StoreInformationContractViewModel(IContractService contractService)
        {
            _contractService = contractService;
            NextCommand = new AsyncRelayCommand(OnNextClicked);
            PreviewCommand = new AsyncRelayCommand(OnPreviewClicked);

            // Auto-load data từ StoreRegistrationData
            LoadDataFromRegistration();
        }

        private async Task OnNextClicked()
        {
            try
            {
                IsBusy = true;
                // TODO: Lưu thông tin hợp đồng (DB/local)
                await Application.Current.MainPage.Navigation.PushAsync(new QuanQuenBackOfficeMobile.Views.DrawingExamplePage());
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

        private async Task OnPreviewClicked()
        {
            try
            {
                IsBusy = true;
                ValidationError = string.Empty; // Clear previous validation errors

                // 1) Validate required fields
                if (!ValidateRequiredFields())
                {
                    return;
                }

                // 2) Validate các trường ngày theo chuẩn dd/MM/yyyy và không lớn hơn hôm nay
                bool allDateOk = true;

                if (!TryValidateDate_ddMMyyyy(ProviderRepresentativeIdDate, out var normProv, out var errProv))
                {
                    ProviderRepresentativeIdDateError = errProv;
                    allDateOk = false;
                }
                else
                {
                    ProviderRepresentativeIdDateError = string.Empty;
                    if (ProviderRepresentativeIdDate != normProv) ProviderRepresentativeIdDate = normProv;
                }

                if (!TryValidateDate_ddMMyyyy(BusinessLicenseDate, out var normBiz, out var errBiz))
                {
                    BusinessLicenseDateError = errBiz;
                    allDateOk = false;
                }
                else
                {
                    BusinessLicenseDateError = string.Empty;
                    if (BusinessLicenseDate != normBiz) BusinessLicenseDate = normBiz;
                }

                if (!TryValidateDate_ddMMyyyy(RepresentativeIdDate, out var normRep, out var errRep))
                {
                    RepresentativeIdDateError = errRep;
                    allDateOk = false;
                }
                else
                {
                    RepresentativeIdDateError = string.Empty;
                    if (RepresentativeIdDate != normRep) RepresentativeIdDate = normRep;
                }

                if (!string.IsNullOrEmpty(StoreRegistrationData.BusinessType) &&
                    StoreRegistrationData.BusinessType.Contains("Hộ"))
                {
                    if (!TryValidateDate_ddMMyyyy(OwnerIdDate, out var normOwner, out var errOwner))
                    {
                        OwnerIdDateError = errOwner;
                        allDateOk = false;
                    }
                    else
                    {
                        OwnerIdDateError = string.Empty;
                        if (OwnerIdDate != normOwner) OwnerIdDate = normOwner;
                    }
                }

                if (!allDateOk)
                {
                    await App.Current.MainPage.DisplayAlert("Thiếu/ Sai thông tin",
                        "Vui lòng kiểm tra lại các ngày theo định dạng dd/MM/yyyy và không lớn hơn ngày hiện tại.",
                        "OK");
                    return;
                }

                // 3) Convert ViewModel data to API request
                var apiRequest = await _contractService.ConvertToPreviewContractRequestAsync(this);

                // 4) Call PreviewContract API
                var apiResponse = await _contractService.CallPreviewContractApiAsync(apiRequest);

                if (apiResponse.IsSuccess)
                {
                    var previewViewModel = new ContractPreviewViewModel();
                    previewViewModel.SetApiPdfContent(apiResponse.GetPdfDataUrl());

                    var previewPage = new QuanQuenBackOfficeMobile.Views.Stores.ContractPreviewPage(previewViewModel, this);
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
            var missingFields = new List<string>();

            // Validate Provider fields (required for both contracts)
            if (string.IsNullOrWhiteSpace(ProviderRepresentativeIdNumber))
                missingFields.Add("Số CCCD/CMND/Hộ chiếu đại diện (Bên cung cấp)");
            if (string.IsNullOrWhiteSpace(ProviderRepresentativeIdDate))
                missingFields.Add("Cấp ngày (Bên cung cấp)");
            if (string.IsNullOrWhiteSpace(ProviderRepresentativeIdPlace))
                missingFields.Add("Tại (Bên cung cấp)");

            // Validate Organization fields
            if (string.IsNullOrWhiteSpace(CompanyName))
                missingFields.Add("Tên doanh nghiệp");
            if (string.IsNullOrWhiteSpace(StoreName))
                missingFields.Add("Tên cửa hàng");
            if (string.IsNullOrWhiteSpace(BusinessLicenseNumber))
                missingFields.Add("Số giấy chứng nhận đăng ký doanh nghiệp");
            if (string.IsNullOrWhiteSpace(BusinessLicenseDate))
                missingFields.Add("Ngày cấp GCN đăng ký doanh nghiệp");
            if (string.IsNullOrWhiteSpace(BusinessLicensePlace))
                missingFields.Add("Nơi cấp GCN đăng ký doanh nghiệp");
            if (string.IsNullOrWhiteSpace(RepresentativeName))
                missingFields.Add("Đại diện bởi (Họ tên)");
            if (string.IsNullOrWhiteSpace(RepresentativePosition))
                missingFields.Add("Chức vụ đại diện");
            if (string.IsNullOrWhiteSpace(RepresentativeIdNumber))
                missingFields.Add("Số CCCD/CMND/Hộ chiếu đại diện");
            if (string.IsNullOrWhiteSpace(RepresentativeIdDate))
                missingFields.Add("Ngày cấp CCCD/CMND/Hộ chiếu đại diện");
            if (string.IsNullOrWhiteSpace(RepresentativeIdPlace))
                missingFields.Add("Nơi cấp CCCD/CMND/Hộ chiếu đại diện");
            if (string.IsNullOrWhiteSpace(RepresentativePermanentAddress))
                missingFields.Add("Địa chỉ thường trú đại diện");
            if (string.IsNullOrWhiteSpace(StoreHeadOfficeAddress))
                missingFields.Add("Địa chỉ trụ sở chính của cửa hàng");
            if (string.IsNullOrWhiteSpace(PhoneNumber))
                missingFields.Add("Số điện thoại liên hệ");
            if (string.IsNullOrWhiteSpace(HeadquartersAddress))
                missingFields.Add("Địa chỉ trụ sở chính doanh nghiệp");

            // Validate Individual Business fields if applicable
            if (!string.IsNullOrEmpty(StoreRegistrationData.BusinessType) &&
                StoreRegistrationData.BusinessType.Contains("Hộ"))
            {
                if (string.IsNullOrWhiteSpace(OwnerName))
                    missingFields.Add("Họ tên chủ hộ kinh doanh");
                if (string.IsNullOrWhiteSpace(OwnerGender))
                    missingFields.Add("Giới tính");
                if (string.IsNullOrWhiteSpace(OwnerIdNumber))
                    missingFields.Add("Số CCCD/CMND/Hộ chiếu chủ hộ");
                if (string.IsNullOrWhiteSpace(OwnerIdDate))
                    missingFields.Add("Ngày cấp CCCD/CMND/Hộ chiếu chủ hộ");
                if (string.IsNullOrWhiteSpace(OwnerIdPlace))
                    missingFields.Add("Nơi cấp CCCD/CMND/Hộ chiếu chủ hộ");
            }

            if (missingFields.Any())
            {
                ValidationError = $"⚠️ Vui lòng điền đầy đủ thông tin:\n• {string.Join("\n• ", missingFields)}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load dữ liệu từ StoreRegistrationData để auto-fill vào UI
        /// </summary>
        private void LoadDataFromRegistration()
        {
            try
            {
                // Auto-fill các thông tin cơ bản đã đăng ký
                StoreName = StoreRegistrationData.StoreName ?? string.Empty;
                RepresentativeName = StoreRegistrationData.RepresentativeName ?? string.Empty;
                PhoneNumber = StoreRegistrationData.PhoneNumber ?? string.Empty;

                // Auto-fill địa chỉ (combine từ Address + Ward + District + Province)
                var fullAddress = BuildFullAddress();
                StoreHeadOfficeAddress = fullAddress;
                HeadquartersAddress = fullAddress; // Dùng chung cho trụ sở chính

                // Auto-fill thông tin chủ hộ kinh doanh (nếu là hộ KD)
                if (!string.IsNullOrEmpty(StoreRegistrationData.BusinessType) &&
                    StoreRegistrationData.BusinessType.Contains("Hộ"))
                {
                    OwnerName = StoreRegistrationData.RepresentativeName ?? string.Empty;
                }

                // Auto-fill tên công ty mặc định từ tên cửa hàng
                if (string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(StoreName))
                {
                    CompanyName = StoreName;
                }

                System.Diagnostics.Debug.WriteLine($"[StoreInformationContract] Auto-loaded: {StoreName}, {RepresentativeName}, {PhoneNumber}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StoreInformationContract] Error loading registration data: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo địa chỉ đầy đủ từ StoreRegistrationData
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

        // Validation khi PhoneNumber thay đổi
        partial void OnPhoneNumberChanged(string value)
        {
            ValidatePhoneNumber();
        }

        /// <summary>
        /// Validate số điện thoại
        /// </summary>
        private void ValidatePhoneNumber()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                PhoneError = "Vui lòng nhập số điện thoại";
            }
            else if (!IsValidPhoneNumber(PhoneNumber))
            {
                PhoneError = "Số điện thoại không hợp lệ. Vui lòng nhập đúng định dạng số điện thoại Việt Nam";
            }
            else
            {
                PhoneError = "";
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của số điện thoại Việt Nam
        /// </summary>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Loại bỏ khoảng trắng và ký tự đặc biệt
            var cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");

            // Kiểm tra độ dài
            if (cleanPhone.Length != 10)
                return false;

            // Kiểm tra format số điện thoại Việt Nam
            // Bắt đầu bằng 0, tiếp theo là 3, 5, 7, 8, 9 và có 8 chữ số tiếp theo
            var phonePattern = @"^0[35789][0-9]{8}$";
            return Regex.IsMatch(cleanPhone, phonePattern);
        }

        // ---------- DATE VALIDATION (dd/MM/yyyy) ----------
        private static bool TryValidateDate_ddMMyyyy(string input, out string normalized, out string error)
        {
            normalized = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Vui lòng nhập ngày theo định dạng dd/MM/yyyy.";
                return false;
            }

            if (!DateTime.TryParseExact(input.Trim(),
                                        "dd/MM/yyyy",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out var date))
            {
                error = "Sai định dạng ngày. Định dạng đúng: dd/MM/yyyy (ví dụ: 25/12/2024).";
                return false;
            }

            if (date.Date > DateTime.Today)
            {
                error = "Ngày không được lớn hơn ngày hiện tại.";
                return false;
            }

            normalized = date.ToString("dd/MM/yyyy");
            return true;
        }

        // Thay đổi từng trường ngày -> validate ngay và chuẩn hóa
        partial void OnProviderRepresentativeIdDateChanged(string value)
        {
            if (TryValidateDate_ddMMyyyy(value, out var normalized, out var err))
            {
                ProviderRepresentativeIdDateError = string.Empty;
                if (ProviderRepresentativeIdDate != normalized)
                    ProviderRepresentativeIdDate = normalized;
            }
            else
            {
                ProviderRepresentativeIdDateError = err;
            }
        }

        partial void OnBusinessLicenseDateChanged(string value)
        {
            if (TryValidateDate_ddMMyyyy(value, out var normalized, out var err))
            {
                BusinessLicenseDateError = string.Empty;
                if (BusinessLicenseDate != normalized)
                    BusinessLicenseDate = normalized;
            }
            else
            {
                BusinessLicenseDateError = err;
            }
        }

        partial void OnRepresentativeIdDateChanged(string value)
        {
            if (TryValidateDate_ddMMyyyy(value, out var normalized, out var err))
            {
                RepresentativeIdDateError = string.Empty;
                if (RepresentativeIdDate != normalized)
                    RepresentativeIdDate = normalized;
            }
            else
            {
                RepresentativeIdDateError = err;
            }
        }

        partial void OnOwnerIdDateChanged(string value)
        {
            if (TryValidateDate_ddMMyyyy(value, out var normalized, out var err))
            {
                OwnerIdDateError = string.Empty;
                if (OwnerIdDate != normalized)
                    OwnerIdDate = normalized;
            }
            else
            {
                OwnerIdDateError = err;
            }
        }
    }
}
