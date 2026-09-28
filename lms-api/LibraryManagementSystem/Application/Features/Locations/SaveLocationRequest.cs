namespace Application.Features.Locations
{
    public record SaveLocationRequest
    {
        public string Area { get; set; }
        public string LineNo { get; set; }
        public string ShelfNo { get; set; }
        public int RowNo { get; set; }
        public int ColumnNo { get; set; }
    }
}
