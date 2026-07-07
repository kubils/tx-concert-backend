using System.Net;
using TxConcert.IntegrationTests.Fixtures;
using FluentAssertions;

namespace TxConcert.IntegrationTests.Controllers;

public class HealthControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthControllerTests()
    {
        // Uses a real connection string from env or a default local one for CI
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=tx_concert_test;Username=postgres;Password=postgres";

        _factory = new TestWebApplicationFactory(connectionString);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/health-check");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
