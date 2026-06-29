using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.Project
{
    /// <inheritdoc />
    public partial class InitialProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "yhschema.Project");

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                schema: "yhschema.Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
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
                    table.PrimaryKey("PK_ProjectMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "yhschema.Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DescriptionText = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DescriptionHtml = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Network = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    Identifier = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectLeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultAssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Emoji = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IconProp = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    LogoProps = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "UTC"),
                    ModuleViewEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CycleViewEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IssueViewsViewEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PageViewEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IntakeViewEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    GuestViewAllFeatures = table.Column<bool>(type: "boolean", nullable: false),
                    IsTimeTrackingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsIssueTypeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveIn = table.Column<int>(type: "integer", nullable: false),
                    CloseIn = table.Column<int>(type: "integer", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalSource = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
                    TenantId = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_Project_Active",
                schema: "yhschema.Project",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_Tenant_Project_User",
                schema: "yhschema.Project",
                table: "ProjectMembers",
                columns: new[] { "TenantId", "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_Tenant_User_Active",
                schema: "yhschema.Project",
                table: "ProjectMembers",
                columns: new[] { "TenantId", "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ArchivedAt_SortOrder",
                schema: "yhschema.Project",
                table: "Projects",
                columns: new[] { "ArchivedAt", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Owner",
                schema: "yhschema.Project",
                table: "Projects",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Slug",
                schema: "yhschema.Project",
                table: "Projects",
                columns: new[] { "Slug", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Tenant_Identifier",
                schema: "yhschema.Project",
                table: "Projects",
                columns: new[] { "TenantId", "Identifier" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Tenant_Name",
                schema: "yhschema.Project",
                table: "Projects",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMembers",
                schema: "yhschema.Project");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "yhschema.Project");
        }
    }
}
