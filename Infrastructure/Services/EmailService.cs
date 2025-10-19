using Application.Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _enableSsl;
    private readonly string _templatePath;

    public EmailService()
    {
        _smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? throw new ArgumentNullException("EMAIL_SMTP_HOST environment variable is missing");
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ?? "587");
        _smtpUsername = Environment.GetEnvironmentVariable("EMAIL_SMTP_USERNAME") ?? throw new ArgumentNullException("EMAIL_SMTP_USERNAME environment variable is missing");
        _smtpPassword = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD") ?? throw new ArgumentNullException("EMAIL_SMTP_PASSWORD environment variable is missing");
        _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_EMAIL") ?? throw new ArgumentNullException("EMAIL_FROM_EMAIL environment variable is missing");
        _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? "PandoraLock";
        _enableSsl = bool.Parse(Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL") ?? "true");
        _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string username, string resetLink)
    {
        var subject = "PandoraLock - Password Reset Request 🔐";
        var body = await LoadTemplateAsync("PasswordResetEmail.html", new Dictionary<string, string>
        {
            { "{{USERNAME}}", username },
            { "{{RESET_LINK}}", resetLink }
        });

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendUserCreatedEmailAsync(string toEmail, string username)
    {
        var subject = "Welcome to PandoraLock - Your Account is Ready! 🎉";
        var body = await LoadTemplateAsync("UserCreatedEmail.html", new Dictionary<string, string>
        {
            { "{{USERNAME}}", username }
        });

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task<string> LoadTemplateAsync(string templateName, Dictionary<string, string> replacements)
    {
        var baseTemplatePath = Path.Combine(_templatePath, "BaseTemplate.html");
        var contentTemplatePath = Path.Combine(_templatePath, templateName);

        var baseTemplate = await File.ReadAllTextAsync(baseTemplatePath);
        var contentTemplate = await File.ReadAllTextAsync(contentTemplatePath);

        foreach (var replacement in replacements)
        {
            contentTemplate = contentTemplate.Replace(replacement.Key, replacement.Value);
        }

        return baseTemplate.Replace("{{CONTENT}}", contentTemplate);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_fromEmail, _fromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
        smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
        smtpClient.EnableSsl = _enableSsl;

        await smtpClient.SendMailAsync(message);
    }
}