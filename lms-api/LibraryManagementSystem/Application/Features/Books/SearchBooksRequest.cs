namespace Application.Features.Books
{
    public record SearchBooksRequest
    {
        public const int MaxPageSize = 100;

        // Matches a title fragment, or an exact ISBN when the text is a valid ISBN.
        public string Search { get; set; }
        public int? PublisherId { get; set; }
        public int? AuthorId { get; set; }
        public int? GenreId { get; set; }
        // Admin Only
        public bool IncludeInactive { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
