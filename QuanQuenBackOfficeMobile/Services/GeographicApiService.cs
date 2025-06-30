using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace QuanQuenBackOfficeMobile.Services;

public class GeographicApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _provincePath;
    private readonly string _districtPath;
    private readonly string _wardPath;

    public GeographicApiService(IConfiguration config)
    {
        var geoConfig = config.GetSection("API:Geographic");
        var baseUrl = geoConfig["BaseUrl"] ?? throw new Exception("Missing Geographic BaseUrl");
        _provincePath = geoConfig["GetProvincePath"] ?? throw new Exception("Missing GetProvincePath");
        _districtPath = geoConfig["GetDistrictPath"] ?? throw new Exception("Missing GetDistrictPath");
        _wardPath = geoConfig["GetWardPath"] ?? throw new Exception("Missing GetWardPath");
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
    public string Name => zoneName; // Để binding cho Picker
}
public class DistrictDto
{
    public string territoryId { get; set; }
    public string territoryName { get; set; }
    public string Name => territoryName; // Để binding cho Picker
}
public class WardDto
{
    public string stateId { get; set; }
    public string stateName { get; set; }
    public string Name => stateName; // Để binding cho Picker
} 