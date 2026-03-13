using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Repositories.Base;

namespace ShipmentService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Repository interface cho Shipment
/// </summary>
public interface IShipmentRepository : IGenericRepository<Shipment>
{
    Task<List<Shipment>> GetByOrderIdAsync(Guid orderId);
    Task<List<Shipment>> GetByStatusAsync(string status);
    Task<bool> ExistsByTrackingNumberAsync(string trackingNumber);
    Task<bool> HasNonTerminalByProviderIdAsync(Guid providerId);
    Task<bool> HasNonTerminalByServiceIdAsync(Guid serviceId);
}
