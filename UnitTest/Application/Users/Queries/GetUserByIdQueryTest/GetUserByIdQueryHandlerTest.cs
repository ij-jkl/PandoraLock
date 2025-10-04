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

namespace UnitTest.Application.Users.Queries.GetUserByIdQueryTest;

[TestFixture]
public class GetUserByIdQueryHandlerTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private GetUserByIdQueryHandler _handler;
    private UserEntity _testUser;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
        
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
        var query = new GetUserByIdQuery(1);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
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
        var query = new GetUserByIdQuery(999);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(999))
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
        var query = new GetUserByIdQuery(1);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(500);
        result.Message.Should().StartWith("Exception during user retrieval:");
        result.Response.Should().BeNull();
    }
}
