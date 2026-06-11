using MailKit.Net.Smtp;
using MimeKit;
using SheetFlow.Infrastructure;
using SheetFlow.Models;
using SheetFlow.Repositories;

namespace SheetFlow.Services;

public class EmailNotificationService : INotificationService
{
    private readonly IUserRepository _userRepo;
    private readonly IEmployeeProfileRepository _profileRepo;
    private readonly INotificationLogRepository _logRepo;
    private readonly IConfiguration _config;

    public EmailNotificationService(
        IUserRepository userRepo,
        IEmployeeProfileRepository profileRepo,
        INotificationLogRepository logRepo,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _logRepo = logRepo;
        _config = config;
    }

    private async Task<string?> GetEmailByUsernameAsync(string username)
    {
        var profile = await _profileRepo.GetByUsernameAsync(username);
        return profile?.Email;
    }

    public async Task NotifyManagersAsync(string subject, string message)
    {
        var managers = await _userRepo.GetAllAsync();
        foreach (var manager in managers.Where(u => (u.Role == "Manager" || u.Role == "Admin") && u.IsActive))
        {
            var email = await GetEmailByUsernameAsync(manager.Username);
            if (!string.IsNullOrEmpty(email))
                await SendEmailAsync(email, subject, message, 0);
        }
    }

    public async Task NotifyUserAsync(long userId, string subject, string message)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;
        var email = await GetEmailByUsernameAsync(user.Username);
        if (!string.IsNullOrEmpty(email))
            await SendEmailAsync(email, subject, message, 0);
    }

    private async Task SendEmailAsync(string to, string subject, string body, long requestId)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];
        var fromEmail = _config["Email:FromEmail"] ?? "noreply@sheetflow.com";
        var fromName = _config["Email:FromName"] ?? "SheetFlow";

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
        {
            var log = new NotificationLog
            {
                FormRequestId = requestId,
                NotifyType = "Email",
                Recipient = to,
                Subject = subject,
                Content = body,
                IsSuccess = false,
                ErrorMessage = "SMTP not configured",
                SentAt = DateTime.UtcNow
            };
            await _logRepo.CreateAsync(log);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            var log = new NotificationLog
            {
                FormRequestId = requestId,
                NotifyType = "Email",
                Recipient = to,
                Subject = subject,
                Content = body,
                IsSuccess = true,
                SentAt = DateTime.UtcNow
            };
            await _logRepo.CreateAsync(log);
        }
        catch (Exception ex)
        {
            var log = new NotificationLog
            {
                FormRequestId = requestId,
                NotifyType = "Email",
                Recipient = to,
                Subject = subject,
                Content = body,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                SentAt = DateTime.UtcNow
            };
            await _logRepo.CreateAsync(log);
        }
    }
}
