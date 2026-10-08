namespace GameInventoryApi.DTOs;

// Null means "leave unchanged", so a PATCH body only needs the fields being modified.
public record PatchInventoryItemDto(
    string? ItemId,
    string? Name,
    int? Quantity,
    string? PlayerId);
