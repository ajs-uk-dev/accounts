using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantTestRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "practice_operations");

            migrationBuilder.CreateTable(
                name: "tenant_test_rows",
                schema: "practice_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    firm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_test_rows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_test_rows_firm_id",
                schema: "practice_operations",
                table: "tenant_test_rows",
                column: "firm_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_test_rows",
                schema: "practice_operations");
        }
    }
}
