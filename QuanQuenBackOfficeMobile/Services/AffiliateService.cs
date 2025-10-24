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
                form.Add(new StringContent(request.Latitude?.ToString() ?? ""), "Latitude");
                form.Add(new StringContent(request.Longitude?.ToString() ?? ""), "Longitude");                
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

        public async Task<StoreRecommendationsResponse> GetStoreRecommendationsByLocationAsync(StoreRecommendationsRequest request, string token)
        {
            try
            {
                var baseUrl = _configuration["API:Store:BaseUrl"];
                var path = _configuration["API:Store:GetStoreRecommendation"];
                
                // Add PageSize and PageNumber to URL query parameter
                var queryParams = new List<string>();
                if (request.PageSize.HasValue)
                {
                    queryParams.Add($"PageSize={request.PageSize.Value}");
                }
                if (request.PageNumber.HasValue)
                {
                    queryParams.Add($"PageNumber={request.PageNumber.Value}");
                }
                
                var endpoint = $"{baseUrl}{path}";
                if (queryParams.Any())
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                _logger.LogInformation($"Calling StoreRecommendationsByLocation API: {endpoint}");
                _logger.LogInformation($"Request data: LatUser={request.LatUser}, LonUser={request.LonUser}, PageSize={request.PageSize}, PageNumber={request.PageNumber}");

                // Add Authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Create request without PageSize for JSON body
                var jsonRequest = new
                {
                    latUser = request.LatUser,
                    lonUser = request.LonUser,
                    radius = request.Radius,
                    isRecently = request.IsRecently,
                    isMap = request.IsMap,
                    latPoint1 = request.LatPoint1,
                    lonPoint1 = request.LonPoint1,
                    latPoint2 = request.LatPoint2,
                    lonPoint2 = request.LonPoint2,
                    latPoint3 = request.LatPoint3,
                    lonPoint3 = request.LonPoint3,
                    latPoint4 = request.LatPoint4,
                    lonPoint4 = request.LonPoint4
                };

                var json = JsonSerializer.Serialize(jsonRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                _logger.LogInformation($"Request JSON: {json}");

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Store recommendations response status: {response.StatusCode}");
                _logger.LogInformation($"Store recommendations response content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<StoreRecommendationsResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new StoreRecommendationsResponse { Result = new List<StoreRecommendation>() };
                }
                else
                {
                    _logger.LogError($"Store recommendations failed. Status: {response.StatusCode}, Content: {responseContent}");
                    return new StoreRecommendationsResponse { Result = new List<StoreRecommendation>() };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during get store recommendations");
                return new StoreRecommendationsResponse { Result = new List<StoreRecommendation>() };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during get store recommendations");
                return new StoreRecommendationsResponse { Result = new List<StoreRecommendation>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get store recommendations");
                return new StoreRecommendationsResponse { Result = new List<StoreRecommendation>() };
            }
        }

        public async Task<PreviewCrawlDataAgreementResponse> PreviewCrawlDataAgreementAsync(PreviewCrawlDataAgreementRequest request, string token)
        {
            try
            {
                var baseUrl = _configuration["API:Identity:BaseUrl"];
                var path = _configuration["API:Identity:PreviewDataAgreement"];
                var endpoint = $"{baseUrl}{path}";

                _logger.LogInformation("Calling PreviewCrawlDataAgreement API: {Endpoint}", endpoint);
                _logger.LogInformation("Request data: StoreId={StoreId}, StoreName={StoreName}", request.StoreId, request.StoreName);

                using var formContent = new MultipartFormDataContent();
                formContent.Add(new StringContent(request.StoreId), "StoreId");
                formContent.Add(new StringContent(request.StoreName), "StoreName");
                formContent.Add(new StringContent(request.PhoneNumber), "PhoneNumber");
                formContent.Add(new StringContent(request.Address), "Address");
                formContent.Add(new StringContent(request.RepresentativeName), "RepresentativeName");
                formContent.Add(new StringContent(request.AffiliateName), "AffiliateName");
                formContent.Add(new StringContent(request.AffiliatePhoneNumber), "AffiliatePhoneNumber");
                
                // Add signature files
                if (request.SignatureFile != null && request.SignatureFile.Length > 0)
                {
                    formContent.Add(new ByteArrayContent(request.SignatureFile), "SignatureFile", "store_signature.png");
                }
                else
                {
                    formContent.Add(new StringContent(string.Empty), "SignatureFile");
                }
                
                if (request.SignatureFileAffilate != null && request.SignatureFileAffilate.Length > 0)
                {
                    formContent.Add(new ByteArrayContent(request.SignatureFileAffilate), "SignatureFileAffilate", "affiliate_signature.png");
                }
                else
                {
                    formContent.Add(new StringContent(string.Empty), "SignatureFileAffilate");
                }
                
                // Add QR signature file (optional)
                if (request.SignatureQrFile != null && request.SignatureQrFile.Length > 0)
                {
                    formContent.Add(new ByteArrayContent(request.SignatureQrFile), "SignatureQrFile", "qr_signature.png");
                }
                else
                {
                    formContent.Add(new StringContent(string.Empty), "SignatureQrFile");
                }

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = formContent
                };

                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using var response = await _httpClient.SendAsync(requestMessage);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Preview data agreement response status: {Status}", response.StatusCode);
                _logger.LogInformation("Preview data agreement response content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PreviewCrawlDataAgreementResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new PreviewCrawlDataAgreementResponse();
                }
                else
                {
                    _logger.LogError("Preview data agreement failed. Status: {Status}, Content: {Content}", response.StatusCode, content);
                    return new PreviewCrawlDataAgreementResponse { Success = false };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during preview data agreement");
                return new PreviewCrawlDataAgreementResponse { Success = false };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during preview data agreement");
                return new PreviewCrawlDataAgreementResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during preview data agreement");
                return new PreviewCrawlDataAgreementResponse { Success = false };
            }
        }

        public async Task<AffiliateStatisticsResponse> GetReferralStatisticsAsync(string referralId, string token)
        {
            try
            {
                var baseUrl = _configuration["API:Affiliate:BaseUrl"];
                var path = _configuration["API:Affiliate:GetReferralStatistics"];
                var endpoint = $"{baseUrl}{path}?id={referralId}";

                _logger.LogInformation("Calling GetReferralStatistics API: {Endpoint}", endpoint);
                
                // Add Authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Get referral statistics response status: {Status}", response.StatusCode);
                _logger.LogInformation("Get referral statistics response content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AffiliateStatisticsResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new AffiliateStatisticsResponse { Success = false, Message = "Invalid response format" };
                }
                else
                {
                    _logger.LogError("Get referral statistics failed. Status: {Status}, Content: {Content}", response.StatusCode, responseContent);
                    return new AffiliateStatisticsResponse { Success = false, Message = $"API call failed: {response.StatusCode}" };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during get referral statistics");
                return new AffiliateStatisticsResponse { Success = false, Message = "Lỗi kết nối mạng" };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout during get referral statistics");
                return new AffiliateStatisticsResponse { Success = false, Message = "Hết thời gian chờ" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get referral statistics");
                return new AffiliateStatisticsResponse { Success = false, Message = "Có lỗi xảy ra" };
            }
        }
    }
}
