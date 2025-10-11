using Common.MassTransit;
using Common.MongoDB;
using Inventory.Service.Clients;
using Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryitems")
    .AddMongoRepository<CatalogItem>("catalogitems")
    .AddMassTransitWithRabbitMQ();

var services = builder.Services;

AddCatalogClient(services);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

void AddCatalogClient(IServiceCollection services)
{
    Random jitterer = new Random();
    
    services.AddHttpClient<CatalogClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:5000");
        })
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
            onRetry: (outcome, timespan, retryAttempt) =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClient>>() ?
                    .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
            }
        ))
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(15),
            onBreak: (outcome, timespan) =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClient>>() ?
                    .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds");
            },
            onReset: () =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClient>>() ?
                    .LogWarning($"Closing the circuit...");  
            }
        ))
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}

app.Run();