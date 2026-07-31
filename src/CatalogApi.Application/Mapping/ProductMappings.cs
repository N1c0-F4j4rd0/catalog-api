using CatalogApi.Application.DTOs;
using CatalogApi.Domain.Entities;

namespace CatalogApi.Application.Mapping;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product p) =>
        new(p.Id, p.Name, p.UnitPrice, p.UnitsInStock, p.Discontinued,
            p.CategoryId, p.Category?.Name ?? "");

    public static ProductDetailDto ToDetail(this Product p) =>
        new(p.Id, p.Name, p.QuantityPerUnit, p.UnitPrice, p.UnitsInStock,
            p.UnitsOnOrder, p.ReorderLevel, p.Discontinued, p.CategoryId,
            p.Category.Name, p.Category.PictureUrl);

    public static Product ToEntity(this CreateProductDto d) => new()
    {
        Name = d.Name,
        UnitPrice = d.UnitPrice,
        UnitsInStock = d.UnitsInStock,
        CategoryId = d.CategoryId,
        QuantityPerUnit = d.QuantityPerUnit,
        UnitsOnOrder = d.UnitsOnOrder,
        ReorderLevel = d.ReorderLevel,
        Discontinued = d.Discontinued
    };
}
