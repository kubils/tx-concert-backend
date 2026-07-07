using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using TxConcert.Domain.Common.Enums;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;

public sealed class EvaluateConcertsHandler(
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
    IOptions<AiSettings> aiSettings,
    ILogger<EvaluateConcertsHandler> logger)
    : IRequestHandler<EvaluateConcertsCommand, EvaluateConcertsResult>
{
    public async Task<EvaluateConcertsResult> Handle(
        EvaluateConcertsCommand request,
        CancellationToken ct)
    {
        AiSettings settings = aiSettings.Value;
        logger.LogInformation(
            "Starting concert evaluation batch with explicit IDs: {HasConcertIds}, requested batch size: {RequestedBatchSize}, default batch size: {DefaultBatchSize}",
            request.ConcertIds is { Count: > 0 },
            request.BatchSize,
            settings.BatchSize);

        IReadOnlyList<ConcertEntity> concerts = await GetConcertsToEvaluateAsync(request, settings, ct);

        if (concerts.Count == 0)
        {
            logger.LogInformation("No unevaluated concerts found");
            return new EvaluateConcertsResult(0, 0, 0, 0, 0);
        }

        logger.LogInformation("Evaluating {Count} concerts with AI", concerts.Count);

        int evaluated = 0, skipped = 0, failed = 0;
        int articlesGenerated = 0, articlesFailed = 0;

        foreach (ConcertEntity concert in concerts)
        {
            try
            {
                ConcertEvaluationInput input = await BuildInputAsync(concert, ct);

                if (input.Artists.Count == 0)
                {
                    logger.LogWarning("Skipping concert {ConcertId} — no artists linked", concert.Id);
                    skipped++;
                    continue;
                }

                ConcertEvaluationResult result = await aiEvaluator.EvaluateAsync(input, ct);

                await SaveResultAsync(concert, result, ct);
                evaluated++;

                logger.LogInformation("Evaluated concert {ConcertName} ({ConcertId})",
                    concert.Name, concert.Id);

                bool articleOk = await TryGenerateArticleAsync(concert, input, result, ct);
                if (articleOk) articlesGenerated++;
                else articlesFailed++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to evaluate concert {ConcertId}: {Name}",
                    concert.Id, concert.Name);
                failed++;
            }
        }

        logger.LogInformation(
            "AI evaluation complete: {Evaluated} evaluated, {Skipped} skipped, {Failed} failed, {ArticlesGenerated} articles, {ArticlesFailed} article failures",
            evaluated, skipped, failed, articlesGenerated, articlesFailed);

        return new EvaluateConcertsResult(evaluated, skipped, failed, articlesGenerated, articlesFailed);
    }

    private async Task<IReadOnlyList<ConcertEntity>> GetConcertsToEvaluateAsync(
        EvaluateConcertsCommand request,
        AiSettings settings,
        CancellationToken ct)
    {
        if (request.ConcertIds is not { Count: > 0 })
        {
            int batchSize = request.BatchSize ?? settings.BatchSize;
            logger.LogDebug("Loading up to {BatchSize} unevaluated concerts", batchSize);
            return await concertRepository.GetUnevaluatedAsync(batchSize, ct);
        }

        int limit = request.BatchSize ?? request.ConcertIds.Count;
        LocalDate today = clock.GetCurrentInstant().InUtc().Date;
        IReadOnlyList<ConcertEntity> concerts = await concertRepository.GetByIdsAsync(request.ConcertIds, ct);

        logger.LogDebug(
            "Loaded {LoadedCount} requested concerts by ID; filtering to limit {Limit}",
            concerts.Count,
            limit);

        return concerts
            .Where(concert => concert.EventDate >= today && concert.Status != ConcertStatus.Cancelled)
            .OrderBy(concert => concert.EventDate)
            .Take(limit)
            .ToList();
    }

    private async Task<bool> TryGenerateArticleAsync(
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
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Article generation failed for concert {ConcertId} — evaluation was still saved",
                concert.Id);
            return false;
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
        // Load venue
        VenueEntity? venue = await venueRepository.GetByIdAsync(concert.VenueId, ct);

        // Load artists via concert_artists join
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
                GenreName = null, // genre is resolved at sync time, not stored on artist
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

        // 1. Save concert description
        int nextVersion = await concertDescriptionRepository.GetNextVersionNumberAsync(concert.Id, ct);

        // Deactivate previous active description
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

        // Build artist lookup map: artist name → (artist ID, concert artist entity)
        var artistLookup = new Dictionary<string, (string ArtistId, ConcertArtistEntity ConcertArtist)>(
            StringComparer.OrdinalIgnoreCase);
        
        foreach (ConcertArtistEntity concertArtist in concertArtists)
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(concertArtist.ArtistId, ct);
            if (artist is not null)
            {
                artistLookup[artist.Name] = (artist.Id, concertArtist);
            }
        }

        foreach (ArtistVibeResult artistVibe in result.ArtistVibes)
        {
            // Look up artist ID by name
            if (!artistLookup.TryGetValue(artistVibe.ArtistName, out var artistInfo))
            {
                logger.LogWarning(
                    "Artist not found in concert: {ArtistName}",
                    artistVibe.ArtistName);
                continue;
            }

            string artistId = artistInfo.ArtistId;

            // Skip artists that already have a vibe profile — avoid overwriting with redundant data
            ArtistVibeProfileEntity? existingProfile =
                await artistVibeProfileRepository.GetByArtistIdAsync(artistId, ct);

            if (existingProfile is not null)
            {
                logger.LogDebug(
                    "Skipping artist vibe save for {ArtistName} — profile already exists",
                    artistVibe.ArtistName);
                continue;
            }

            ArtistVibeProfileEntity profile = ArtistVibeProfileEntity.Create(
                artistId,
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
