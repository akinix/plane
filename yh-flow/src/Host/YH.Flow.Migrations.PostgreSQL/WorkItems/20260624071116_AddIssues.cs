using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.WorkItems
{
    /// <inheritdoc />
    public partial class AddIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Issues",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    DescriptionJson = table.Column<string>(type: "text", nullable: true),
                    DescriptionStripped = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    Priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "none"),
                    SequenceId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<double>(type: "double precision", nullable: false, defaultValue: 65535.0),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StateId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstimatePointId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDraft = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_Issues_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IssueAssignees",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssigneeId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueAssignees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueAssignees_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueLabels",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueLabels_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueLinks",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedIssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LinkType = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueLinks_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueAssignees_IssueId",
                schema: "yhschema.WorkItems",
                table: "IssueAssignees",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueAssignees_Tenant_Issue_Assignee",
                schema: "yhschema.WorkItems",
                table: "IssueAssignees",
                columns: new[] { "TenantId", "IssueId", "AssigneeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueLabels_IssueId",
                schema: "yhschema.WorkItems",
                table: "IssueLabels",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueLabels_Tenant_Issue_Label",
                schema: "yhschema.WorkItems",
                table: "IssueLabels",
                columns: new[] { "TenantId", "IssueId", "LabelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_Issue_LinkType",
                schema: "yhschema.WorkItems",
                table: "IssueLinks",
                columns: new[] { "IssueId", "LinkType" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_RelatedIssue",
                schema: "yhschema.WorkItems",
                table: "IssueLinks",
                column: "RelatedIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentId",
                schema: "yhschema.WorkItems",
                table: "Issues",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Project_Parent",
                schema: "yhschema.WorkItems",
                table: "Issues",
                columns: new[] { "ProjectId", "ParentId" },
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Project_Priority",
                schema: "yhschema.WorkItems",
                table: "Issues",
                columns: new[] { "ProjectId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Project_Sequence",
                schema: "yhschema.WorkItems",
                table: "Issues",
                columns: new[] { "ProjectId", "SequenceId", "TenantId" },
                unique: true,
                filter: "[DeletedOnUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Project_State",
                schema: "yhschema.WorkItems",
                table: "Issues",
                columns: new[] { "ProjectId", "StateId" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Tenant_Deleted_Project",
                schema: "yhschema.WorkItems",
                table: "Issues",
                columns: new[] { "TenantId", "IsDeleted", "ProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueAssignees",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "IssueLabels",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "IssueLinks",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "Issues",
                schema: "yhschema.WorkItems");
        }
    }
}
