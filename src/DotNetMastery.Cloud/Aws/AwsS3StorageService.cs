using System.Collections.Concurrent;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Aws;

/// <summary>
/// Demonstrates AWS Simple Storage Service (S3) client patterns (corresponding to AWSSDK.S3).
/// In AWS, objects are stored in Buckets identified by S3 keys, with optional versioning and lifecycle policies.
/// Best practice in .NET: Register AWSSDK via AddAWSService&lt;IAmazonS3&gt;() and use IAM Roles.
/// </summary>
public class AwsS3StorageService : IBlobStorageService
{
    public CloudProvider Provider => CloudProvider.Aws;

    private readonly string _region;
    private readonly ConcurrentDictionary<string, byte[]> _s3Store = new();

    public AwsS3StorageService(string region = "us-east-1")
    {
        _region = region;
    }

    public async Task<string> UploadBlobAsync(
        string containerOrBucket,
        string blobName,
        Stream content,
        string contentType = "application/octet-stream",
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerOrBucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, ct);
        byte[] data = memoryStream.ToArray();

        string key = BuildKey(containerOrBucket, blobName);
        _s3Store[key] = data;

        // AWS S3 standard virtual-hosted-style URL
        return $"https://{containerOrBucket}.s3.{_region}.amazonaws.com/{blobName}";
    }

    public Task<Stream?> DownloadBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        if (_s3Store.TryGetValue(key, out byte[]? data))
        {
            Stream stream = new MemoryStream(data);
            return Task.FromResult<Stream?>(stream);
        }

        return Task.FromResult<Stream?>(null);
    }

    public Task<bool> BlobExistsAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        return Task.FromResult(_s3Store.ContainsKey(key));
    }

    public Task<bool> DeleteBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        return Task.FromResult(_s3Store.TryRemove(key, out _));
    }

    private static string BuildKey(string bucket, string key) => $"{bucket}/{key}";
}
