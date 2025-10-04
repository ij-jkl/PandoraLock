using NUnit.Framework;
using FluentAssertions;
using Application.Users.Validators;
using Application.Users.Commands;

namespace UnitTest.Application.Users.Validators.LoginUserCommandValidatorTest;

[TestFixture]
public class LoginUserCommandValidatorTest
{
    private LoginUserCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new LoginUserCommandValidator();
    }

    [Test]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new LoginUserCommand("testuser", "password123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void Validate_EmptyUsernameOrEmail_FailsValidation()
    {
        var command = new LoginUserCommand("", "password123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "UsernameOrEmail");
        result.Errors.First(e => e.PropertyName == "UsernameOrEmail").ErrorMessage.Should().Be("Username or email is required");
    }

    [Test]
    public void Validate_NullUsernameOrEmail_FailsValidation()
    {
        var command = new LoginUserCommand(null!, "password123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "UsernameOrEmail");
    }

    [Test]
    public void Validate_EmptyPassword_FailsValidation()
    {
        var command = new LoginUserCommand("testuser", "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Password");
        result.Errors.First(e => e.PropertyName == "Password").ErrorMessage.Should().Be("Password is required");
    }

    [Test]
    public void Validate_NullPassword_FailsValidation()
    {
        var command = new LoginUserCommand("testuser", null!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Password");
    }

    [Test]
    public void Validate_BothFieldsEmpty_FailsValidationForBoth()
    {
        var command = new LoginUserCommand("", "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == "UsernameOrEmail");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
