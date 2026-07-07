using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TxConcert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "command_outbox",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Command = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    PublishedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    ProcessedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: true),
                    IsRetry = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_outbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_outbox",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    PublishedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    ProcessedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: true),
                    ProcessedBy = table.Column<List<string>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_outbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "external_audits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    RequestBody = table.Column<string>(type: "text", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CanRetry = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Latest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ResponseTime = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "genres",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "states",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "US"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Bio = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OriginCity = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OriginStateId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OriginCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "US"),
                    PopularityScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SpotifyUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    InstagramUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_artists_states_OriginStateId",
                        column: x => x.OriginStateId,
                        principalTable: "states",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "venues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StateId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "US"),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    VenueType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_venues_states_StateId",
                        column: x => x.StateId,
                        principalTable: "states",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "artist_genres",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GenreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artist_genres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_artist_genres_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_artist_genres_genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "artist_vibe_profiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Visuals = table.Column<int>(type: "integer", nullable: false),
                    Sound = table.Column<int>(type: "integer", nullable: false),
                    Energy = table.Column<int>(type: "integer", nullable: false),
                    FanInteraction = table.Column<int>(type: "integer", nullable: false),
                    TexasSpirit = table.Column<int>(type: "integer", nullable: false),
                    AiSummary = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    AiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artist_vibe_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_artist_vibe_profiles_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "concerts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    VenueId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventDate = table.Column<LocalDate>(type: "date", nullable: false),
                    DoorsOpen = table.Column<LocalTime>(type: "time", nullable: true),
                    StartTime = table.Column<LocalTime>(type: "time", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "America/Chicago"),
                    ConcertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TicketUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PriceMin = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    PriceMax = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsSoldOut = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RescheduledFromId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concerts_concerts_RescheduledFromId",
                        column: x => x.RescheduledFromId,
                        principalTable: "concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_concerts_venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "concert_artists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConcertId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsHeadliner = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BillingOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SetTime = table.Column<LocalTime>(type: "time", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concert_artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concert_artists_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_concert_artists_concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "concert_descriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConcertId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    AiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PromptUsed = table.Column<string>(type: "text", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    GeneratedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    TokensUsed = table.Column<int>(type: "integer", nullable: true),
                    GenerationTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concert_descriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concert_descriptions_concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "concert_vibes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConcertId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Energy = table.Column<int>(type: "integer", nullable: false),
                    SoundQuality = table.Column<int>(type: "integer", nullable: false),
                    Hype = table.Column<int>(type: "integer", nullable: false),
                    CrowdVibe = table.Column<int>(type: "integer", nullable: false),
                    ValueForMoney = table.Column<int>(type: "integer", nullable: false),
                    AiSummary = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    AiModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedById = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedByName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concert_vibes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concert_vibes_concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "genres",
                columns: new[] { "Id", "CreatedAt", "CreatedById", "CreatedByName", "DeletedAt", "ModifiedById", "ModifiedByName", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { "genre_a1b2c3d4000140008000000000000001", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Country & Western", "country-western", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) },
                    { "genre_a1b2c3d4000240008000000000000002", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Pop & Rock", "pop-rock", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) },
                    { "genre_a1b2c3d4000340008000000000000003", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Hip-Hop & R&B", "hip-hop-rnb", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) },
                    { "genre_a1b2c3d4000440008000000000000004", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Latin & Tejano", "latin-tejano", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) },
                    { "genre_a1b2c3d4000540008000000000000005", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Electronic & Dance", "electronic-dance", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) },
                    { "genre_a1b2c3d4000640008000000000000006", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Jazz & Blues", "jazz-blues", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) },
                    { "genre_a1b2c3d4000740008000000000000007", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L), null, null, null, null, null, "Classical & Opera", "classical-opera", NodaTime.Instant.FromUnixTimeTicks(17756064000000000L) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_artist_genres_ArtistId_GenreId",
                table: "artist_genres",
                columns: new[] { "ArtistId", "GenreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_artist_genres_DeletedAt",
                table: "artist_genres",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_artist_genres_GenreId",
                table: "artist_genres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_artist_vibe_profiles_ArtistId",
                table: "artist_vibe_profiles",
                column: "ArtistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_artist_vibe_profiles_DeletedAt",
                table: "artist_vibe_profiles",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_artists_DeletedAt",
                table: "artists",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_artists_IsActive",
                table: "artists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_artists_OriginStateId",
                table: "artists",
                column: "OriginStateId");

            migrationBuilder.CreateIndex(
                name: "IX_artists_Slug",
                table: "artists",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_command_outbox_Command",
                table: "command_outbox",
                column: "Command");

            migrationBuilder.CreateIndex(
                name: "IX_command_outbox_DeletedAt",
                table: "command_outbox",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_command_outbox_ProcessedOn",
                table: "command_outbox",
                column: "ProcessedOn");

            migrationBuilder.CreateIndex(
                name: "IX_command_outbox_PublishedOn",
                table: "command_outbox",
                column: "PublishedOn");

            migrationBuilder.CreateIndex(
                name: "IX_concert_artists_ArtistId",
                table: "concert_artists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_concert_artists_ConcertId_ArtistId",
                table: "concert_artists",
                columns: new[] { "ConcertId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_concert_artists_ConcertId_BillingOrder",
                table: "concert_artists",
                columns: new[] { "ConcertId", "BillingOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_concert_artists_DeletedAt",
                table: "concert_artists",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_concert_descriptions_ConcertId",
                table: "concert_descriptions",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_concert_descriptions_ConcertId_IsActive",
                table: "concert_descriptions",
                columns: new[] { "ConcertId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_concert_descriptions_ConcertId_VersionNumber",
                table: "concert_descriptions",
                columns: new[] { "ConcertId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_concert_descriptions_DeletedAt",
                table: "concert_descriptions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_concert_vibes_ConcertId",
                table: "concert_vibes",
                column: "ConcertId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_concert_vibes_DeletedAt",
                table: "concert_vibes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_ConcertType",
                table: "concerts",
                column: "ConcertType");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_DeletedAt",
                table: "concerts",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_EventDate",
                table: "concerts",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_IsFeatured",
                table: "concerts",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_RescheduledFromId",
                table: "concerts",
                column: "RescheduledFromId");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_Slug",
                table: "concerts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_concerts_Status",
                table: "concerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_concerts_VenueId",
                table: "concerts",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_event_outbox_DeletedAt",
                table: "event_outbox",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_event_outbox_EventName",
                table: "event_outbox",
                column: "EventName");

            migrationBuilder.CreateIndex(
                name: "IX_event_outbox_PublishedOn",
                table: "event_outbox",
                column: "PublishedOn");

            migrationBuilder.CreateIndex(
                name: "IX_external_audits_CorrelationId",
                table: "external_audits",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_external_audits_DeletedAt",
                table: "external_audits",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_external_audits_Latest",
                table: "external_audits",
                column: "Latest");

            migrationBuilder.CreateIndex(
                name: "IX_external_audits_TargetId",
                table: "external_audits",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_external_audits_Type",
                table: "external_audits",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_genres_DeletedAt",
                table: "genres",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_genres_Name",
                table: "genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_genres_Slug",
                table: "genres",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_states_Abbreviation",
                table: "states",
                column: "Abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_states_DeletedAt",
                table: "states",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_states_IsActive",
                table: "states",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_venues_City_StateId",
                table: "venues",
                columns: new[] { "City", "StateId" });

            migrationBuilder.CreateIndex(
                name: "IX_venues_DeletedAt",
                table: "venues",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_venues_Slug",
                table: "venues",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venues_StateId",
                table: "venues",
                column: "StateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artist_genres");

            migrationBuilder.DropTable(
                name: "artist_vibe_profiles");

            migrationBuilder.DropTable(
                name: "command_outbox");

            migrationBuilder.DropTable(
                name: "concert_artists");

            migrationBuilder.DropTable(
                name: "concert_descriptions");

            migrationBuilder.DropTable(
                name: "concert_vibes");

            migrationBuilder.DropTable(
                name: "event_outbox");

            migrationBuilder.DropTable(
                name: "external_audits");

            migrationBuilder.DropTable(
                name: "genres");

            migrationBuilder.DropTable(
                name: "artists");

            migrationBuilder.DropTable(
                name: "concerts");

            migrationBuilder.DropTable(
                name: "venues");

            migrationBuilder.DropTable(
                name: "states");
        }
    }
}
