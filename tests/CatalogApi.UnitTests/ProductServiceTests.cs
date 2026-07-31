using CatalogApi.Application.DTOs;
using CatalogApi.Application.Interfaces;
using CatalogApi.Application.Services;
using CatalogApi.Domain.Entities;
using Moq;
using Xunit;

namespace CatalogApi.UnitTests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetPaged_recorta_pagesize_mayor_a_100()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetPagedAsync(It.IsAny<ProductQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductQuery q, CancellationToken _) =>
                new PagedResult<Product>(new List<Product>(), q.Page, q.PageSize, 0));

        var service = new ProductService(repo.Object);
        await service.GetPagedAsync(new ProductQuery(PageSize: 5000), default);

        repo.Verify(r => r.GetPagedAsync(
            It.Is<ProductQuery>(q => q.PageSize == 100), It.IsAny<CancellationToken>()), Times.Once);
    }
}
