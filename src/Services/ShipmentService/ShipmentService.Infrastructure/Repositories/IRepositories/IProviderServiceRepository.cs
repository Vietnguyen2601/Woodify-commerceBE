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

    /// <summary>
    /// Tìm dịch vụ active theo code (e.g., "STD", "ECO", "EXP").
    /// </summary>
    Task<ProviderService?> GetByCodeAsync(string code);

    /// <summary>
    /// Tìm dịch vụ active theo shopId và providerServiceCode.
    /// </summary>
    Task<ProviderService?> GetByShopIdAndCodeAsync(Guid shopId, string code);
}
