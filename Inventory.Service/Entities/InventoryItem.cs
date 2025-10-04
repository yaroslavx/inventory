using Common;

namespace Inventory.Service.Entities;

public class InventoryItem : IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CatalogItemId { get; set; }

    public int Quatity { get; set; }

    public DateTimeOffset AcquireDate { get; set; }
}