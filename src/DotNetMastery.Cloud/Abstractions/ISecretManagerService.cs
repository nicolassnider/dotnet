namespace DotNetMastery.Cloud.Abstractions;

/// <summary>
/// Cloud-agnostic abstraction for Secret Management across Azure Key Vault, AWS Secrets Manager, and GCP Secret Manager.
/// Centralizes retrieval of database connection strings, API tokens, and cryptographic keys.
/// </summary>
public interface ISecretManagerService
{
    CloudProvider Provider { get; }

    Task<string?> GetSecretAsync(string secretName, CancellationToken ct = default);

    Task SetSecretAsync(string secretName, string secretValue, CancellationToken ct = default);
}
