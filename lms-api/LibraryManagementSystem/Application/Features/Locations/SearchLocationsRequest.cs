namespace Application.Features.Locations
{
    public record SearchLocationsRequest
    {
        public const int MaxPageSize = 100;

        // Exact matches; get all if not provided.
        public string Area { get; set; }
        public string LineNo { get; set; }
        public string ShelfNo { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
