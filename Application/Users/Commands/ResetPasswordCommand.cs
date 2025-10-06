using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Users.Commands;

public class ResetPasswordCommand : IRequest<ResponseObjectJsonDto>
{
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmNewPassword { get; set; } = default!;

    public ResetPasswordCommand(string token, string newPassword, string confirmNewPassword)
    {
        Token = token;
        NewPassword = newPassword;
        ConfirmNewPassword = confirmNewPassword;
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResponseObjectJsonDto>
{
    private readonly IUserRepository _userRepository;

    public ResetPasswordCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Passwords do not match",
                    Code = 400,
                    Response = null
                };
            }

            var user = await _userRepository.GetByResetTokenAsync(request.Token);

            if (user == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Invalid or expired reset token",
                    Code = 400,
                    Response = null
                };
            }

            if (user.PasswordResetTokenExpiresAt == null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "Reset token has expired",
                    Code = 400,
                    Response = null
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;

            await _userRepository.UpdateAsync(user);

            return new ResponseObjectJsonDto
            {
                Message = "Password has been reset successfully",
                Code = 200,
                Response = null
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Code = 500,
                Message = "Exception during password reset: " + ex.Message,
                Response = null
            };
        }
    }
}

