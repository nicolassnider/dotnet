using DotNetMastery.Cloud.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNetMastery.Cloud.Health;

/// <summary>
/// Health check that verifies connectivity to the configured Cloud Storage provider.
/// Used for Kubernetes readiness/liveness probes and container health endpoints (/healthz).
/// </summary>
public class CloudStorageHealthCheck : IHealthCheck
{
    private readonly IBlobStorageService _storageService;
    private readonly string _healthContainer;

    public CloudStorageHealthCheck(IBlobStorageService storageService, string healthContainer = "healthcheck")
    {
        _storageService = storageService;
        _healthContainer = healthContainer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify basic existence query to ensure credentials and connectivity are alive
            await _storageService.BlobExistsAsync(_healthContainer, "probe.txt", cancellationToken);

            return HealthCheckResult.Healthy(
                $"{_storageService.Provider} storage is operational and responsive.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"{_storageService.Provider} storage probe failed: {ex.Message}", ex);
        }
    }
}
