using System.Collections.Concurrent;
using System.Text.Json;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Azure;

/// <summary>
/// Demonstrates Azure Service Bus queue & topic messaging (Azure.Messaging.ServiceBus SDK).
/// Supports enterprise messaging, sessions, dead-lettering, duplicate detection, and transactions.
/// </summary>
public class AzureServiceBusQueueService : IMessageQueueService
{
    public CloudProvider Provider => CloudProvider.Azure;

    private readonly ConcurrentDictionary<string, ConcurrentQueue<CloudMessage<string>>> _queues = new();

    public Task PublishMessageAsync<T>(string queueOrTopic, T message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueOrTopic);

        var queue = _queues.GetOrAdd(queueOrTopic, _ => new ConcurrentQueue<CloudMessage<string>>());
        string serialized = JsonSerializer.Serialize(message);
        var cloudMessage = new CloudMessage<string>(
            MessageId: Guid.NewGuid().ToString("N"),
            Payload: serialized,
            EnqueuedAtUtc: DateTime.UtcNow,
            ReceiptHandle: Guid.NewGuid().ToString("D"));

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
        if (_queues.TryGetValue(queueOrSubscription, out var queue))
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
/// Demonstrates Azure passwordless identity using Azure AD / Entra ID Managed Identity
/// and the DefaultAzureCredential token resolution chain:
/// Environment -> Workload Identity -> Managed Identity -> Visual Studio / Azure CLI -> InteractiveBrowser.
/// </summary>
public class AzureManagedIdentityService : ICloudIdentityService
{
    public CloudProvider Provider => CloudProvider.Azure;

    public Task<CloudIdentityToken> AcquireTokenAsync(string scopeOrAudience, CancellationToken ct = default)
    {
        // DefaultAzureCredential simulates acquiring a JWT bearer token for the target Azure resource
        var token = new CloudIdentityToken(
            Token: $"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.azure.{Guid.NewGuid():N}",
            ExpiresAtUtc: DateTime.UtcNow.AddHours(1),
            IdentityProvider: "Microsoft Entra ID (Managed Identity)",
            PrincipalId: "spn:azure-app-identity-guid");

        return Task.FromResult(token);
    }
}
