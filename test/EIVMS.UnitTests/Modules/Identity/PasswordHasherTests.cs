using EIVMS.Infrastructure.Services;
using FluentAssertions;

namespace EIVMS.UnitTests.Modules.Identity;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        var password = "TestPassword@123";

        var hash = _hasher.HashPassword(password);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_SamPasswordShouldProduceDifferentHashes()
    {
        var password = "TestPassword@123";

        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "TestPassword@123";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
    {
        var password = "TestPassword@123";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword("WrongPassword@123", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        var hash = _hasher.HashPassword("TestPassword@123");

        var result = _hasher.VerifyPassword("", hash);

        result.Should().BeFalse();
    }
}