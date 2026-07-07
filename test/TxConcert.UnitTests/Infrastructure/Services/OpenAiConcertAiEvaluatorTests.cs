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

public class OpenAiConcertAiEvaluatorTests
{
    [Fact]
    public async Task EvaluateAsync_ValidJsonResponse_ReturnsParsedResultWithClampedScores()
    {
        // Arrange
        const string openAiPayload = """
        {
          "id": "chatcmpl-1",
          "object": "chat.completion",
          "model": "gpt-4o",
          "usage": { "prompt_tokens": 20, "completion_tokens": 80 },
          "choices": [
            {
              "index": 0,
              "message": {
                "role": "assistant",
                "content": "```json\n{\n  \"description\": \"Texas rocks tonight.\",\n  \"concertVibes\": { \"energy\": 11, \"soundQuality\": 0, \"hype\": 7, \"crowdVibe\": 8, \"valueForMoney\": 9, \"summary\": \"Loud.\" },\n  \"artistVibes\": [ { \"artistName\": \"The Headliner\", \"visuals\": -5, \"sound\": 8, \"energy\": 9, \"fanInteraction\": 7, \"texasSpirit\": 10, \"summary\": \"Rocks.\" } ]\n}\n```"
              }
            }
          ]
        }
        """;

        var handler = new Mock<HttpMessageHandler>();
        handler.SetupJsonResponse(HttpStatusCode.OK, openAiPayload);

        var braveSearch = new Mock<IBraveSearchService>();
        braveSearch.Setup(b => b.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebSearchResult([]));

        var promptProvider = new Mock<IAiPromptProvider>();
        promptProvider.Setup(p => p.GetConcertEvaluationPromptAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiPromptContent("system prompt", "user prompt instructions", "test", 1));

        var settings = Options.Create(new OpenAiSettings
        {
            ApiKey = "test-key",
            Model = "gpt-4o",
            MaxTokens = 2048
        });

        OpenAiConcertAiEvaluator evaluator = new(
            handler.ToClient(),
            braveSearch.Object,
            promptProvider.Object,
            settings,
            NullLogger<OpenAiConcertAiEvaluator>.Instance);

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
        result.Description.Should().Be("Texas rocks tonight.");
        result.AiModel.Should().Be("gpt-4o");
        result.ConcertVibes.Energy.Should().Be(10); // clamped down from 11
        result.ConcertVibes.SoundQuality.Should().Be(1); // clamped up from 0
        result.ArtistVibes.Should().HaveCount(1);
        result.ArtistVibes[0].Visuals.Should().Be(1); // clamped up from -5
        result.ArtistVibes[0].TexasSpirit.Should().Be(10);
    }
}
