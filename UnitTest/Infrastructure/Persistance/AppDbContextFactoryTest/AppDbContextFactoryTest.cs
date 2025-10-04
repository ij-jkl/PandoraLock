using NUnit.Framework;
using FluentAssertions;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace UnitTest.Infrastructure.Persistance.AppDbContextFactoryTest;

[TestFixture]
public class AppDbContextFactoryTest
{
    private AppDbContextFactory _factory;

    [SetUp]
    public void Setup()
    {
        _factory = new AppDbContextFactory();
        
        // Set up test environment variables
        Environment.SetEnvironmentVariable("MYSQL_CONNECTION_STRING", "server=localhost;port=3306;database=test_db;uid=test_user;pwd=test_password");
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("MYSQL_CONNECTION_STRING", null);
    }

    [Test]
    public void AppDbContextFactory_ImplementsCorrectInterface()
    {
        // Assert
        _factory.Should().BeAssignableTo<IDesignTimeDbContextFactory<AppDbContext>>();
    }
}
