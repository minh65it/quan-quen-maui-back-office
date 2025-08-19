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

                // Thêm MainContract fields - THÊM PROVIDER FIELDS
                // Bên Cung Cấp Dịch Vụ
                formContent.Add(new StringContent(request.MainContract.ProviderRepresentativeIdNumber), "MainContract.ProviderRepresentativeIdNumber");
                formContent.Add(new StringContent(request.MainContract.ProviderRepresentativeIdDate), "MainContract.ProviderRepresentativeIdDate");
                formContent.Add(new StringContent(request.MainContract.ProviderRepresentativeIdPlace), "MainContract.ProviderRepresentativeIdPlace");
                
                // Debug logging cho Provider fields
                System.Diagnostics.Debug.WriteLine($"[ContractService] ProviderRepresentativeIdNumber: {request.MainContract.ProviderRepresentativeIdNumber}");
                System.Diagnostics.Debug.WriteLine($"[ContractService] ProviderRepresentativeIdDate: {request.MainContract.ProviderRepresentativeIdDate}");
                System.Diagnostics.Debug.WriteLine($"[ContractService] ProviderRepresentativeIdPlace: {request.MainContract.ProviderRepresentativeIdPlace}");
                
                // Bên Thuê Dịch Vụ
                formContent.Add(new StringContent(request.MainContract.BusinessType), "MainContract.BusinessType");
                formContent.Add(new StringContent(request.MainContract.CompanyName), "MainContract.CompanyName");
                formContent.Add(new StringContent(request.MainContract.StoreName), "MainContract.StoreName");
                formContent.Add(new StringContent(request.MainContract.HeadquartersAddress), "MainContract.HeadquartersAddress");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseNumber), "MainContract.BusinessLicenseNumber");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseDate), "MainContract.BusinessLicenseDate");
                formContent.Add(new StringContent(request.MainContract.BusinessLicensePlace), "MainContract.BusinessLicensePlace");
                formContent.Add(new StringContent(request.MainContract.StoreHeadOfficeAddress), "MainContract.StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.MainContract.PhoneNumber), "MainContract.PhoneNumber");
                
                // Thông tin đại diện - sử dụng cấu trúc flat như backend expect
                formContent.Add(new StringContent(request.MainContract.Representative.Name), "MainContract.RepresentativeName");
                formContent.Add(new StringContent(request.MainContract.Representative.Position), "MainContract.RepresentativePosition");
                formContent.Add(new StringContent(request.MainContract.Representative.IdNumber), "MainContract.RepresentativeIdNumber");
                formContent.Add(new StringContent(request.MainContract.Representative.IdDate), "MainContract.RepresentativeIdDate");
                formContent.Add(new StringContent(request.MainContract.Representative.IdPlace), "MainContract.RepresentativeIdPlace");
                formContent.Add(new StringContent(request.MainContract.Representative.PermanentAddress), "MainContract.RepresentativePermanentAddress");
                formContent.Add(new StringContent(request.MainContract.Representative.TemporaryAddress), "MainContract.RepresentativeTemporaryAddress");
                
                // Thông tin chủ hộ kinh doanh - sử dụng cấu trúc flat như backend expect
                formContent.Add(new StringContent(request.MainContract.Owner.Name), "MainContract.OwnerName");
                formContent.Add(new StringContent(request.MainContract.Owner.Gender), "MainContract.OwnerGender");
                formContent.Add(new StringContent(request.MainContract.Owner.IdNumber), "MainContract.OwnerIdNumber");
                formContent.Add(new StringContent(request.MainContract.Owner.IdDate), "MainContract.OwnerIdDate");
                formContent.Add(new StringContent(request.MainContract.Owner.IdPlace), "MainContract.OwnerIdPlace");
                formContent.Add(new StringContent(request.MainContract.Owner.PermanentAddress), "MainContract.OwnerPermanentAddress");
                formContent.Add(new StringContent(request.MainContract.Owner.TemporaryAddress), "MainContract.OwnerTemporaryAddress");
                
                // Field riêng của MainContract
                formContent.Add(new StringContent(request.MainContract.BankAccountNumber), "MainContract.BankAccountNumber");
                formContent.Add(new StringContent(request.MainContract.BankAccountHolder), "MainContract.BankAccountHolder");
                formContent.Add(new StringContent(request.MainContract.BankName), "MainContract.BankName");

                // Thêm AppendixContract fields - CHỈ 2 FIELDS NHƯ CURL EXAMPLE
                formContent.Add(new StringContent(request.AppendixContract.AppendixSignDate), "AppendixContract.AppendixSignDate");
                formContent.Add(new StringContent(request.AppendixContract.AppendixNumber), "AppendixContract.AppendixNumber");

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
            var storeId = StoreRegistrationData.StoreId ;
            var userId = StoreRegistrationData.StoreUserId ;
            
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
                    // Bên Cung Cấp Dịch Vụ
                    ProviderRepresentativeIdNumber = viewModel.ProviderRepresentativeIdNumber ?? "",
                    ProviderRepresentativeIdDate = viewModel.ProviderRepresentativeIdDate ?? "",
                    ProviderRepresentativeIdPlace = viewModel.ProviderRepresentativeIdPlace ?? "",
                    
                    // Bên Thuê Dịch Vụ
                    BusinessType = StoreRegistrationData.BusinessType ?? "",
                    CompanyName = viewModel.CompanyName ?? "",
                    StoreName = viewModel.StoreName ?? "",
                    HeadquartersAddress = viewModel.HeadquartersAddress ?? "",
                    BusinessLicenseNumber = viewModel.BusinessLicenseNumber ?? "",
                    BusinessLicenseDate = viewModel.BusinessLicenseDate ?? "",
                    BusinessLicensePlace = viewModel.BusinessLicensePlace ?? "",
                    StoreHeadOfficeAddress = viewModel.StoreHeadOfficeAddress ?? "",
                    PhoneNumber = viewModel.PhoneNumber ?? "",
                    
                    // Thông tin đại diện - sử dụng cấu trúc mới
                    Representative = new PersonalInfoDto
                    {
                        Name = viewModel.RepresentativeName ?? "",
                        Position = viewModel.RepresentativePosition ?? "",
                        IdNumber = viewModel.RepresentativeIdNumber ?? "",
                        IdDate = viewModel.RepresentativeIdDate ?? "",
                        IdPlace = viewModel.RepresentativeIdPlace ?? "",
                        PermanentAddress = viewModel.RepresentativePermanentAddress ?? "",
                        TemporaryAddress = viewModel.RepresentativeTemporaryAddress ?? ""
                    },
                    
                    // Thông tin chủ hộ kinh doanh - sử dụng cấu trúc mới
                    Owner = new PersonalInfoDto
                    {
                        Name = viewModel.OwnerName ?? "",
                        Gender = viewModel.OwnerGender ?? "",
                        IdNumber = viewModel.OwnerIdNumber ?? "",
                        IdDate = viewModel.OwnerIdDate ?? "",
                        IdPlace = viewModel.OwnerIdPlace ?? "",
                        PermanentAddress = viewModel.OwnerPermanentAddress ?? "",
                        TemporaryAddress = viewModel.OwnerTemporaryAddress ?? ""
                    },
                    
                    // Field riêng của MainContract
                    BankAccountNumber = viewModel.BankAccountNumber ?? "",
                    BankAccountHolder = viewModel.BankAccountHolder ?? "",
                    BankName = viewModel.BankName ?? ""
                },
                AppendixContract = new AppendixContractDto
                {
                    // CHỈ 2 FIELDS NHƯ CURL EXAMPLE
                    AppendixSignDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    AppendixNumber = $"PL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}"
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

                // Add MainContract fields - MATCH VỚI CURL EXAMPLE
                // Bên Cung Cấp Dịch Vụ
                formContent.Add(new StringContent(request.MainContract.ProviderRepresentativeIdNumber), "MainContract.ProviderRepresentativeIdNumber");
                formContent.Add(new StringContent(request.MainContract.ProviderRepresentativeIdDate), "MainContract.ProviderRepresentativeIdDate");
                formContent.Add(new StringContent(request.MainContract.ProviderRepresentativeIdPlace), "MainContract.ProviderRepresentativeIdPlace");
                
                // Debug logging cho Provider fields
                System.Diagnostics.Debug.WriteLine($"[ContractService] WithSignatures - ProviderRepresentativeIdNumber: {request.MainContract.ProviderRepresentativeIdNumber}");
                System.Diagnostics.Debug.WriteLine($"[ContractService] WithSignatures - ProviderRepresentativeIdDate: {request.MainContract.ProviderRepresentativeIdDate}");
                System.Diagnostics.Debug.WriteLine($"[ContractService] WithSignatures - ProviderRepresentativeIdPlace: {request.MainContract.ProviderRepresentativeIdPlace}");
                
                // Bên Thuê Dịch Vụ
                formContent.Add(new StringContent(request.MainContract.BusinessType), "MainContract.BusinessType");
                formContent.Add(new StringContent(request.MainContract.CompanyName), "MainContract.CompanyName");
                formContent.Add(new StringContent(request.MainContract.StoreName), "MainContract.StoreName");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseNumber), "MainContract.BusinessLicenseNumber");
                formContent.Add(new StringContent(request.MainContract.BusinessLicenseDate), "MainContract.BusinessLicenseDate");
                formContent.Add(new StringContent(request.MainContract.BusinessLicensePlace), "MainContract.BusinessLicensePlace");
                formContent.Add(new StringContent(request.MainContract.StoreHeadOfficeAddress), "MainContract.StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.MainContract.PhoneNumber), "MainContract.PhoneNumber");
                
                // Thông tin đại diện - sử dụng cấu trúc flat như backend expect
                formContent.Add(new StringContent(request.MainContract.Representative.Name), "MainContract.RepresentativeName");
                formContent.Add(new StringContent(request.MainContract.Representative.Position), "MainContract.RepresentativePosition");
                formContent.Add(new StringContent(request.MainContract.Representative.IdNumber), "MainContract.RepresentativeIdNumber");
                formContent.Add(new StringContent(request.MainContract.Representative.IdDate), "MainContract.RepresentativeIdDate");
                formContent.Add(new StringContent(request.MainContract.Representative.IdPlace), "MainContract.RepresentativeIdPlace");
                formContent.Add(new StringContent(request.MainContract.Representative.PermanentAddress), "MainContract.RepresentativePermanentAddress");
                formContent.Add(new StringContent(request.MainContract.Representative.TemporaryAddress), "MainContract.RepresentativeTemporaryAddress");
                
                // Thông tin chủ hộ kinh doanh - sử dụng cấu trúc flat như backend expect
                formContent.Add(new StringContent(request.MainContract.Owner.Name), "MainContract.OwnerName");
                formContent.Add(new StringContent(request.MainContract.Owner.Gender), "MainContract.OwnerGender");
                formContent.Add(new StringContent(request.MainContract.Owner.IdNumber), "MainContract.OwnerIdNumber");
                formContent.Add(new StringContent(request.MainContract.Owner.IdDate), "MainContract.OwnerIdDate");
                formContent.Add(new StringContent(request.MainContract.Owner.IdPlace), "MainContract.OwnerIdPlace");
                formContent.Add(new StringContent(request.MainContract.Owner.PermanentAddress), "MainContract.OwnerPermanentAddress");
                formContent.Add(new StringContent(request.MainContract.Owner.TemporaryAddress), "MainContract.OwnerTemporaryAddress");
                
                // Field riêng của MainContract
                formContent.Add(new StringContent(request.MainContract.BankAccountNumber), "MainContract.BankAccountNumber");
                formContent.Add(new StringContent(request.MainContract.BankAccountHolder), "MainContract.BankAccountHolder");
                formContent.Add(new StringContent(request.MainContract.BankName), "MainContract.BankName");

                // Add AppendixContract fields - CHỈ 2 FIELDS NHƯ CURL EXAMPLE
                formContent.Add(new StringContent(request.AppendixContract.AppendixSignDate), "AppendixContract.AppendixSignDate");
                formContent.Add(new StringContent(request.AppendixContract.AppendixNumber), "AppendixContract.AppendixNumber");

                // Add signature files - MATCH VỚI CURL EXAMPLE
                if (contractSignature != null && contractSignature.Length > 0)
                {
                    var contractImageContent = new ByteArrayContent(contractSignature);
                    contractImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formContent.Add(contractImageContent, "File", "contract_signature.jpg");
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
                    formContent.Add(appendixImageContent, "File2", "appendix_signature.jpg");
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
                    Message = $"Lỗi khi gọi API PreviewContract với signatures: {ex.Message}"
                };
            }
        }

        // ========== AGREEMENT API METHODS ==========

        public async Task<PreviewAgreementResponse> CallPreviewAgreementApiAsync(PreviewAgreementRequest request)
        {
            try
            {
                // Lấy URL từ configuration
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:PreviewAgreement"];
                var url = $"{baseUrl}{path}";
                
                System.Diagnostics.Debug.WriteLine($"[ContractService] Preview Agreement URL: {url}");
                
                // Lấy auth_token từ SecureStorage
                var token = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[ContractService] Token: {token}");
                
                if (string.IsNullOrEmpty(token))
                {
                    return new PreviewAgreementResponse
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
                formContent.Add(new StringContent(request.SignatureFile), "SignatureFile");
                formContent.Add(new StringContent(request.StoreName), "StoreName");
                formContent.Add(new StringContent(request.RepresentativeName), "RepresentativeName");
                formContent.Add(new StringContent(request.RepresentativePosition), "RepresentativePosition");
                formContent.Add(new StringContent(request.RepresentativeIdNumber), "RepresentativeIdNumber");
                formContent.Add(new StringContent(request.StoreHeadOfficeAddress), "StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.PhoneNumber), "PhoneNumber");

                // Gọi API
                var response = await _httpClient.PostAsync(url, formContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ContractService] Agreement API Response: {responseJson}");
                    
                    // Parse JSON response
                    using var jsonDocument = JsonDocument.Parse(responseJson);
                    var root = jsonDocument.RootElement;
                    
                    return new PreviewAgreementResponse
                    {
                        IsSuccess = true,
                        Message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() ?? "" : "",
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new PreviewAgreementResponse
                    {
                        IsSuccess = false,
                        Message = $"API Error ({response.StatusCode}): {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PreviewAgreementResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi gọi API PreviewAgreement: {ex.Message}"
                };
            }
        }

        public async Task<PreviewAgreementRequest> ConvertToPreviewAgreementRequestAsync(AgreementInfoDto agreementInfo)
        {
            // Lấy StoreId và UserId từ StoreRegistrationData
            var storeId = StoreRegistrationData.StoreId;
            var userId = StoreRegistrationData.StoreUserId;
            
            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(userId))
            {
                throw new Exception("Thiếu thông tin StoreId hoặc UserId. Vui lòng hoàn tất đăng ký trước.");
            }
            
            return await Task.FromResult(new PreviewAgreementRequest
            {
                StoreId = storeId,
                UserId = userId,
                SignatureFile = string.Empty,
                StoreName = agreementInfo.StoreName,
                RepresentativeName = agreementInfo.RepresentativeName,
                RepresentativePosition = agreementInfo.RepresentativePosition,
                RepresentativeIdNumber = agreementInfo.RepresentativeIdNumber,
                StoreHeadOfficeAddress = agreementInfo.StoreHeadOfficeAddress,
                PhoneNumber = agreementInfo.PhoneNumber
            });
        }

        public async Task<PreviewAgreementResponse> CallPreviewAgreementApiWithSignatureAsync(PreviewAgreementRequest request, byte[] signatureFile)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:PreviewAgreement"];
                var url = $"{baseUrl}{path}";
                
                System.Diagnostics.Debug.WriteLine($"[ContractService] Preview Agreement with Signature URL: {url}");
                
                var token = await SecureStorage.GetAsync("auth_token");
                System.Diagnostics.Debug.WriteLine($"[ContractService] Token: {token}");
                
                if (string.IsNullOrEmpty(token))
                {
                    return new PreviewAgreementResponse
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
                formContent.Add(new StringContent(request.StoreName), "StoreName");
                formContent.Add(new StringContent(request.RepresentativeName), "RepresentativeName");
                formContent.Add(new StringContent(request.RepresentativePosition), "RepresentativePosition");
                formContent.Add(new StringContent(request.RepresentativeIdNumber), "RepresentativeIdNumber");
                formContent.Add(new StringContent(request.StoreHeadOfficeAddress), "StoreHeadOfficeAddress");
                formContent.Add(new StringContent(request.PhoneNumber), "PhoneNumber");

                // Add signature file
                if (signatureFile != null && signatureFile.Length > 0)
                {
                    var signatureImageContent = new ByteArrayContent(signatureFile);
                    signatureImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formContent.Add(signatureImageContent, "SignatureFile", "agreement_signature.jpg");
                    System.Diagnostics.Debug.WriteLine($"[ContractService] Added agreement signature: {signatureFile.Length} bytes");
                }
                else
                {
                    formContent.Add(new StringContent(""), "SignatureFile");
                }

                var response = await _httpClient.PostAsync(url, formContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ContractService] Agreement API Response with signature: {responseJson}");
                    
                    using var jsonDocument = System.Text.Json.JsonDocument.Parse(responseJson);
                    var root = jsonDocument.RootElement;
                    
                    return new PreviewAgreementResponse
                    {
                        IsSuccess = root.TryGetProperty("success", out var successElement) ? successElement.GetBoolean() : false,
                        Message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() ?? "" : "",
                        AgreementId = root.TryGetProperty("agreementId", out var agreementIdElement) ? agreementIdElement.GetString() ?? "" : "",
                        AgreementNumber = root.TryGetProperty("agreementNumber", out var agreementNumberElement) ? agreementNumberElement.GetString() ?? "" : ""
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new PreviewAgreementResponse
                    {
                        IsSuccess = false,
                        Message = $"API Error ({response.StatusCode}): {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PreviewAgreementResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi gọi API PreviewAgreement với signature: {ex.Message}"
                };
            }
        }

    }
} 