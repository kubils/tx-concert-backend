using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using TxConcert.Infrastructure.Persistence;

#nullable disable

namespace TxConcert.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260520121000_AddAiPrompts")]
    public partial class AddAiPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_prompts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SystemPrompt = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    UserPromptInstructions = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ai_prompts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_prompts_DeletedAt",
                table: "ai_prompts",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ai_prompts_IsActive",
                table: "ai_prompts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ai_prompts_Key_VersionNumber",
                table: "ai_prompts",
                columns: new[] { "Key", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_prompts_key_active",
                table: "ai_prompts",
                column: "Key",
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_prompts");
        }
    }
}
