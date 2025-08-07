using CsvReportApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(EmailReport report, CancellationToken cancellationToken = default);
    }
}
