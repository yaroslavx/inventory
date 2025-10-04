using Inventory.Service.Entities;
using Inventory.Service.Dtos;

namespace Inventory.Service;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem item)
    {
        return new InventoryItemDto(item.CatalogItemId, item.Quatity, item.AcquireDate);
    }
}