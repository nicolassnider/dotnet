namespace DotNetMastery.Cloud.Abstractions;

public record CloudIdentityToken(
    string Token,
    DateTime ExpiresAtUtc,
    string IdentityProvider,
    string PrincipalId);

/// <summary>
/// Cloud-agnostic abstraction for passwordless identity and workload federation:
/// - Azure: Entra ID Managed Identity / DefaultAzureCredential
/// - AWS: IAM Roles for Service Accounts (IRSA) / AWS STS
/// - GCP: Workload Identity / Google Application Default Credentials (ADC)
/// </summary>
public interface ICloudIdentityService
{
    CloudProvider Provider { get; }

    Task<CloudIdentityToken> AcquireTokenAsync(string scopeOrAudience, CancellationToken ct = default);
}
