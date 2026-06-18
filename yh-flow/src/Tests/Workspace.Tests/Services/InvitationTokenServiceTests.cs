using Shouldly;
using YH.Modules.Workspace.Services;

namespace YH.Tests.Workspace.Services;

/// <summary>
/// Unit tests for <see cref="InvitationTokenService"/> static crypto surface (D-12, plan 02-05
/// Task 1, threats T-2-token [BLOCKING] + T-2-tokenleak).
/// </summary>
/// <remarks>
/// <para>
/// These are pure-static-method tests — no DbContext, no DI. They guard the
/// <c>GenerateToken</c> / <c>HashToken</c> crypto contract (CSPRNG raw, SHA-256 hex hash,
/// deterministic, no plaintext leakage) so a regression in those helpers fails fast here.
/// The Create/Validate flows are exercised by the integration tests in
/// <c>Integration/InvitationHashTests.cs</c> / <c>InvitationInvalidateTests.cs</c> /
/// <c>InvitationTtlTests.cs</c>.
/// </para>
/// </remarks>
public sealed class InvitationTokenServiceTests
{
    [Fact]
    public void GenerateToken_ReturnsDifferentRawEachCall()
    {
        // CSPRNG must produce distinct raw tokens on successive calls (no shared seed).
        var a = InvitationTokenService.GenerateToken();
        var b = InvitationTokenService.GenerateToken();

        a.RawToken.ShouldNotBe(b.RawToken);
        a.Hash.ShouldNotBe(b.Hash);
    }

    [Fact]
    public void HashToken_IsDeterministic_ForSameRaw()
    {
        var raw = "deadbeefcafebabe" + Guid.NewGuid().ToString("N");
        var hash1 = InvitationTokenService.HashToken(raw);
        var hash2 = InvitationTokenService.HashToken(raw);

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HashToken_DiffersFromRaw_NoPlaintextLeakage()
    {
        // T-2-tokenleak mitigation: the hash MUST NOT equal the raw token. If it did, storing
        // TokenHash would be equivalent to storing the raw token in plaintext.
        var (raw, hash) = InvitationTokenService.GenerateToken();

        hash.ShouldNotBe(raw);
        hash.ShouldNotContain(raw);
        raw.ShouldNotContain(hash);
    }

    [Fact]
    public void GenerateToken_ProducesHexLowercase_OfExpectedLength()
    {
        // 32 bytes → 64 lowercase hex chars. Matches ApiTokenService byte length.
        var (raw, _) = InvitationTokenService.GenerateToken();

        raw.Length.ShouldBe(InvitationTokenService.TokenHexLength);
        raw.ShouldBeLowerCasedHex();
    }

    [Fact]
    public void HashToken_Produces64CharLowercaseHex_Sha256()
    {
        var raw = "anything-fixed";
        var hash = InvitationTokenService.HashToken(raw);

        // SHA-256 = 32 bytes = 64 hex chars, lowercase per the conversion choice.
        hash.Length.ShouldBe(64);
        hash.ShouldBeLowerCasedHex();
    }
}

internal static class StringHexExtensions
{
    internal static void ShouldBeLowerCasedHex(this string value)
    {
        foreach (var c in value)
        {
            (char.IsDigit(c) || (c >= 'a' && c <= 'f'))
                .ShouldBeTrue($"expected lowercase hex char, got '{c}' in \"{value}\"");
        }
    }
}
