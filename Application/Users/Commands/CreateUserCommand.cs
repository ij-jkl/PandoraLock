using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands;

public class CreateUserCommand : IRequest<ResponseObjectJsonDto<UserDto>>
{
    public CreateUserDto UserData { get; set; } = default!;

    public CreateUserCommand(CreateUserDto userData)
    {
        UserData = userData;
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResponseObjectJsonDto<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseObjectJsonDto<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _userRepository.ExistsAsync(request.UserData.Email, request.UserData.Username))
            {
                return new ResponseObjectJsonDto<UserDto>
                {
                    Message = "User with this email or username already exists",
                    Code = 409,
                    Response = null!
                };
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.UserData.Password);

            var user = new UserEntity
            {
                Email = request.UserData.Email,
                Username = request.UserData.Username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
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

            return new ResponseObjectJsonDto<UserDto>
            {
                Message = "User created successfully",
                Code = 201,
                Response = userDto
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto<UserDto>
            {
                Code = 500,
                Message = "Exception during user creation: " + ex.Message,
                Response = null!
            };
        }
    }
}
