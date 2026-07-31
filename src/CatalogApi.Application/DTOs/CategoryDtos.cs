namespace CatalogApi.Application.DTOs;

public record CategoryDto(int Id, string Name, string? Description, string? PictureUrl);
public record CreateCategoryDto(string Name, string? Description, string? PictureUrl);
