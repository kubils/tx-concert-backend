using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.Application.Features.Concerts.Commands.EvaluateConcertById;

public sealed class EvaluateConcertByIdHandler(
    IConcertRepository concertRepository,
    IConcertArtistRepository concertArtistRepository,
    IArtistRepository artistRepository,
    IVenueRepository venueRepository,
    IConcertDescriptionRepository concertDescriptionRepository,
    IConcertVibeRepository concertVibeRepository,
    IArtistVibeProfileRepository artistVibeProfileRepository,
    IConcertArticleRepository concertArticleRepository,
    IConcertAiEvaluator aiEvaluator,
    IConcertArticleWriter articleWriter,
    ISpotifyService spotifyService,
    IClock clock,
    ILogger<EvaluateConcertByIdHandler> logger)
    : IRequestHandler<EvaluateConcertByIdCommand, EvaluateConcertByIdResult>
{
    public async Task<EvaluateConcertByIdResult> Handle(
        EvaluateConcertByIdCommand request,
        CancellationToken ct)
    {
        ConcertEntity concert = await concertRepository.GetByIdAsync(request.ConcertId, ct)
            ?? throw new KeyNotFoundException($"Concert '{request.ConcertId}' not found");

        logger.LogInformation("Evaluating concert {ConcertId} ({ConcertName}) with AI",
            concert.Id, concert.Name);

        ConcertEvaluationInput input = await BuildInputAsync(concert, ct);

        if (input.Artists.Count == 0)
            throw new InvalidOperationException(
                $"Concert '{concert.Name}' has no linked artists — cannot evaluate");

        ConcertEvaluationResult result = await aiEvaluator.EvaluateAsync(input, ct);

        await SaveResultAsync(concert, result, ct);

        logger.LogInformation("AI evaluation complete for concert {ConcertId}", concert.Id);

        (bool articleGenerated, string? articleTitle, string? articleError) =
            await TryGenerateArticleAsync(concert, input, result, ct);

        return new EvaluateConcertByIdResult(
            concert.Id,
            concert.Name,
            result.Description,
            new ConcertVibeScoresDto(
                result.ConcertVibes.Energy,
                result.ConcertVibes.SoundQuality,
                result.ConcertVibes.Hype,
                result.ConcertVibes.CrowdVibe,
                result.ConcertVibes.ValueForMoney,
                result.ConcertVibes.Summary),
            result.ArtistVibes.Select(a => new ArtistVibeDto(
                a.ArtistName,
                a.Visuals,
                a.Sound,
                a.Energy,
                a.FanInteraction,
                a.TexasSpirit,
                a.Summary)).ToList(),
            articleGenerated,
            articleTitle,
            articleError);
    }

    private async Task<(bool Generated, string? Title, string? Error)> TryGenerateArticleAsync(
        ConcertEntity concert,
        ConcertEvaluationInput evalInput,
        ConcertEvaluationResult evalResult,
        CancellationToken ct)
    {
        try
        {
            ConcertArticleInput articleInput = await BuildArticleInputAsync(concert, evalInput, evalResult, ct);
            ConcertArticleResult articleResult = await articleWriter.WriteArticleAsync(articleInput, ct);
            await SaveArticleAsync(concert, articleResult, ct);

            logger.LogInformation(
                "Article generated for concert {ConcertId}: {Title}",
                concert.Id, articleResult.Title);

            return (true, articleResult.Title, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Article generation failed for concert {ConcertId} — evaluation was still saved",
                concert.Id);
            return (false, null, ex.Message);
        }
    }

    private async Task<ConcertArticleInput> BuildArticleInputAsync(
        ConcertEntity concert,
        ConcertEvaluationInput evalInput,
        ConcertEvaluationResult evalResult,
        CancellationToken ct)
    {
        var artistInputs = new List<ArticleArtistInput>();
        foreach (ArtistEvaluationInput ea in evalInput.Artists)
        {
            string? spotifyImage = ea.SpotifyUrl is not null
                ? await spotifyService.GetArtistImageUrlAsync(ea.SpotifyUrl, ct)
                : null;

            artistInputs.Add(new ArticleArtistInput
            {
                Name = ea.Name,
                GenreName = ea.GenreName,
                Bio = ea.Bio,
                IsHeadliner = ea.IsHeadliner,
                SpotifyImageUrl = spotifyImage,
                ArtistImageUrl = null
            });
        }

        return new ConcertArticleInput
        {
            ConcertName = evalInput.ConcertName,
            EventDate = evalInput.EventDate,
            StartTime = evalInput.StartTime,
            VenueName = evalInput.VenueName,
            VenueCity = evalInput.VenueCity,
            GenreName = evalInput.GenreName,
            SubGenreName = evalInput.SubGenreName,
            PriceMin = evalInput.PriceMin,
            PriceMax = evalInput.PriceMax,
            Info = evalInput.Info,
            PleaseNote = evalInput.PleaseNote,
            Artists = artistInputs,
            EvaluatorDescription = evalResult.Description,
            EvaluatorVibeSummary = evalResult.ConcertVibes.Summary,
            TicketmasterImageUrl = concert.ExternalData?.ImageUrl
        };
    }

    private async Task SaveArticleAsync(
        ConcertEntity concert,
        ConcertArticleResult result,
        CancellationToken ct)
    {
        Instant now = clock.GetCurrentInstant();
        int nextVersion = await concertArticleRepository.GetNextVersionNumberAsync(concert.Id, ct);

        ConcertArticleEntity? active =
            await concertArticleRepository.GetActiveForConcertAsync(concert.Id, ct);
        if (active is not null)
        {
            active.Deactivate();
            await concertArticleRepository.UpsertAsync(active, ct);
        }

        ConcertArticleEntity article = ConcertArticleEntity.Create(
            concert.Id,
            result.Title,
            result.Spot,
            result.Body,
            result.SeoKeywords,
            result.MetaDescription,
            result.ImageUrl,
            result.ImageAltText,
            result.ImageCredit,
            result.ImageSource,
            result.AiModel,
            "concert-article-writer-v1",
            nextVersion,
            now,
            result.TokensUsed,
            result.GenerationTimeMs);
        article.Activate();
        await concertArticleRepository.UpsertAsync(article, ct);
    }

    private async Task<ConcertEvaluationInput> BuildInputAsync(ConcertEntity concert, CancellationToken ct)
    {
        VenueEntity? venue = await venueRepository.GetByIdAsync(concert.VenueId, ct);

        IReadOnlyList<ConcertArtistEntity> concertArtists =
            await concertArtistRepository.GetByConcertIdAsync(concert.Id, ct);

        var artistInputs = new List<ArtistEvaluationInput>();
        foreach (ConcertArtistEntity ca in concertArtists.OrderBy(ca => ca.BillingOrder))
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(ca.ArtistId, ct);
            if (artist is null) continue;

            // Check if the artist already has an AI-generated vibe profile
            ArtistVibeProfileEntity? existingProfile =
                await artistVibeProfileRepository.GetByArtistIdAsync(artist.Id, ct);

            artistInputs.Add(new ArtistEvaluationInput
            {
                Name = artist.Name,
                GenreName = null,
                Bio = artist.Bio,
                SpotifyUrl = artist.SpotifyUrl,
                IsHeadliner = ca.IsHeadliner,
                HasExistingVibeProfile = existingProfile is not null
            });
        }

        return new ConcertEvaluationInput
        {
            ConcertName = concert.Name,
            EventDate = concert.EventDate.ToString(),
            StartTime = concert.StartTime?.ToString(),
            VenueName = venue?.Name,
            VenueCity = venue?.City,
            GenreName = concert.ExternalData?.SegmentName,
            SubGenreName = concert.ExternalData?.SubGenreName,
            PriceMin = concert.PriceMin,
            PriceMax = concert.PriceMax,
            Info = concert.ExternalData?.Info,
            PleaseNote = concert.ExternalData?.PleaseNote,
            Artists = artistInputs
        };
    }

    private async Task SaveResultAsync(
        ConcertEntity concert,
        ConcertEvaluationResult result,
        CancellationToken ct)
    {
        string aiModel = result.AiModel;
        Instant now = clock.GetCurrentInstant();

        // 1. Save concert description (versioned)
        int nextVersion = await concertDescriptionRepository.GetNextVersionNumberAsync(concert.Id, ct);

        ConcertDescriptionEntity? activeDesc =
            await concertDescriptionRepository.GetActiveForConcertAsync(concert.Id, ct);
        activeDesc?.Deactivate();
        if (activeDesc is not null)
            await concertDescriptionRepository.UpsertAsync(activeDesc, ct);

        ConcertDescriptionEntity description = ConcertDescriptionEntity.Create(
            concert.Id,
            result.Description,
            aiModel,
            "concert-evaluation-v1",
            nextVersion,
            now);
        description.Activate();
        await concertDescriptionRepository.UpsertAsync(description, ct);

        // 2. Save concert vibe scores
        ConcertVibeEntity? existingVibe = await concertVibeRepository.GetByConcertIdAsync(concert.Id, ct);
        if (existingVibe is not null)
        {
            existingVibe.UpdateScores(
                result.ConcertVibes.Energy,
                result.ConcertVibes.SoundQuality,
                result.ConcertVibes.Hype,
                result.ConcertVibes.CrowdVibe,
                result.ConcertVibes.ValueForMoney,
                result.ConcertVibes.Summary,
                aiModel);
            await concertVibeRepository.UpsertAsync(existingVibe, ct);
        }
        else
        {
            ConcertVibeEntity vibe = ConcertVibeEntity.Create(
                concert.Id,
                result.ConcertVibes.Energy,
                result.ConcertVibes.SoundQuality,
                result.ConcertVibes.Hype,
                result.ConcertVibes.CrowdVibe,
                result.ConcertVibes.ValueForMoney,
                result.ConcertVibes.Summary,
                aiModel);
            await concertVibeRepository.UpsertAsync(vibe, ct);
        }

        // 3. Save artist vibe profiles — only for artists that don't already have one
        IReadOnlyList<ConcertArtistEntity> concertArtists =
            await concertArtistRepository.GetByConcertIdAsync(concert.Id, ct);

        foreach (ArtistVibeResult artistVibe in result.ArtistVibes)
        {
            ConcertArtistEntity? ca = null;
            foreach (ConcertArtistEntity concertArtist in concertArtists)
            {
                ArtistEntity? artist = await artistRepository.GetByIdAsync(concertArtist.ArtistId, ct);
                if (artist is not null &&
                    string.Equals(artist.Name, artistVibe.ArtistName, StringComparison.OrdinalIgnoreCase))
                {
                    ca = concertArtist;
                    break;
                }
            }

            if (ca is null) continue;

            // Skip artists that already have a vibe profile — avoid overwriting with redundant data
            ArtistVibeProfileEntity? existingProfile =
                await artistVibeProfileRepository.GetByArtistIdAsync(ca.ArtistId, ct);

            if (existingProfile is not null)
            {
                logger.LogDebug(
                    "Skipping artist vibe save for {ArtistName} — profile already exists",
                    artistVibe.ArtistName);
                continue;
            }

            ArtistVibeProfileEntity profile = ArtistVibeProfileEntity.Create(
                ca.ArtistId,
                artistVibe.Visuals,
                artistVibe.Sound,
                artistVibe.Energy,
                artistVibe.FanInteraction,
                artistVibe.TexasSpirit,
                artistVibe.Summary,
                aiModel);
            await artistVibeProfileRepository.UpsertAsync(profile, ct);
        }
    }
}
