using MailKit.Net.Smtp;
using MimeKit;
using SheetFlow.Infrastructure;
using SheetFlow.Models;
using SheetFlow.Repositories;

namespace SheetFlow.Services;

public class EmailNotificationService : INotificationService
{
    private readonly IUserRepository _userRepo;
    private readonly INotificationLogRepository _logRepo;
    private readonly IConfiguration _config;

    public EmailNotificationService(
        IUserRepository userRepo,
        INotificationLogRepository logRepo,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _logRepo = logRepo;
        _config = config;
    }

    public async Task NotifyManagersAsync(string subject, string message)
    {
        var managers = await _userRepo.GetAllAsync();
        managers = managers.Where(u =>
            (u.Role == "Manager" || u.Role == "Admin") && u.IsActive && !string.IsNullOrEmpty(u.Email));

        foreach (var manager in managers)
        {
            await SendEmailAsync(manager.Email!, subject, message, 0);
        }
    }

    public async Task NotifyUserAsync(long userId, string subject, string message)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        await SendEmailAsync(user.Email, subject, message, 0);
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
