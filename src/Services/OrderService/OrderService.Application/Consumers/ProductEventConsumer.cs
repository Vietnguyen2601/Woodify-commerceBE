using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Shared.Events;
using Shared.Messaging;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe events từ ProductService
/// Đồng bộ product data vào local cache
/// </summary>
public class ProductEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;

    public ProductEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public void StartListening()
    {
        // Subscribe to ProductVersion events
        _rabbitMQConsumer.Subscribe<ProductVersionUpdatedEvent>(
            queueName: "orderservice.product.version.updated",
            exchange: "product.events",
            routingKey: "product.version.updated",
            handler: async (message) => await HandleProductVersionUpdated(message)
        );

        // Subscribe to ProductStatus events
        _rabbitMQConsumer.Subscribe<ProductStatusChangedEvent>(
            queueName: "orderservice.product.status.changed",
            exchange: "product.events",
            routingKey: "product.status.changed",
            handler: async (message) => await HandleProductStatusChanged(message)
        );

        // Subscribe to ProductVersion Deleted events
        _rabbitMQConsumer.Subscribe<ProductVersionDeletedEvent>(
            queueName: "orderservice.product.version.deleted",
            exchange: "product.events",
            routingKey: "product.version.deleted",
            handler: async (message) => await HandleProductVersionDeleted(message)
        );

        // Subscribe to ProductVersion Restored events
        _rabbitMQConsumer.Subscribe<ProductVersionRestoredEvent>(
            queueName: "orderservice.product.version.restored",
            exchange: "product.events",
            routingKey: "product.version.restored",
            handler: async (message) => await HandleProductVersionRestored(message)
        );

        Console.WriteLine("ProductEventConsumer started listening for Product events");
        Console.WriteLine("Subscribed to: product.events exchange with routing keys: product.version.updated, product.status.changed, product.version.deleted, product.version.restored");
    }

    private async Task HandleProductVersionUpdated(ProductVersionUpdatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received ProductVersionUpdated event: VersionId={evt.VersionId}, Type={evt.EventType}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductVersionCacheRepository>();

            if (evt.EventType == "Deleted")
            {
                // TODO: Handle deletion - có thể soft delete hoặc mark as unavailable
                var existing = await cacheRepository.GetByVersionIdAsync(evt.VersionId);
                if (existing != null)
                {
                    await cacheRepository.RemoveAsync(existing);
                }
            }
            else
            {
                var cache = new ProductVersionCache
                {
                    VersionId = evt.VersionId,
                    ProductId = evt.ProductId,
                    ShopId = evt.ShopId,
                    
                    // Product Master Info
                    ProductName = evt.ProductName,
                    ProductDescription = evt.ProductDescription,
                    ProductStatus = evt.ProductStatus,
                    
                    // Version Info
                    SellerSku = evt.SellerSku,
                    VersionName = evt.VersionName,
                    
                    // Pricing
                    Price = evt.Price,
                    Currency = evt.Currency,
                    
                    // Stock
                    StockQuantity = evt.StockQuantity,
                    AllowBackorder = false, // Default to false as ProductVersion doesn't have this field
                    
                    // Shipping Dimensions
                    WeightGrams = evt.WeightGrams,
                    LengthCm = evt.LengthCm,
                    WidthCm = evt.WidthCm,
                    HeightCm = evt.HeightCm,
                    
                    // Status
                    IsActive = evt.IsActive,
                    
                    LastUpdated = evt.UpdatedAt
                };

                await cacheRepository.UpsertAsync(cache);
                Console.WriteLine($"[OrderService] Product version cache updated: {evt.VersionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling ProductVersionUpdated: {ex.Message}");
        }
    }

    private async Task HandleProductStatusChanged(ProductStatusChangedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received ProductStatusChanged event: ProductId={evt.ProductId}, Status={evt.Status}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductVersionCacheRepository>();

            await cacheRepository.UpdateProductStatusAsync(evt.ProductId, evt.Status);
            Console.WriteLine($"[OrderService] Product status updated in cache: {evt.ProductId} -> {evt.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling ProductStatusChanged: {ex.Message}");
        }
    }

    private async Task HandleProductVersionDeleted(ProductVersionDeletedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received ProductVersionDeleted event: VersionId={evt.VersionId}, ProductId={evt.ProductId}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductVersionCacheRepository>();

            var existing = await cacheRepository.GetByVersionIdAsync(evt.VersionId);
            if (existing != null)
            {
                // Soft delete in cache
                existing.IsDeleted = true;
                existing.DeletedAt = evt.DeletedAt;
                existing.LastUpdated = DateTime.UtcNow;
                
                await cacheRepository.UpdateAsync(existing);
                Console.WriteLine($"[OrderService] Product version marked as deleted in cache: {evt.VersionId}");
            }
            else
            {
                Console.WriteLine($"[OrderService] Product version not found in cache: {evt.VersionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling ProductVersionDeleted: {ex.Message}");
        }
    }

    private async Task HandleProductVersionRestored(ProductVersionRestoredEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received ProductVersionRestored event: VersionId={evt.VersionId}, ProductId={evt.ProductId}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductVersionCacheRepository>();

            var existing = await cacheRepository.GetByVersionIdAsync(evt.VersionId);
            if (existing != null)
            {
                // Restore in cache
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.LastUpdated = DateTime.UtcNow;
                
                await cacheRepository.UpdateAsync(existing);
                Console.WriteLine($"[OrderService] Product version restored in cache: {evt.VersionId}");
            }
            else
            {
                Console.WriteLine($"[OrderService] Product version not found in cache: {evt.VersionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling ProductVersionRestored: {ex.Message}");
        }
    }
}
