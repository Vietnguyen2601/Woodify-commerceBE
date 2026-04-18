using ProductService.Domain.Entities;
using Shared.Events;

namespace ProductService.Infrastructure.Repositories.IRepositories;

public interface IOrderProductMirrorRepository
{
    Task UpsertAsync(OrderProductMirror row, CancellationToken cancellationToken = default);

    /// <summary>Lưu mirror + cập nhật <see cref="ProductMaster.Sales"/> theo version trong line items (idempotent theo chuyển trạng thái đơn).</summary>
    Task IngestSnapshotAndSyncSalesAsync(OrderSnapshotForProductEvent evt, CancellationToken cancellationToken = default);
}
