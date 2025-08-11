using CsvReportApp.Configuration;
using CsvReportApp.Interfaces;
using CsvReportApp.Models;
using CsvReportApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var app = host.Services.GetRequiredService<IReportApplication>();

        while (true)
        {
            await app.RunAsync();

            Console.WriteLine("\nDo you want to run again? (y/n): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (input == "n" || input == "no" || input == "q" || input == "quit")
            {
                Console.WriteLine("Exiting application. Goodbye!");
                break;
            }

            Console.Clear(); // Optional: clears console before next run
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<EmailSettings>(context.Configuration.GetSection("EmailSettings"));
                services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));

                services.AddScoped<ICsvReader<Book>, BookCsvReader>();
                services.AddScoped<IReportFormatter<Book>, BookReportFormatter>();
                services.AddScoped<IEmailService, SmtpEmailService>();
                services.AddScoped<IReportApplication, ReportApplication>();

                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}