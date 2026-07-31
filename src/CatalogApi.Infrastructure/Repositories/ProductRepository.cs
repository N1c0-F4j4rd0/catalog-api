using System.Data;
using CatalogApi.Application.DTOs;
using CatalogApi.Application.Interfaces;
using CatalogApi.Domain.Entities;
using CatalogApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace CatalogApi.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery q, CancellationToken ct)
    {
        var query = _db.Products.Include(p => p.Category).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q.Search}%"));
        if (q.CategoryId is int cat) query = query.Where(p => p.CategoryId == cat);
        if (q.MinPrice is decimal min) query = query.Where(p => p.UnitPrice >= min);
        if (q.MaxPrice is decimal max) query = query.Where(p => p.UnitPrice <= max);
        if (q.Discontinued is bool d) query = query.Where(p => p.Discontinued == d);

        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(p => p.Id)
                               .Skip((q.Page - 1) * q.PageSize)
                               .Take(q.PageSize)
                               .ToListAsync(ct);

        return new PagedResult<Product>(items, q.Page, q.PageSize, total);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct) =>
        await _db.Products.Include(p => p.Category).AsNoTracking()
                          .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product> AddAsync(Product p, CancellationToken ct)
    {
        _db.Products.Add(p);
        await _db.SaveChangesAsync(ct);
        return p;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct)
    {
        var rows = await _db.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Name, dto.Name)
                .SetProperty(p => p.UnitPrice, dto.UnitPrice)
                .SetProperty(p => p.UnitsInStock, dto.UnitsInStock)
                .SetProperty(p => p.Discontinued, dto.Discontinued), ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await _db.Products.Where(p => p.Id == id).ExecuteDeleteAsync(ct) > 0;

    public async Task BulkInsertRandomAsync(int count, IReadOnlyList<int> categoryIds, CancellationToken ct)
    {
        var faker = BuildFaker(categoryIds);

        var conn = (NpgsqlConnection)_db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY products (name, quantity_per_unit, unit_price, units_in_stock,
                           units_on_order, reorder_level, discontinued, category_id)
            FROM STDIN (FORMAT BINARY)
            """, ct);

        for (var i = 0; i < count; i++)
        {
            var p = faker.Generate();
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(p.Name, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(p.QuantityPerUnit, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(p.UnitPrice, NpgsqlDbType.Numeric, ct);
            await writer.WriteAsync(p.UnitsInStock, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(p.UnitsOnOrder, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(p.ReorderLevel, NpgsqlDbType.Integer, ct);
            await writer.WriteAsync(p.Discontinued, NpgsqlDbType.Boolean, ct);
            await writer.WriteAsync(p.CategoryId, NpgsqlDbType.Integer, ct);
        }

        await writer.CompleteAsync(ct);
    }

    private static Bogus.Faker<Product> BuildFaker(IReadOnlyList<int> categoryIds) =>
        new Bogus.Faker<Product>("es")
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.QuantityPerUnit, f => $"{f.Random.Int(1, 50)} und")
            .RuleFor(p => p.UnitPrice, f => Math.Round(f.Random.Decimal(1, 5000), 2))
            .RuleFor(p => p.UnitsInStock, f => f.Random.Int(0, 1000))
            .RuleFor(p => p.UnitsOnOrder, f => f.Random.Int(0, 200))
            .RuleFor(p => p.ReorderLevel, f => f.Random.Int(0, 50))
            .RuleFor(p => p.Discontinued, f => f.Random.Bool(0.1f))
            .RuleFor(p => p.CategoryId, f => categoryIds[f.Random.Int(0, categoryIds.Count - 1)]);
}
