using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Models
{
    public sealed record EmailReport
    {
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;

        public bool IsHtml { get; init; }

        public string ToEmail { get; init; } = string.Empty;

        public string FromEmail { get; init; } = string.Empty;

    }
}
