using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TxConcert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcertArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataHash",
                table: "concerts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "concert_articles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConcertId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Spot = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    SeoKeywords = table.Column<List<string>>(type: "text[]", nullable: false),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ImageAltText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageCredit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ImageSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_concert_articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concert_articles_concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_concert_articles_ConcertId_VersionNumber",
                table: "concert_articles",
                columns: new[] { "ConcertId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_concert_articles_DeletedAt",
                table: "concert_articles",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "ix_concert_articles_concert_id_active",
                table: "concert_articles",
                column: "ConcertId",
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "concert_articles");

            migrationBuilder.DropColumn(
                name: "DataHash",
                table: "concerts");
        }
    }
}
