using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Repositories.Base;

namespace ShipmentService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Repository interface cho ShippingProvider
/// </summary>
public interface IShippingProviderRepository : IGenericRepository<ShippingProvider>
{
    Task<List<ShippingProvider>> GetAllActiveAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludedProviderId);
}
