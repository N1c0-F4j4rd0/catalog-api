using CatalogApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category
                {
                    Name = "SERVIDORES",
                    Description = "Infraestructura física y virtual",
                    PictureUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=600&q=80"
                },
                new Category
                {
                    Name = "CLOUD",
                    Description = "Servicios en la nube",
                    PictureUrl = "https://images.unsplash.com/photo-1544197150-b99a580bb7a8?w=600&q=80"
                });
            await db.SaveChangesAsync();
        }
    }
}
