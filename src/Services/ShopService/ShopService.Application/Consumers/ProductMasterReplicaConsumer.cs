using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using Shared.Messaging;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data.Context;

namespace ShopService.Application.Consumers;

/// <summary>
/// Đồng bộ ProductMaster từ ProductService qua các event hiện có:
/// product.master.created, product.master.updated, product.deleted.
/// </summary>
public class ProductMasterReplicaConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductMasterReplicaConsumer(RabbitMQConsumer rabbitMQConsumer, IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        const string exchange = "product.events";

        _rabbitMQConsumer.Subscribe<ProductMasterCreatedEvent>(
            queueName: "shopservice.product_master.replica.created",
            exchange: exchange,
            routingKey: "product.master.created",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                var now = evt.CreatedAt;
                if (now == default)
                    now = DateTime.UtcNow;

                var row = await db.ProductMasterReplicas.AsTracking()
                    .FirstOrDefaultAsync(r => r.ProductId == evt.ProductId);
                if (row == null)
                {
                    db.ProductMasterReplicas.Add(new ProductMasterReplica
                    {
                        ProductId = evt.ProductId,
                        ShopId = evt.ShopId,
                        CategoryId = evt.CategoryId,
                        Name = evt.Name,
                        Description = evt.Description,
                        Status = evt.Status,
                        ModerationStatus = evt.ModerationStatus,
                        HasVersions = evt.HasVersions,
                        IsDeleted = string.Equals(evt.Status, "DELETED", StringComparison.OrdinalIgnoreCase),
                        DeletedAtUtc = string.Equals(evt.Status, "DELETED", StringComparison.OrdinalIgnoreCase)
                            ? now
                            : null,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }
                else
                {
                    ApplyMasterFields(row, evt.ShopId, evt.CategoryId, evt.Name, evt.Description, evt.Status,
                        evt.ModerationStatus, evt.HasVersions, now);
                    row.UpdatedAtUtc = now;
                }

                await db.SaveChangesAsync();
            });

        _rabbitMQConsumer.Subscribe<ProductMasterUpdatedEvent>(
            queueName: "shopservice.product_master.replica.updated",
            exchange: exchange,
            routingKey: "product.master.updated",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                var now = evt.UpdatedAt;
                if (now == default)
                    now = DateTime.UtcNow;

                var row = await db.ProductMasterReplicas.AsTracking()
                    .FirstOrDefaultAsync(r => r.ProductId == evt.ProductId);
                if (row == null)
                {
                    db.ProductMasterReplicas.Add(new ProductMasterReplica
                    {
                        ProductId = evt.ProductId,
                        ShopId = evt.ShopId,
                        CategoryId = evt.CategoryId,
                        Name = evt.Name,
                        Description = evt.Description,
                        Status = evt.Status,
                        ModerationStatus = evt.ModerationStatus,
                        HasVersions = evt.HasVersions,
                        IsDeleted = string.Equals(evt.Status, "DELETED", StringComparison.OrdinalIgnoreCase),
                        DeletedAtUtc = string.Equals(evt.Status, "DELETED", StringComparison.OrdinalIgnoreCase)
                            ? now
                            : null,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }
                else
                {
                    ApplyMasterFields(row, evt.ShopId, evt.CategoryId, evt.Name, evt.Description, evt.Status,
                        evt.ModerationStatus, evt.HasVersions, now);
                    row.UpdatedAtUtc = now;
                }

                await db.SaveChangesAsync();
            });

        _rabbitMQConsumer.Subscribe<ProductDeletedEvent>(
            queueName: "shopservice.product_master.replica.deleted",
            exchange: exchange,
            routingKey: "product.deleted",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                var row = await db.ProductMasterReplicas.AsTracking()
                    .FirstOrDefaultAsync(r => r.ProductId == evt.ProductId);
                if (row == null)
                    return;

                row.IsDeleted = true;
                row.DeletedAtUtc = evt.DeletedAt == default ? DateTime.UtcNow : evt.DeletedAt;
                row.Status = "DELETED";
                row.Name = string.IsNullOrEmpty(evt.ProductName) ? row.Name : evt.ProductName;
                row.UpdatedAtUtc = DateTime.UtcNow;
                await db.SaveChangesAsync();
            });

        Console.WriteLine(
            "[ShopService] ProductMasterReplicaConsumer listening: product.events → product.master.created|updated, product.deleted");
    }

    private static void ApplyMasterFields(
        ProductMasterReplica row,
        Guid shopId,
        Guid categoryId,
        string name,
        string? description,
        string status,
        string? moderationStatus,
        bool hasVersions,
        DateTime changeTime)
    {
        row.ShopId = shopId;
        row.CategoryId = categoryId;
        row.Name = name;
        row.Description = description;
        row.Status = status;
        row.ModerationStatus = moderationStatus;
        row.HasVersions = hasVersions;

        if (string.Equals(status, "DELETED", StringComparison.OrdinalIgnoreCase))
        {
            row.IsDeleted = true;
            row.DeletedAtUtc ??= changeTime;
        }
        else
        {
            row.IsDeleted = false;
            row.DeletedAtUtc = null;
        }
    }
}
