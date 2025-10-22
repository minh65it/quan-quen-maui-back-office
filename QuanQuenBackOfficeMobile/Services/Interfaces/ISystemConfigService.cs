using System.Text.Json.Serialization;

namespace QuanQuenBackOfficeMobile.Services.Interfaces;

public interface ISystemConfigService
{
    Task<SystemConfigResponse> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    Task<SystemConfigListResponse> GetValuesAsListAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
}

public sealed class SystemConfigResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public sealed class SystemConfigListResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, List<string>> Data { get; set; } = new();
}


