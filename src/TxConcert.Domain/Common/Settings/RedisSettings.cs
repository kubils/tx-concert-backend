namespace TxConcert.Domain.Common.Settings;

public sealed class RedisSettings
{
    public const string SectionName = "Redis";

    public bool Enabled { get; init; } = false;
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 6379;
    public string Password { get; init; } = string.Empty;
    public int Db { get; init; } = 0;
    public string? KeyPrefix { get; init; }

    public string ConnectionString =>
        string.IsNullOrEmpty(Password)
            ? $"{Host}:{Port},defaultDatabase={Db},abortConnect=false"
            : $"{Host}:{Port},password={Password},defaultDatabase={Db},abortConnect=false";
}
