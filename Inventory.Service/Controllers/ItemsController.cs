using Common;
using Inventory.Service.Dtos;
using Inventory.Service.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController(IRepository<InventoryItem> itemsRepository) : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository = itemsRepository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var items = (await _itemsRepository.GetAllAsync(items => items.UserId == userId))
            .Select(item => item.AsDto());
        
        return Ok(items);
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