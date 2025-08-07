using CsvReportApp.Interfaces;
using CsvReportApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("Excell file not found: {FilePath}", filePath);
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }


            try
            {
                var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
                var books = new List<Book>();

                if (lines.Length <= 1)
                {
                    _logger.LogWarning("Excell file is empty or contains only headers");
                    return books.AsReadOnly();
                }

                var dateLines = lines.Skip(1);
                var lineNumber = 2;

                foreach (var line in dateLines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        lineNumber++;
                        continue;
                    }

                    try
                    {
                        var book = ParseBookFromLine(line);
                        if (book.IsValid())
                        {
                            books.Add(book);
                        }
                        else
                        {
                            _logger.LogWarning("Invalid book data at line {LineNumber}: {Line}", lineNumber, line);
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing line {LineNumber}: {Line}", lineNumber, line);
                    }

                    lineNumber++;
                }

                _logger.LogInformation("Successfully parsed {Count} books from CSV", books.Count);
                return books.AsReadOnly();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading CSV file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to read CSV file: {filePath}", ex);
            }

        }

        private static Book ParseBookFromLine(string line)
        {
            var fields = ParseCsvLine(line);

            if (fields.Length < 3)
            {
                throw new InvalidOperationException($"Invalid CSV line format. Expected 3 fields, got {fields.Length}");
            }

            return new Book
            {
                Title = fields[0].Trim(' ', '"'),
                Author = fields[1].Trim(' ', '"'),
                PublicationYear = int.TryParse(fields[2].Trim(' ', '"'), out var year) ? year : 0,
            };
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

    }
}
