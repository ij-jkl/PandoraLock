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

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"))
            };

            var createdUser = await _userRepository.CreateAsync(user);

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
