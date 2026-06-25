using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.Pages
{
    /// <inheritdoc />
    public partial class AddPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "yhschema.Page");

            migrationBuilder.CreateTable(
                name: "PageFavorites",
                schema: "yhschema.Page",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageFavorites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                schema: "yhschema.Page",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "text", nullable: true),
                    DescriptionStripped = table.Column<string>(type: "text", nullable: true),
                    DescriptionJson = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ViewProps = table.Column<string>(type: "text", nullable: true),
                    LogoProps = table.Column<string>(type: "text", nullable: true),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExternalSource = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_Pages_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "yhschema.Page",
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPages",
                schema: "yhschema.Page",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageFavorites_PageId",
                schema: "yhschema.Page",
                table: "PageFavorites",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageFavorites_Tenant_Page_User",
                schema: "yhschema.Page",
                table: "PageFavorites",
                columns: new[] { "TenantId", "PageId", "UserId" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ArchivedAt",
                schema: "yhschema.Page",
                table: "Pages",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ParentId",
                schema: "yhschema.Page",
                table: "Pages",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Project_SortOrder",
                schema: "yhschema.Page",
                table: "Pages",
                columns: new[] { "ProjectId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPages_Tenant_Project_Page",
                schema: "yhschema.Page",
                table: "ProjectPages",
                columns: new[] { "TenantId", "ProjectId", "PageId" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageFavorites",
                schema: "yhschema.Page");

            migrationBuilder.DropTable(
                name: "Pages",
                schema: "yhschema.Page");

            migrationBuilder.DropTable(
                name: "ProjectPages",
                schema: "yhschema.Page");
        }
    }
}
