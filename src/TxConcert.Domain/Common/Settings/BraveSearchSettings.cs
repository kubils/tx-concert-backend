namespace TxConcert.Domain.Common.Settings;

public sealed class BraveSearchSettings
{
    public const string SectionName = "BraveSearch";

    public string ApiKey { get; init; } = "";
    public string BaseUrl { get; init; } = "https://api.search.brave.com";
    public int MaxResultsPerQuery { get; init; } = 5;
}
