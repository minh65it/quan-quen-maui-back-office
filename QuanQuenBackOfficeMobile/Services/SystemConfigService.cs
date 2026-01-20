using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;

namespace QuanQuenBackOfficeMobile.Services;

public class SystemConfigService : ISystemConfigService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SystemConfigService> _logger;
    private readonly IConfiguration _configuration;

    public SystemConfigService(HttpClient httpClient, ILogger<SystemConfigService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SystemConfigResponse> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var keyList = keys.Distinct().ToList();
        if (keyList.Count == 0)
        {
            return new SystemConfigResponse { Success = true, Data = new Dictionary<string, string>() };
        }

        var token = await SecureStorage.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var baseUrl = _configuration["API:Identity:BaseUrl"];
        var path = _configuration["API:Identity:SystemConfig"];
        var param = Uri.EscapeDataString(string.Join(',', keyList));
        var url = $"{baseUrl}{path}?param={param}";

        _logger.LogInformation("Fetching SystemConfig for keys: {Keys}", string.Join(",", keyList));

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = JsonSerializer.Deserialize<SystemConfigResponse>(json, options) ?? new SystemConfigResponse { Success = false };
        return result;
    }

    public async Task<SystemConfigListResponse> GetValuesAsListAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var distinctKeys = keys.Distinct().ToList();
        var raw = await GetValuesAsync(distinctKeys, cancellationToken);
        var listResp = new SystemConfigListResponse
        {
            Success = raw.Success,
            Message = raw.Message
        };

        if (!raw.Success)
        {
            return listResp;
        }

        // If API didn't return any data or seems to not support multi-keys, try fetching key-by-key
        if (raw.Data == null || raw.Data.Count == 0 || distinctKeys.Any(k => !raw.Data.ContainsKey(k)))
        {
            var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (raw.Data != null)
            {
                foreach (var kv in raw.Data) merged[kv.Key] = kv.Value;
            }

            foreach (var key in distinctKeys)
            {
                if (merged.ContainsKey(key)) continue;
                try
                {
                    var single = await GetValuesAsync(new[] { key }, cancellationToken);
                    if (single.Success && single.Data != null && single.Data.TryGetValue(key, out var val))
                    {
                        merged[key] = val;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch SystemConfig key {Key} individually", key);
                }
            }

            raw.Data = merged;
        }

        foreach (var kv in raw.Data)
        {
            var resultList = new List<string>();
            var rawValue = kv.Value;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                listResp.Data[kv.Key] = resultList;
                continue;
            }

            // Try robust parsing: accept JSON array string or single string value
            try
            {
                using var doc = JsonDocument.Parse(rawValue);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.String)
                        {
                            var s = el.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) resultList.Add(s!);
                        }
                        else
                        {
                            // Fallback: convert non-string values to string
                            resultList.Add(el.ToString());
                        }
                    }
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.String)
                {
                    var s = doc.RootElement.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) resultList.Add(s!);
                }
                else
                {
                    // As a last resort, try standard deserialization
                    var arr = JsonSerializer.Deserialize<List<string>>(rawValue);
                    if (arr != null) resultList = arr;
                }
            }
            catch
            {
                try
                {
                    // If parsing as JSON fails, try to treat it as a delimited string
                    // e.g., "item1\nitem2" or "item1,item2"
                    var byNewLine = rawValue.Replace("\r", string.Empty)
                                            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                            .ToList();
                    if (byNewLine.Count > 1)
                    {
                        resultList = byNewLine;
                    }
                    else
                    {
                        var byComma = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                        if (byComma.Count > 1) resultList = byComma;
                    }
                }
                catch
                {
                    // ignore and keep empty list
                }
            }

            listResp.Data[kv.Key] = resultList;
        }

        return listResp;
    }
}


