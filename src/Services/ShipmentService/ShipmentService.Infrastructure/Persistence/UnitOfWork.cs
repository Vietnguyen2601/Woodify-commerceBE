using Microsoft.EntityFrameworkCore.Storage;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using ShipmentService.Infrastructure.Repositories.Repository;

namespace ShipmentService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ShipmentDbContext _context;
    private IDbContextTransaction? _transaction;

    private IShipmentRepository? _shipments;
    private IShippingProviderRepository? _shippingProviders;
    private IProviderServiceRepository? _providerServices;

    public UnitOfWork(ShipmentDbContext context)
    {
        _context = context;
    }

    public IShipmentRepository Shipments => _shipments ??= new ShipmentRepository(_context);
    public IShippingProviderRepository ShippingProviders => _shippingProviders ??= new ShippingProviderRepository(_context);
    public IProviderServiceRepository ProviderServices => _providerServices ??= new ProviderServiceRepository(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
