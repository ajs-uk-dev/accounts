using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditEventAllowsNullFirmAndSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "firm_id",
                schema: "practice_operations",
                table: "audit_events",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "subject",
                schema: "practice_operations",
                table: "audit_events",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subject",
                schema: "practice_operations",
                table: "audit_events");

            migrationBuilder.AlterColumn<Guid>(
                name: "firm_id",
                schema: "practice_operations",
                table: "audit_events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
