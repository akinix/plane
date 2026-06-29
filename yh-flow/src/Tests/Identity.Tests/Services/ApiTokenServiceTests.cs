using System.Security.Cryptography;
using System.Text;

namespace Identity.Tests.Services;

/// <summary>
/// Unit tests for API token key generation and hashing logic.
/// These tests validate the cryptographic operations used by the ApiTokenService
/// without requiring a full DbContext setup.
/// </summary>
public class ApiTokenServiceTests
{
    private const string KeyPrefix = "pk_";
    private const int KeyByteLength = 32;

    [Fact]
    public void GenerateRawKey_ShouldProduceCorrectFormat()
    {
        // Arrange & Act
        var rawKey = GenerateRawKey();

        // Assert
        rawKey.ShouldStartWith("pk_");
        rawKey.Length.ShouldBe(3 + 64); // "pk_" + 32 bytes = 64 hex chars
    }

    [Fact]
    public void GenerateRawKey_ShouldBeUnique()
    {
        // Act
        var key1 = GenerateRawKey();
        var key2 = GenerateRawKey();

        // Assert
        key1.ShouldNotBe(key2);
    }

    [Fact]
    public void HashToken_ShouldProduceConsistentHash()
    {
        // Arrange
        var rawKey = GenerateRawKey();

        // Act
        var hash1 = HashToken(rawKey);
        var hash2 = HashToken(rawKey);

        // Assert
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HashToken_ShouldProduceDifferentHashesForDifferentKeys()
    {
        // Arrange
        var key1 = GenerateRawKey();
        var key2 = GenerateRawKey();

        // Act
        var hash1 = HashToken(key1);
        var hash2 = HashToken(key2);

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void HashToken_ShouldProduceValidHex()
    {
        // Arrange
        var rawKey = GenerateRawKey();

        // Act
        var hash = HashToken(rawKey);

        // Assert
        hash.Length.ShouldBe(64); // SHA-256 = 32 bytes = 64 hex chars
        hash.ShouldSatisfyAllConditions(() =>
        {
            // Verify it's valid hex (uppercase)
            var bytes = Convert.FromHexString(hash);
            bytes.Length.ShouldBe(32);
        });
    }

    [Fact]
    public void HashToken_MatchesManualComputation()
    {
        // Arrange
        var rawKey = "pk_test123";

        // Act
        var hash = HashToken(rawKey);

        // Assert - verify against manual computation
        var expectedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        var expectedHash = Convert.ToHexString(expectedBytes);
        hash.ShouldBe(expectedHash);
    }

    [Fact]
    public void KeyPrefix_ShouldBeCorrectLength()
    {
        // Assert
        KeyPrefix.ShouldBe("pk_");
        KeyPrefix.Length.ShouldBe(3);
    }

    [Fact]
    public void GeneratedKey_ShouldContainOnlyHexCharsAfterPrefix()
    {
        // Act
        var rawKey = GenerateRawKey();
        var hexPart = rawKey[KeyPrefix.Length..];

        // Assert
        hexPart.ShouldSatisfyAllConditions(() =>
        {
            foreach (var c in hexPart)
            {
                ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')).ShouldBeTrue();
            }
        });
    }

    [Fact]
    public void Prefix_Extraction_ShouldTakeFirst8HexChars()
    {
        // Arrange
        var rawKey = GenerateRawKey();

        // Act
        var prefix = KeyPrefix + rawKey[KeyPrefix.Length..Math.Min(KeyPrefix.Length + 8, rawKey.Length)];

        // Assert
        prefix.ShouldStartWith("pk_");
        prefix.Length.ShouldBe(3 + 8); // "pk_" + 8 hex chars
    }

    // Copy of the static helper methods from ApiTokenService for unit testing
    private static string GenerateRawKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(KeyByteLength);
        var hex = Convert.ToHexString(bytes);
        return KeyPrefix + hex;
    }

    private static string HashToken(string rawKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
