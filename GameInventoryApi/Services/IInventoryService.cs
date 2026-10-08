using GameInventoryApi.DTOs;
using GameInventoryApi.Models;

namespace GameInventoryApi.Services;

public interface IInventoryService
{
    Task<List<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(string id);
    Task CreateAsync(InventoryItem item);
    Task UpdateAsync(string id, InventoryItem item);
    Task<InventoryItem?> PatchAsync(string id, PatchInventoryItemDto patch);
    Task DeleteAsync(string id);
}
