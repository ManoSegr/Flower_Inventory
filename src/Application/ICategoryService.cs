using FlowerShop.Domain;

namespace FlowerShop.Application;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default);
    Task<Category> GetAsync(int id, CancellationToken ct = default);
    Task<Category> GetWithFlowersAsync(int id, CancellationToken ct = default); 
    Task<int> CreateAsync(Category input, CancellationToken ct = default);
    Task UpdateAsync(Category input, byte[] originalRowVersion, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
