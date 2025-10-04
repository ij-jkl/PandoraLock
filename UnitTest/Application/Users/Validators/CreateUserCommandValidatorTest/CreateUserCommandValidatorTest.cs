using NUnit.Framework;
using FluentAssertions;
using Application.Users.Validators;
using Application.Users.Commands;
using Application.DTOs;

namespace UnitTest.Application.Users.Validators.CreateUserCommandValidatorTest;

[TestFixture]
public class CreateUserCommandValidatorTest
{
    private CreateUserCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Test]
    public void Validate_ValidCommand_PassesValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void Validate_EmptyEmail_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "",
            Username = "testuser",
            Password = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Email" && e.ErrorMessage == "Email is required");
    }

    [Test]
    public void Validate_InvalidEmailFormat_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "invalid-email",
            Username = "testuser",
            Password = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Email" && e.ErrorMessage == "Email must be a valid email format");
    }

    [Test]
    public void Validate_EmptyUsername_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "",
            Password = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Username" && e.ErrorMessage == "Username is required");
    }

    [Test]
    public void Validate_ShortUsername_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "ab",
            Password = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Username" && e.ErrorMessage == "Username must be at least 3 characters long");
    }

    [Test]
    public void Validate_EmptyPassword_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "",
            ConfirmPassword = ""
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Password" && e.ErrorMessage == "Password is required");
    }

    [Test]
    public void Validate_ShortPassword_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Short1!",
            ConfirmPassword = "Short1!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Password" && e.ErrorMessage == "Password must be at least 8 characters long");
    }

    [Test]
    public void Validate_PasswordWithoutNumber_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "ValidPass!",
            ConfirmPassword = "ValidPass!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Password" && e.ErrorMessage == "Password must contain at least one number");
    }

    [Test]
    public void Validate_PasswordWithoutUppercase_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "validpass123!",
            ConfirmPassword = "validpass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Password" && e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    [Test]
    public void Validate_PasswordWithoutLowercase_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "VALIDPASS123!",
            ConfirmPassword = "VALIDPASS123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Password" && e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    [Test]
    public void Validate_PasswordWithoutSymbol_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.Password" && e.ErrorMessage == "Password must contain at least one symbol");
    }

    [Test]
    public void Validate_MismatchedPasswords_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "ValidPass123!",
            ConfirmPassword = "DifferentPass123!"
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.ConfirmPassword" && e.ErrorMessage == "Confirm Password must match the password field");
    }

    [Test]
    public void Validate_EmptyConfirmPassword_FailsValidation()
    {
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "ValidPass123!",
            ConfirmPassword = ""
        };
        var command = new CreateUserCommand(userDto);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserData.ConfirmPassword" && e.ErrorMessage == "Confirm Password is required");
    }
}
