using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Events;

namespace ProductService.Application.Services;

/// <summary>
/// Applies <see cref="OrderDeliveredStockEvent"/> — decrements <c>stock_quantity</c> once per order (ledger).
/// </summary>
public class OrderDeliveredStockIngestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderDeliveredStockLedgerRepository _ledger;
    private readonly IImageUrlRepository _imageUrlRepository;
    private readonly ProductEventPublisher _eventPublisher;

    public OrderDeliveredStockIngestService(
        IUnitOfWork unitOfWork,
        IOrderDeliveredStockLedgerRepository ledger,
        IImageUrlRepository imageUrlRepository,
        ProductEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _ledger = ledger;
        _imageUrlRepository = imageUrlRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task IngestAsync(OrderDeliveredStockEvent evt, CancellationToken cancellationToken = default)
    {
        if (evt.Lines.Count == 0)
            return;

        if (await _ledger.ExistsForOrderAsync(evt.OrderId, cancellationToken))
            return;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _ledger.AddAsync(new OrderDeliveredStockLedger
            {
                OrderId = evt.OrderId,
                ProcessedAt = DateTime.UtcNow
            }, cancellationToken);

            var versionsToNotify = new List<ProductVersion>();

            var byVersion = evt.Lines
                .Where(l => l.Quantity > 0)
                .GroupBy(l => l.VersionId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            foreach (var (versionId, qty) in byVersion)
            {
                var version = await _unitOfWork.ProductVersions.GetByIdAsync(versionId);
                if (version?.Product == null)
                    continue;

                version.StockQuantity = Math.Max(0, version.StockQuantity - qty);
                version.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.MarkAsModified(version);
                versionsToNotify.Add(version);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            foreach (var version in versionsToNotify.DistinctBy(v => v.VersionId))
            {
                if (version.Product is null)
                    continue;

                var product = version.Product;
                var thumbMap = await _imageUrlRepository.GetPrimaryImageBatchAsync(
                    "PRODUCT_VERSION",
                    new[] { version.VersionId });
                var thumb = thumbMap.GetValueOrDefault(version.VersionId);
                _eventPublisher.PublishProductVersionUpdated(new ProductVersionUpdatedEvent
                {
                    VersionId = version.VersionId,
                    ProductId = version.ProductId,
                    ShopId = product.ShopId,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    ProductStatus = product.Status.ToString(),
                    SellerSku = version.SellerSku,
                    VersionNumber = version.VersionNumber,
                    VersionName = version.VersionName,
                    Price = version.Price,
                    Currency = "VND",
                    StockQuantity = version.StockQuantity,
                    WoodType = version.WoodType,
                    WeightGrams = version.WeightGrams,
                    LengthCm = version.LengthCm,
                    WidthCm = version.WidthCm,
                    HeightCm = version.HeightCm,
                    IsActive = version.IsActive,
                    ThumbnailUrl = thumb,
                    UpdatedAt = version.UpdatedAt ?? DateTime.UtcNow,
                    EventType = "Updated"
                });
            }
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
