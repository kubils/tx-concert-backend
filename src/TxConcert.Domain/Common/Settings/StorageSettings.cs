namespace TxConcert.Domain.Common.Settings;

public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>Provider: S3 | GCS</summary>
    public string Provider { get; init; } = "S3";
    public string BucketName { get; init; } = default!;
    public string Region { get; init; } = "eu-central-1";
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
    /// <summary>Custom endpoint for LocalStack or MinIO.</summary>
    public string? Endpoint { get; init; }
    /// <summary>GCS Project ID.</summary>
    public string? ProjectId { get; init; }
}
