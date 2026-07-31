using CatalogApi.Application.Interfaces;
using CatalogApi.Domain.Entities;
using CatalogApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<Category> AddAsync(Category c, CancellationToken ct)
    {
        _db.Categories.Add(c);
        await _db.SaveChangesAsync(ct);
        return c;
    }

    public async Task<IReadOnlyList<int>> GetAllIdsAsync(CancellationToken ct) =>
        await _db.Categories.Select(c => c.Id).ToListAsync(ct);
}
