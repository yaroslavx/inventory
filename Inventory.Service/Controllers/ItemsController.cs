using Common;
using Inventory.Service.Clients;
using Inventory.Service.Dtos;
using Inventory.Service.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient) : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository = itemsRepository;
    private readonly CatalogClient _catalogClient = catalogClient;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var catalogItems = await _catalogClient.GetCatalogItemsAsync();
        var inventoryItemEntities = await _itemsRepository.GetAllAsync(item => item.UserId == userId);

        var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });
        
        return Ok(inventoryItemDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await _itemsRepository.GetAsync(
            item => item.UserId == grantItemsDto.UserId 
                    && item.CatalogItemId == grantItemsDto.CatalogItemId);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = grantItemsDto.CatalogItemId,
                UserId = grantItemsDto.UserId,
                Quatity = grantItemsDto.Quantity,
                AcquireDate = DateTimeOffset.Now,
            };

            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quatity += grantItemsDto.Quantity;
            await _itemsRepository.UpdateAsync(inventoryItem);
        }
        
        return Ok();
    }
}