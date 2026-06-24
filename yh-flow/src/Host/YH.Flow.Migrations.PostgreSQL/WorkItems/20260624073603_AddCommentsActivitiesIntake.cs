using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YH.Flow.Migrations.PostgreSQL.WorkItems
{
    /// <inheritdoc />
    public partial class AddCommentsActivitiesIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeIssues",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: -2),
                    SnoozedTill = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DuplicateToIssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "IN_APP"),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeIssues_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueActivities",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Verb = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IssueCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Epoch = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueComments",
                schema: "yhschema.WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentHtml = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CommentJson = table.Column<string>(type: "text", nullable: true),
                    CommentStripped = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EditedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_IssueComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueComments_IssueComments_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "yhschema.WorkItems",
                        principalTable: "IssueComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeIssues_IssueId",
                schema: "yhschema.WorkItems",
                table: "IntakeIssues",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeIssues_Project_Status",
                schema: "yhschema.WorkItems",
                table: "IntakeIssues",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeIssues_Tenant_Project",
                schema: "yhschema.WorkItems",
                table: "IntakeIssues",
                columns: new[] { "TenantId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueActivities_Issue_Epoch",
                schema: "yhschema.WorkItems",
                table: "IssueActivities",
                columns: new[] { "IssueId", "Epoch" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueActivities_Issue_Verb",
                schema: "yhschema.WorkItems",
                table: "IssueActivities",
                columns: new[] { "IssueId", "Verb" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueActivities_Tenant_Issue",
                schema: "yhschema.WorkItems",
                table: "IssueActivities",
                columns: new[] { "TenantId", "IssueId" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_Issue_CreatedOn",
                schema: "yhschema.WorkItems",
                table: "IssueComments",
                columns: new[] { "IssueId", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_ParentId",
                schema: "yhschema.WorkItems",
                table: "IssueComments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_Tenant_Deleted_Issue",
                schema: "yhschema.WorkItems",
                table: "IssueComments",
                columns: new[] { "TenantId", "IsDeleted", "IssueId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeIssues",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "IssueActivities",
                schema: "yhschema.WorkItems");

            migrationBuilder.DropTable(
                name: "IssueComments",
                schema: "yhschema.WorkItems");
        }
    }
}
