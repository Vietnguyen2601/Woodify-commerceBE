namespace ShopService.Application.Interfaces;

/// <summary>
/// Tổng sản phẩm / đơn hàng từ read model (product_master_replica, OrderMetricsSnapshots), không dùng cột đếm trên bảng shops.
/// </summary>
public interface IShopReadModelTotalsQuery
{
    Task<IReadOnlyDictionary<Guid, (int TotalProducts, int TotalOrders)>> GetTotalsByShopIdsAsync(
        IReadOnlyList<Guid> shopIds,
        CancellationToken cancellationToken = default);
}
