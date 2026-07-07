using TxConcert.Domain.Storage;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;

namespace TxConcert.Infrastructure.Storage;

public sealed class GoogleCloudStorageService(
    StorageClient client,
    ILogger<GoogleCloudStorageService> logger) : IStorageService
{
    public async Task<string> UploadAsync(
        string bucketName, string objectKey, Stream content, string contentType, CancellationToken ct = default)
    {
        Google.Apis.Storage.v1.Data.Object obj = await client.UploadObjectAsync(
            bucketName, objectKey, contentType, content, cancellationToken: ct);
        logger.LogInformation("Uploaded {ObjectKey} to bucket {Bucket}", objectKey, bucketName);
        return obj.MediaLink;
    }

    public async Task<Stream> DownloadAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        MemoryStream stream = new();
        await client.DownloadObjectAsync(bucketName, objectKey, stream, cancellationToken: ct);
        stream.Position = 0;
        logger.LogInformation("Downloaded {ObjectKey} from bucket {Bucket}", objectKey, bucketName);
        return stream;
    }

    public async Task DeleteAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        await client.DeleteObjectAsync(bucketName, objectKey, cancellationToken: ct);
        logger.LogInformation("Deleted {ObjectKey} from bucket {Bucket}", objectKey, bucketName);
    }

    public Task<string> GeneratePresignedUrlAsync(
        string bucketName, string objectKey, TimeSpan expiry, CancellationToken ct = default)
    {
        UrlSigner signer = UrlSigner.FromCredential(Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefault());
        string url = signer.Sign(bucketName, objectKey, expiry, HttpMethod.Get);
        logger.LogInformation(
            "Generated presigned URL for {ObjectKey} in bucket {Bucket} with expiry {ExpiryMinutes} minutes",
            objectKey,
            bucketName,
            expiry.TotalMinutes);
        return Task.FromResult(url);
    }

    public async Task<bool> ExistsAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        try
        {
            await client.GetObjectAsync(bucketName, objectKey, cancellationToken: ct);
            logger.LogDebug("Storage object exists: {ObjectKey} in bucket {Bucket}", objectKey, bucketName);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogDebug("Storage object not found: {ObjectKey} in bucket {Bucket}", objectKey, bucketName);
            return false;
        }
    }
}
