using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Commands;
using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Events;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.ExternalAudit;
using TxConcert.Domain.Features.Genres;
using TxConcert.Domain.Features.States;
using TxConcert.Domain.Features.Venues;
using TxConcert.Domain.Jobs;
using TxConcert.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<CommandOutboxEntity> CommandOutbox => Set<CommandOutboxEntity>();
    public DbSet<EventOutboxEntity> EventOutbox => Set<EventOutboxEntity>();
    public DbSet<ExternalAuditEntity> ExternalAudits => Set<ExternalAuditEntity>();
    public DbSet<JobExecutionLogEntity> JobExecutionLogs => Set<JobExecutionLogEntity>();
    public DbSet<AiPromptEntity> AiPrompts => Set<AiPromptEntity>();

    // Concert & Artist domain
    public DbSet<GenreEntity> Genres => Set<GenreEntity>();
    public DbSet<StateEntity> States => Set<StateEntity>();
    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();
    public DbSet<ArtistGenreEntity> ArtistGenres => Set<ArtistGenreEntity>();
    public DbSet<VenueEntity> Venues => Set<VenueEntity>();
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    public DbSet<ConcertArtistEntity> ConcertArtists => Set<ConcertArtistEntity>();
    public DbSet<ConcertDescriptionEntity> ConcertDescriptions => Set<ConcertDescriptionEntity>();
    public DbSet<ConcertArticleEntity> ConcertArticles => Set<ConcertArticleEntity>();
    public DbSet<ConcertVibeEntity> ConcertVibes => Set<ConcertVibeEntity>();
    public DbSet<ArtistVibeProfileEntity> ArtistVibeProfiles => Set<ArtistVibeProfileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft deletes on all BaseEntity-derived types
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            System.Linq.Expressions.ParameterExpression parameter =
                System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

            System.Linq.Expressions.MemberExpression property =
                System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.DeletedAt));

            System.Linq.Expressions.BinaryExpression filter =
                System.Linq.Expressions.Expression.Equal(property,
                    System.Linq.Expressions.Expression.Constant(null, property.Type));

            System.Linq.Expressions.LambdaExpression lambda =
                System.Linq.Expressions.Expression.Lambda(filter, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
