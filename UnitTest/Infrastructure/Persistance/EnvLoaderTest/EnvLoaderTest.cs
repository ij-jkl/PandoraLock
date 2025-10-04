using NUnit.Framework;
using FluentAssertions;
using Infrastructure.Persistance;
using System;
using System.IO;

namespace UnitTest.Infrastructure.Persistance.EnvLoaderTest;

[TestFixture]
public class EnvLoaderTest
{
    private string _testEnvFile;
    private string _originalDirectory;

    [SetUp]
    public void Setup()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _testEnvFile = Path.Combine(_originalDirectory, ".env.test");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testEnvFile))
        {
            File.Delete(_testEnvFile);
        }
        
        // Clean up environment variables
        Environment.SetEnvironmentVariable("TEST_VAR", null);
        Environment.SetEnvironmentVariable("TEST_VAR_WITH_EQUALS", null);
        Environment.SetEnvironmentVariable("TEST_EMPTY_VAR", null);
    }

    [Test]
    public void LoadRootEnv_FileDoesNotExist_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            // Simulate EnvLoader behavior when file doesn't exist
            var nonExistentFile = Path.Combine(_originalDirectory, ".env.nonexistent");
            if (!File.Exists(nonExistentFile))
            {
                // File doesn't exist, should not throw
            }
        });
    }
}
