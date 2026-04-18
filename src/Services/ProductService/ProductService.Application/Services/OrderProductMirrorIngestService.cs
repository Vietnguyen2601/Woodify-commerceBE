using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Events;

namespace ProductService.Application.Services;

public sealed class OrderProductMirrorIngestService
{
    private readonly IOrderProductMirrorRepository _repository;

    public OrderProductMirrorIngestService(IOrderProductMirrorRepository repository)
    {
        _repository = repository;
    }

    public Task IngestAsync(OrderSnapshotForProductEvent evt, CancellationToken cancellationToken = default)
    {
        if (evt.OrderId == Guid.Empty)
            return Task.CompletedTask;

        return _repository.IngestSnapshotAndSyncSalesAsync(evt, cancellationToken);
    }
}
