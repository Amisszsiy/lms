using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Borrowings
{
    public enum BorrowingStatus
    {
        Borrowed = 1,
        Returned = 2,
        Overdue = 3,
        // Borrower asked to return, waiting for an admin to process it.
        ReturnRequested = 4
    }

    public record BorrowingResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookCopyId { get; set; }
        public BorrowingUserResponse User { get; set; }
        public BorrowingBookResponse Book { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowingStatus Status { get; set; }
    }

    // Only what a borrowing row shows. Full details come from the user/book endpoints.
    public record BorrowingUserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public record BorrowingBookResponse
    {
        public long Id { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
    }
}
