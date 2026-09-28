using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.BookCopies
{
    // Borrowed/returned status is handled by the borrowing flow, not here.
    public record UpdateBookCopyRequest
    {
        public int LocationId { get; set; }
        public BookCopyStatus Status { get; set; }
    }
}
