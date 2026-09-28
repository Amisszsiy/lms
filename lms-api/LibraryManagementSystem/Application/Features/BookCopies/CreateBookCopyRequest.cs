using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.BookCopies
{
    public record CreateBookCopyRequest
    {
        public long BookId { get; set; }
        public int LocationId { get; set; }
    }
}
