using System.Collections.Concurrent;
using System.Text.Json;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Aws;

/// <summary>
/// Demonstrates AWS Secrets Manager client patterns (AWSSDK.SecretsManager).
/// AWS Secrets Manager supports secret versioning, encryption via AWS KMS,
/// and automated rotation for RDS database credentials.
/// </summary>
public class AwsSecretsManagerService : ISecretManagerService
{
    public CloudProvider Provider => CloudProvider.Aws;

    private readonly string _region;
    private readonly ConcurrentDictionary<string, string> _secrets = new(StringComparer.OrdinalIgnoreCase);

    public AwsSecretsManagerService(string region = "us-east-1")
    {
        _region = region;
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
/// Demonstrates AWS Simple Queue Service (SQS) client patterns (AWSSDK.SQS).
/// Provides Standard (best-effort ordering) and FIFO (exactly-once processing) queues.
/// </summary>
public class AwsSqsQueueService : IMessageQueueService
{
    public CloudProvider Provider => CloudProvider.Aws;

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
            ReceiptHandle: Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

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
/// Demonstrates AWS IAM and STS token acquisition (e.g., IRSA in EKS or EC2 Instance Metadata Service).
/// Avoids storing hardcoded AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY.
/// </summary>
public class AwsIamIdentityService : ICloudIdentityService
{
    public CloudProvider Provider => CloudProvider.Aws;

    public Task<CloudIdentityToken> AcquireTokenAsync(string scopeOrAudience, CancellationToken ct = default)
    {
        var token = new CloudIdentityToken(
            Token: $"aws-session-token-{Guid.NewGuid():N}",
            ExpiresAtUtc: DateTime.UtcNow.AddHours(1),
            IdentityProvider: "AWS STS (AssumeRoleWithWebIdentity)",
            PrincipalId: "arn:aws:iam::123456789012:role/DotNetMasteryAppRole");

        return Task.FromResult(token);
    }
}
