using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.Views
{
    /// <inheritdoc />
    public partial class AddViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "yhschema.View");

            migrationBuilder.CreateTable(
                name: "ViewFavorites",
                schema: "yhschema.View",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewFavorites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Views",
                schema: "yhschema.View",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Query = table.Column<string>(type: "jsonb", nullable: true),
                    Filters = table.Column<string>(type: "jsonb", nullable: true),
                    DisplayFilters = table.Column<string>(type: "jsonb", nullable: true),
                    DisplayProperties = table.Column<string>(type: "jsonb", nullable: true),
                    RichFilters = table.Column<string>(type: "jsonb", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
                    LogoProps = table.Column<string>(type: "jsonb", nullable: true),
                    OwnedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Views", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewFavorites_Tenant_View_User",
                schema: "yhschema.View",
                table: "ViewFavorites",
                columns: new[] { "TenantId", "ViewId", "UserId" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ViewFavorites_ViewId",
                schema: "yhschema.View",
                table: "ViewFavorites",
                column: "ViewId");

            migrationBuilder.CreateIndex(
                name: "IX_Views_ArchivedAt",
                schema: "yhschema.View",
                table: "Views",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Views_OwnedBy",
                schema: "yhschema.View",
                table: "Views",
                column: "OwnedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Views_Project_SortOrder",
                schema: "yhschema.View",
                table: "Views",
                columns: new[] { "ProjectId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Views_ProjectId",
                schema: "yhschema.View",
                table: "Views",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewFavorites",
                schema: "yhschema.View");

            migrationBuilder.DropTable(
                name: "Views",
                schema: "yhschema.View");
        }
    }
}
