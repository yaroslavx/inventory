using Inventory.Service.Entities;
using Inventory.Service.Dtos;

namespace Inventory.Service;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem item, string name, string description)
    {
        return new InventoryItemDto(item.CatalogItemId, name, description, item.Quatity, item.AcquireDate);
    }
}