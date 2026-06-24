using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.WorkItems
{
    /// <inheritdoc />
    public partial class AddModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleIssues",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleLinks",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleMembers",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "planned"),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TargetDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgressSnapshot = table.Column<string>(type: "text", nullable: true),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LogoProps = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleIssues_Tenant_Module_Issue",
                schema: "yhschema.WorkItems",
                table: "ModuleIssues",
                columns: new[] { "TenantId", "ModuleId", "IssueId" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleLinks_Tenant_Deleted_Module",
                schema: "yhschema.WorkItems",
                table: "ModuleLinks",
                columns: new[] { "TenantId", "IsDeleted", "ModuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleMembers_Tenant_Module_Member",
                schema: "yhschema.WorkItems",
                table: "ModuleMembers",
                columns: new[] { "TenantId", "ModuleId", "MemberId" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Tenant_Deleted_Project",
                schema: "yhschema.WorkItems",
                table: "Modules",
                columns: new[] { "TenantId", "IsDeleted", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Tenant_Project_Archived",
                schema: "yhschema.WorkItems",
                table: "Modules",
                columns: new[] { "TenantId", "ProjectId", "ArchivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Tenant_Project_Name",
                schema: "yhschema.WorkItems",
                table: "Modules",
                columns: new[] { "TenantId", "ProjectId", "Name" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Tenant_Project_SortOrder",
                schema: "yhschema.WorkItems",
                table: "Modules",
                columns: new[] { "TenantId", "ProjectId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleIssues",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "ModuleLinks",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "ModuleMembers",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "yhschema.WorkItems");
        }
    }
}
