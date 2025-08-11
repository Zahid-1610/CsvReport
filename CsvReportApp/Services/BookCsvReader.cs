using CsvReportApp.Interfaces;
using CsvReportApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Services
{
    public sealed class BookCsvReader : ICsvReader<Book>
    {
        private readonly ILogger<BookCsvReader> _logger;


        public BookCsvReader(ILogger<BookCsvReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<Book>> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("CSV file not found at path: {Path}", filePath);
                return Array.Empty<Book>();
            }

            var books = new List<Book>();

            try
            {
                using var reader = new StreamReader(filePath);
                string? line;
                bool isFirstLine = true;
                string? headerLine = await reader.ReadLineAsync();

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Skip header
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    if (headerLine == null)
                    {
                        _logger.LogWarning("CSV file is empty.");
                        return books;
                    }

                    var headers = ParseCsvLine(headerLine);

                    var parts = ParseCsvLine(line);

                    if (parts.Length < 3)
                    {
                        _logger.LogWarning("Skipping invalid row: {Row}", line);
                        continue;
                    }

                    var book = new Book
                    {
                        Title = parts[0].Trim(),
                        Author = parts[1].Trim(),
                        PublicationYear = TryParseYear(parts[2].Trim())
                    };

                    for (int i = 3; i < parts.Length && i < headers.Length; i++)
                    {
                        book.ExtraFields[headers[i]] = parts[i].Trim();
                    }

                    if (book.IsValid())
                    {
                        books.Add(book);
                    }
                    else
                    {
                        _logger.LogWarning("Skipping invalid book entry: {Book}", book);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read or parse CSV file");
            }

            return books;
        }
       
        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new List<char>();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var ch = line[i];

                switch (ch)
                {
                    case '"':
                        if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                        {
                            // Escaped quote
                            currentField.Add('"');
                            i++; // Skip next quote
                        }
                        else
                        {
                            inQuotes = !inQuotes;
                        }
                        break;

                    case ',' when !inQuotes:
                        fields.Add(new string(currentField.ToArray()));
                        currentField.Clear();
                        break;

                    default:
                        currentField.Add(ch);
                        break;
                }

                i++;
            }

            fields.Add(new string(currentField.ToArray()));
            return fields.ToArray();
        }
        
        private int TryParseYear(string input)
        {
            return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year)
                ? year
                : 0;
        }
    }
}
