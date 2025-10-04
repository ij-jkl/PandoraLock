using NUnit.Framework;
using FluentAssertions;
using Application.DTOs;

namespace UnitTest.Application.DTOs.UserDtosTest;

[TestFixture]
public class UserDtosTest
{
    [Test]
    public void CreateUserDto_SetProperties_WorksCorrectly()
    {
        var dto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        
        dto.Email.Should().Be("test@example.com");
        dto.Username.Should().Be("testuser");
        dto.Password.Should().Be("Password123!");
        dto.ConfirmPassword.Should().Be("Password123!");
    }

    [Test]
    public void LoginUserDto_SetProperties_WorksCorrectly()
    {
        var dto = new LoginUserDto
        {
            UsernameOrEmail = "testuser",
            Password = "Password123!"
        };
        
        dto.UsernameOrEmail.Should().Be("testuser");
        dto.Password.Should().Be("Password123!");
    }

    [Test]
    public void UserDto_SetProperties_WorksCorrectly()
    {
        var createdAt = DateTime.Now;
        var lastLoginAt = DateTime.Now.AddHours(-1);
        
        var dto = new UserDto
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            CreatedAt = createdAt,
            LastLoginAt = lastLoginAt
        };
        
        dto.Id.Should().Be(1);
        dto.Email.Should().Be("test@example.com");
        dto.Username.Should().Be("testuser");
        dto.CreatedAt.Should().Be(createdAt);
        dto.LastLoginAt.Should().Be(lastLoginAt);
    }

    [Test]
    public void LoginResponseDto_SetProperties_WorksCorrectly()
    {
        var userDto = new UserDto
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            CreatedAt = DateTime.Now
        };
        
        var dto = new LoginResponseDto
        {
            Token = "jwt.token.here",
            User = userDto
        };
        
        dto.Token.Should().Be("jwt.token.here");
        dto.User.Should().Be(userDto);
        dto.User.Id.Should().Be(1);
        dto.User.Username.Should().Be("testuser");
    }
}
