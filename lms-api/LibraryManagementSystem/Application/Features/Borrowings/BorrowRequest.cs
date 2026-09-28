using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Borrowings
{
    // Borrower is passed separately. Current user when borrowing, or the chosen user when a librarian assigns.
    public record BorrowRequest
    {
        public Guid BookCopyId { get; set; }
    }
}
