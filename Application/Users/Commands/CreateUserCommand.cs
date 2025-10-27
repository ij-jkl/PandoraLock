using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands;

public class CreateUserCommand : IRequest<ResponseObjectJsonDto>
{
    public CreateUserDto UserData { get; set; } = default!;

    public CreateUserCommand(CreateUserDto userData)
    {
        UserData = userData;
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResponseObjectJsonDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IAuditLogger _auditLogger;
    private readonly IDateTimeService _dateTimeService;

    public CreateUserCommandHandler(IUserRepository userRepository, IEmailService emailService, IAuditLogger auditLogger, IDateTimeService dateTimeService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _auditLogger = auditLogger;
        _dateTimeService = dateTimeService;
    }

    public async Task<ResponseObjectJsonDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailExists = await _userRepository.ExistsByEmailAsync(request.UserData.Email);
            var usernameExists = await _userRepository.ExistsByUsernameAsync(request.UserData.Username);

            if (emailExists)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Email is already in use",
                    Code = 409,
                    Response = null
                };
            }

            if (usernameExists)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Username is already in use",
                    Code = 409,
                    Response = null
                };
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.UserData.Password);

            var user = new UserEntity
            {
                Email = request.UserData.Email,
                Username = request.UserData.Username,
                PasswordHash = passwordHash,
                CreatedAt = _dateTimeService.Now
            };

            var createdUser = await _userRepository.CreateAsync(user);

            await _auditLogger.LogCreateAsync(
                entityName: "User",
                entityId: createdUser.Id.ToString(),
                userId: createdUser.Id,
                username: createdUser.Username,
                createdValue: new { createdUser.Id, createdUser.Username, createdUser.Email, createdUser.CreatedAt },
                additionalInfo: "User registered"
            );

            await _emailService.SendUserCreatedEmailAsync(createdUser.Email, createdUser.Username);

            var userDto = new UserDto
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                Username = createdUser.Username,
                CreatedAt = createdUser.CreatedAt,
                LastLoginAt = createdUser.LastLoginAt
            };

            return new ResponseObjectJsonDto
            {
                Message = "User created successfully",
                Code = 201,
                Response = userDto
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Code = 500,
                Message = "Exception during user creation: " + ex.Message,
                Response = null
            };
        }
    }
}
