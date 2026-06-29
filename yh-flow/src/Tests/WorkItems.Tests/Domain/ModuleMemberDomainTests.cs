namespace YH.Tests.WorkItems.Domain;

/// <summary>
/// Domain unit tests for <see cref="ModuleMember"/> entity.
/// Covers creation and validation of the module-member M2M through entity.
/// </summary>
public sealed class ModuleMemberDomainTests
{
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid ModuleId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var moduleMember = ModuleMember.Create(MemberId, ModuleId);

        moduleMember.MemberId.ShouldBe(MemberId);
        moduleMember.ModuleId.ShouldBe(ModuleId);
        moduleMember.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyMemberId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ModuleMember.Create(Guid.Empty, ModuleId));
    }

    [Fact]
    public void Create_WithEmptyModuleId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            ModuleMember.Create(MemberId, Guid.Empty));
    }

    [Fact]
    public void SoftDelete_SetsDeletedFlag()
    {
        var moduleMember = ModuleMember.Create(MemberId, ModuleId);
        var now = DateTimeOffset.UtcNow;

        moduleMember.SoftDelete(now);

        moduleMember.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void IsDeleted_DefaultIsFalse()
    {
        var moduleMember = ModuleMember.Create(MemberId, ModuleId);

        moduleMember.IsDeleted.ShouldBeFalse();
    }
}
