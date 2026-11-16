using System.Collections.Concurrent;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Azure;

/// <summary>
/// Demonstrates Azure Key Vault secret management client patterns (Azure.Security.KeyVault.Secrets SDK).
/// Key Vault provides hardware security module (HSM) backed secret, key, and certificate storage.
/// In .NET applications, Key Vault can also be plugged directly into IConfiguration via
/// builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
/// </summary>
public class AzureKeyVaultSecretService : ISecretManagerService
{
    public CloudProvider Provider => CloudProvider.Azure;

    private readonly string _vaultUri;
    private readonly ConcurrentDictionary<string, string> _secrets = new(StringComparer.OrdinalIgnoreCase);

    public AzureKeyVaultSecretService(string vaultUri = "https://my-keyvault.vault.azure.net/")
    {
        _vaultUri = vaultUri;
    }

    public Task<string?> GetSecretAsync(string secretName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        // Normalize secret names (Key Vault uses hyphens instead of underscores or colons)
        string normalizedKey = secretName.Replace(":", "--").Replace("_", "-");
        _secrets.TryGetValue(normalizedKey, out string? value);
        return Task.FromResult(value);
    }

    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        string normalizedKey = secretName.Replace(":", "--").Replace("_", "-");
        _secrets[normalizedKey] = secretValue;
        return Task.CompletedTask;
    }
}
