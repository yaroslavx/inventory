namespace Inventory.Service;

public record GrantItemsDto(Guid UserId, Guid CatalogItemId, int Quantity);

public record InventoryItemDto(Guid CatalogItemId, int Quantity, DateTimeOffset AcquireDate);