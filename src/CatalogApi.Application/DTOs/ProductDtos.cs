namespace CatalogApi.Application.DTOs;

public record ProductDto(
    int Id, string Name, decimal UnitPrice, int UnitsInStock,
    bool Discontinued, int CategoryId, string CategoryName);

public record ProductDetailDto(
    int Id, string Name, string? QuantityPerUnit, decimal UnitPrice,
    int UnitsInStock, int UnitsOnOrder, int ReorderLevel, bool Discontinued,
    int CategoryId, string CategoryName, string? CategoryPictureUrl);

public record CreateProductDto(
    string Name, decimal UnitPrice, int UnitsInStock, int CategoryId,
    string? QuantityPerUnit = null, int UnitsOnOrder = 0,
    int ReorderLevel = 0, bool Discontinued = false);

public record UpdateProductDto(
    string Name, decimal UnitPrice, int UnitsInStock, bool Discontinued);
