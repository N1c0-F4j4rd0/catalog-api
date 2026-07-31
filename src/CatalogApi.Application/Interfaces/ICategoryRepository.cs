using CatalogApi.Domain.Entities;

namespace CatalogApi.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category> AddAsync(Category c, CancellationToken ct);
    Task<IReadOnlyList<int>> GetAllIdsAsync(CancellationToken ct);
}
