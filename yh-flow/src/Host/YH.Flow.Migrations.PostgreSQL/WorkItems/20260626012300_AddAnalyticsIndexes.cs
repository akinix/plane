using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.WorkItems
{
    /// <inheritdoc />
    public partial class AddAnalyticsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Issues_Tenant_Project_CreatedAt_StateId",
                schema: "yhschema.WorkItems",
                table: "Issues",
                columns: new[] { "TenantId", "ProjectId", "CreatedOnUtc", "StateId" },
                filter: "[DeletedOnUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issues_Tenant_Project_CreatedAt_StateId",
                schema: "yhschema.WorkItems",
                table: "Issues");
        }
    }
}
