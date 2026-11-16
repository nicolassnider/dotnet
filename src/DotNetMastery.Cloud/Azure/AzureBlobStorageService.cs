using System.Collections.Concurrent;
using DotNetMastery.Cloud.Abstractions;

namespace DotNetMastery.Cloud.Azure;

/// <summary>
/// Demonstrates Azure Blob Storage client patterns (corresponding to Azure.Storage.Blobs SDK).
/// In Azure, storage is organized as: Storage Account -> Blob Container -> Block/Append/Page Blob.
/// Recommended auth: DefaultAzureCredential (Entra ID) rather than account connection strings.
/// </summary>
public class AzureBlobStorageService : IBlobStorageService
{
    public CloudProvider Provider => CloudProvider.Azure;

    private readonly string _storageAccountName;
    private readonly ConcurrentDictionary<string, byte[]> _blobStore = new();

    public AzureBlobStorageService(string storageAccountName = "mystorageacct")
    {
        _storageAccountName = storageAccountName;
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
        _blobStore[key] = data;

        // Return Azure Blob Storage standard canonical URI
        return $"https://{_storageAccountName}.blob.core.windows.net/{containerOrBucket}/{blobName}";
    }

    public Task<Stream?> DownloadBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        if (_blobStore.TryGetValue(key, out byte[]? data))
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
        return Task.FromResult(_blobStore.ContainsKey(key));
    }

    public Task<bool> DeleteBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default)
    {
        string key = BuildKey(containerOrBucket, blobName);
        return Task.FromResult(_blobStore.TryRemove(key, out _));
    }

    private static string BuildKey(string container, string blobName) => $"{container.ToLowerInvariant()}/{blobName}";
}
