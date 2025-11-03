using NUnit.Framework;
using FluentAssertions;
using Infrastructure.Auth.JWT;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using Application.Common.Interfaces;
using Moq;

namespace UnitTest.Infrastructure.Auth.JWT.TokenServiceTest;

[TestFixture]
public class TokenServiceTest
{
    private TokenService _tokenService;
    private UserEntity _testUser;
    private Mock<IDateTimeService> _mockDateTimeService;

    [SetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "TestSecretKeyForUnitTestingPurposes1234567890");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME_MINUTES", "30");

        _mockDateTimeService = new Mock<IDateTimeService>();
        _mockDateTimeService.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _tokenService = new TokenService(null!, _mockDateTimeService.Object);
        
        _testUser = new UserEntity
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.User,
            CreatedAt = DateTime.Now
        };
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME_MINUTES", null);
    }

    [Test]
    public void CreateAccessToken_ValidUser_ReturnsValidJwtToken()
    {
        var token = _tokenService.CreateAccessToken(_testUser);

        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        jsonToken.Should().NotBeNull();
        jsonToken.Issuer.Should().Be("TestIssuer");
        jsonToken.Audiences.First().Should().Be("TestAudience");
    }

    [Test]
    public void CreateAccessToken_ValidUser_ContainsCorrectClaims()
    {
        var token = _tokenService.CreateAccessToken(_testUser);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(_testUser.Id.ToString());
        
        var usernameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
        usernameClaim.Should().NotBeNull();
        usernameClaim!.Value.Should().Be(_testUser.Username);
        
        var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(_testUser.Email);
        
        var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(_testUser.Role.ToString());
    }

    [Test]
    public void CreateAccessToken_AdminUser_ContainsAdminRole()
    {
        _testUser.Role = UserRole.Admin;

        var token = _tokenService.CreateAccessToken(_testUser);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be("Admin");
    }

    [Test]
    public void CreateAccessToken_ValidUser_HasCorrectExpiration()
    {
        var beforeCreation = DateTime.UtcNow;

        var token = _tokenService.CreateAccessToken(_testUser);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        var expectedExpiry = beforeCreation.AddMinutes(30);
        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }


    [Test]
    public void ValidateAccessToken_InvalidToken_ReturnsNull()
    {
        var invalidToken = "invalid.jwt.token";

        var principal = _tokenService.ValidateAccessToken(invalidToken);

        principal.Should().BeNull();
    }

    [Test]
    public async Task ValidateAccessToken_ExpiredToken_WithValidationDisabled_ReturnsClaimsPrincipal()
    {
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME_MINUTES", "0");
        var mockExpiredDateTime = new Mock<IDateTimeService>();
        mockExpiredDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        var expiredTokenService = new TokenService(null!, mockExpiredDateTime.Object);
        var token = expiredTokenService.CreateAccessToken(_testUser);
        
        await Task.Delay(1000);

        var principal = _tokenService.ValidateAccessToken(token, validateLifetime: false);

        principal.Should().NotBeNull();
    }

    [Test]
    public void Constructor_MissingJwtSecret_ThrowsArgumentNullException()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", null);

        var exception = Assert.Throws<ArgumentNullException>(() => new TokenService(null!, _mockDateTimeService.Object));
        exception!.Message.Should().Contain("JWT_SECRET");
    }

    [Test]
    public void Constructor_MissingJwtIssuer_ThrowsArgumentNullException()
    {
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);

        var exception = Assert.Throws<ArgumentNullException>(() => new TokenService(null!, _mockDateTimeService.Object));
        exception!.Message.Should().Contain("JWT_ISSUER");
    }

    [Test]
    public void Constructor_MissingJwtAudience_ThrowsArgumentNullException()
    {
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);

        var exception = Assert.Throws<ArgumentNullException>(() => new TokenService(null!, _mockDateTimeService.Object));
        exception!.Message.Should().Contain("JWT_AUDIENCE");
    }

    [Test]
    public void Constructor_InvalidLifetimeMinutes_UsesDefaultValue()
    {
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME_MINUTES", "invalid");
        var mockDateTime = new Mock<IDateTimeService>();
        var baseTime = DateTime.UtcNow;
        mockDateTime.Setup(x => x.UtcNow).Returns(baseTime);
        var tokenService = new TokenService(null!, mockDateTime.Object);

        var token = tokenService.CreateAccessToken(_testUser);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        
        var expectedExpiry = baseTime.AddMinutes(20);
        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }
}
