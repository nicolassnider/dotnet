namespace DotNetMastery.Cloud.Abstractions;

public record CloudMessage<T>(
    string MessageId,
    T Payload,
    DateTime EnqueuedAtUtc,
    string ReceiptHandle);

/// <summary>
/// Cloud-agnostic abstraction for Message Queuing and Asynchronous Event-driven pipelines.
/// Maps to Azure Service Bus / Storage Queues, AWS SQS / SNS, and GCP Cloud Pub/Sub.
/// </summary>
public interface IMessageQueueService
{
    CloudProvider Provider { get; }

    Task PublishMessageAsync<T>(string queueOrTopic, T message, CancellationToken ct = default);

    Task<IReadOnlyList<CloudMessage<T>>> ReceiveMessagesAsync<T>(
        string queueOrSubscription,
        int maxMessages = 10,
        CancellationToken ct = default);
}
