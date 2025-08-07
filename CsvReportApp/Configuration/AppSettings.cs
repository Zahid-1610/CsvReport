using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Configuration
{
    public sealed class AppSettings
    {
        public const string SectionName = "AppSettings";

        public string CsvFilePath { get; init; } = "./Books/Books.xlsx";
        public string DefaultOutputFormat { get; set; } = "PlainText";

        public bool AutoSendEmail { get; set; } = false;
    }
}
