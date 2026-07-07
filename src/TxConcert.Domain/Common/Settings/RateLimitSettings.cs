namespace TxConcert.Domain.Common.Settings;

public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimit";

    /// <summary>
    /// Set to true to enable rate limiting. Default: false (opt-out globally).
    /// </summary>
    public bool Enabled { get; init; } = false;
}
