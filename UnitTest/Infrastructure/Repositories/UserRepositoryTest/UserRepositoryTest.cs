using NUnit.Framework;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Persistance;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace UnitTest.Infrastructure.Repositories.UserRepositoryTest;

[TestFixture]
public class UserRepositoryTest
{
    private AppDbContext _context;
    private UserRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CreateAsync_ValidUser_ReturnsCreatedUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };

        var result = await _repository.CreateAsync(user);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Test]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        var createdUser = await _repository.CreateAsync(user);

        var result = await _repository.GetByIdAsync(createdUser.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdUser.Id);
        result.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Test]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.GetByEmailAsync("test@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Test]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Test]
    public async Task GetByEmailAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        result.Should().BeNull();
    }

    [Test]
    public async Task GetByUsernameAsync_ExistingUser_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.GetByUsernameAsync("testuser");

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetByUsernameAsync_CaseInsensitive_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.GetByUsernameAsync("TESTUSER");

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetByUsernameAsync_NonExistingUser_ReturnsNull()
    {
        var result = await _repository.GetByUsernameAsync("nonexistent");

        result.Should().BeNull();
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_WithUsername_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.GetByUsernameOrEmailAsync("testuser");

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Test]
    public async Task GetByUsernameOrEmailAsync_WithEmail_ReturnsUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.GetByUsernameOrEmailAsync("test@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Test]
    public async Task ExistsByEmailAsync_ExistingEmail_ReturnsTrue()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.ExistsByEmailAsync("test@example.com");

        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsByEmailAsync_NonExistingEmail_ReturnsFalse()
    {
        var result = await _repository.ExistsByEmailAsync("nonexistent@example.com");

        result.Should().BeFalse();
    }

    [Test]
    public async Task ExistsByUsernameAsync_ExistingUsername_ReturnsTrue()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };
        await _repository.CreateAsync(user);

        var result = await _repository.ExistsByUsernameAsync("testuser");

        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsByUsernameAsync_NonExistingUsername_ReturnsFalse()
    {
        var result = await _repository.ExistsByUsernameAsync("nonexistent");

        result.Should().BeFalse();
    }

    [Test]
    public async Task UpdateAsync_ExistingUser_UpdatesUser()
    {
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User,
            FailedLoginAttempts = 0
        };
        var createdUser = await _repository.CreateAsync(user);

        createdUser.FailedLoginAttempts = 3;
        await _repository.UpdateAsync(createdUser);

        var updatedUser = await _repository.GetByIdAsync(createdUser.Id);
        updatedUser!.FailedLoginAttempts.Should().Be(3);
    }
}
