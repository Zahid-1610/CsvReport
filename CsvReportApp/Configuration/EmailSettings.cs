using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Configuration
{
    public sealed class EmailSettings
    {
        public const string SectionName = "EmailSettings";

        public string SmtpServer { get; init; } = string.Empty;

        public int SmtpPort { get; init; }

        public string SenderEmail { get; init; } = string.Empty;

        public string SenderPassword { get; init; } = string.Empty;

        public string RecipientEmail { get; init; } = string.Empty;

        public bool EnableSsl { get; init; } = true;


    }
}
