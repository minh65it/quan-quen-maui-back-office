using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.ViewModels.Stores;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace QuanQuenBackOfficeMobile.Services
{
    public class ContractService : IContractService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ContractService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PreviewContractResponse> CallPreviewContractApiAsync(PreviewContractRequest request)
        {
            try
            {
                // Lấy URL từ configuration
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:PreviewContract"];
                var url = $"{baseUrl}{path}";
                
                System.Diagnostics.Debug.WriteLine($"[ContractService] Preview Contract URL: {url}");
                
                // Lấy auth_token từ SecureStorage
                var token = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[ContractService] Token: {token}");
                
                if (string.IsNullOrEmpty(token))
                {
                    return new PreviewContractResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy auth_token. Vui lòng đăng nhập lại."
                    };
                }

                // Thêm Bearer token vào header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Tạo multipart/form-data content
                using var formContent = new MultipartFormDataContent();
                
                // Thêm các trường cơ bản
                formContent.Add(new StringContent(request.StoreId), "StoreId");
                formContent.Add(new StringContent(request.UserId), "UserId");
                formContent.Add(new StringContent(request.CreatedDate.ToString("MM-dd-yyyy")), "CreatedDate");

                // Thêm MainContract fields
                formContent.Add(new StringContent(request.MainContract.BusinessType), "MainContract.BusinessType");
                formContent.Add(new StringContent(request.MainContract.CompanyName), "MainContract.CompanyName");
                formContent.Add(new StringContent(request.MainContract.StoreName), "MainContract.StoreName");
                formContent.Add(new StringContent(request.MainContract.HeadquartersAddress), "MainContract.HeadquartersAddress");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseNumber), "MainContract.BusinessLicenseNumber");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseDate), "MainContract.BusinessLicenseDate");
                formContent.Add(new StringContent(request.MainContract.BusinessLicensePlace), "MainContract.BusinessLicensePlace");
                formContent.Add(new StringContent(request.MainContract.RepresentativeName), "MainContract.RepresentativeName");
                formContent.Add(new StringContent(request.MainContract.RepresentativePosition), "MainContract.RepresentativePosition");
                formContent.Add(new StringContent(request.MainContract.RepresentativeIdNumber), "MainContract.RepresentativeIdNumber");
                formContent.Add(new StringContent(request.MainContract.RepresentativeIdDate), "MainContract.RepresentativeIdDate");
                formContent.Add(new StringContent(request.MainContract.RepresentativeIdPlace), "MainContract.RepresentativeIdPlace");
                formContent.Add(new StringContent(request.MainContract.RepresentativePermanentAddress), "MainContract.RepresentativePermanentAddress");
                formContent.Add(new StringContent(request.MainContract.RepresentativeTemporaryAddress), "MainContract.RepresentativeTemporaryAddress");
                formContent.Add(new StringContent(request.MainContract.StoreHeadOfficeAddress), "MainContract.StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.MainContract.PhoneNumber), "MainContract.PhoneNumber");
                formContent.Add(new StringContent(request.MainContract.BankAccountNumber), "MainContract.BankAccountNumber");
                formContent.Add(new StringContent(request.MainContract.BankAccountHolder), "MainContract.BankAccountHolder");
                formContent.Add(new StringContent(request.MainContract.BankName), "MainContract.BankName");
                formContent.Add(new StringContent(request.MainContract.OwnerName), "MainContract.OwnerName");
                formContent.Add(new StringContent(request.MainContract.OwnerGender), "MainContract.OwnerGender");
                formContent.Add(new StringContent(request.MainContract.OwnerIdNumber), "MainContract.OwnerIdNumber");
                formContent.Add(new StringContent(request.MainContract.OwnerIdDate), "MainContract.OwnerIdDate");
                formContent.Add(new StringContent(request.MainContract.OwnerIdPlace), "MainContract.OwnerIdPlace");
                formContent.Add(new StringContent(request.MainContract.OwnerPermanentAddress), "MainContract.OwnerPermanentAddress");
                formContent.Add(new StringContent(request.MainContract.OwnerTemporaryAddress), "MainContract.OwnerTemporaryAddress");

                // Thêm AppendixContract fields
                formContent.Add(new StringContent(request.AppendixContract.BusinessType), "AppendixContract.BusinessType");
                formContent.Add(new StringContent(request.AppendixContract.AppendixSignDate), "AppendixContract.AppendixSignDate");
                formContent.Add(new StringContent(request.AppendixContract.AppendixNumber), "AppendixContract.AppendixNumber");
                formContent.Add(new StringContent(request.AppendixContract.CompanyName), "AppendixContract.CompanyName");
                formContent.Add(new StringContent(request.AppendixContract.StoreName), "AppendixContract.StoreName");
                formContent.Add(new StringContent(request.AppendixContract.headquartersAddress), "AppendixContract.headquartersAddress");
                formContent.Add(new StringContent(request.AppendixContract.BusinessLicenseNumber), "AppendixContract.BusinessLicenseNumber");
                formContent.Add(new StringContent(request.AppendixContract.BusinessLicenseDate), "AppendixContract.BusinessLicenseDate");
                formContent.Add(new StringContent(request.AppendixContract.BusinessLicensePlace), "AppendixContract.BusinessLicensePlace");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeName), "AppendixContract.RepresentativeName");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativePosition), "AppendixContract.RepresentativePosition");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeIdNumber), "AppendixContract.RepresentativeIdNumber");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeIdDate), "AppendixContract.RepresentativeIdDate");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeIdPlace), "AppendixContract.RepresentativeIdPlace");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativePermanentAddress), "AppendixContract.RepresentativePermanentAddress");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeTemporaryAddress), "AppendixContract.RepresentativeTemporaryAddress");
                formContent.Add(new StringContent(request.AppendixContract.StoreHeadOfficeAddress), "AppendixContract.StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.AppendixContract.PhoneNumber), "AppendixContract.PhoneNumber");
                formContent.Add(new StringContent(request.AppendixContract.BankAccountNumber), "AppendixContract.BankAccountNumber");
                formContent.Add(new StringContent(request.AppendixContract.BankAccountHolder), "AppendixContract.BankAccountHolder");
                formContent.Add(new StringContent(request.AppendixContract.BankName), "AppendixContract.BankName");
                formContent.Add(new StringContent(request.AppendixContract.OwnerName), "AppendixContract.OwnerName");
                formContent.Add(new StringContent(request.AppendixContract.OwnerGender), "AppendixContract.OwnerGender");
                formContent.Add(new StringContent(request.AppendixContract.OwnerIdNumber), "AppendixContract.OwnerIdNumber");
                formContent.Add(new StringContent(request.AppendixContract.OwnerIdDate), "AppendixContract.OwnerIdDate");
                formContent.Add(new StringContent(request.AppendixContract.OwnerIdPlace), "AppendixContract.OwnerIdPlace");
                formContent.Add(new StringContent(request.AppendixContract.OwnerPermanentAddress), "AppendixContract.OwnerPermanentAddress");
                formContent.Add(new StringContent(request.AppendixContract.OwnerTemporaryAddress), "AppendixContract.OwnerTemporaryAddress");

                // Thêm File và File2 (empty)
                formContent.Add(new StringContent(""), "File");
                formContent.Add(new StringContent(""), "File2");

                // Gọi API
                var response = await _httpClient.PostAsync(url, formContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ContractService] API Response: {responseJson}");
                    
                    // Parse JSON response
                    using var jsonDocument = JsonDocument.Parse(responseJson);
                    var root = jsonDocument.RootElement;
                    
                    return new PreviewContractResponse
                    {
                        IsSuccess = true,
                        Message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() ?? "" : "",
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new PreviewContractResponse
                    {
                        IsSuccess = false,
                        Message = $"API Error ({response.StatusCode}): {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PreviewContractResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi gọi API PreviewContract: {ex.Message}"
                };
            }
        }

        public async Task<PreviewContractRequest> ConvertToPreviewContractRequestAsync(StoreInformationContractViewModel viewModel)
        {
            var storeId = StoreRegistrationData.StoreId ?? "";
            var userId = StoreRegistrationData.StoreUserId ?? "";
            
            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(userId))
            {
                throw new Exception("Thiếu thông tin StoreId hoặc UserId. Vui lòng hoàn tất đăng ký trước.");
            }

            return await Task.FromResult(new PreviewContractRequest
            {
                StoreId = storeId,
                UserId = userId,
                MainContract = new MainContractDto
                {
                    BusinessType = viewModel.SelectedBusinessType ?? "",
                    CompanyName = viewModel.CompanyName ?? "",
                    StoreName = viewModel.StoreName ?? "",
                    HeadquartersAddress = viewModel.StoreHeadOfficeAddress ?? "",
                    BusinessLicenseNumber = viewModel.BusinessLicenseNumber ?? "",
                    BusinessLicenseDate = viewModel.BusinessLicenseDate ?? "",
                    BusinessLicensePlace = viewModel.BusinessLicensePlace ?? "",
                    RepresentativeName = viewModel.RepresentativeName ?? "",
                    RepresentativePosition = viewModel.RepresentativePosition ?? "",
                    RepresentativeIdNumber = viewModel.RepresentativeIdNumber ?? "",
                    RepresentativeIdDate = viewModel.RepresentativeIdDate ?? "",
                    RepresentativeIdPlace = viewModel.RepresentativeIdPlace ?? "",
                    RepresentativePermanentAddress = viewModel.RepresentativePermanentAddress ?? "",
                    RepresentativeTemporaryAddress = viewModel.RepresentativeTemporaryAddress ?? "",
                    StoreHeadOfficeAddress = viewModel.StoreHeadOfficeAddress ?? "",
                    PhoneNumber = viewModel.PhoneNumber ?? "",
                    BankAccountNumber = viewModel.BankAccountNumber ?? "",
                    BankAccountHolder = viewModel.BankAccountHolder ?? "",
                    BankName = viewModel.BankName ?? "",
                    
                    // Thông tin chủ hộ kinh doanh
                    OwnerName = viewModel.OwnerName ?? "",
                    OwnerGender = viewModel.OwnerGender ?? "",
                    OwnerIdNumber = viewModel.OwnerIdNumber ?? "",
                    OwnerIdDate = viewModel.OwnerIdDate ?? "",
                    OwnerIdPlace = viewModel.OwnerIdPlace ?? "",
                    OwnerPermanentAddress = viewModel.OwnerPermanentAddress ?? "",
                    OwnerTemporaryAddress = viewModel.OwnerTemporaryAddress ?? ""
                },
                AppendixContract = new AppendixContractDto
                {
                    BusinessType = viewModel.SelectedBusinessType ?? "",
                    AppendixSignDate = "dfsfsdfsdf",
                    AppendixNumber = "adfsdf",
                    CompanyName = viewModel.CompanyName ?? "",
                    StoreName = viewModel.StoreName ?? "",
                    headquartersAddress = viewModel.StoreHeadOfficeAddress ?? "", 
                    BusinessLicenseNumber = viewModel.BusinessLicenseNumber ?? "",
                    BusinessLicenseDate = viewModel.BusinessLicenseDate ?? "",
                    BusinessLicensePlace = viewModel.BusinessLicensePlace ?? "",
                    RepresentativeName = viewModel.RepresentativeName ?? "",
                    RepresentativePosition = viewModel.RepresentativePosition ?? "",
                    RepresentativeIdNumber = viewModel.RepresentativeIdNumber ?? "",
                    RepresentativeIdDate = viewModel.RepresentativeIdDate ?? "",
                    RepresentativeIdPlace = viewModel.RepresentativeIdPlace ?? "",
                    RepresentativePermanentAddress = viewModel.RepresentativePermanentAddress ?? "",
                    RepresentativeTemporaryAddress = viewModel.RepresentativeTemporaryAddress ?? "",
                    StoreHeadOfficeAddress = viewModel.StoreHeadOfficeAddress ?? "",
                    PhoneNumber = viewModel.PhoneNumber ?? "",
                    BankAccountNumber = viewModel.BankAccountNumber ?? "",
                    BankAccountHolder = viewModel.BankAccountHolder ?? "",
                    BankName = viewModel.BankName ?? "",
                    
                    // Thông tin chủ hộ kinh doanh
                    OwnerName = viewModel.OwnerName ?? "",
                    OwnerGender = viewModel.OwnerGender ?? "",
                    OwnerIdNumber = viewModel.OwnerIdNumber ?? "",
                    OwnerIdDate = viewModel.OwnerIdDate ?? "",
                    OwnerIdPlace = viewModel.OwnerIdPlace ?? "",
                    OwnerPermanentAddress = viewModel.OwnerPermanentAddress ?? "",
                    OwnerTemporaryAddress = viewModel.OwnerTemporaryAddress ?? ""
                },
                CreatedDate = DateTime.Now,
                File = null,
                File2 = null
            });
        }

        public async Task<PreviewContractResponse> CallPreviewContractApiWithSignaturesAsync(PreviewContractRequest request, byte[] contractSignature, byte[] appendixSignature)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:PreviewContract"];
                var url = $"{baseUrl}{path}";
                
                System.Diagnostics.Debug.WriteLine($"[ContractService] Preview Contract with Signatures URL: {url}");
                
                var token = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[ContractService] Token: {token}");
                
                if (string.IsNullOrEmpty(token))
                {
                    return new PreviewContractResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy auth_token. Vui lòng đăng nhập lại."
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var formContent = new MultipartFormDataContent();
                
                // Add basic fields
                formContent.Add(new StringContent(request.StoreId), "StoreId");
                formContent.Add(new StringContent(request.UserId), "UserId");
                formContent.Add(new StringContent(request.CreatedDate.ToString("MM-dd-yyyy")), "CreatedDate");

                // Add MainContract fields
                formContent.Add(new StringContent(request.MainContract.BusinessType), "MainContract.BusinessType");
                formContent.Add(new StringContent(request.MainContract.CompanyName), "MainContract.CompanyName");
                formContent.Add(new StringContent(request.MainContract.StoreName), "MainContract.StoreName");
                formContent.Add(new StringContent(request.MainContract.HeadquartersAddress), "MainContract.HeadquartersAddress");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseNumber), "MainContract.BusinessLicenseNumber");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseDate), "MainContract.BusinessLicenseDate");
                formContent.Add(new StringContent(request.MainContract.BusinessLicensePlace), "MainContract.BusinessLicensePlace");
                formContent.Add(new StringContent(request.MainContract.RepresentativeName), "MainContract.RepresentativeName");
                formContent.Add(new StringContent(request.MainContract.RepresentativePosition), "MainContract.RepresentativePosition");
                formContent.Add(new StringContent(request.MainContract.RepresentativeIdNumber), "MainContract.RepresentativeIdNumber");
                formContent.Add(new StringContent(request.MainContract.RepresentativeIdDate), "MainContract.RepresentativeIdDate");
                formContent.Add(new StringContent(request.MainContract.RepresentativeIdPlace), "MainContract.RepresentativeIdPlace");
                formContent.Add(new StringContent(request.MainContract.RepresentativePermanentAddress), "MainContract.RepresentativePermanentAddress");
                formContent.Add(new StringContent(request.MainContract.RepresentativeTemporaryAddress), "MainContract.RepresentativeTemporaryAddress");
                formContent.Add(new StringContent(request.MainContract.StoreHeadOfficeAddress), "MainContract.StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.MainContract.PhoneNumber), "MainContract.PhoneNumber");
                formContent.Add(new StringContent(request.MainContract.BankAccountNumber), "MainContract.BankAccountNumber");
                formContent.Add(new StringContent(request.MainContract.BankAccountHolder), "MainContract.BankAccountHolder");
                formContent.Add(new StringContent(request.MainContract.BankName), "MainContract.BankName");
                formContent.Add(new StringContent(request.MainContract.OwnerName), "MainContract.OwnerName");
                formContent.Add(new StringContent(request.MainContract.OwnerGender), "MainContract.OwnerGender");
                formContent.Add(new StringContent(request.MainContract.OwnerIdNumber), "MainContract.OwnerIdNumber");
                formContent.Add(new StringContent(request.MainContract.OwnerIdDate), "MainContract.OwnerIdDate");
                formContent.Add(new StringContent(request.MainContract.OwnerIdPlace), "MainContract.OwnerIdPlace");
                formContent.Add(new StringContent(request.MainContract.OwnerPermanentAddress), "MainContract.OwnerPermanentAddress");
                formContent.Add(new StringContent(request.MainContract.OwnerTemporaryAddress), "MainContract.OwnerTemporaryAddress");

                // Add AppendixContract fields
                formContent.Add(new StringContent(request.AppendixContract.BusinessType), "AppendixContract.BusinessType");
                formContent.Add(new StringContent(request.AppendixContract.AppendixSignDate), "AppendixContract.AppendixSignDate");
                formContent.Add(new StringContent(request.AppendixContract.AppendixNumber), "AppendixContract.AppendixNumber");
                formContent.Add(new StringContent(request.AppendixContract.CompanyName), "AppendixContract.CompanyName");
                formContent.Add(new StringContent(request.AppendixContract.StoreName), "AppendixContract.StoreName");
                formContent.Add(new StringContent(request.AppendixContract.headquartersAddress), "AppendixContract.headquartersAddress");
                formContent.Add(new StringContent(request.AppendixContract.BusinessLicenseNumber), "AppendixContract.BusinessLicenseNumber");
                formContent.Add(new StringContent(request.AppendixContract.BusinessLicenseDate), "AppendixContract.BusinessLicenseDate");
                formContent.Add(new StringContent(request.AppendixContract.BusinessLicensePlace), "AppendixContract.BusinessLicensePlace");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeName), "AppendixContract.RepresentativeName");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativePosition), "AppendixContract.RepresentativePosition");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeIdNumber), "AppendixContract.RepresentativeIdNumber");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeIdDate), "AppendixContract.RepresentativeIdDate");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeIdPlace), "AppendixContract.RepresentativeIdPlace");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativePermanentAddress), "AppendixContract.RepresentativePermanentAddress");
                formContent.Add(new StringContent(request.AppendixContract.RepresentativeTemporaryAddress), "AppendixContract.RepresentativeTemporaryAddress");
                formContent.Add(new StringContent(request.AppendixContract.StoreHeadOfficeAddress), "AppendixContract.StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.AppendixContract.PhoneNumber), "AppendixContract.PhoneNumber");
                formContent.Add(new StringContent(request.AppendixContract.BankAccountNumber), "AppendixContract.BankAccountNumber");
                formContent.Add(new StringContent(request.AppendixContract.BankAccountHolder), "AppendixContract.BankAccountHolder");
                formContent.Add(new StringContent(request.AppendixContract.BankName), "AppendixContract.BankName");
                formContent.Add(new StringContent(request.AppendixContract.OwnerName), "AppendixContract.OwnerName");
                formContent.Add(new StringContent(request.AppendixContract.OwnerGender), "AppendixContract.OwnerGender");
                formContent.Add(new StringContent(request.AppendixContract.OwnerIdNumber), "AppendixContract.OwnerIdNumber");
                formContent.Add(new StringContent(request.AppendixContract.OwnerIdDate), "AppendixContract.OwnerIdDate");
                formContent.Add(new StringContent(request.AppendixContract.OwnerIdPlace), "AppendixContract.OwnerIdPlace");
                formContent.Add(new StringContent(request.AppendixContract.OwnerPermanentAddress), "AppendixContract.OwnerPermanentAddress");
                formContent.Add(new StringContent(request.AppendixContract.OwnerTemporaryAddress), "AppendixContract.OwnerTemporaryAddress");

                // Add signature files
                if (contractSignature != null && contractSignature.Length > 0)
                {
                    var contractImageContent = new ByteArrayContent(contractSignature);
                    contractImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formContent.Add(contractImageContent, "File", $"contract_signature_{DateTime.Now:yyyyMMddHHmmss}.jpg");
                    System.Diagnostics.Debug.WriteLine($"[ContractService] Added contract signature: {contractSignature.Length} bytes");
                }
                else
                {
                    formContent.Add(new StringContent(""), "File");
                }

                if (appendixSignature != null && appendixSignature.Length > 0)
                {
                    var appendixImageContent = new ByteArrayContent(appendixSignature);
                    appendixImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formContent.Add(appendixImageContent, "File2", $"appendix_signature_{DateTime.Now:yyyyMMddHHmmss}.jpg");
                    System.Diagnostics.Debug.WriteLine($"[ContractService] Added appendix signature: {appendixSignature.Length} bytes");
                }
                else
                {
                    formContent.Add(new StringContent(""), "File2");
                }

                var response = await _httpClient.PostAsync(url, formContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ContractService] API Response with signatures: {responseJson}");
                    
                    using var jsonDocument = System.Text.Json.JsonDocument.Parse(responseJson);
                    var root = jsonDocument.RootElement;
                    
                    return new PreviewContractResponse
                    {
                        IsSuccess = root.TryGetProperty("success", out var successElement) ? successElement.GetBoolean() : false,
                        Message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() ?? "" : "",
                        ContractId = root.TryGetProperty("contractId", out var contractIdElement) ? contractIdElement.GetString() ?? "" : "",
                        ContractNumber = root.TryGetProperty("contractNumber", out var contractNumberElement) ? contractNumberElement.GetString() ?? "" : ""
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new PreviewContractResponse
                    {
                        IsSuccess = false,
                        Message = $"API Error ({response.StatusCode}): {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PreviewContractResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi gọi API PreviewContract với chữ ký: {ex.Message}"
                };
            }
        }

    }
} 