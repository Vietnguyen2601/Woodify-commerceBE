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
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391

    /// <summary>
    /// Tìm dịch vụ active theo code (e.g., "STD", "ECO", "EXP").
    /// </summary>
    Task<ProviderService?> GetByCodeAsync(string code);

    /// <summary>
    /// Tìm dịch vụ active theo shopId và providerServiceCode.
    /// </summary>
    Task<ProviderService?> GetByShopIdAndCodeAsync(Guid shopId, string code);
<<<<<<< HEAD
=======
    Task<bool> HasActiveByProviderIdAsync(Guid providerId);
    Task<bool> ExistsByCodeForProviderAsync(Guid providerId, string code);
>>>>>>> develop
=======
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
}
