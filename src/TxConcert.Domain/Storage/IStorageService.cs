namespace TxConcert.Domain.Storage;

/// <summary>
/// Cloud storage abstraction. Implementations: S3StorageService, GoogleCloudStorageService.
/// </summary>
public interface IStorageService
{
    Task<string> UploadAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken ct = default);

    Task<Stream> DownloadAsync(
        string bucketName,
        string objectKey,
        CancellationToken ct = default);

    Task DeleteAsync(
        string bucketName,
        string objectKey,
        CancellationToken ct = default);

    Task<string> GeneratePresignedUrlAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string bucketName,
        string objectKey,
        CancellationToken ct = default);
}
