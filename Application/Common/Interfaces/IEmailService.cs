namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetLink);
    Task SendUserCreatedEmailAsync(string toEmail, string username);
}
