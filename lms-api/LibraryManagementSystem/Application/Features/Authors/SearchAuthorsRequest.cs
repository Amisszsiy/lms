namespace Application.Features.Authors
{
    public record SearchAuthorsRequest
    {
        public const int MaxPageSize = 100;

        // Matches a fragment of the first name, last name or any pen name.
        public string Search { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
