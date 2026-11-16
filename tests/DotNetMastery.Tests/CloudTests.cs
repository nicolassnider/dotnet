using System.Text;
using DotNetMastery.Cloud.Abstractions;
using DotNetMastery.Cloud.Factory;
using DotNetMastery.Cloud.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNetMastery.Tests;

public class CloudTests
{
    private readonly ICloudServiceFactory _cloudFactory = new CloudServiceFactory();

    [Theory]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.Aws)]
    [InlineData(CloudProvider.Gcp)]
    public async Task BlobStorage_UploadDownloadDelete_ShouldWorkConsistentlyAcrossProviders(CloudProvider provider)
    {
        var storage = _cloudFactory.GetBlobStorage(provider);
        string bucket = "mastery-bucket";
        string blobName = "test-document.txt";
        byte[] originalContent = Encoding.UTF8.GetBytes("Hello Multi-Cloud from .NET!");

        // 1. Upload
        using var uploadStream = new MemoryStream(originalContent);
        string blobUri = await storage.UploadBlobAsync(bucket, blobName, uploadStream, "text/plain");

        Assert.NotEmpty(blobUri);

        // 2. Exists check
        bool exists = await storage.BlobExistsAsync(bucket, blobName);
        Assert.True(exists);

        // 3. Download
        using var downloadedStream = await storage.DownloadBlobAsync(bucket, blobName);
        Assert.NotNull(downloadedStream);

        using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);
        byte[] downloadedBytes = memoryStream.ToArray();

        Assert.Equal(originalContent, downloadedBytes);

        // 4. Delete
        bool deleted = await storage.DeleteBlobAsync(bucket, blobName);
        Assert.True(deleted);

        bool existsAfterDelete = await storage.BlobExistsAsync(bucket, blobName);
        Assert.False(existsAfterDelete);
    }

    [Theory]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.Aws)]
    [InlineData(CloudProvider.Gcp)]
    public async Task SecretManager_GetAndSet_ShouldWorkConsistentlyAcrossProviders(CloudProvider provider)
    {
        var secretService = _cloudFactory.GetSecretManager(provider);
        string secretKey = "Database--ConnectionString";
        string secretValue = "Server=mycloudserver;Database=mastery;User=admin;";

        await secretService.SetSecretAsync(secretKey, secretValue);
        string? retrieved = await secretService.GetSecretAsync(secretKey);

        Assert.Equal(secretValue, retrieved);
    }

    [Theory]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.Aws)]
    [InlineData(CloudProvider.Gcp)]
    public async Task MessageQueue_PublishAndReceive_ShouldProcessMessagesCorrectly(CloudProvider provider)
    {
        var queueService = _cloudFactory.GetMessageQueue(provider);
        string queueName = "orders-pipeline";

        var payload = new OrderCreatedEvent(OrderId: 1001, TotalAmount: 299.99m, CustomerId: "cust_42");

        await queueService.PublishMessageAsync(queueName, payload);

        var receivedList = await queueService.ReceiveMessagesAsync<OrderCreatedEvent>(queueName, maxMessages: 5);

        Assert.Single(receivedList);
        var message = receivedList[0];
        Assert.Equal(payload.OrderId, message.Payload.OrderId);
        Assert.Equal(payload.TotalAmount, message.Payload.TotalAmount);
        Assert.NotEmpty(message.MessageId);
        Assert.NotEmpty(message.ReceiptHandle);
    }

    [Theory]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.Aws)]
    [InlineData(CloudProvider.Gcp)]
    public async Task IdentityService_AcquireToken_ShouldReturnValidToken(CloudProvider provider)
    {
        var identityService = _cloudFactory.GetIdentityService(provider);

        var tokenResult = await identityService.AcquireTokenAsync("https://management.core.windows.net/.default");

        Assert.NotNull(tokenResult);
        Assert.NotEmpty(tokenResult.Token);
        Assert.NotEmpty(tokenResult.IdentityProvider);
        Assert.True(tokenResult.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task CloudStorageHealthCheck_ShouldReturnHealthy_WhenStorageAccessible()
    {
        var azureStorage = _cloudFactory.GetBlobStorage(CloudProvider.Azure);
        var healthCheck = new CloudStorageHealthCheck(azureStorage, "healthcheck");

        var context = new HealthCheckContext();
        var result = await healthCheck.CheckHealthAsync(context);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("Azure storage is operational", result.Description);
    }

    private record OrderCreatedEvent(int OrderId, decimal TotalAmount, string CustomerId);
}
