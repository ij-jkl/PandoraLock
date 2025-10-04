using NUnit.Framework;
using FluentAssertions;
using Application.Common.Models;
using Application.DTOs;

namespace UnitTest.Application.Common.Models.ResponseObjectJsonDtoTest;

[TestFixture]
public class ResponseObjectJsonDtoTest
{
    [Test]
    public void ResponseObjectJsonDto_SetProperties_WorksCorrectly()
    {
        var userDto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now
        };

        var validationErrors = new Dictionary<string, string[]>
        {
            { "Username", new[] { "Username is required" } },
            { "Password", new[] { "Password is required", "Password must be strong" } }
        };

        var response = new ResponseObjectJsonDto
        {
            Message = "Test message",
            Code = 200,
            Response = userDto,
            ValidationErrors = validationErrors
        };

        response.Message.Should().Be("Test message");
        response.Code.Should().Be(200);
        response.Response.Should().Be(userDto);
        response.ValidationErrors.Should().NotBeNull();
        response.ValidationErrors.Should().HaveCount(2);
        response.ValidationErrors!["Username"].Should().ContainSingle("Username is required");
        response.ValidationErrors["Password"].Should().HaveCount(2);
    }

    [Test]
    public void ResponseObjectJsonDto_SuccessResponse_HasCorrectStructure()
    {
        var data = new { Name = "Test", Value = 123 };

        var response = new ResponseObjectJsonDto
        {
            Message = "Operation successful",
            Code = 200,
            Response = data
        };

        response.Message.Should().Be("Operation successful");
        response.Code.Should().Be(200);
        response.Response.Should().NotBeNull();
        response.ValidationErrors.Should().BeNull();
    }

    [Test]
    public void ResponseObjectJsonDto_ErrorResponse_HasCorrectStructure()
    {
        var response = new ResponseObjectJsonDto
        {
            Message = "Operation failed",
            Code = 500,
            Response = null
        };

        response.Message.Should().Be("Operation failed");
        response.Code.Should().Be(500);
        response.Response.Should().BeNull();
    }

    [Test]
    public void ResponseObjectJsonDto_ValidationErrorResponse_HasCorrectStructure()
    {
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } },
            { "Field2", new[] { "Error 2", "Error 3" } }
        };

        var response = new ResponseObjectJsonDto
        {
            Message = "Validation failed",
            Code = 400,
            Response = null,
            ValidationErrors = validationErrors
        };

        response.Message.Should().Be("Validation failed");
        response.Code.Should().Be(400);
        response.Response.Should().BeNull();
        response.ValidationErrors.Should().NotBeNull();
        response.ValidationErrors.Should().HaveCount(2);
    }
}
