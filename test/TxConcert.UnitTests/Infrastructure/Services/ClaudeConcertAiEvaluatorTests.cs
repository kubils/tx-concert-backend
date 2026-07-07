using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Prompts;
using TxConcert.Infrastructure.Services;
using TxConcert.UnitTests.TestSupport.MockHttp;

namespace TxConcert.UnitTests.Infrastructure.Services;

public class ClaudeConcertAiEvaluatorTests
{
    [Fact]
    public async Task EvaluateAsync_ValidJsonResponse_ReturnsParsedResultWithClampedScores()
    {
        // Arrange
        const string claudePayload = """
        {
          "id": "msg_01",
          "type": "message",
          "model": "claude-sonnet-4-20250514",
          "usage": { "input_tokens": 10, "output_tokens": 42 },
          "content": [
            {
              "type": "text",
              "text": "```json\n{\n  \"description\": \"A high-voltage Texas night.\",\n  \"concertVibes\": { \"energy\": 12, \"soundQuality\": 0, \"hype\": 8, \"crowdVibe\": 9, \"valueForMoney\": -3, \"summary\": \"Electric.\" },\n  \"artistVibes\": [ { \"artistName\": \"The Headliner\", \"visuals\": 15, \"sound\": 7, \"energy\": 10, \"fanInteraction\": 6, \"texasSpirit\": 8, \"summary\": \"Solid.\" } ]\n}\n```"
            }
          ]
        }
        """;

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, claudePayload);

        var braveSearch = new Mock<IBraveSearchService>();
        braveSearch.Setup(b => b.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebSearchResult([]));

        var promptProvider = new Mock<IAiPromptProvider>();
        promptProvider.Setup(p => p.GetConcertEvaluationPromptAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiPromptContent("system prompt", "user prompt instructions", "test", 1));

        var settings = Options.Create(new ClaudeSettings
        {
            ApiKey = "test-key",
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 2048
        });

        ClaudeConcertAiEvaluator evaluator = new(
            handler.ToClient(),
            braveSearch.Object,
            promptProvider.Object,
            settings,
            NullLogger<ClaudeConcertAiEvaluator>.Instance);

        ConcertEvaluationInput input = new()
        {
            ConcertName = "Rock Night",
            EventDate = "2026-07-15",
            Artists =
            [
                new ArtistEvaluationInput { Name = "The Headliner", IsHeadliner = true }
            ]
        };

        // Act
        ConcertEvaluationResult result = await evaluator.EvaluateAsync(input);

        // Assert
        result.Description.Should().Be("A high-voltage Texas night.");
        result.AiModel.Should().Be("claude-sonnet-4-20250514");
        result.ConcertVibes.Energy.Should().Be(10); // clamped down from 12
        result.ConcertVibes.SoundQuality.Should().Be(1); // clamped up from 0
        result.ConcertVibes.Hype.Should().Be(8);
        result.ConcertVibes.ValueForMoney.Should().Be(1); // clamped up from -3
        result.ArtistVibes.Should().HaveCount(1);
        result.ArtistVibes[0].Visuals.Should().Be(10); // clamped down from 15
        result.ArtistVibes[0].Sound.Should().Be(7);
    }
}
