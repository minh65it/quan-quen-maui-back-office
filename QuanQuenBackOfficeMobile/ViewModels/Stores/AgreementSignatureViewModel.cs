using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace QuanQuenBackOfficeMobile.ViewModels.Stores
{
    public partial class AgreementSignatureViewModel : ObservableObject
    {
        private readonly IContractService _contractService;
        private readonly IQRCodeService _qrCodeService;
        private readonly IConfiguration _configuration;
        private readonly AgreementInfoDto _agreementInfo;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private ObservableCollection<IDrawingLine> signatureLines = new();

        public IAsyncRelayCommand ClearSignatureCommand { get; }
        public IAsyncRelayCommand SaveAgreementWithSignatureCommand { get; }

        public AgreementSignatureViewModel(IContractService contractService, AgreementInfoDto agreementInfo)
        {
            _contractService = contractService;
            _agreementInfo = agreementInfo;
            
            // Get services from DI container
            var qrCodeService = App.Services.GetService<IQRCodeService>();
            var configuration = App.Services.GetService<IConfiguration>();
            
            _qrCodeService = qrCodeService;
            _configuration = configuration;
            
            ClearSignatureCommand = new AsyncRelayCommand(ClearSignatureAsync);
            SaveAgreementWithSignatureCommand = new AsyncRelayCommand<byte[]>(SaveAgreementWithSignatureAsync);
        }

        /// <summary>
        /// Xóa chữ ký
        /// </summary>
        private async Task ClearSignatureAsync()
        {
            try
            {
                SignatureLines.Clear();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Tạo QR Code cho store và download image
        /// </summary>
        private async Task<byte[]?> GenerateAndDownloadQRCodeAsync()
        {
            try
            {
                // Lấy thông tin agent từ storage
                var agentName = await SecureStorage.GetAsync("userName") ?? "Unknown Agent";
                var agentId = await SecureStorage.GetAsync("userId") ?? "Unknown ID";

                // Tạo dữ liệu QR Code từ agreement info
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
                    System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] Essentials Geocoding failed: {geoEx.Message}");
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

                // Gọi service tạo QR Code
                System.Diagnostics.Debug.WriteLine("[AgreementSignatureViewModel] Gọi QRCodeService.GenerateStoreQRCodeAsync...");
                var qrCodeResponse = await _qrCodeService.GenerateStoreQRCodeAsync(qrCodeData, qrCodeData.AgentInfo);

                if (qrCodeResponse.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] QR Code API thành công. Image URL: {qrCodeResponse.QRCodeImage}");
                    
                    // Download QR code image
                    var qrCodeBytes = await DownloadQRCodeImage(qrCodeResponse.QRCodeImage);
                    
                    if (qrCodeBytes != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] Download QR code thành công: {qrCodeBytes.Length} bytes");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[AgreementSignatureViewModel] LỖI: Download QR code thất bại (null)");
                    }
                    
                    return qrCodeBytes;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] LỖI: QR Code generation failed: {qrCodeResponse.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] Error generating QR Code: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Download QR Code image từ URL
        /// </summary>
        private async Task<byte[]?> DownloadQRCodeImage(string imageUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] Downloading QR image from: {imageUrl}");
                
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Không tìm thấy auth_token");
                }

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Không thể tải QR image: {response.StatusCode}");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] QR image downloaded: {imageBytes.Length} bytes");
                
                return imageBytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] Failed to download QR image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu thỏa thuận với chữ ký và QR code
        /// </summary>
        public async Task<PreviewAgreementResponse> SaveAgreementWithSignatureAsync(byte[] signatureBytes)
        {
            try
            {
                IsBusy = true;

                // Tạo QR code trước
                System.Diagnostics.Debug.WriteLine("[AgreementSignatureViewModel] Bắt đầu tạo QR code...");
                var qrCodeBytes = await GenerateAndDownloadQRCodeAsync();
                
                if (qrCodeBytes != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] QR code được tạo thành công: {qrCodeBytes.Length} bytes");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AgreementSignatureViewModel] CẢNH BÁO: QR code không được tạo (null)");
                }

                // Convert to API request
                var apiRequest = await _contractService.ConvertToPreviewAgreementRequestAsync(_agreementInfo);

                // Call API với signature và QR code
                System.Diagnostics.Debug.WriteLine($"[AgreementSignatureViewModel] Gọi API với signature ({signatureBytes.Length} bytes) và QR code ({qrCodeBytes?.Length ?? 0} bytes)");
                var apiResponse = await _contractService.CallPreviewAgreementApiWithSignatureAsync(apiRequest, signatureBytes, qrCodeBytes);

                return apiResponse;
            }
            catch (Exception ex)
            {
                return new PreviewAgreementResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lưu thỏa thuận: {ex.Message}"
                };
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
