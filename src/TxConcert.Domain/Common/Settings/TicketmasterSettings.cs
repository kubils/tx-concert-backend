namespace TxConcert.Domain.Common.Settings;

public sealed class TicketmasterSettings
{
    public const string SectionName = "Ticketmaster";

    public string ApiKey { get; init; } = "";
    public string BaseUrl { get; init; } = "https://app.ticketmaster.com/discovery/v2";
    public string StateCode { get; init; } = "TX";
    public string ClassificationName { get; init; } = "music";
    public int PageSize { get; init; } = 200;
}
