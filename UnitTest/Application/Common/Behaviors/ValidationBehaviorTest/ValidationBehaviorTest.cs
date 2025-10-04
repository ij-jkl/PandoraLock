using NUnit.Framework;
using Moq;
using FluentAssertions;
using Application.Common.Behaviors;
using Application.Common.Models;
using Application.Users.Commands;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Application.Common.Behaviors.ValidationBehaviorTest;

[TestFixture]
public class ValidationBehaviorTest
{
    private Mock<IValidator<LoginUserCommand>> _validatorMock;
    private Mock<RequestHandlerDelegate<ResponseObjectJsonDto>> _nextMock;
    private ValidationBehavior<LoginUserCommand, ResponseObjectJsonDto> _behavior;

    [SetUp]
    public void Setup()
    {
        _validatorMock = new Mock<IValidator<LoginUserCommand>>();
        _nextMock = new Mock<RequestHandlerDelegate<ResponseObjectJsonDto>>();
        _behavior = new ValidationBehavior<LoginUserCommand, ResponseObjectJsonDto>(new[] { _validatorMock.Object });
    }

    [Test]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behaviorWithoutValidators = new ValidationBehavior<LoginUserCommand, ResponseObjectJsonDto>(Array.Empty<IValidator<LoginUserCommand>>());
        var request = new LoginUserCommand("test", "password");
        var expectedResponse = new ResponseObjectJsonDto { Code = 200, Message = "Success" };
        
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        var result = await behaviorWithoutValidators.Handle(request, _nextMock.Object, CancellationToken.None);

        result.Should().Be(expectedResponse);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Test]
    public async Task Handle_ValidationPasses_CallsNext()
    {
        var request = new LoginUserCommand("test", "password");
        var expectedResponse = new ResponseObjectJsonDto { Code = 200, Message = "Success" };
        var validationResult = new FluentValidation.Results.ValidationResult();
        
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<LoginUserCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        result.Should().Be(expectedResponse);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Test]
    public async Task Handle_ValidationFails_ReturnsValidationErrorResponse()
    {
        var request = new LoginUserCommand("", "");
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("UsernameOrEmail", "Username or email is required"),
            new("Password", "Password is required")
        };
        var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
        
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<LoginUserCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Validation failed");
        result.Response.Should().BeNull();
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().ContainKey("UsernameOrEmail");
        result.ValidationErrors.Should().ContainKey("Password");
        
        _nextMock.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Handle_MultipleValidationErrorsForSameProperty_GroupsErrors()
    {
        var request = new LoginUserCommand("", "");
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Password", "Password is required"),
            new("Password", "Password must be at least 8 characters")
        };
        var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);
        
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<LoginUserCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        result.ValidationErrors.Should().ContainKey("Password");
        result.ValidationErrors!["Password"].Should().HaveCount(2);
        result.ValidationErrors["Password"].Should().Contain("Password is required");
        result.ValidationErrors["Password"].Should().Contain("Password must be at least 8 characters");
    }
}
