using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowerShop.Domain;
using FlowerShop.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowerShop.Application
{
    public class FlowerService : IFlowerService
    {
        private readonly FlowerShopDbContext _db;
        private readonly ILogger<FlowerService> _log;

        public FlowerService(FlowerShopDbContext db, ILogger<FlowerService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<(IReadOnlyList<Flower> Items, int Total)> SearchAsync(
            string? q, int? categoryId, decimal? minPrice, decimal? maxPrice,
            string sort = "name", bool desc = false, int page = 1, int pageSize = 10,
            CancellationToken ct = default)
        {
            var dir = desc ? "DESC" : "ASC";

            
            var items = await _db.Set<Flower>().FromSqlRaw(
                    "EXEC inv.usp_Flower_Search @q, @categoryId, @minPrice, @maxPrice, @activeOnly, @sort, @dir, @page, @pageSize",
                    new SqlParameter("@q", (object?)q ?? DBNull.Value),
                    new SqlParameter("@categoryId", (object?)categoryId ?? DBNull.Value),
                    new SqlParameter("@minPrice", (object?)minPrice ?? DBNull.Value),
                    new SqlParameter("@maxPrice", (object?)maxPrice ?? DBNull.Value),
                    new SqlParameter("@activeOnly", true),
                    new SqlParameter("@sort", sort),
                    new SqlParameter("@dir", dir),
                    new SqlParameter("@page", page),
                    new SqlParameter("@pageSize", pageSize)
                )
                .AsNoTracking()
                .ToListAsync(ct);

           
            if (items.Count > 0)
            {
                var catIds = items.Select(f => f.CategoryId).Distinct().ToList();
                var cats = await _db.Categories
                                    .AsNoTracking()
                                    .Where(c => catIds.Contains(c.CategoryId))
                                    .ToDictionaryAsync(c => c.CategoryId, ct);

                foreach (var f in items)
                    f.Category = cats.TryGetValue(f.CategoryId, out var cat) ? cat : null;
            }

            return (items, items.Count);
        }

        public async Task<Flower> GetAsync(int id, CancellationToken ct = default)
        {
            var item = await _db.Flowers
                                .Include(x => x.Category)
                                .FirstOrDefaultAsync(x => x.FlowerId == id, ct);

            if (item == null)
                throw new KeyNotFoundException($"Flower {id} not found");

            return item;
        }

        public async Task<int> CreateAsync(Flower input, CancellationToken ct = default)
        {
            input.CreatedAt = DateTime.UtcNow;
            input.UpdatedAt = DateTime.UtcNow;

            _db.Flowers.Add(input);
            await _db.SaveChangesAsync(ct);
            return input.FlowerId;
        }

        public async Task UpdateAsync(Flower input, byte[] originalRowVersion, CancellationToken ct = default)
        {
            var existing = await _db.Flowers
                                    .FirstOrDefaultAsync(f => f.FlowerId == input.FlowerId, ct)
                           ?? throw new KeyNotFoundException($"Flower {input.FlowerId} not found");

            _db.Entry(existing).Property(x => x.RowVersion).OriginalValue = originalRowVersion;

            existing.Name = input.Name;
            existing.Type = input.Type;
            existing.SKU = input.SKU;
            existing.CategoryId = input.CategoryId;
            existing.Price = input.Price;
            existing.QuantityInStock = input.QuantityInStock;
            existing.Color = input.Color;
            existing.StemLengthCm = input.StemLengthCm;
            existing.ImageUrl = input.ImageUrl;
            existing.Active = input.Active;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Flowers.FindAsync(new object?[] { id }, ct);
            if (entity == null) return;

            _db.Flowers.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
