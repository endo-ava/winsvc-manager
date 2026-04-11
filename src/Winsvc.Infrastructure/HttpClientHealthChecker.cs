using System;
using System.Net.Http;
using System.Threading.Tasks;
using Winsvc.Contracts;
using Winsvc.Contracts.Manifest;
using Winsvc.Core;

namespace Winsvc.Infrastructure;

public class HttpClientHealthChecker : IHealthChecker
{
    private readonly HttpClient _httpClient;

    public HttpClientHealthChecker()
    {
        _httpClient = new HttpClient();
    }

    public async Task<HealthState> CheckAsync(HealthConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Url))
            return HealthState.Unknown;

        try
        {
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(config.TimeoutSec > 0 ? config.TimeoutSec : 5));
            var response = await _httpClient.GetAsync(config.Url, cts.Token);
            
            return response.IsSuccessStatusCode ? HealthState.Healthy : HealthState.Unhealthy;
        }
        catch (Exception)
        {
            return HealthState.Unhealthy;
        }
    }
}
