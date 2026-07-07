namespace TxConcert.Domain.Common.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Authority { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    /// <summary>
    /// Optional symmetric key for local development without Auth0.
    /// When set, JWT validation uses HS256 instead of RS256/JWKS.
    /// </summary>
    public string? LocalSecret { get; init; }
}
