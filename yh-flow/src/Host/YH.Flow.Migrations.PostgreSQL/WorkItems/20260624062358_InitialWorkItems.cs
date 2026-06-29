using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.WorkItems
{
    /// <inheritdoc />
    public partial class InitialWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "yhschema.WorkItems");

            migrationBuilder.CreateTable(
                name: "Estimates",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLastUsed = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Estimates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_Labels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Labels_Labels_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "States",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_States", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstimatePoints",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimatePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstimatePoints_Estimates_EstimateId",
                        column: x => x.EstimateId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Estimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstimatePoints_Estimate_Key",
                schema: "yhschema.WorkItems",
                table: "EstimatePoints",
                columns: new[] { "EstimateId", "Key", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstimatePoints_Estimate_SortOrder",
                schema: "yhschema.WorkItems",
                table: "EstimatePoints",
                columns: new[] { "EstimateId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Estimates_Project_IsLastUsed",
                schema: "yhschema.WorkItems",
                table: "Estimates",
                columns: new[] { "ProjectId", "IsLastUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_Estimates_Tenant_Project",
                schema: "yhschema.WorkItems",
                table: "Estimates",
                columns: new[] { "TenantId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_Labels_ParentId",
                schema: "yhschema.WorkItems",
                table: "Labels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_Project_Parent",
                schema: "yhschema.WorkItems",
                table: "Labels",
                columns: new[] { "ProjectId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Labels_Tenant_Project_Name",
                schema: "yhschema.WorkItems",
                table: "Labels",
                columns: new[] { "TenantId", "ProjectId", "Name" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_States_Project_Group",
                schema: "yhschema.WorkItems",
                table: "States",
                columns: new[] { "ProjectId", "Group" });

            migrationBuilder.CreateIndex(
                name: "IX_States_Project_IsDefault",
                schema: "yhschema.WorkItems",
                table: "States",
                columns: new[] { "ProjectId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_States_Tenant_Project_Name",
                schema: "yhschema.WorkItems",
                table: "States",
                columns: new[] { "TenantId", "ProjectId", "Name" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstimatePoints",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "Labels",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "States",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "Estimates",
                schema: "yhschema.WorkItems");
        }
    }
}
