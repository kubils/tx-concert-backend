using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Cache;
using TxConcert.Domain.Commands;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Events;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.ExternalAudit;
using TxConcert.Domain.Features.Genres;
using TxConcert.Domain.Features.States;
using TxConcert.Domain.Features.Venues;
using TxConcert.Domain.Prompts;
using TxConcert.Domain.Storage;
using TxConcert.Infrastructure.Cache;
using TxConcert.Infrastructure.Messaging;
using TxConcert.Infrastructure.Messaging.Commands;
using TxConcert.Infrastructure.Messaging.Consumers;
using TxConcert.Infrastructure.Messaging.Events;
using TxConcert.Infrastructure.Persistence;
using TxConcert.Infrastructure.Persistence.Interceptors;
using TxConcert.Infrastructure.Persistence.Repositories;
using TxConcert.Infrastructure.Services;
using TxConcert.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace TxConcert.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));
        services.Configure<TicketmasterSettings>(configuration.GetSection(TicketmasterSettings.SectionName));
        services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));
        services.Configure<ClaudeSettings>(configuration.GetSection(ClaudeSettings.SectionName));
        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName));
        services.Configure<BraveSearchSettings>(configuration.GetSection(BraveSearchSettings.SectionName));
        services.Configure<SpotifySettings>(configuration.GetSection(SpotifySettings.SectionName));

        // NodaTime clock
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddScoped<IAiPromptProvider, AiPromptProvider>();

        // EF Core + PostgreSQL
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.UseNodaTime();
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsql.CommandTimeout(30);
                });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped(typeof(Domain.Common.Base.IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<CommandOutboxRepository>();
        services.AddScoped<EventOutboxRepository>();
        services.AddScoped<JobExecutionLogRepository>();
        services.AddScoped<ExternalAuditRepository>();
        services.AddScoped<IExternalAuditRepository, ExternalAuditRepository>();

        // Concert & Artist domain repositories
        services.AddScoped<GenreRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<StateRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<ArtistRepository>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<VenueRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<ConcertRepository>();
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<ConcertDescriptionRepository>();
        services.AddScoped<IConcertDescriptionRepository, ConcertDescriptionRepository>();
        services.AddScoped<ConcertVibeRepository>();
        services.AddScoped<IConcertVibeRepository, ConcertVibeRepository>();
        services.AddScoped<ArtistVibeProfileRepository>();
        services.AddScoped<IArtistVibeProfileRepository, ArtistVibeProfileRepository>();
        services.AddScoped<ConcertArtistRepository>();
        services.AddScoped<IConcertArtistRepository, ConcertArtistRepository>();
        services.AddScoped<ArtistGenreRepository>();
        services.AddScoped<IArtistGenreRepository, ArtistGenreRepository>();
        services.AddScoped<ConcertArticleRepository>();
        services.AddScoped<IConcertArticleRepository, ConcertArticleRepository>();

        // Redis
        // RedisSettings redisSettings = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>()
        //     ?? new RedisSettings();
        // services.AddSingleton<IConnectionMultiplexer>(_ =>
        //     ConnectionMultiplexer.Connect(redisSettings.ConnectionString));
        // services.AddSingleton<ICacheService, RedisCacheService>();

        // AI evaluator — provider selected via Ai:Provider config
        AiSettings aiSettings = configuration.GetSection(AiSettings.SectionName).Get<AiSettings>()
            ?? new AiSettings();

        if (aiSettings.Provider == AiProvider.OpenAi)
            services.AddHttpClient<IConcertAiEvaluator, OpenAiConcertAiEvaluator>();
        else
            services.AddHttpClient<IConcertAiEvaluator, ClaudeConcertAiEvaluator>();

        // AI article writer — Claude-only for now.
        services.AddHttpClient<IConcertArticleWriter, ClaudeConcertArticleWriter>();

        // Spotify Web API (artist image lookup for article generation)
        services.AddHttpClient<ISpotifyService, SpotifyService>()
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        // Ticketmaster HTTP client
        services.AddHttpClient<ITicketmasterService, TicketmasterService>()
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        // Brave Search HTTP client
        services.AddHttpClient<IBraveSearchService, BraveSearchService>()
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        // Current user
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Command / Event bus (API mode — saves to outbox)
        services.AddScoped<ICommandBus, QueueCommandBusService>();
        services.AddScoped<IEventBus, QueueEventBusService>();

        // Handler registries (singleton — command/event name → type mapping)
        services.AddSingleton<CommandHandlerRegistry>();
        services.AddSingleton<EventSubscriberRegistry>();

        return services;
    }

    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RabbitMqSettings rabbitSettings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<CommandMessageConsumer>();
            bus.AddConsumer<EventMessageConsumer>();

            bus.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitSettings.Host, rabbitSettings.Port, rabbitSettings.VHost, h =>
                {
                    h.Username(rabbitSettings.Username);
                    h.Password(rabbitSettings.Password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    public static IServiceCollection AddGoogleCloudStorage(this IServiceCollection services)
    {
        services.AddSingleton(Google.Cloud.Storage.V1.StorageClient.Create());
        services.AddSingleton<IStorageService, GoogleCloudStorageService>();
        return services;
    }
}
