using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Borrowings
{
    // Librarian assigns a copy directly to the chosen user.
    public record AssignBorrowRequest
    {
        public Guid UserId { get; set; }
        public Guid BookCopyId { get; set; }
    }
}
