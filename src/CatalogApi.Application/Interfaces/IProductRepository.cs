using CatalogApi.Application.DTOs;
using CatalogApi.Domain.Entities;

namespace CatalogApi.Application.Interfaces;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(ProductQuery q, CancellationToken ct);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<Product> AddAsync(Product p, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateProductDto d, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task BulkInsertRandomAsync(int count, IReadOnlyList<int> categoryIds, CancellationToken ct);
}
