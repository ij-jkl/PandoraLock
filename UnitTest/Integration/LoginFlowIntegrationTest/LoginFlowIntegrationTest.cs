using NUnit.Framework;
using Moq;
using FluentAssertions;
using Application.Users.Commands;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Models;
using Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Integration.LoginFlowIntegrationTest;

[TestFixture]
public class LoginFlowIntegrationTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<ITokenService> _tokenServiceMock;
    private LoginUserCommandHandler _loginHandler;
    private CreateUserCommandHandler _createHandler;
    private readonly TimeZoneInfo _argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loginHandler = new LoginUserCommandHandler(_userRepositoryMock.Object, _tokenServiceMock.Object);
        _createHandler = new CreateUserCommandHandler(_userRepositoryMock.Object);
    }

    [Test]
    public async Task FullUserJourney_CreateAndLogin_WorksEndToEnd()
    {
        var createUserDto = new CreateUserDto
        {
            Email = "integration@test.com",
            Username = "integrationuser",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };
        var createCommand = new CreateUserCommand(createUserDto);

        var createdUser = new UserEntity
        {
            Id = 1,
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = UserRole.User,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone),
            FailedLoginAttempts = 0,
            IsLocked = false
        };

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(createUserDto.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(createUserDto.Username)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>())).ReturnsAsync(createdUser);

        var createResult = await _createHandler.Handle(createCommand, CancellationToken.None);

        createResult.Code.Should().Be(201);
        createResult.Message.Should().Be("User created successfully");

        var loginCommand = new LoginUserCommand("integrationuser", "TestPassword123!");
        var expectedToken = "integration.jwt.token";

        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("integrationuser")).ReturnsAsync(createdUser);
        _tokenServiceMock.Setup(x => x.CreateAccessToken(createdUser)).Returns(expectedToken);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>())).ReturnsAsync((UserEntity user) => user);

        var loginResult = await _loginHandler.Handle(loginCommand, CancellationToken.None);

        loginResult.Code.Should().Be(200);
        loginResult.Message.Should().Be("Login successful");
        
        var loginResponse = loginResult.Response as LoginResponseDto;
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().Be(expectedToken);
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Username.Should().Be("integrationuser");
    }

    [Test]
    public async Task TimezoneHandling_ArgentinaTime_IsUsedCorrectly()
    {
        var user = new UserEntity
        {
            Id = 1,
            Username = "timezoneuser",
            Email = "timezone@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword123!"),
            Role = UserRole.User,
            FailedLoginAttempts = 0,
            IsLocked = false,
            LastLoginAt = null
        };

        var beforeLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);
        var command = new LoginUserCommand("timezoneuser", "ValidPassword123!");
        
        _userRepositoryMock.Setup(x => x.GetByUsernameOrEmailAsync("timezoneuser")).ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.CreateAccessToken(user)).Returns("timezone.token");
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UserEntity>())).ReturnsAsync((UserEntity user) => user);

        var result = await _loginHandler.Handle(command, CancellationToken.None);
        var afterLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);

        result.Code.Should().Be(200);
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            u.LastLoginAt.HasValue && 
            u.LastLoginAt.Value >= beforeLogin && 
            u.LastLoginAt.Value <= afterLogin)), Times.Once);
    }
}
