using CatalogApi.Application.DTOs;
using CatalogApi.Application.Interfaces;
using CatalogApi.Application.Mapping;
using CatalogApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;
    private readonly IProductRepository _repo;
    private readonly ICategoryRepository _categories;

    public ProductController(ProductService service, IProductRepository repo, ICategoryRepository categories)
    {
        _service = service;
        _repo = repo;
        _categories = categories;
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromQuery] int count = 100_000, CancellationToken ct = default)
    {
        var catIds = await _categories.GetAllIdsAsync(ct);
        if (catIds.Count == 0) return BadRequest("Primero crea categorías (POST /Category).");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _repo.BulkInsertRandomAsync(count, catIds, ct);
        sw.Stop();

        return Ok(new { inserted = count, elapsedMs = sw.ElapsedMilliseconds });
    }

    [HttpPost("single")]
    public async Task<IActionResult> CreateOne(CreateProductDto dto, CancellationToken ct)
    {
        var p = await _repo.AddAsync(dto.ToEntity(), ct);
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, p.ToDto());
    }

    [HttpGet("/Products")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ProductDto>>> List([FromQuery] ProductQuery q, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(q, ct));

    [HttpGet("/Products/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
        => await _service.GetByIdAsync(id, ct) is { } dto ? Ok(dto) : NotFound();

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto, CancellationToken ct)
        => await _repo.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _repo.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
