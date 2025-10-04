using NUnit.Framework;
using FluentAssertions;
using Infrastructure.Auth.BCrypt;

namespace UnitTest.Infrastructure.Auth.BCrypt.BcryptPasswordHasherTest;

[TestFixture]
public class BcryptPasswordHasherTest
{
    private BcryptPasswordHasher _passwordHasher;

    [SetUp]
    public void Setup()
    {
        _passwordHasher = new BcryptPasswordHasher();
    }

    [Test]
    public void Hash_ValidPassword_ReturnsHashedPassword()
    {
        var password = "TestPassword123";

        var hash = _passwordHasher.Hash(password);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2a$");
    }

    [Test]
    public void Hash_SamePassword_ReturnsDifferentHashes()
    {
        var password = "TestPassword123";

        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        hash1.Should().NotBe(hash2);
    }

    [Test]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var password = "TestPassword123";
        var hash = _passwordHasher.Hash(password);

        var result = _passwordHasher.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Test]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword456";
        var hash = _passwordHasher.Hash(password);

        var result = _passwordHasher.Verify(wrongPassword, hash);

        result.Should().BeFalse();
    }

    [Test]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var password = "TestPassword123";
        var hash = _passwordHasher.Hash(password);

        var result = _passwordHasher.Verify("", hash);

        result.Should().BeFalse();
    }

    [Test]
    public void Hash_NullPassword_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _passwordHasher.Hash(null!));
    }

    [Test]
    public void Hash_LongPassword_ProcessesSuccessfully()
    {
        var longPassword = new string('a', 1000);

        var hash = _passwordHasher.Hash(longPassword);

        hash.Should().NotBeNullOrEmpty();
        _passwordHasher.Verify(longPassword, hash).Should().BeTrue();
    }

    [Test]
    public void Hash_SpecialCharacters_ProcessesSuccessfully()
    {
        var specialPassword = "P@ssw0rd!#$%^&*()";

        var hash = _passwordHasher.Hash(specialPassword);

        hash.Should().NotBeNullOrEmpty();
        _passwordHasher.Verify(specialPassword, hash).Should().BeTrue();
    }
}
