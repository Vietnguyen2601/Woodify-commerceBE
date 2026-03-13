using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho business service tính và preview phí vận chuyển.
/// </summary>
public interface IShippingFeePreviewService
{
    /// <summary>
    /// Tính toán và trả về bản preview phí ship cho checkout.
    /// Không tạo bất kỳ record nào trong DB — chỉ đọc và tính toán.
    /// </summary>
    /// <param name="request">Thông tin để tính phí ship.</param>
    /// <returns>Chi tiết phí ship đã tính.</returns>
    Task<ServiceResult<ShippingFeePreviewResponse>> CalculateAsync(ShippingFeePreviewRequest request);
}
