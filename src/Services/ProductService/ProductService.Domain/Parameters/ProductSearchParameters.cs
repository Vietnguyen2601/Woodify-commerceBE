namespace ProductService.Domain.Parameters;

public class ProductSearchParameters
{
    public string? Keyword { get; set; }
    public string Status { get; set; } = "PUBLISHED";
    public string? CategoryName { get; set; }
    public double? MinRating { get; set; }
    public double? MaxRating { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "DESC";
}
