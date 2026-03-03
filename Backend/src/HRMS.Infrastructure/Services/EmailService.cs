using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using HRMS.Application.Interfaces;

namespace HRMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        try
        {
            var senderEmail = _config["Email:SenderEmail"] ?? _config["Email:From"] ?? "noreply@hrms.com";
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var username = _config["Email:SenderEmail"] ?? _config["Email:Username"] ?? senderEmail;
            var password = (_config["Email:SenderPassword"] ?? _config["Email:Password"])?.Replace(" ", "");

            if (string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email sending skipped: SMTP Password is not configured.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            if (attachment != null && !string.IsNullOrEmpty(attachmentName))
                builder.Attachments.Add(attachmentName, attachment);
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            
            _logger.LogInformation("Successfully sent email to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Reason: {Message}", to, ex.Message);
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string name, string password)
    {
        var body = $"<h2>Welcome to HRMS, {name}!</h2><p>Your account has been created.</p><p>Temporary password: <strong>{password}</strong></p><p>Please change your password after first login.</p>";
        await SendEmailAsync(to, "Welcome to HRMS - Account Created", body);
    }

    public async Task SendPasswordResetEmailAsync(string to, string name, string newPassword)
    {
        var body = $"<h2>Password Reset</h2><p>Hi {name}, your password has been reset.</p><p>New password: <strong>{newPassword}</strong></p><p>Please change your password after login.</p>";
        await SendEmailAsync(to, "HRMS - Password Reset", body);
    }

    public async Task SendLeaveStatusEmailAsync(string to, string name, string leaveType, string status, string? remarks)
    {
        var body = $"<h2>Leave Application Update</h2><p>Hi {name}, your {leaveType} application has been <strong>{status}</strong>.</p>";
        if (!string.IsNullOrEmpty(remarks)) body += $"<p>Remarks: {remarks}</p>";
        await SendEmailAsync(to, $"HRMS - Leave {status}", body);
    }

    public async Task SendSalarySlipEmailAsync(string to, string name, int month, int year, byte[] pdfBytes)
    {
        var body = $"<h2>Salary Slip</h2><p>Hi {name}, your salary slip for {month}/{year} is attached.</p>";
        await SendEmailAsync(to, $"HRMS - Salary Slip {month}/{year}", body, pdfBytes, $"SalarySlip_{month}_{year}.pdf");
    }

    public async Task SendIncrementEmailAsync(string to, string name, decimal oldCtc, decimal newCtc, decimal percentage)
    {
        var body = $"<h2>Increment Approved!</h2><p>Hi {name}, your increment of {percentage}% has been approved.</p><p>Old CTC: ₹{oldCtc:N2} → New CTC: ₹{newCtc:N2}</p>";
        await SendEmailAsync(to, "HRMS - Increment Approved", body);
    }
}
