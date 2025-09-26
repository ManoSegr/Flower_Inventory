using FlowerShop.Domain;
using FlowerShop.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Application;

public class CategoryService : ICategoryService
{
    private readonly FlowerShopDbContext _db;
    public CategoryService(FlowerShopDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<Category> GetAsync(int id, CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id, ct)
           ?? throw new KeyNotFoundException($"Category {id} not found");

    public async Task<Category> GetWithFlowersAsync(int id, CancellationToken ct = default)
        => await _db.Categories
            .AsNoTracking()
            .Include(c => c.Flowers)
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct)
           ?? throw new KeyNotFoundException($"Category {id} not found");

    public async Task<int> CreateAsync(Category input, CancellationToken ct = default)
    {
        input.CreatedAt = DateTime.UtcNow;
        _db.Categories.Add(input);
        await _db.SaveChangesAsync(ct);
        return input.CategoryId;
    }

    public async Task UpdateAsync(Category input, byte[] originalRowVersion, CancellationToken ct = default)
    {
        var existing = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == input.CategoryId, ct)
                      ?? throw new KeyNotFoundException($"Category {input.CategoryId} not found");

        _db.Entry(existing).Property(x => x.RowVersion).OriginalValue = originalRowVersion;
        existing.Name = input.Name;
        existing.Description = input.Description;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FindAsync(new object?[] { id }, ct);
        if (entity == null) return;
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
