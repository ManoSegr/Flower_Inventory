using System;
using System.Threading.Tasks;
using FlowerShop.Application;
using FlowerShop.Domain;
using FlowerShop.Infrastructure;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class FlowerServiceTests
{
    private static FlowerShopDbContext MakeDb()
    {
        var opts = new DbContextOptionsBuilder<FlowerShopDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new FlowerShopDbContext(opts);
        db.Database.EnsureCreated();

        db.Categories.Add(new Category { CategoryId = 1, Name = "Roses", CreatedAt = DateTime.UtcNow });
        db.Flowers.Add(new Flower
        {
            FlowerId = 1,
            CategoryId = 1,
            Name = "Test Rose",
            Type = "Cut",
            SKU = "SKU-001",
            Price = 1.23m,
            QuantityInStock = 10,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Get_Returns_Item()
    {
        await using var db = MakeDb();
        var svc = new FlowerService(db, NullLogger<FlowerService>.Instance);

        var item = await svc.GetAsync(1);

        Assert.NotNull(item);
        Assert.Equal("Test Rose", item.Name);
        Assert.Equal(1, item.CategoryId);
    }

    [Fact]
    public async Task Create_Then_Get_Works()
    {
        await using var db = MakeDb();
        var svc = new FlowerService(db, NullLogger<FlowerService>.Instance);

        var id = await svc.CreateAsync(new Flower
        {
            CategoryId = 1,
            Name = "New Rose",
            Type = "Cut",
            SKU = "SKU-NEW",
            Price = 2.50m,
            QuantityInStock = 5,
            Active = true
        });

        var item = await svc.GetAsync(id);

        Assert.Equal("New Rose", item.Name);
        Assert.Equal(2.50m, item.Price);
    }

    [Fact]
    public async Task Update_Changes_Are_Persisted()
    {
        await using var db = MakeDb();
        var svc = new FlowerService(db, NullLogger<FlowerService>.Instance);

        var existing = await svc.GetAsync(1);
        var beforeUpdatedAt = existing.UpdatedAt;
        var originalRv = existing.RowVersion;

        existing.Price = 9.99m;
        existing.QuantityInStock = 99;

        await svc.UpdateAsync(existing, originalRv);

        var reloaded = await svc.GetAsync(1);
        Assert.Equal(9.99m, reloaded.Price);
        Assert.Equal(99, reloaded.QuantityInStock);
        Assert.True(reloaded.UpdatedAt >= beforeUpdatedAt);
  
    }

    [Fact]
    public async Task Update_With_Wrong_RowVersion_Throws()
    {
        await using var db = MakeDb();
        var svc = new FlowerService(db, NullLogger<FlowerService>.Instance);

        var existing = await svc.GetAsync(1);
        existing.Price = 5.00m;
        var wrongOriginal = new byte[] { 1, 2, 3, 4 };

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await svc.UpdateAsync(existing, wrongOriginal));
    }

    [Fact]
    public async Task Delete_Removes_Entity()
    {
        await using var db = MakeDb();
        var svc = new FlowerService(db, NullLogger<FlowerService>.Instance);

        await svc.DeleteAsync(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.GetAsync(1));
    }
}
