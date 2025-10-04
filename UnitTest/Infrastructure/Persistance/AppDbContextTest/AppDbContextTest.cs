using NUnit.Framework;
using FluentAssertions;
using Infrastructure.Persistance;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace UnitTest.Infrastructure.Persistance.AppDbContextTest;

[TestFixture]
public class AppDbContextTest
{
    private AppDbContext _context;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void AppDbContext_HasUsersDbSet()
    {
        // Assert
        _context.Users.Should().NotBeNull();
    }

    [Test]
    public async Task AppDbContext_CanAddAndRetrieveUser()
    {
        // Arrange
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be("test@example.com");
    }

    [Test]
    public async Task AppDbContext_CanUpdateUser()
    {
        // Arrange
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User,
            FailedLoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        user.FailedLoginAttempts = 3;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser!.FailedLoginAttempts.Should().Be(3);
    }

    [Test]
    public async Task AppDbContext_CanDeleteUser()
    {
        // Arrange
        var user = new UserEntity
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Assert
        var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        deletedUser.Should().BeNull();
    }

    [Test]
    public async Task AppDbContext_UserRoleEnum_IsPersisted()
    {
        // Arrange
        var adminUser = new UserEntity
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.Admin
        };

        // Act
        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        retrievedUser!.Role.Should().Be(UserRole.Admin);
    }
}
