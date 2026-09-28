namespace Domain.Enums
{
    public enum BookCopyStatus
    {
        Available = 1,
        Borrowed = 2,
        Damaged = 3,
        Lost = 4,
        Withdrawn = 5,
        // Borrower asked to return; stays on loan until an admin processes the return.
        IssueReturn = 6
    }
}
