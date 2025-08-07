using CsvReportApp.Interfaces;
using CsvReportApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CsvReportApp.Services
{
    public sealed class BookReportFormatter : IReportFormatter<Book>
    {
        private readonly ILogger<BookReportFormatter> _logger;

        public BookReportFormatter(ILogger<BookReportFormatter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public string Format(IReadOnlyList<Book> books, ReportFormat format)
        {
            ArgumentNullException.ThrowIfNull(books);

            _logger.LogInformation("Formatting {Count} books as {Format}", books.Count, format);

            return format switch
            {
                ReportFormat.PlainText => FormatAsPlainText(books),
                ReportFormat.Html => FormatAsHtml(books),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported report format"),
            };
        }

        private string FormatAsHtml(IReadOnlyList<Book> books)
        {
            if (books.Count == 0)
            {
                return CreateHtmlWrapper("<p>No books found in the dataset.</p>");
            }

            var tableBuilder = new StringBuilder();
            tableBuilder.AppendLine("<table>");
            tableBuilder.AppendLine("  <thead>");
            tableBuilder.AppendLine("    <tr>");
            tableBuilder.AppendLine("      <th>Title</th>");
            tableBuilder.AppendLine("      <th>Author</th>");
            tableBuilder.AppendLine("      <th>Publication Year</th>");
            tableBuilder.AppendLine("    </tr>");
            tableBuilder.AppendLine("  </thead>");
            tableBuilder.AppendLine("  <tbody>");

            foreach (var book in books)
            {
                tableBuilder.AppendLine("    <tr>");
                tableBuilder.AppendLine($"      <td>{System.Net.WebUtility.HtmlEncode(book.Title)}</td>");
                tableBuilder.AppendLine($"      <td>{System.Net.WebUtility.HtmlEncode(book.Author)}</td>");
                tableBuilder.AppendLine($"      <td>{book.PublicationYear}</td>");
                tableBuilder.AppendLine("    </tr>");
            }

            tableBuilder.AppendLine("  </tbody>");
            tableBuilder.AppendLine("</table>");

            return CreateHtmlWrapper(tableBuilder.ToString());
        }


        private string FormatAsPlainText(IReadOnlyList<Book> books)
        {
            if (books.Count == 0)
            {
                return "No books found in the dataset.";
            }

            var report = new StringBuilder();
            report.AppendLine($"Book Report - {DateTime.Now:yyyy-MM-dd}");
            report.AppendLine();

            // Calculate optimal column widths
            var titleWidth = Math.Max("Title".Length, books.Max(b => b.Title.Length)) + 2;
            var authorWidth = Math.Max("Author".Length, books.Max(b => b.Author.Length)) + 2;
            const int yearWidth = 6;

            // Header
            var headerSeparator = CreateTableSeparator(titleWidth, authorWidth, yearWidth);
            report.AppendLine($"| {"Title".PadRight(titleWidth)} | {"Author".PadRight(authorWidth)} | {"Year".PadRight(yearWidth)} |");
            report.AppendLine(headerSeparator);

            // Data rows
            foreach (var book in books)
            {
                var title = TruncateIfNeeded(book.Title, titleWidth);
                var author = TruncateIfNeeded(book.Author, authorWidth);

                report.AppendLine($"| {title.PadRight(titleWidth)} | {author.PadRight(authorWidth)} | {book.PublicationYear.ToString().PadRight(yearWidth)} |");
            }

            return report.ToString();
        }
        private string CreateHtmlWrapper(string content)
        {
            return $$$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Book Report - {DateTime.Now:yyyy-MM-dd}</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; background-color: #f9f9f9; }}
                    .container {{ max-width: 1000px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                    h1 {{ color: #333; text-align: center; margin-bottom: 30px; }}
                    table {{ border-collapse: collapse; width: 100%; margin: 20px 0; }}
                    th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                    th {{ background-color: #4CAF50; color: white; font-weight: bold; }}
                    tr:nth-child(even) {{ background-color: #f2f2f2; }}
                    tr:hover {{ background-color: #f5f5f5; }}
                    .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class="container">
                    <h1>Book Report - {DateTime.Now:yyyy-MM-dd}</h1>
                    {content}
                    <div class="footer">
                        <p>Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                    </div>
                </div>
            </body>
            </html>

            """;
        }

        private static string CreateTableSeparator(int titleWidth, int authorWidth, int yearWidth)
        {
            return $"|{new string('-', titleWidth + 2)}|{new string('-', authorWidth + 2)}|{new string('-', yearWidth + 2)}|";
        }

        private static string TruncateIfNeeded(string text, int maxLength)
        {
            return text.Length > maxLength ? text[..(maxLength - 3)] + "..." : text;
        }
    }
}
