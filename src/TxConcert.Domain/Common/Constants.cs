namespace TxConcert.Domain.Common;

/// <summary>
/// Application-wide constants. Single source of truth for all shared string values.
/// </summary>
public static class Constants
{
    public static class Headers
    {
        public const string AdminApiKey = "x-api-key";
        public const string Lang = "x-lang";
        public const string ForwardedFor = "x-forwarded-for";
        public const string CorrelationId = "x-correlation-id";
    }

    public static class RateLimit
    {
        public static class Names
        {
            public const string Default = "default";
            public const string Auth = "auth";
            public const string Public = "public";
        }

        public static class Limits
        {
            public const int DefaultLimit = 100;
            public const int AuthLimit = 5;
            public const int PublicLimit = 60;
        }

        public static class Windows
        {
            public static readonly TimeSpan Default = TimeSpan.FromSeconds(60);
            public static readonly TimeSpan Auth = TimeSpan.FromMinutes(15);
            public static readonly TimeSpan Public = TimeSpan.FromSeconds(60);
        }
    }

    public static class Jwt
    {
        public const string SchemeName = "JWT";
        public const string ApiKeySchemeName = "ApiKey";
        public const string RolesClaim = "roles";
        public const string UserIdClaim = "sub";
        public const string RealmRolesClaim = "realm_access";
    }

    public static class IdPrefix
    {
        public const string CommandOutbox = "cdout";          // 5 chars
        public const string EventOutbox = "evout";            // 5 chars
        public const string ExternalAudit = "exaud";          // 5 chars
        public const string Genre = "genre";                  // 5 chars
        public const string State = "state";                  // 5 chars
        public const string Artist = "artst";                 // 5 chars
        public const string Venue = "venue";                  // 5 chars
        public const string Concert = "cncrt";                // 5 chars
        public const string ArtistGenre = "artgn";            // 5 chars
        public const string ConcertArtist = "cnart";          // 5 chars
        public const string ConcertDescription = "cndsc";     // 5 chars
        public const string ConcertVibe = "cnvib";              // 5 chars
        public const string ArtistVibeProfile = "arvib";        // 5 chars
        public const string ConcertArticle = "cncar";           // 5 chars
        public const string JobExecutionLog = "jobex";          // 5 chars
        public const string AiPrompt = "aiprm";                 // 5 chars
    }

    public static class AiPrompts
    {
        public const string ConcertEvaluation = "concert-evaluation";
    }

    public static class Pagination
    {
        public const int DefaultLimit = 20;
        public const int MaxLimit = 100;
        public const int DefaultOffset = 0;
    }

    public static class Validation
    {
        public const int MaxNameLength = 255;
        public const int MaxIdLength = 128;
        public const int MaxShortStringLength = 100;
        public const int MaxUrlLength = 2048;
        public const int MaxBioLength = 5000;
        public const int MaxDescriptionLength = 10_000;
        public const int MaxAddressLength = 500;
        public const int MaxZipLength = 20;
        public const int MaxPhoneLength = 50;
        public const int MaxPromptLength = 10_000;
        public const int MaxModelNameLength = 100;
        public const int MaxErrorMessageLength = 2000;
        public const int MaxAbbreviationLength = 10;
        public const int MaxTimeZoneLength = 50;
        public const int DefaultMaxFileSizeMb = 10;
        public const int DefaultMaxRequestSizeMb = 50;
    }

    public static class Logging
    {
        public const int MaxLogLength = 10_000;
    }

    public static class Security
    {
        public const string AdminApiKeyConfigPath = "Security:AdminApiKey";
        public const string AllowedIpsConfigPath = "Security:AllowedIps";
    }

    /// <summary>
    /// Command name constants used as routing keys in the outbox and message bus.
    /// </summary>
    public static class Commands
    {
        public static class ExternalAudit
        {
            public const string Persist = "external-audit.persist";
            public const string Retry = "external-audit.retry";
        }
    }

    /// <summary>
    /// Event name constants used as routing keys in the outbox and message bus.
    /// </summary>
    public static class Events
    {
        // Add event names here as features are built, e.g.:
        // public static class ExternalAudit
        // {
        //     public const string Created = "external-audit.created";
        // }
    }
}
