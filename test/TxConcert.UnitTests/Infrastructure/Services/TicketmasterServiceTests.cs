using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Infrastructure.Services;
using TxConcert.UnitTests.TestSupport.MockHttp;

namespace TxConcert.UnitTests.Infrastructure.Services;

public class TicketmasterServiceTests
{
    private static IOptions<TicketmasterSettings> DefaultSettings() =>
        Options.Create(new TicketmasterSettings
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test/ticketmaster",
            StateCode = "TX",
            ClassificationName = "music",
            PageSize = 200
        });

    [Fact]
    public async Task GetUpcomingEventsAsync_SuccessfulResponse_ReturnsMappedDtos()
    {
        // Arrange
        const string page0Json = """
        {
          "_embedded": {
            "events": [
              {
                "id": "tm_abc123",
                "name": "Rolling Rocks Live",
                "url": "https://tm.test/event/abc",
                "images": [
                  { "url": "https://img.test/16x9-large.jpg", "ratio": "16_9", "width": 2048, "height": 1152, "fallback": false },
                  { "url": "https://img.test/fallback.jpg", "ratio": "16_9", "width": 4096, "height": 2304, "fallback": true }
                ],
                "dates": {
                  "start": { "localDate": "2026-07-15", "localTime": "20:30:00" },
                  "timezone": "America/Chicago",
                  "status": { "code": "onsale" }
                },
                "priceRanges": [
                  { "min": 45.0, "max": 150.0 }
                ],
                "classifications": [
                  { "segment": { "name": "Music" }, "genre": { "name": "Rock" }, "subGenre": { "name": "Alternative Rock" } }
                ],
                "info": "Age 18+",
                "pleaseNote": "No cameras.",
                "_embedded": {
                  "venues": [
                    {
                      "name": "Austin City Limits",
                      "postalCode": "78701",
                      "address": { "line1": "310 Willie Nelson Blvd" },
                      "city": { "name": "Austin" },
                      "state": { "stateCode": "TX" },
                      "location": { "latitude": "30.2652", "longitude": "-97.7494" }
                    }
                  ],
                  "attractions": [
                    {
                      "name": "The Headliner",
                      "classifications": [
                        { "genre": { "name": "Rock" }, "subGenre": { "name": "Alternative Rock" } }
                      ],
                      "externalLinks": {
                        "spotify": [ { "url": "https://open.spotify.com/artist/abc" } ]
                      }
                    }
                  ]
                }
              }
            ]
          }
        }
        """;

        const string emptyPageJson = """{ "_embedded": { "events": [] } }""";

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupSequentialJsonResponses(
            (HttpStatusCode.OK, page0Json),
            (HttpStatusCode.OK, emptyPageJson));

        TicketmasterService service = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<TicketmasterService>.Instance);

        // Act
        IReadOnlyList<TicketmasterEventDto> result = await service.GetUpcomingEventsAsync();

        // Assert
        result.Should().HaveCount(1);
        TicketmasterEventDto dto = result[0];
        dto.TicketmasterId.Should().Be("tm_abc123");
        dto.Name.Should().Be("Rolling Rocks Live");
        dto.EventDate.Should().Be(new LocalDate(2026, 7, 15));
        dto.StartTime.Should().Be(new LocalTime(20, 30, 0));
        dto.TimeZone.Should().Be("America/Chicago");
        dto.VenueName.Should().Be("Austin City Limits");
        dto.VenueCity.Should().Be("Austin");
        dto.VenueStateCode.Should().Be("TX");
        dto.VenueLatitude.Should().NotBeNull();
        dto.VenueLongitude.Should().NotBeNull();
        dto.PriceMin.Should().Be(45.0m);
        dto.PriceMax.Should().Be(150.0m);
        dto.ImageUrl.Should().Be("https://img.test/16x9-large.jpg");
        dto.SegmentName.Should().Be("Music");
        dto.GenreName.Should().Be("Rock");
        dto.SalesStatus.Should().Be("onsale");
        dto.Attractions.Should().HaveCount(1);
        dto.Attractions[0].Name.Should().Be("The Headliner");
        dto.Attractions[0].SpotifyUrl.Should().Be("https://open.spotify.com/artist/abc");
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_NonSuccessStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.InternalServerError, """{"fault":"boom"}""");

        TicketmasterService service = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<TicketmasterService>.Instance);

        // Act
        Func<Task> act = () => service.GetUpcomingEventsAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_MaxEventsReached_StopsEarly()
    {
        // Arrange
        const string page0Json = """
        {
          "_embedded": {
            "events": [
              {
                "id": "tm_1",
                "name": "Concert One",
                "url": "https://tm.test/event/1",
                "dates": {
                  "start": { "localDate": "2026-07-15", "localTime": "20:30:00" }
                },
                "_embedded": {
                  "venues": [
                    {
                      "name": "Austin City Limits",
                      "city": { "name": "Austin" },
                      "state": { "stateCode": "TX" }
                    }
                  ]
                }
              },
              {
                "id": "tm_2",
                "name": "Concert Two",
                "url": "https://tm.test/event/2",
                "dates": {
                  "start": { "localDate": "2026-07-16", "localTime": "20:30:00" }
                },
                "_embedded": {
                  "venues": [
                    {
                      "name": "Austin City Limits",
                      "city": { "name": "Austin" },
                      "state": { "stateCode": "TX" }
                    }
                  ]
                }
              }
            ]
          }
        }
        """;

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, page0Json);

        TicketmasterService service = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<TicketmasterService>.Instance);

        // Act
        IReadOnlyList<TicketmasterEventDto> result = await service.GetUpcomingEventsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result[0].TicketmasterId.Should().Be("tm_1");
    }
}
