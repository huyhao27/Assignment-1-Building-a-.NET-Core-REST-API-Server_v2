using GameInventoryApi.DTOs;
using GameInventoryApi.Models;
using GameInventoryApi.Repositories;

namespace GameInventoryApi.Services;

public class InventoryService : IInventoryService
{
    private readonly IMongoRepository<InventoryItem> _repository;

    public InventoryService(IMongoRepository<InventoryItem> repository)
    {
        _repository = repository;
    }

    public Task<List<InventoryItem>> GetAllAsync() => _repository.GetAllAsync();
    public Task<InventoryItem?> GetByIdAsync(string id) => _repository.GetByIdAsync(id);
    public Task CreateAsync(InventoryItem item) => _repository.CreateAsync(item);
    public Task UpdateAsync(string id, InventoryItem item) => _repository.UpdateAsync(id, item);
    public Task DeleteAsync(string id) => _repository.DeleteAsync(id);

    public async Task<InventoryItem?> PatchAsync(string id, PatchInventoryItemDto patch)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item is null) return null;

        if (patch.ItemId is not null) item.ItemId = patch.ItemId;
        if (patch.Name is not null) item.Name = patch.Name;
        if (patch.Quantity is not null) item.Quantity = patch.Quantity.Value;
        if (patch.PlayerId is not null) item.PlayerId = patch.PlayerId;
        item.LastUpdated = DateTime.UtcNow;

        await _repository.UpdateAsync(id, item);
        return item;
    }
}
