using System.Collections.Concurrent;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Gcp;

/// <summary>
/// Demonstrates Google Cloud Storage (GCS) client patterns (corresponding to Google.Cloud.Storage.V1).
/// In GCP, objects reside in globally-named Buckets with Cloud IAM fine-grained or uniform access control.
/// Standard auth in .NET: Google Application Default Credentials (ADC) via GOOGLE_APPLICATION_CREDENTIALS
/// or GKE Workload Identity.
/// </summary>
public class GcpCloudStorageService : IBlobStorageService
{
    public CloudProvider Provider => CloudProvider.Gcp;

    private readonly ConcurrentDictionary<string, byte[]> _gcsStore = new();

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
        _gcsStore[key] = data;

        // Google Cloud Storage canonical URL format
        return $"https://storage.googleapis.com/{containerOrBucket}/{blobName}";
    }

    public Task<Stream?> DownloadBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        if (_gcsStore.TryGetValue(key, out byte[]? data))
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
        return Task.FromResult(_gcsStore.ContainsKey(key));
    }

    public Task<bool> DeleteBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        return Task.FromResult(_gcsStore.TryRemove(key, out _));
    }

    private static string BuildKey(string bucket, string objectName) => $"{bucket}/{objectName}";
}
