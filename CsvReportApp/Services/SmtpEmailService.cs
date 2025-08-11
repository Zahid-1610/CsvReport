using CsvReportApp.Configuration;
using CsvReportApp.Interfaces;
using CsvReportApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Services
{
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> emailSettings, ILogger<SmtpEmailService> logger)
        {
            _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ValidateEmailSettings();
        }

        public async Task<bool> SendAsync(EmailReport report, string? attachmentPath, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(report);

            try
            {
                using var client = CreateSmtpClient();
                using var mailMessage = CreateMailMessage(report, attachmentPath);

                _logger.LogInformation("Sending email to {Recipient}", report.ToEmail);
                await client.SendMailAsync(mailMessage, cancellationToken);
                _logger.LogInformation("Email sent successfully to {Recipient}", report.ToEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", report.ToEmail);
                return false;
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword)
            };

            return client;
        }

        private MailMessage CreateMailMessage(EmailReport report, string? attachmentPath)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(report.FromEmail),
                Subject = report.Subject,
                Body = report.Body,
                IsBodyHtml = report.IsHtml,
                Priority = MailPriority.Normal
            };

            mail.To.Add(new MailAddress(report.ToEmail));

            if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
            {
                mail.Attachments.Add(new Attachment(attachmentPath));
                _logger.LogInformation("Attached file: {File}", attachmentPath);
            }
            else
            {
                _logger.LogWarning("CSV attachment file not found: {File}", attachmentPath);
            }

            return mail;
        }

        private void ValidateEmailSettings()
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
                throw new InvalidOperationException("SMTP server is not configured");

            if (_emailSettings.SmtpPort <= 0)
                throw new InvalidOperationException("SMTP port is not configured");

            if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                throw new InvalidOperationException("Sender email is not configured");

            if (string.IsNullOrWhiteSpace(_emailSettings.SenderPassword))
                throw new InvalidOperationException("Sender password is not configured");
        }
    }
}
