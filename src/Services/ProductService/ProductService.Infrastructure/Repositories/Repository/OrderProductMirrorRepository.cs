using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Events;

namespace ProductService.Infrastructure.Repositories.Repository;

public sealed class OrderProductMirrorRepository : IOrderProductMirrorRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ProductDbContext _context;

    public OrderProductMirrorRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(OrderProductMirror row, CancellationToken cancellationToken = default)
    {
        if (row.OrderId == Guid.Empty)
            return;

        var existing = await _context.OrderProductMirrors
            .AsTracking()
            .FirstOrDefaultAsync(x => x.OrderId == row.OrderId, cancellationToken);

        if (existing is null)
        {
            await _context.OrderProductMirrors.AddAsync(row, cancellationToken);
        }
        else
        {
            existing.ShopId = row.ShopId;
            existing.AccountId = row.AccountId;
            existing.Status = row.Status;
            existing.SubtotalVnd = row.SubtotalVnd;
            existing.TotalAmountVnd = row.TotalAmountVnd;
            existing.CommissionVnd = row.CommissionVnd;
            existing.CommissionRate = row.CommissionRate;
            existing.VoucherId = row.VoucherId;
            existing.DeliveryAddress = row.DeliveryAddress;
            existing.ProviderServiceCode = row.ProviderServiceCode;
            existing.CreatedAt = row.CreatedAt;
            existing.UpdatedAt = row.UpdatedAt;
            existing.LastSnapshotAt = row.LastSnapshotAt;
            existing.LineItemsJson = row.LineItemsJson;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task IngestSnapshotAndSyncSalesAsync(OrderSnapshotForProductEvent evt, CancellationToken cancellationToken = default)
    {
        if (evt.OrderId == Guid.Empty)
            return;

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var existing = await _context.OrderProductMirrors
            .AsTracking()
            .FirstOrDefaultAsync(x => x.OrderId == evt.OrderId, cancellationToken);

        var oldStatus = existing?.Status ?? string.Empty;
        var newStatus = evt.Status?.Trim() ?? string.Empty;

        var wasCountable = IsSalesCountableStatus(oldStatus);
        var nowCountable = IsSalesCountableStatus(newStatus);

        var lines = evt.Lines ?? new List<OrderSnapshotLineForProduct>();
        if (!wasCountable && nowCountable)
            await ApplySalesDeltaAsync(lines, add: true, cancellationToken);
        else if (wasCountable && !nowCountable && IsSalesReversalStatus(newStatus))
        {
            var linesForReversal = lines.Count > 0
                ? lines
                : DeserializeLines(existing?.LineItemsJson);
            await ApplySalesDeltaAsync(linesForReversal, add: false, cancellationToken);
        }

        var lineItemsJson = JsonSerializer.Serialize(lines, JsonOpts);
        var lastSnap = evt.SnapshotAt == default ? DateTime.UtcNow : evt.SnapshotAt;

        if (existing is null)
        {
            await _context.OrderProductMirrors.AddAsync(new OrderProductMirror
            {
                OrderId = evt.OrderId,
                ShopId = evt.ShopId,
                AccountId = evt.AccountId,
                Status = newStatus,
                SubtotalVnd = evt.SubtotalVnd,
                TotalAmountVnd = evt.TotalAmountVnd,
                CommissionVnd = evt.CommissionVnd,
                CommissionRate = evt.CommissionRate,
                VoucherId = evt.VoucherId,
                DeliveryAddress = evt.DeliveryAddress,
                ProviderServiceCode = evt.ProviderServiceCode ?? string.Empty,
                CreatedAt = evt.CreatedAt,
                UpdatedAt = evt.UpdatedAt,
                LastSnapshotAt = lastSnap,
                LineItemsJson = lineItemsJson
            }, cancellationToken);
        }
        else
        {
            existing.ShopId = evt.ShopId;
            existing.AccountId = evt.AccountId;
            existing.Status = newStatus;
            existing.SubtotalVnd = evt.SubtotalVnd;
            existing.TotalAmountVnd = evt.TotalAmountVnd;
            existing.CommissionVnd = evt.CommissionVnd;
            existing.CommissionRate = evt.CommissionRate;
            existing.VoucherId = evt.VoucherId;
            existing.DeliveryAddress = evt.DeliveryAddress;
            existing.ProviderServiceCode = evt.ProviderServiceCode ?? string.Empty;
            existing.CreatedAt = evt.CreatedAt;
            existing.UpdatedAt = evt.UpdatedAt;
            existing.LastSnapshotAt = lastSnap;
            existing.LineItemsJson = lineItemsJson;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private static bool IsSalesCountableStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;
        return status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase)
               || status.Equals("DELIVERED", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSalesReversalStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;
        return status.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase)
               || status.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ApplySalesDeltaAsync(
        IReadOnlyList<OrderSnapshotLineForProduct> lines,
        bool add,
        CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
            return;

        var versionIds = lines
            .Select(l => l.VersionId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (versionIds.Count == 0)
            return;

        var versionToProduct = await _context.ProductVersions
            .AsNoTracking()
            .Where(v => versionIds.Contains(v.VersionId))
            .Select(v => new { v.VersionId, v.ProductId })
            .ToDictionaryAsync(x => x.VersionId, x => x.ProductId, cancellationToken);

        var qtyByProduct = new Dictionary<Guid, int>();
        foreach (var line in lines)
        {
            if (!versionToProduct.TryGetValue(line.VersionId, out var productId) || productId == Guid.Empty)
                continue;
            var q = Math.Max(0, line.Quantity);
            if (q == 0)
                continue;
            qtyByProduct[productId] = qtyByProduct.GetValueOrDefault(productId) + q;
        }

        if (qtyByProduct.Count == 0)
            return;

        var productIds = qtyByProduct.Keys.ToList();
        var masters = await _context.ProductMasters
            .AsTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync(cancellationToken);

        var utc = DateTime.UtcNow;
        foreach (var m in masters)
        {
            var dq = qtyByProduct.GetValueOrDefault(m.ProductId);
            if (dq <= 0)
                continue;

            if (add)
                m.Sales += dq;
            else
                m.Sales = Math.Max(0, m.Sales - dq);

            m.UpdatedAt = utc;
        }
    }

    private static List<OrderSnapshotLineForProduct> DeserializeLines(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<OrderSnapshotLineForProduct>();

        try
        {
            return JsonSerializer.Deserialize<List<OrderSnapshotLineForProduct>>(json, JsonOpts)
                   ?? new List<OrderSnapshotLineForProduct>();
        }
        catch
        {
            return new List<OrderSnapshotLineForProduct>();
        }
    }
}
