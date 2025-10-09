using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.ViewModels.Affiliates
{
    public partial class AffiliateAgreementSignatureViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly AgreementInfoDto _agreementInfo;

        [ObservableProperty]
        private bool isBusy = false;

        public AffiliateAgreementSignatureViewModel(IContractService contractService, AgreementInfoDto agreementInfo)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
        }

        public async Task<PreviewAgreementResponse> SaveAgreementWithSignatureAsync(byte[] signatureImageBytes)
        {
            try
            {
                IsBusy = true;
                // Tạo QR code tương tự user
                var qrBytes = await GenerateAndDownloadQRCodeAsync();

                // Convert request và gọi API
                var apiRequest = await _contractService.ConvertToPreviewAgreementRequestAsync(_agreementInfo);
                var apiResponse = await _contractService.CallPreviewAgreementApiWithSignatureAsync(apiRequest, signatureImageBytes, qrBytes);
                return apiResponse;
            }
            catch (Exception ex)
            {
                return new PreviewAgreementResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<byte[]?> GenerateAndDownloadQRCodeAsync()
        {
            try
            {
                // Tối giản: sử dụng cùng logic như user nếu có QRCodeService; fallback null nếu thiếu
                var qrService = App.Services.GetService<IQRCodeService>();
                if (qrService == null)
                {
                    return null;
                }

                var agentName = await Microsoft.Maui.Storage.SecureStorage.GetAsync("userName") ?? "Affiliate";
                var agentId = await Microsoft.Maui.Storage.SecureStorage.GetAsync("userId") ?? "";

                // Ghép địa chỉ đầy đủ từ dữ liệu đã chọn (Address, Ward, District, Province)
                var addressParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(StoreRegistrationData.Address)) addressParts.Add(StoreRegistrationData.Address);
                if (!string.IsNullOrWhiteSpace(StoreRegistrationData.Ward)) addressParts.Add(StoreRegistrationData.Ward);
                if (!string.IsNullOrWhiteSpace(StoreRegistrationData.District)) addressParts.Add(StoreRegistrationData.District);
                if (!string.IsNullOrWhiteSpace(StoreRegistrationData.Province)) addressParts.Add(StoreRegistrationData.Province);
                var fullAddress = addressParts.Count > 0 ? string.Join(", ", addressParts) : (_agreementInfo?.StoreHeadOfficeAddress ?? string.Empty);

                // Lấy tọa độ từ địa chỉ cửa hàng bằng MAUI Essentials Geocoding (giống user)
                (double Latitude, double Longitude)? coordinates = null;
                try
                {
                    var locations = await Microsoft.Maui.Devices.Sensors.Geocoding.GetLocationsAsync(fullAddress);
                    var location = locations?.FirstOrDefault();
                    if (location != null)
                    {
                        coordinates = (location.Latitude, location.Longitude);
                    }
                }
                catch (Exception geoEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[AffiliateAgreementSignatureViewModel] Essentials Geocoding failed: {geoEx.Message}");
                }

                var qrData = new QRCodeData
                {
                    DiaChi = fullAddress,
                    ToaDo = new Coordinates { Latitude = coordinates?.Latitude ?? 10.762622, Longitude = coordinates?.Longitude ?? 106.660172 },
                    TenNguoiDaiDien = _agreementInfo?.RepresentativeName ?? string.Empty,
                    AgentInfo = new AgentInfo { Ten = agentName, Id = agentId },
                    Timestamp = DateTime.UtcNow
                };

                var resp = await qrService.GenerateStoreQRCodeAsync(qrData, qrData.AgentInfo);
                if (!resp.Success || string.IsNullOrEmpty(resp.QRCodeImage)) return null;

                // Download ảnh
                var token = await Microsoft.Maui.Storage.SecureStorage.GetAsync("auth_token");
                var http = new System.Net.Http.HttpClient();
                if (!string.IsNullOrEmpty(token))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var httpResp = await http.GetAsync(resp.QRCodeImage);
                if (!httpResp.IsSuccessStatusCode) return null;
                return await httpResp.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}


