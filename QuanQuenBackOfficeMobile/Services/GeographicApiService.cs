using QuanQuenBackOfficeMobile.Services.Interfaces;


namespace QuanQuenBackOfficeMobile.Services;

public class GeographicApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _provincePath;
    private readonly string _districtPath;
    private readonly string _wardPath;

    public GeographicApiService(IConfigurationService configService)
    {
        var baseUrl = configService.GetApiUrl("Geographic");
        _provincePath = configService.GetApiPath("Geographic", "GetProvincePath");
        _districtPath = configService.GetApiPath("Geographic", "GetDistrictPath");
        _wardPath = configService.GetApiPath("Geographic", "GetWardPath");
        _clientId = "GeographicAPI";
        _clientSecret = "ASJD2KAZ31XN1CBQWE";

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Add("clientId", _clientId);
        _httpClient.DefaultRequestHeaders.Add("clientSecret", _clientSecret);
    }

    public async Task<List<ProvinceDto>> GetProvincesAsync()
    {
        var response = await _httpClient.GetAsync(_provincePath);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ProvinceDto>>(json) ?? new();
    }

    public async Task<List<DistrictDto>> GetDistrictsAsync(string zoneId)
    {
        var url = $"{_districtPath}?zoneId={zoneId}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DistrictDto>>(json) ?? new();
    }

    public async Task<List<WardDto>> GetWardsAsync(string territoryId)
    {
        var url = $"{_wardPath}?territoryId={territoryId}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<WardDto>>(json) ?? new();
    }
}

// DTOs mapping đúng với response thực tế
public class ProvinceDto
{
    public string zoneId { get; set; }
    public string zoneName { get; set; }
    public string Name => zoneName;
}
public class DistrictDto
{
    public string territoryId { get; set; }
    public string territoryName { get; set; }
    public string Name => territoryName;
}
public class WardDto
{
    public string stateId { get; set; }
    public string stateName { get; set; }
    public string Name => stateName;
} 