namespace TxConcert.Domain.Common.Settings;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public string VHost { get; init; } = "/";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public ushort Port { get; init; } = 5672;
}
