using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Shared.Events;
using Shared.Messaging;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe events từ ProductService
/// Đồng bộ category data vào local cache
/// </summary>
public class CategoryEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer? _rabbitMQConsumer;

    public CategoryEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer? rabbitMQConsumer = null)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public void StartListening()
    {
        if (_rabbitMQConsumer == null)
        {
            Console.WriteLine("CategoryEventConsumer: RabbitMQConsumer is not available. Skipping listener initialization.");
            return;
        }

        // Subscribe to Category Created events
        _rabbitMQConsumer.Subscribe<CategoryCreatedEvent>(
            queueName: "orderservice.category.created",
            exchange: "product.events",
            routingKey: "category.created",
            handler: async (message) => await HandleCategoryCreated(message)
        );

        // Subscribe to Category Updated events
        _rabbitMQConsumer.Subscribe<CategoryUpdatedEvent>(
            queueName: "orderservice.category.updated",
            exchange: "product.events",
            routingKey: "category.updated",
            handler: async (message) => await HandleCategoryUpdated(message)
        );

        // Subscribe to Category Deleted events
        _rabbitMQConsumer.Subscribe<CategoryDeletedEvent>(
            queueName: "orderservice.category.deleted",
            exchange: "product.events",
            routingKey: "category.deleted",
            handler: async (message) => await HandleCategoryDeleted(message)
        );

        Console.WriteLine("CategoryEventConsumer started listening for Category events");
        Console.WriteLine("Subscribed to: product.events exchange with routing keys: category.created, category.updated, category.deleted");
    }

    private async Task HandleCategoryCreated(CategoryCreatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received CategoryCreated event: CategoryId={evt.CategoryId}, Name={evt.Name}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<ICategoryCacheRepository>();

            var cache = new CategoryCache
            {
                CategoryId = evt.CategoryId,
                Name = evt.Name,
                Description = evt.Description,
                ParentCategoryId = evt.ParentCategoryId,
                Level = evt.Level,
                IsActive = evt.IsActive,
                LastUpdated = evt.CreatedAt
            };

            await cacheRepository.CreateAsync(cache);
            Console.WriteLine($"[OrderService] Category cache created: {evt.CategoryId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling CategoryCreated: {ex.Message}");
        }
    }

    private async Task HandleCategoryUpdated(CategoryUpdatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received CategoryUpdated event: CategoryId={evt.CategoryId}, Name={evt.Name}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<ICategoryCacheRepository>();

            var cache = new CategoryCache
            {
                CategoryId = evt.CategoryId,
                Name = evt.Name,
                Description = evt.Description,
                ParentCategoryId = evt.ParentCategoryId,
                Level = evt.Level,
                IsActive = evt.IsActive,
                LastUpdated = evt.UpdatedAt
            };

            await cacheRepository.UpsertAsync(cache);
            Console.WriteLine($"[OrderService] Category cache updated: {evt.CategoryId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling CategoryUpdated: {ex.Message}");
        }
    }

    private async Task HandleCategoryDeleted(CategoryDeletedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received CategoryDeleted event: CategoryId={evt.CategoryId}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<ICategoryCacheRepository>();

            await cacheRepository.SoftDeleteAsync(evt.CategoryId);
            Console.WriteLine($"[OrderService] Category marked as deleted in cache: {evt.CategoryId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling CategoryDeleted: {ex.Message}");
        }
    }
}
