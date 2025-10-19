using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.Users.Commands;

public class ForgotPasswordCommand : IRequest<ResponseObjectJsonDto>
{
    public string Email { get; set; } = default!;

    public ForgotPasswordCommand(string email)
    {
        Email = email;
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ResponseObjectJsonDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<ResponseObjectJsonDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Return this message so we dont reveal for sure if an user has an account in our page
            if (user == null)
            {
                return new ResponseObjectJsonDto
                {
                    Message = "If the email exists, a reset link has been generated",
                    Code = 200,
                    Response = null
                };
            }

            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

            await _userRepository.UpdateAsync(user);

            var resetLink = $"http://localhost:5000/api/Users/reset-password?token={token}";

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetLink);

            var response = new ForgotPasswordResponseDto
            {
                ResetLink = resetLink
            };

            return new ResponseObjectJsonDto
            {
                Message = "Password reset link generated successfully",
                Code = 200,
                Response = response
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Code = 500,
                Message = "Exception during forgot password process: " + ex.Message,
                Response = null
            };
        }
    }
}

