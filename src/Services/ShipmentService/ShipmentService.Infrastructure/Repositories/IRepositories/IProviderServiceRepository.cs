using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Repositories.Base;

namespace ShipmentService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Repository interface cho Provider Service
/// </summary>
public interface IProviderServiceRepository : IGenericRepository<ProviderService>
{
    Task<List<ProviderService>> GetByProviderIdAsync(Guid providerId);
    Task<List<ProviderService>> GetAllActiveAsync();
}
