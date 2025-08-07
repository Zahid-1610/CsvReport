using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Interfaces
{
    public interface IReportApplication
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
