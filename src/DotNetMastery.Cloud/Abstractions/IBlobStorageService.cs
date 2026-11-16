namespace DotNetMastery.Cloud.Abstractions;

/// <summary>
/// Cloud-agnostic abstraction for Object/Blob storage across Azure Blob Storage, AWS S3, and GCP Cloud Storage.
/// Enables multi-cloud portability and simplifies local unit testing without direct cloud vendor SDK coupling.
/// </summary>
public interface IBlobStorageService
{
    CloudProvider Provider { get; }

    Task<string> UploadBlobAsync(
        string containerOrBucket,
        string blobName,
        Stream content,
        string contentType = "application/octet-stream",
        CancellationToken ct = default);

    Task<Stream?> DownloadBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default);

    Task<bool> BlobExistsAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default);

    Task<bool> DeleteBlobAsync(
        string containerOrBucket,
        string blobName,
        CancellationToken ct = default);
}
