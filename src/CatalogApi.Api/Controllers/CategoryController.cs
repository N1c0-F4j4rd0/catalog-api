using CatalogApi.Application.DTOs;
using CatalogApi.Application.Interfaces;
using CatalogApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryRepository _repo;
    public CategoryController(ICategoryRepository repo) => _repo = repo;

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto, CancellationToken ct)
    {
        var c = await _repo.AddAsync(new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            PictureUrl = dto.PictureUrl
        }, ct);
        return CreatedAtAction(nameof(Create), new { id = c.Id },
            new CategoryDto(c.Id, c.Name, c.Description, c.PictureUrl));
    }
}
