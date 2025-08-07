using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvReportApp.Models
{
    public sealed record Book
    {
        public string Title { get; set; } = string.Empty;
        public string Author  { get; set; } = string.Empty;
        public int PublicationYear { get; set; }


        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Author) && PublicationYear > 0;

    }
}
