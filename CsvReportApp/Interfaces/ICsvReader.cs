using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Interfaces
{
    public interface ICsvReader<T> where T : class
    {
        Task<IReadOnlyList<T>> ReadAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
