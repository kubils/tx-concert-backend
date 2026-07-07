using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TxConcert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtractTicketmasterIdFromJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketmasterId",
                table: "concerts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            // Backfill from existing JSONB data
            migrationBuilder.Sql("""
                UPDATE concerts
                SET "TicketmasterId" = "ExternalData"->>'TicketmasterId'
                WHERE "ExternalData" IS NOT NULL
                  AND "ExternalData"->>'TicketmasterId' IS NOT NULL
                  AND "TicketmasterId" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_concerts_TicketmasterId",
                table: "concerts",
                column: "TicketmasterId",
                unique: true,
                filter: "\"TicketmasterId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_concerts_TicketmasterId",
                table: "concerts");

            migrationBuilder.DropColumn(
                name: "TicketmasterId",
                table: "concerts");
        }
    }
}
