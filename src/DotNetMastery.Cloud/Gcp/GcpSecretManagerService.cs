using System.Collections.Concurrent;
using System.Text.Json;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Gcp;

/// <summary>
/// Demonstrates Google Cloud Secret Manager client patterns (Google.Cloud.SecretManager.V1).
/// Secrets are referenced via resource paths: projects/{project}/secrets/{secret}/versions/{version|latest}.
/// </summary>
public class GcpSecretManagerService : ISecretManagerService
{
    public CloudProvider Provider => CloudProvider.Gcp;

    private readonly string _projectId;
    private readonly ConcurrentDictionary<string, string> _secrets = new(StringComparer.OrdinalIgnoreCase);

    public GcpSecretManagerService(string projectId = "my-gcp-project")
    {
        _projectId = projectId;
    }

    public Task<string?> GetSecretAsync(string secretName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        _secrets.TryGetValue(secretName, out string? value);
        return Task.FromResult(value);
    }

    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        _secrets[secretName] = secretValue;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Demonstrates Google Cloud Pub/Sub messaging client patterns (Google.Cloud.PubSub.V1).
/// Decouples services producing events (Topics) from services processing events (Subscriptions).
/// </summary>
public class GcpPubSubQueueService : IMessageQueueService
{
    public CloudProvider Provider => CloudProvider.Gcp;

    private readonly ConcurrentDictionary<string, ConcurrentQueue<CloudMessage<string>>> _topics = new();

    public Task PublishMessageAsync<T>(string queueOrTopic, T message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueOrTopic);

        var queue = _topics.GetOrAdd(queueOrTopic, _ => new ConcurrentQueue<CloudMessage<string>>());
        string serialized = JsonSerializer.Serialize(message);
        var cloudMessage = new CloudMessage<string>(
            MessageId: Guid.NewGuid().ToString("N"),
            Payload: serialized,
            EnqueuedAtUtc: DateTime.UtcNow,
            ReceiptHandle: $"ack-id-{Guid.NewGuid():N}");

        queue.Enqueue(cloudMessage);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CloudMessage<T>>> ReceiveMessagesAsync<T>(
        string queueOrSubscription,
        int maxMessages = 10,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueOrSubscription);

        var results = new List<CloudMessage<T>>();
        if (_topics.TryGetValue(queueOrSubscription, out var queue))
        {
            while (results.Count < maxMessages && queue.TryDequeue(out var rawMsg))
            {
                var payload = JsonSerializer.Deserialize<T>(rawMsg.Payload);
                if (payload != null)
                {
                    results.Add(new CloudMessage<T>(
                        rawMsg.MessageId,
                        payload,
                        rawMsg.EnqueuedAtUtc,
                        rawMsg.ReceiptHandle));
                }
            }
        }

        return Task.FromResult<IReadOnlyList<CloudMessage<T>>>(results);
    }
}

/// <summary>
/// Demonstrates Google Cloud Workload Identity Federation (ADC).
/// Allows GKE pods or Cloud Run services to impersonate Google Service Accounts without private keys.
/// </summary>
public class GcpWorkloadIdentityService : ICloudIdentityService
{
    public CloudProvider Provider => CloudProvider.Gcp;

    public Task<CloudIdentityToken> AcquireTokenAsync(string scopeOrAudience, CancellationToken ct = default)
    {
        var token = new CloudIdentityToken(
            Token: $"gcp-oauth2-token-{Guid.NewGuid():N}",
            ExpiresAtUtc: DateTime.UtcNow.AddHours(1),
            IdentityProvider: "Google Cloud Workload Identity",
            PrincipalId: "sa-dotnet-app@my-gcp-project.iam.gserviceaccount.com");

        return Task.FromResult(token);
    }
}
