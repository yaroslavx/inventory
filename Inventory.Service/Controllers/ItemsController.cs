using Common;
using Inventory.Service.Clients;
using Inventory.Service.Dtos;
using Inventory.Service.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem>  catalogItemRepository) : ControllerBase
{
    private readonly IRepository<InventoryItem> _inventoryItemsRepository = inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemRepository = catalogItemRepository;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var inventoryItemEntities = await _inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);
        var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
        var catalogItemEntities = await _catalogItemRepository.GetAllAsync(item => itemIds.Contains(item.Id));

        var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });
        
        return Ok(inventoryItemDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await _inventoryItemsRepository.GetAsync(
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

            await _inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quatity += grantItemsDto.Quantity;
            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
        }
        
        return Ok();
    }
}