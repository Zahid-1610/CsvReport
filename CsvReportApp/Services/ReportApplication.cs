using CsvReportApp.Configuration;
using CsvReportApp.Interfaces;
using CsvReportApp.Models;
using DocumentFormat.OpenXml.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Services
{
    public sealed class ReportApplication : IReportApplication
    {
        private readonly ICsvReader<Book> _csvReader;
        private readonly IReportFormatter<Book> _reportFormatter;
        private readonly IEmailService _emailService;
        private readonly AppSettings _appSettings;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<ReportApplication> _logger;




        public ReportApplication(
              ICsvReader<Book> csvReader,
        IReportFormatter<Book> reportFormatter,
        IEmailService emailService,
        IOptions<AppSettings> appSettings,
        IOptions<EmailSettings> emailSettings,
        ILogger<ReportApplication> logger
            )
        {
            _csvReader = csvReader ?? throw new ArgumentNullException(nameof(csvReader));
            _reportFormatter = reportFormatter ?? throw new ArgumentNullException(nameof(reportFormatter));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting CSV Report Application");

                var books = await _csvReader.ReadAsync(_appSettings.CsvFilePath, cancellationToken);



                if (books.Count == 0)
                {
                    _logger.LogWarning("No valid books found in the CSV file");
                    Console.WriteLine("No valid books found in the CSV file. Please check your data");
                    return;
                }

                var format = GetReportFormat();
                var shouldSendEmail = GetEmailPreference();


                var reportContent = _reportFormatter.Format(books, format);

                if (format == ReportFormat.PlainText)
                {
                    Console.WriteLine("\n" + reportContent);
                }
                else
                {
                    Console.WriteLine("\nHtml report generated Successfully");
                }


                if (shouldSendEmail)
                {
                    await SendEmailReport(reportContent, format == ReportFormat.Html, cancellationToken);
                }

                _logger.LogInformation("Application completed sucessfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Application failed with error");
                Console.WriteLine($"Application error: {ex.Message}");
            }
        }


        private ReportFormat GetReportFormat()
        {
            if (_appSettings.DefaultOutputFormat.Equals("Html", StringComparison.OrdinalIgnoreCase))
            {
                return ReportFormat.Html;
            }

            Console.WriteLine("\nChoose output format:");
            Console.WriteLine("1. Plain Text");
            Console.WriteLine("2. HTML");
            Console.Write("Enter choice (1 or 2): ");


            var input = Console.ReadLine();
            return input == "2" ? ReportFormat.Html : ReportFormat.PlainText;
        }

        private bool GetEmailPreference()
        {
            if (_appSettings.AutoSendEmail)
            {
                return true;
            }

            Console.WriteLine("\nDo you want to send this report via email? (y/n): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();
            return input == "y" || input == "yes";
        }

        private async Task SendEmailReport(string content, bool isHtml, CancellationToken cancellationToken)
        {
            var report = new EmailReport
            {
                Subject = $"Book Report - {DateTime.Now:yyyy-MM-dd}",
                Body = content,
                IsHtml = isHtml,
                ToEmail = _emailSettings.RecipientEmail,
                FromEmail = _emailSettings.SenderEmail
            };

            Console.WriteLine("Sending email...");

            var success = await _emailService.SendAsync(report, cancellationToken);

            if (success)
            {
                Console.WriteLine("Email sent successfully!");
            }
            else
            {
                Console.WriteLine("Failed to send email. Check logs for details.");
            }
        }
    }
}
