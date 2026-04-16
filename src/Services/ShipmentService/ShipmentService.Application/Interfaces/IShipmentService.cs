using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho Shipment Business Service
/// </summary>
public interface IShipmentService
{
    Task<ServiceResult<IEnumerable<ShipmentDto>>> GetAllAsync();
    Task<ServiceResult<ShipmentDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<IEnumerable<ShipmentDto>>> GetByOrderIdAsync(Guid orderId);
    Task<ServiceResult<IEnumerable<ShipmentDto>>> GetByShopIdAsync(Guid shopId, string? status = null);
    Task<ServiceResult<ShipmentDto>> CreateAsync(CreateShipmentDto dto);
    Task<ServiceResult<ShipmentDto>> UpdateAsync(Guid id, UpdateShipmentDto dto);
    Task<ServiceResult<ShipmentDto>> UpdateStatusAsync(Guid id, UpdateShipmentStatusDto dto);
    Task<ServiceResult<ShipmentDto>> UpdatePickupAsync(Guid id, UpdateShipmentPickupDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
}
