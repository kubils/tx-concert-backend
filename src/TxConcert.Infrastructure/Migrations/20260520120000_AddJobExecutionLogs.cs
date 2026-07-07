using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using TxConcert.Infrastructure.Persistence;

#nullable disable

namespace TxConcert.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260520120000_AddJobExecutionLogs")]
    public partial class AddJobExecutionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_execution_logs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    JobGroup = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TriggerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TriggerGroup = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FireInstanceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_job_execution_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_DeletedAt",
                table: "job_execution_logs",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_FireInstanceId",
                table: "job_execution_logs",
                column: "FireInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_JobName",
                table: "job_execution_logs",
                column: "JobName");

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_StartedAt",
                table: "job_execution_logs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_Status",
                table: "job_execution_logs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_execution_logs");
        }
    }
}
