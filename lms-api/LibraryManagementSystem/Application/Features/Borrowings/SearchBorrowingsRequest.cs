namespace Application.Features.Borrowings
{
    public record SearchBorrowingsRequest
    {
        public const int MaxPageSize = 100;

        // Matches a book title or borrower name fragment, or an exact ISBN when the text is a valid ISBN.
        public string Search { get; set; }
        // Admin only. End users are always limited to their own borrowings.
        public Guid? UserId { get; set; }
        // e.g. ReturnRequested lists the returns waiting to be processed.
        public BorrowingStatus? Status { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
