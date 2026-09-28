namespace Application.Features.Users
{
    public record SearchUsersRequest
    {
        public const int MaxPageSize = 100;

        // Matches a first/last name fragment, or an exact email when the text is a valid email.
        public string Search { get; set; }
        // Get all if not provided.
        public bool? IsActive { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
