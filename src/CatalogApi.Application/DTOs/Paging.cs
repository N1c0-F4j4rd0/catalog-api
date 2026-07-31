namespace CatalogApi.Application.DTOs;

public record ProductQuery(
    int Page = 1, int PageSize = 20, string? Search = null,
    int? CategoryId = null, decimal? MinPrice = null,
    decimal? MaxPrice = null, bool? Discontinued = null);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}
