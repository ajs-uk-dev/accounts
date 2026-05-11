using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEvents : Migration
    {
        private static readonly string[] FirmIdActionOccurredAtColumns = ["firm_id", "Action", "OccurredAt"];
        private static readonly string[] FirmIdOccurredAtColumns = ["firm_id", "OccurredAt"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "practice_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    firm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_firm_id_Action_OccurredAt",
                schema: "practice_operations",
                table: "audit_events",
                columns: FirmIdActionOccurredAtColumns);

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_firm_id_OccurredAt",
                schema: "practice_operations",
                table: "audit_events",
                columns: FirmIdOccurredAtColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events",
                schema: "practice_operations");
        }
    }
}
