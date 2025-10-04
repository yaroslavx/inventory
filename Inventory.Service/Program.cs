using Common.MongoDB;
using Inventory.Service.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventoryitems");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
