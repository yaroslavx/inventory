using Common;
using Contracts;
using Inventory.Service.Entities;
using MassTransit;

namespace Inventory.Service.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
    {
        _repository = repository;
    }
    
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);

        if (item == null)
        {
           return;
        }

        await _repository.RemoveAsync(message.ItemId);
    }
}