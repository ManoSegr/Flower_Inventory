
using FlowerShop.Domain;

namespace FlowerShop.Application;

public interface IFlowerService
{
    Task<(IReadOnlyList<Flower> Items, int Total)> SearchAsync(string? q, int? categoryId, decimal? minPrice, decimal? maxPrice,
        string sort = "name", bool desc = false, int page = 1, int pageSize = 10, CancellationToken ct = default);

    Task<Flower> GetAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(Flower input, CancellationToken ct = default);
    Task UpdateAsync(Flower input, byte[] originalRowVersion, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}