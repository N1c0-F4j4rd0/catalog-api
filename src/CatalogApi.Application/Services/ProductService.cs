using CatalogApi.Application.DTOs;
using CatalogApi.Application.Interfaces;
using CatalogApi.Application.Mapping;

namespace CatalogApi.Application.Services;

public class ProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery q, CancellationToken ct)
    {
        q = q with { Page = Math.Max(1, q.Page), PageSize = Math.Clamp(q.PageSize, 1, 100) };
        var page = await _repo.GetPagedAsync(q, ct);
        var dtos = page.Items.Select(p => p.ToDto()).ToList();
        return new PagedResult<ProductDto>(dtos, page.Page, page.PageSize, page.Total);
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken ct)
        => (await _repo.GetByIdAsync(id, ct))?.ToDetail();
}
