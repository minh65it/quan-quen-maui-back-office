using Microsoft.Extensions.Configuration;
using QuanQuenBackOfficeMobile.Services.Interfaces;

namespace QuanQuenBackOfficeMobile.Services;


public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetApiUrl(string serviceName)
    {
        return _configuration[$"API:{serviceName}:BaseUrl"] 
            ?? throw new InvalidOperationException($"Configuration key 'API:{serviceName}:BaseUrl' is missing.");
    }

    public string GetApiPath(string serviceName, string pathName)
    {
        return _configuration[$"API:{serviceName}:{pathName}"] 
            ?? throw new InvalidOperationException($"Configuration key 'API:{serviceName}:{pathName}' is missing.");
    }

    public string GetAssetUrl()
    {
        return _configuration["API:Asset:ImageUrl"] 
            ?? throw new InvalidOperationException("Configuration key 'API:Asset:ImageUrl' is missing.");
    }

    public string GetGoogleMapsApiKey()
    {
        return _configuration["API:GoogleMaps:ApiKey"] 
            ?? throw new InvalidOperationException("Configuration key 'API:GoogleMaps:ApiKey' is missing.");
    }
} 