using TxConcert.Domain.Common.Enums;

namespace TxConcert.Domain.Common.Settings;

public sealed class AppSettings
{
    public const string SectionName = "App";

    public string Name { get; init; } = "tx-concert";
    public AppEnvironment Environment { get; init; } = AppEnvironment.Development;
    public int Port { get; init; } = 5000;

    public bool IsProduction => Environment == AppEnvironment.Production;
    public bool IsDevelopment => Environment == AppEnvironment.Development;
    public bool IsLocal => Environment == AppEnvironment.Local;
}
