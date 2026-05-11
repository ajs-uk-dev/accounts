using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "practice_operations");

            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "practice_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "firms",
                schema: "practice_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_firms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_test_rows",
                schema: "practice_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_test_rows", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "practice_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    totp_enrolled = table.Column<bool>(type: "boolean", nullable: false),
                    totp_secret = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    last_sign_in_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failed_sign_in_attempts = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_firm_id_action_occurred_at",
                schema: "practice_operations",
                table: "audit_events",
                columns: new[] { "firm_id", "action", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_firm_id_occurred_at",
                schema: "practice_operations",
                table: "audit_events",
                columns: new[] { "firm_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_firms_slug",
                schema: "practice_operations",
                table: "firms",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_test_rows_firm_id",
                schema: "practice_operations",
                table: "tenant_test_rows",
                column: "firm_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_firm_id",
                schema: "practice_operations",
                table: "users",
                column: "firm_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_firm_id_email",
                schema: "practice_operations",
                table: "users",
                columns: new[] { "firm_id", "email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events",
                schema: "practice_operations");

            migrationBuilder.DropTable(
                name: "firms",
                schema: "practice_operations");

            migrationBuilder.DropTable(
                name: "tenant_test_rows",
                schema: "practice_operations");

            migrationBuilder.DropTable(
                name: "users",
                schema: "practice_operations");
        }
    }
}
