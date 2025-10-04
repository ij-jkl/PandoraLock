using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Users.Commands;

public class LoginUserCommand : IRequest<ResponseObjectJsonDto>
{
    public string UsernameOrEmail { get; set; } = default!;
    public string Password { get; set; } = default!;

    public LoginUserCommand(string usernameOrEmail, string password)
    {
        UsernameOrEmail = usernameOrEmail;
        Password = password;
    }
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ResponseObjectJsonDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<ResponseObjectJsonDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

            if (user == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Invalid username/email or password",
                    Code = 401,
                    Response = null
                };
            }

            // Check if account is already locked - reject immediately
            if (user.IsLocked)
            {
                return new ResponseObjectJsonDto
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

                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    await _userRepository.UpdateAsync(user);

                    return new ResponseObjectJsonDto
                    {
                        Message = "Invalid username/email or password. Account has been locked after 5 failed attempts",
                        Code = 401,
                        Response = null
                    };
                }

                await _userRepository.UpdateAsync(user);

                return new ResponseObjectJsonDto
                {
                    Message = $"Invalid username/email or password. {5 - user.FailedLoginAttempts} attempts remaining",
                    Code = 401,
                    Response = null
                };
            }

            // If user login is successful, reset failed attempts and update last login time
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
            await _userRepository.UpdateAsync(user);

            var token = _tokenService.CreateAccessToken(user);
            
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            var loginResponse = new LoginResponseDto
            {
                Token = token,
                User = userDto
            };

            return new ResponseObjectJsonDto
            {
                Message = "Login successful",
                Code = 200,
                Response = loginResponse
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Code = 500,
                Message = "Exception during login: " + ex.Message,
                Response = null
            };
        }
    }
}
