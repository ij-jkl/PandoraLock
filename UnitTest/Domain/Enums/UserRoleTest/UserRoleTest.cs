using NUnit.Framework;
using FluentAssertions;
using Domain.Enums;

namespace UnitTest.Domain.Enums.UserRoleTest;

[TestFixture]
public class UserRoleTest
{
    [Test]
    public void UserRole_HasCorrectValues()
    {
        UserRole.User.Should().Be(UserRole.User);
        UserRole.Admin.Should().Be(UserRole.Admin);
    }

    [Test]
    public void UserRole_ToString_ReturnsCorrectNames()
    {
        UserRole.User.ToString().Should().Be("User");
        UserRole.Admin.ToString().Should().Be("Admin");
    }

    [Test]
    public void UserRole_EnumValues_AreSequential()
    {
        ((int)UserRole.User).Should().Be(0);
        ((int)UserRole.Admin).Should().Be(1);
    }

    [Test]
    public void UserRole_Parse_WorksCorrectly()
    {
        Enum.Parse<UserRole>("User").Should().Be(UserRole.User);
        Enum.Parse<UserRole>("Admin").Should().Be(UserRole.Admin);
    }

    [Test]
    public void UserRole_TryParse_WorksCorrectly()
    {
        Enum.TryParse<UserRole>("User", out var userRole).Should().BeTrue();
        userRole.Should().Be(UserRole.User);
        
        Enum.TryParse<UserRole>("Admin", out var adminRole).Should().BeTrue();
        adminRole.Should().Be(UserRole.Admin);
        
        Enum.TryParse<UserRole>("Invalid", out var invalidRole).Should().BeFalse();
    }
}
