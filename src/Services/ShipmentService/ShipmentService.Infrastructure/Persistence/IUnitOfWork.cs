using ShipmentService.Infrastructure.Repositories.IRepositories;

namespace ShipmentService.Infrastructure.Persistence;

/// <summary>
/// Unit of Work Interface — quản lý transaction và repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IShipmentRepository Shipments { get; }
    IShippingProviderRepository ShippingProviders { get; }
    IProviderServiceRepository ProviderServices { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
