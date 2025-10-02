using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Users.Commands;

public class LoginUserCommand : IRequest<ResponseObjectJsonDto<UserDto>>
{
    public string UsernameOrEmail { get; set; } = default!;
    public string Password { get; set; } = default!;

    public LoginUserCommand(string usernameOrEmail, string password)
    {
        UsernameOrEmail = usernameOrEmail;
        Password = password;
    }
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ResponseObjectJsonDto<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public LoginUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseObjectJsonDto<UserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

            if (user == null)
            {
                return new ResponseObjectJsonDto<UserDto>
                {
                    Message = "Invalid username/email or password",
                    Code = 401,
                    Response = null
                };
            }

            if (user.IsLocked)
            {
                return new ResponseObjectJsonDto<UserDto>
                {
                    Message = "Account is locked due to too many failed login attempts",
                    Code = 403,
                    Response = null
                };
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                user.FailedLoginAttempts++;
                user.IsLocked = true;
                await _userRepository.UpdateAsync(user);

                return new ResponseObjectJsonDto<UserDto>
                {
                    Message = "Invalid username/email or password. Account has been locked",
                    Code = 401,
                    Response = null
                };
            }

            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return new ResponseObjectJsonDto<UserDto>
            {
                Message = "Login successful",
                Code = 200,
                Response = userDto
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto<UserDto>
            {
                Code = 500,
                Message = "Exception during login: " + ex.Message,
                Response = null
            };
        }
    }
}
