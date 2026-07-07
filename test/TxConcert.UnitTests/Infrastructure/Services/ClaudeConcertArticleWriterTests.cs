using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Infrastructure.Services;
using TxConcert.UnitTests.TestSupport.MockHttp;

namespace TxConcert.UnitTests.Infrastructure.Services;

public class ClaudeConcertArticleWriterTests
{
    private static IOptions<ClaudeSettings> DefaultSettings() =>
        Options.Create(new ClaudeSettings
        {
            ApiKey = "test-key",
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 2048
        });

    private static string ClaudePayload(string innerJsonEscaped) => $$"""
        {
          "id": "msg_01",
          "type": "message",
          "model": "claude-sonnet-4-20250514",
          "usage": { "input_tokens": 10, "output_tokens": 42 },
          "content": [
            {
              "type": "text",
              "text": "{{innerJsonEscaped}}"
            }
          ]
        }
        """;

    private static ConcertArticleInput MakeInput(
        string? spotifyImageUrl,
        string? ticketmasterImageUrl) => new()
        {
            ConcertName = "Rock Night",
            EventDate = "2026-07-15",
            Artists =
            [
                new ArticleArtistInput
                {
                    Name = "The Headliner",
                    IsHeadliner = true,
                    SpotifyImageUrl = spotifyImageUrl
                }
            ],
            TicketmasterImageUrl = ticketmasterImageUrl
        };

    [Fact]
    public async Task WriteArticleAsync_ValidResponseWithSpotifySource_PrefersSpotifyImage()
    {
        // Arrange
        string innerJson = "{\\n  \\\"title\\\": \\\"Epic Texas Night\\\",\\n  \\\"spot\\\": \\\"Austin rocks.\\\",\\n  \\\"body\\\": \\\"Para one.\\\\n\\\\nPara two.\\\",\\n  \\\"seoKeywords\\\": [\\\"rock\\\",\\\"texas\\\"],\\n  \\\"metaDescription\\\": \\\"An amazing night\\\",\\n  \\\"chosenImageSource\\\": \\\"spotify\\\",\\n  \\\"imageAltText\\\": \\\"Headliner on stage\\\"\\n}";

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, ClaudePayload(innerJson));

        ClaudeConcertArticleWriter writer = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<ClaudeConcertArticleWriter>.Instance);

        ConcertArticleInput input = MakeInput(
            spotifyImageUrl: "https://img.spotify/headliner.jpg",
            ticketmasterImageUrl: "https://img.tm/fallback.jpg");

        // Act
        ConcertArticleResult result = await writer.WriteArticleAsync(input);

        // Assert
        result.Title.Should().Be("Epic Texas Night");
        result.Body.Should().Contain("Para one.");
        result.ImageSource.Should().Be(ArticleImageSource.Spotify);
        result.ImageUrl.Should().Be("https://img.spotify/headliner.jpg");
        result.ImageCredit.Should().Be("Photo via Spotify API");
    }

    [Fact]
    public async Task WriteArticleAsync_NoSpotifyImage_FallsBackToTicketmasterWhenRequested()
    {
        // Arrange
        string innerJson = "{\\n  \\\"title\\\": \\\"Epic Texas Night\\\",\\n  \\\"spot\\\": \\\"Austin rocks.\\\",\\n  \\\"body\\\": \\\"Para one.\\\",\\n  \\\"seoKeywords\\\": [],\\n  \\\"metaDescription\\\": \\\"A night\\\",\\n  \\\"chosenImageSource\\\": \\\"ticketmaster\\\",\\n  \\\"imageAltText\\\": \\\"Poster\\\"\\n}";

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, ClaudePayload(innerJson));

        ClaudeConcertArticleWriter writer = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<ClaudeConcertArticleWriter>.Instance);

        ConcertArticleInput input = MakeInput(
            spotifyImageUrl: null,
            ticketmasterImageUrl: "https://img.tm/poster.jpg");

        // Act
        ConcertArticleResult result = await writer.WriteArticleAsync(input);

        // Assert
        result.ImageSource.Should().Be(ArticleImageSource.Ticketmaster);
        result.ImageUrl.Should().Be("https://img.tm/poster.jpg");
        result.ImageCredit.Should().Be("Photo via Ticketmaster API");
    }

    [Fact]
    public async Task WriteArticleAsync_ResponseMissingTitle_Throws()
    {
        // Arrange
        string innerJson = "{\\n  \\\"spot\\\": \\\"no title\\\",\\n  \\\"body\\\": \\\"has body\\\"\\n}";

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, ClaudePayload(innerJson));

        ClaudeConcertArticleWriter writer = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<ClaudeConcertArticleWriter>.Instance);

        // Act
        Func<Task> act = () => writer.WriteArticleAsync(MakeInput(null, null));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*required fields*");
    }

    [Fact]
    public async Task WriteArticleAsync_ResponseMissingBody_Throws()
    {
        // Arrange
        string innerJson = "{\\n  \\\"title\\\": \\\"Has Title\\\",\\n  \\\"spot\\\": \\\"\\\"\\n}";

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, ClaudePayload(innerJson));

        ClaudeConcertArticleWriter writer = new(
            handler.ToClient(),
            DefaultSettings(),
            NullLogger<ClaudeConcertArticleWriter>.Instance);

        // Act
        Func<Task> act = () => writer.WriteArticleAsync(MakeInput(null, null));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*required fields*");
    }
}
