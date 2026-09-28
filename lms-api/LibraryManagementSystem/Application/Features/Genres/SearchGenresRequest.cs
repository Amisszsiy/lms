namespace Application.Features.Genres
{
    public record SearchGenresRequest
    {
        public const int MaxPageSize = 100;

        // Matches a fragment of the genre name.
        public string Search { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
