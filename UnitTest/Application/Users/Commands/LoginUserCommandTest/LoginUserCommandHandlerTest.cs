using NUnit.Framework;
using Moq;
using FluentAssertions;
using Application.Users.Commands;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Application.Users.Commands.LoginUserCommandTest;

[TestFixture]
public class LoginUserCommandHandlerTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<ITokenService> _tokenServiceMock;
    private LoginUserCommandHandler _handler;
    private UserEntity _testUser;
    private readonly TimeZoneInfo _argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new LoginUserCommandHandler(_userRepositoryMock.Object, _tokenServiceMock.Object);
        
        _testUser = new UserEntity
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword123"),
            Role = UserRole.User,
            FailedLoginAttempts = 0,
            IsLocked = false,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone),
            LastLoginAt = null
        };
    }

    [Test]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        var command = new LoginUserCommand("testuser", "ValidPassword123");
        var expectedToken = "valid.jwt.token";
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ReturnsAsync(_testUser);
        _tokenServiceMock.Setup(x => x.CreateAccessToken(_testUser))
            .Returns(expectedToken);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) => user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("Login successful");
        result.Response.Should().NotBeNull();
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            u.FailedLoginAttempts == 0 && 
            u.LastLoginAt.HasValue)), Times.Once);
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsUnauthorized()
    {
        var command = new LoginUserCommand("nonexistent", "password");
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("nonexistent"))
            .ReturnsAsync((UserEntity?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("Invalid username/email or password");
        result.Response.Should().BeNull();
    }

    [Test]
    public async Task Handle_AccountLocked_ReturnsForbidden()
    {
        var command = new LoginUserCommand("testuser", "ValidPassword123");
        _testUser.IsLocked = true;
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ReturnsAsync(_testUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Account is locked due to too many failed login attempts");
        result.Response.Should().BeNull();
    }

    [Test]
    public async Task Handle_InvalidPassword_IncrementsFailedAttempts()
    {
        var command = new LoginUserCommand("testuser", "WrongPassword");
        _testUser.FailedLoginAttempts = 2;
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ReturnsAsync(_testUser);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) => user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("Invalid username/email or password. 2 attempts remaining");
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            u.FailedLoginAttempts == 3)), Times.Once);
    }

    [Test]
    public async Task Handle_FifthFailedAttempt_LocksAccount()
    {
        var command = new LoginUserCommand("testuser", "WrongPassword");
        _testUser.FailedLoginAttempts = 4;
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ReturnsAsync(_testUser);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) => user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("Invalid username/email or password. Account has been locked after 5 failed attempts");
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            u.FailedLoginAttempts == 5 && u.IsLocked == true)), Times.Once);
    }

    [Test]
    public async Task Handle_SuccessfulLoginAfterFailedAttempts_ResetsCounter()
    {
        var command = new LoginUserCommand("testuser", "ValidPassword123");
        var expectedToken = "valid.jwt.token";
        _testUser.FailedLoginAttempts = 3;
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ReturnsAsync(_testUser);
        _tokenServiceMock.Setup(x => x.CreateAccessToken(_testUser))
            .Returns(expectedToken);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) => user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            u.FailedLoginAttempts == 0)), Times.Once);
    }

    [Test]
    public async Task Handle_ArgentinaTimezone_SetsCorrectLastLoginTime()
    {
        var command = new LoginUserCommand("testuser", "ValidPassword123");
        var expectedToken = "valid.jwt.token";
        var beforeLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ReturnsAsync(_testUser);
        _tokenServiceMock.Setup(x => x.CreateAccessToken(_testUser))
            .Returns(expectedToken);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) => user);

        var result = await _handler.Handle(command, CancellationToken.None);
        var afterLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            u.LastLoginAt.HasValue && 
            u.LastLoginAt.Value >= beforeLogin && 
            u.LastLoginAt.Value <= afterLogin)), Times.Once);
    }

    [Test]
    public async Task Handle_ExceptionThrown_ReturnsServerError()
    {
        var command = new LoginUserCommand("testuser", "ValidPassword123");
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("testuser"))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(500);
        result.Message.Should().StartWith("Exception during login:");
        result.Response.Should().BeNull();
    }
}
