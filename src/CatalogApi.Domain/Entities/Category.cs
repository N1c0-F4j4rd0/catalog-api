namespace CatalogApi.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? PictureUrl { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
