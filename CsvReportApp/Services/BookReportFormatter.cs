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
                return CreateHtmlWrapper("<p>No books found in the dataset.</p>");

            const int RowsPerPage = 50;
            var baseProps = typeof(Book).GetProperties()
                .Where(p => p.Name != nameof(Book.ExtraFields)) 
                .ToList();

           
            var dynamicHeaders = books
                .SelectMany(b => b.ExtraFields.Keys)
                .Distinct()
                .ToList();

            int totalPages = (int)Math.Ceiling((double)books.Count / RowsPerPage);
            var html = new StringBuilder();

            for (int page = 0; page < totalPages; page++)
            {
                html.AppendLine("<div class='page'>");
                html.AppendLine($"<h2>Page {page + 1}</h2>");
                html.AppendLine("<table><thead><tr>");

                foreach (var prop in baseProps)
                    html.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(prop.Name)}</th>");

                foreach (var header in dynamicHeaders)
                    html.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(header)}</th>");

                html.AppendLine("</tr></thead><tbody>");

                var pageBooks = books.Skip(page * RowsPerPage).Take(RowsPerPage);
                foreach (var book in pageBooks)
                {
                    html.AppendLine("<tr>");

                    foreach (var prop in baseProps)
                    {
                        var value = prop.GetValue(book)?.ToString() ?? "";
                        html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(value)}</td>");
                    }

                    foreach (var header in dynamicHeaders)
                    {
                        book.ExtraFields.TryGetValue(header, out var val);
                        html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(val ?? "")}</td>");
                    }

                    html.AppendLine("</tr>");
                }

                html.AppendLine("</tbody></table>");
                html.AppendLine("</div>");
            }

            return CreateHtmlWrapper(html.ToString());
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
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>Book Report - {DateTime.Now:yyyy-MM-dd}</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f9f9f9; }");
            html.AppendLine("        .container { max-width: 1000px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("        h1 { color: #333; text-align: center; margin-bottom: 30px; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine("        th { background-color: #4CAF50; color: white; font-weight: bold; }");
            html.AppendLine("        tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine("        tr:hover { background-color: #f5f5f5; }");
            html.AppendLine("        .footer { text-align: center; margin-top: 30px; color: #666; font-size: 12px; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class=\"container\">");
            html.AppendLine($"        <h1>Book Report - {DateTime.Now:yyyy-MM-dd}</h1>");
            html.AppendLine(content);
            html.AppendLine("        <div class=\"footer\">");
            html.AppendLine($"            <p>Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
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