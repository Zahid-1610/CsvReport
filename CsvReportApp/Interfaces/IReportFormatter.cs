using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Interfaces
{

    public enum ReportFormat
    {
        PlainText,
        Html
    }

    public interface IReportFormatter<T> where T : class
    {
        string Format(IReadOnlyList<T> data, ReportFormat format);
    }

}
