using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace CatalogApi.IntegrationTests;

public class ProductsApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine").Build();
    private WebApplicationFactory<Program> _factory = default!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.UseSetting("ConnectionStrings:Default", _db.GetConnectionString()));
    }

    [Fact]
    public async Task Login_y_listar_productos_funciona()
    {
        var client = _factory.CreateClient();

        var login = await client.PostAsJsonAsync("/auth/login",
            new { username = "admin", password = "Admin123!" });
        login.EnsureSuccessStatusCode();

        var list = await client.GetAsync("/Products?page=1&pageSize=10");
        list.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();
}
