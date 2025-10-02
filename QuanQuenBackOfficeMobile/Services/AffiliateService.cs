using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Models;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services
{
    public class AffiliateService : IAffiliateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AffiliateService> _logger;
        private readonly IConfiguration _configuration;

        public AffiliateService(HttpClient httpClient, ILogger<AffiliateService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ReferralUserResponse> RegisterReferralUserAsync(ReferralUserRequest request)
        {
            try
            {
                _logger.LogInformation("Starting affiliate registration for: {Email}", request.EmailReferrer);

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var baseUrl = _configuration["API:Affiliate:BaseUrl"];
                var path = _configuration["API:Affiliate:Register"];
                var url = $"{baseUrl}{path}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Affiliate registration successful for: {Email}", request.EmailReferrer);

                    // Parse response
                    var result = JsonSerializer.Deserialize<ReferralUserResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result ?? new ReferralUserResponse
                    {
                        Success = true,
                        Message = "Đăng ký thành công"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Affiliate registration failed. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, errorContent);

                    return new ReferralUserResponse
                    {
                        Success = false,
                        Message = $"Đăng ký thất bại: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during affiliate registration");
                return new ReferralUserResponse
                {
                    Success = false,
                    Message = "Lỗi kết nối mạng. Vui lòng kiểm tra kết nối internet."
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout during affiliate registration");
                return new ReferralUserResponse
                {
                    Success = false,
                    Message = "Hết thời gian chờ. Vui lòng thử lại."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate registration");
                return new ReferralUserResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra. Vui lòng thử lại sau."
                };
            }
        }

        public async Task<ReferralUserResponse> LoginReferralUserAsync(ReferralUserLoginRequest request)
        {
            try
            {
                _logger.LogInformation("Starting affiliate login for: {Email}", request.Email);

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var baseUrl = _configuration["API:Affiliate:BaseUrl"];
                var path = _configuration["API:Affiliate:LoginReferral"];
                var url = $"{baseUrl}{path}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Affiliate login successful for: {Email}", request.Email);
                    _logger.LogInformation("Response content: {Content}", responseContent);

                    // Parse response
                    var result = JsonSerializer.Deserialize<ReferralUserResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Parsed result - Success: {Success}, Data: {Data}, Token: {Token}", result?.Success, result?.Data != null, result?.Data?.Token);

                    return result ?? new ReferralUserResponse
                    {
                        Success = true,
                        Message = "Đăng nhập thành công"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Affiliate login failed. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, errorContent);

                    return new ReferralUserResponse
                    {
                        Success = false,
                        Message = $"Đăng nhập thất bại: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during affiliate login");
                return new ReferralUserResponse
                {
                    Success = false,
                    Message = "Lỗi kết nối mạng. Vui lòng kiểm tra kết nối internet."
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout during affiliate login");
                return new ReferralUserResponse
                {
                    Success = false,
                    Message = "Hết thời gian chờ. Vui lòng thử lại."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate login");
                return new ReferralUserResponse
                {
                    Success = false,
                    Message = "Có lỗi xảy ra. Vui lòng thử lại sau."
                };
            }
        }

        public async Task<StoreUserRegistrationResponse> RegisterAffiliateUserAsync(AffiliateUserRegisterRequest request)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:RegisterPath"];
                var endpoint = $"{baseUrl}{path}";

                // Convert AffiliateUserRegisterRequest to StoreUserRegistrationRequest
                var storeRequest = new StoreUserRegistrationRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    UserName = request.Email, // Use email as username for affiliate
                    // Other fields will be empty strings as required by the model
                    UserId = string.Empty,
                    StoreId = string.Empty,
                    PhoneNumber = string.Empty,
                    Address = string.Empty,
                    Ward = string.Empty,
                    District = string.Empty,
                    Province = string.Empty,
                    WardId = string.Empty,
                    DistrictId = string.Empty,
                    ProvinceId = string.Empty,
                    StoreName = string.Empty,
                    RepresentativeName = string.Empty,
                    BusinessType = string.Empty
                };

                var response = await _httpClient.PostAsJsonAsync(endpoint, storeRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Affiliate user register response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<StoreUserRegistrationResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new StoreUserRegistrationResponse { IsSuccess = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogError($"Affiliate user register failed. Status: {response.StatusCode}, Content: {responseContent}");
                    return new StoreUserRegistrationResponse
                    {
                        IsSuccess = false,
                        Message = $"Register failed: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during affiliate user register");
                return new StoreUserRegistrationResponse
                {
                    IsSuccess = false,
                    Message = "Network error. Please check your connection."
                };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during affiliate user register");
                return new StoreUserRegistrationResponse
                {
                    IsSuccess = false,
                    Message = "Request timeout. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate user register");
                return new StoreUserRegistrationResponse
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred. Please try again."
                };
            }
        }

        public async Task<CreateStoreResponse> CreateAffiliateStoreAsync(AffiliateCreateStoreRequest request, string token)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:CreateStores"];
                var endpoint = $"{baseUrl}{path}";

                // Add authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation($"Added Authorization header with token: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");

                var storeRequest = new StoreCreateRequest
                {
                    StoreName = request.StoreName,
                    StorePhone = request.PhoneNumber,
                    StoreEmail = request.Email,
                    WardId = request.WardId ?? "",
                    DistrictId = request.DistrictId ?? "",
                    CityId = request.ProvinceId ?? "",
                    IsRegisterByCompany = request.BusinessType == "Công Ty",
                    IsRegisterByPersonality = request.BusinessType == "Tư Nhân",
                    Address = request.Address,
                    RepresentativeName = request.RepresentativeName,
                    ProvinceId = request.ProvinceId ?? "",
                    BusinessType = request.BusinessType,
                    CreatedBy = request.CreatedBy
                };

                // Use MultipartFormDataContent like user thường
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(storeRequest.StoreName ?? ""), "StoreName");
                form.Add(new StringContent(storeRequest.StorePhone ?? ""), "StorePhone");
                form.Add(new StringContent(storeRequest.StoreEmail ?? ""), "StoreEmail");
                form.Add(new StringContent(storeRequest.WardId ?? ""), "WardId");
                form.Add(new StringContent(storeRequest.DistrictId ?? ""), "DistrictId");
                form.Add(new StringContent(storeRequest.CityId ?? ""), "CityId");
                form.Add(new StringContent(storeRequest.IsRegisterByCompany.ToString()), "IsRegisterByCompany");
                form.Add(new StringContent(storeRequest.IsRegisterByPersonality.ToString()), "IsRegisterByPersonality");
                form.Add(new StringContent(storeRequest.Address ?? ""), "Address");
                form.Add(new StringContent(storeRequest.RepresentativeName ?? ""), "RepresentativeName");
                form.Add(new StringContent(storeRequest.ProvinceId ?? ""), "ProvinceId");
                form.Add(new StringContent(storeRequest.BusinessType ?? ""), "BusinessType");
                form.Add(new StringContent(storeRequest.CreatedBy ?? ""), "CreatedBy");
                form.Add(new StringContent(request.ReferralId ?? ""), "ReferralId");                
                _logger.LogInformation($"Affiliate create store Request data:");
                _logger.LogInformation($"  Token: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
                _logger.LogInformation($"  CreatedBy: '{request.CreatedBy}' (length: {request.CreatedBy?.Length ?? 0})");
                _logger.LogInformation($"  ReferralId: '{request.ReferralId}' (length: {request.ReferralId?.Length ?? 0})");
                _logger.LogInformation($"  ReferrerId(dup): '{request.ReferralId}' (length: {request.ReferralId?.Length ?? 0})");

                var response = await _httpClient.PostAsync(endpoint, form);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Affiliate store create response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<CreateStoreResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new CreateStoreResponse { IsSuccess = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogError($"Affiliate store create failed. Status: {response.StatusCode}, Content: {responseContent}");
                    return new CreateStoreResponse
                    {
                        IsSuccess = false,
                        Message = $"Create store failed: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during affiliate store create");
                return new CreateStoreResponse
                {
                    IsSuccess = false,
                    Message = "Network error. Please check your connection."
                };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during affiliate store create");
                return new CreateStoreResponse
                {
                    IsSuccess = false,
                    Message = "Request timeout. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate store create");
                return new CreateStoreResponse
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred. Please try again."
                };
            }
        }

        public async Task<ApiResponse> SendAffiliateOtpAsync(string email)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:SendCodeEmailPath"];
                var url = $"{baseUrl}{path}?email={Uri.EscapeDataString(email)}";

                var response = await _httpClient.PostAsync(url, null);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Affiliate send OTP response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return result ?? new ApiResponse { IsSuccess = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogError($"Affiliate send OTP failed. Status: {response.StatusCode}, Content: {responseContent}");
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"Send OTP failed: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during affiliate send OTP");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Network error. Please check your connection."
                };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during affiliate send OTP");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Request timeout. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate send OTP");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred. Please try again."
                };
            }
        }

        public async Task<ApiResponse> VerifyAffiliateOtpAsync(string email, string otpCode)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:VerifyCodeEmailPath"];
                var url = $"{baseUrl}{path}";

                // Không gửi token cho verify OTP, giống user thường
                var request = new { Email = email, Code = otpCode };
                var response = await _httpClient.PutAsJsonAsync(url, request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Affiliate verify OTP response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new ApiResponse { IsSuccess = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogError($"Affiliate verify OTP failed. Status: {response.StatusCode}, Content: {responseContent}");
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"Verify OTP failed: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during affiliate verify OTP");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Network error. Please check your connection."
                };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during affiliate verify OTP");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Request timeout. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during affiliate verify OTP");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred. Please try again."
                };
            }
        }

        public async Task<AffiliateStoreListResponse> GetStoresByReferralIdAsync(string referralId, string token)
        {
            try
            {
                var baseUrl = _configuration["API:Affiliate:BaseUrl"];
                var path = _configuration["API:Affiliate:GetStoresByReferral"];
                var endpoint = $"{baseUrl}{path}?ReferralId={referralId}";

                // Add Authorization header với affiliate token
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Get stores by referral response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AffiliateStoreListResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new AffiliateStoreListResponse { Result = new List<AffiliateStore>() };
                }
                else
                {
                    _logger.LogError($"Get stores by referral failed. Status: {response.StatusCode}, Content: {responseContent}");
                    return new AffiliateStoreListResponse { Result = new List<AffiliateStore>() };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during get stores by referral");
                return new AffiliateStoreListResponse { Result = new List<AffiliateStore>() };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during get stores by referral");
                return new AffiliateStoreListResponse { Result = new List<AffiliateStore>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get stores by referral");
                return new AffiliateStoreListResponse { Result = new List<AffiliateStore>() };
            }
        }
    }
}
