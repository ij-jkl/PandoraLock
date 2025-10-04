using NUnit.Framework;
using Moq;
using FluentAssertions;
using Application.Users.Commands;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs;
using Application.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Application.Users.Commands.CreateUserCommandTest;

[TestFixture]
public class CreateUserCommandHandlerTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private CreateUserCommandHandler _handler;
    private CreateUserDto _validUserDto;
    private readonly TimeZoneInfo _argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new CreateUserCommandHandler(_userRepositoryMock.Object);
        
        _validUserDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "ValidPassword123",
            ConfirmPassword = "ValidPassword123"
        };
    }

    [Test]
    public async Task Handle_ValidUser_CreatesUserSuccessfully()
    {
        var command = new CreateUserCommand(_validUserDto);
        var createdUser = new UserEntity
        {
            Id = 1,
            Email = _validUserDto.Email,
            Username = _validUserDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(_validUserDto.Password),
            Role = UserRole.User,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone)
        };

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(_validUserDto.Username))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(createdUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(201);
        result.Message.Should().Be("User created successfully");
        result.Response.Should().NotBeNull();
        
        var userDto = result.Response as UserDto;
        userDto.Should().NotBeNull();
        userDto!.Email.Should().Be(_validUserDto.Email);
        userDto.Username.Should().Be(_validUserDto.Username);
        userDto.Id.Should().Be(1);
    }

    [Test]
    public async Task Handle_EmailAlreadyExists_ReturnsConflict()
    {
        var command = new CreateUserCommand(_validUserDto);
        
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(409);
        result.Message.Should().Be("Email is already in use");
        result.Response.Should().BeNull();
        
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task Handle_UsernameAlreadyExists_ReturnsConflict()
    {
        var command = new CreateUserCommand(_validUserDto);
        
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(_validUserDto.Username))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(409);
        result.Message.Should().Be("Username is already in use");
        result.Response.Should().BeNull();
        
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task Handle_ValidUser_HashesPasswordCorrectly()
    {
        var command = new CreateUserCommand(_validUserDto);
        UserEntity capturedUser = null!;

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(_validUserDto.Username))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(user => capturedUser = user)
            .ReturnsAsync((UserEntity user) => { user.Id = 1; return user; });

        await _handler.Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(_validUserDto.Password);
        BCrypt.Net.BCrypt.Verify(_validUserDto.Password, capturedUser.PasswordHash).Should().BeTrue();
    }

    [Test]
    public async Task Handle_ValidUser_SetsArgentinaTimezone()
    {
        var command = new CreateUserCommand(_validUserDto);
        var beforeCreation = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);
        UserEntity capturedUser = null!;

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(_validUserDto.Username))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(user => capturedUser = user)
            .ReturnsAsync((UserEntity user) => { user.Id = 1; return user; });

        await _handler.Handle(command, CancellationToken.None);
        var afterCreation = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);

        capturedUser.Should().NotBeNull();
        capturedUser!.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        capturedUser.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Test]
    public async Task Handle_ValidUser_SetsDefaultUserRole()
    {
        var command = new CreateUserCommand(_validUserDto);
        UserEntity capturedUser = null!;

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(_validUserDto.Username))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(user => capturedUser = user)
            .ReturnsAsync((UserEntity user) => { user.Id = 1; return user; });

        await _handler.Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.User);
    }

    [Test]
    public async Task Handle_ExceptionThrown_ReturnsServerError()
    {
        var command = new CreateUserCommand(_validUserDto);
        
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(_validUserDto.Email))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(500);
        result.Message.Should().StartWith("Exception during user creation:");
        result.Response.Should().BeNull();
    }
}
