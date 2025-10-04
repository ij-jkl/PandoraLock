using NUnit.Framework;
using FluentAssertions;
using Domain.Entities;
using Domain.Enums;

namespace UnitTest.Domain.Entities.UserEntityTest;

[TestFixture]
public class UserEntityTest
{
    [Test]
    public void UserEntity_DefaultValues_SetsCorrectDefaults()
    {
        var user = new UserEntity();

        user.Role.Should().Be(UserRole.User);
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
        user.CreatedAt.Should().BeCloseTo(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")), TimeSpan.FromMinutes(1));
        user.LastLoginAt.Should().BeNull();
    }

    [Test]
    public void UserEntity_SetProperties_WorksCorrectly()
    {
        var argentinaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        
        var user = new UserEntity
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.Admin,
            FailedLoginAttempts = 2,
            IsLocked = true,
            LastLoginAt = argentinaTime
        };

        user.Id.Should().Be(1);
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashedpassword");
        user.Role.Should().Be(UserRole.Admin);
        user.FailedLoginAttempts.Should().Be(2);
        user.IsLocked.Should().BeTrue();
        user.LastLoginAt.Should().Be(argentinaTime);
    }

    [Test]
    public void UserEntity_ArgentinaTimezone_CreatedAtUsesCorrectTimezone()
    {
        var beforeCreation = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        var user = new UserEntity();
        var afterCreation = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        user.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        user.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Test]
    public void UserEntity_RequiredProperties_CanBeSet()
    {
        var user = new UserEntity
        {
            Username = "required_username",
            Email = "required@email.com",
            PasswordHash = "required_hash"
        };

        user.Username.Should().Be("required_username");
        user.Email.Should().Be("required@email.com");
        user.PasswordHash.Should().Be("required_hash");
    }
}
