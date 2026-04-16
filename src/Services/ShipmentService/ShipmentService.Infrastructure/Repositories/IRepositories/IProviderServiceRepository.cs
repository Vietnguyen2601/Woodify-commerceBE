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
    /// Tìm dịch vụ active theo shopId và providerServiceCode (legacy - không filter theo provider).
    /// </summary>
    Task<ProviderService?> GetByShopIdAndCodeAsync(Guid shopId, string code);

    /// <summary>
    /// Tìm dịch vụ active theo providerId và serviceCode.
    /// </summary>
    Task<ProviderService?> GetByProviderIdAndCodeAsync(Guid providerId, string code);

    Task<bool> HasActiveByProviderIdAsync(Guid providerId);
    Task<bool> ExistsByCodeForProviderAsync(Guid providerId, string code);
}
