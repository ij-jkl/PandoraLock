using NUnit.Framework;
using Moq;
using FluentAssertions;
using Application.Users.Queries;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Application.Users.Queries.GetUserByUsernameQueryTest;

[TestFixture]
public class GetUserByUsernameQueryHandlerTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private GetUserByUsernameQueryHandler _handler;
    private UserEntity _testUser;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByUsernameQueryHandler(_userRepositoryMock.Object);
        
        _testUser = new UserEntity
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User,
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now.AddHours(-1)
        };
    }

    [Test]
    public async Task Handle_UserExists_ReturnsUserSuccessfully()
    {
        var query = new GetUserByUsernameQuery("testuser");
        
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("testuser"))
            .ReturnsAsync(_testUser);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("User retrieved successfully");
        result.Response.Should().NotBeNull();
    }

    [Test]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        var query = new GetUserByUsernameQuery("nonexistent");
        
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("nonexistent"))
            .ReturnsAsync((UserEntity?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(404);
        result.Message.Should().Be("User not found");
        result.Response.Should().BeNull();
    }

    [Test]
    public async Task Handle_ExceptionThrown_ReturnsServerError()
    {
        var query = new GetUserByUsernameQuery("testuser");
        
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("testuser"))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(500);
        result.Message.Should().StartWith("Exception during user retrieval:");
        result.Response.Should().BeNull();
    }
}
