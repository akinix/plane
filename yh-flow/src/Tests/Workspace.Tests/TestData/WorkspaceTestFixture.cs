namespace YH.Tests.Workspace.TestData;

/// <summary>
/// Shared test fixture scaffolding for Workspace module tests (Wave 0 of 02-01).
/// <b>Wave 0 status: scaffolded but inert.</b> The full fixture depends on
/// <c>YH.Modules.Workspace.Persistence.WorkspaceDbContext</c>, which is created in plan 02-02 (Wave 1).
/// Until then this class exists so the project compiles and downstream plans can extend it.
///
/// <b>Wave 1 (02-02) planned work</b>:
/// <list type="bullet">
///   <item>Add <c>WorkspaceDbContext CreateInMemoryContext()</c> using
///     <c>DbContextOptionsBuilder&lt;WorkspaceDbContext&gt;().UseInMemoryDatabase(...)</c>
///     (pattern at <c>Identity.Tests/Authorization/OAuthProviderFrameworkTests.cs:136-137</c>).</item>
///   <item>Seed a root user (Guid + email) per plan 02-01 Task 1 done-criteria.</item>
///   <item>Optionally swap InMemory for Testcontainers PostgreSQL in integration tests (02-03+).</item>
/// </list>
///
/// Implementation deferred per Wave 0 plan note (a) — spike tests only verify Finbuckle DI
/// registration behaviour and do not require a DbContext.
/// </summary>
public sealed class WorkspaceTestFixture : IDisposable
{
    /// <summary>
    /// Root user id seeded into the workspaceDbContext by Wave 1.
    /// Hard-coded here so Wave 0 tests that need a stable user id have a single source of truth;
    /// the actual DB seed arrives in 02-02.
    /// </summary>
    public static readonly Guid RootUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>Root user email used by Wave 0 scaffolding.</summary>
    public const string RootUserEmail = "root@example.com";

    public WorkspaceTestFixture()
    {
        // Wave 1 (02-02): construct WorkspaceDbContext via DbContextOptionsBuilder.UseInMemoryDatabase.
    }

    public void Dispose()
    {
        // Wave 1 (02-02): dispose the seeded WorkspaceDbContext / Testcontainers instance.
        GC.SuppressFinalize(this);
    }
}
