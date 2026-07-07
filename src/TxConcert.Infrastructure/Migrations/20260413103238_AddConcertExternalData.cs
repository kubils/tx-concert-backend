using Microsoft.EntityFrameworkCore.Migrations;
using TxConcert.Domain.Features.Concerts;

#nullable disable

namespace TxConcert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcertExternalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ConcertExternalData>(
                name: "ExternalData",
                table: "concerts",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalData",
                table: "concerts");
        }
    }
}
