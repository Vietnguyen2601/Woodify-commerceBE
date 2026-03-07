using Shared.Events;
using Shared.Messaging;
using Shared.Results;
using ShopService.Application.DTOs;
using ShopService.Application.Mappers;
using ShopService.Infrastructure.UnitOfWork;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;

namespace ShopService.Application.Services;

public class ShopService : IShopService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RabbitMQPublisher? _rabbitPublisher;

    public ShopService(IUnitOfWork unitOfWork, RabbitMQPublisher? rabbitPublisher = null)
    {
        _unitOfWork = unitOfWork;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task<ServiceResult<RegisterShopResponseDto>> RegisterShopAsync(RegisterShopDto dto)
    {
        try
        {
            // Validate name uniqueness
            if (await _unitOfWork.Shops.ExistsWithNameAsync(dto.Name))
                return ServiceResult<RegisterShopResponseDto>.BadRequest("A shop with this name already exists");

            var shop = new Shop
            {
                OwnerAccountId = dto.OwnerAccountId,
                Name = dto.Name,
                Description = dto.Description,
                LogoUrl = dto.LogoUrl,
                CoverImageUrl = dto.CoverImageUrl,
                DefaultPickupAddress = dto.DefaultPickupAddress,
                DefaultProvider = dto.DefaultProvider,
                Status = ShopStatus.INACTIVE,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Shops.AddAsync(shop);
            await _unitOfWork.SaveChangesAsync();

            var response = new RegisterShopResponseDto
            {
                ShopId = shop.ShopId,
                Name = shop.Name,
                Status = shop.Status.ToString(),
                CreatedAt = shop.CreatedAt,
                Message = "Shop created successfully. Waiting for admin approval to activate"
            };

            return ServiceResult<RegisterShopResponseDto>.Created(response, response.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<RegisterShopResponseDto>.InternalServerError($"Error registering shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UpdateShopInfoResponseDto>> UpdateShopInfoAsync(Guid shopId, UpdateShopInfoDto dto)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<UpdateShopInfoResponseDto>.NotFound("Shop not found");

            // Block updates for suspended/banned shops
            if (shop.Status == ShopStatus.SUSPENDED ||
                shop.Status == ShopStatus.BANNED)
                return ServiceResult<UpdateShopInfoResponseDto>.BadRequest(
                    $"Cannot update shop with status '{shop.Status}'. Only ACTIVE or INACTIVE shops can be updated");

            // Validate name uniqueness (exclude current shop)
            if (!string.IsNullOrEmpty(dto.Name) && await _unitOfWork.Shops.ExistsWithNameExcludingAsync(dto.Name, shopId))
                return ServiceResult<UpdateShopInfoResponseDto>.BadRequest("A shop with this name already exists");

            shop.MapToShopInfo(dto);
            _unitOfWork.Shops.Update(shop);
            await _unitOfWork.SaveChangesAsync();

            var response = new UpdateShopInfoResponseDto
            {
                ShopId = shop.ShopId,
                Name = shop.Name,
                Status = shop.Status.ToString(),
                DefaultProvider = shop.DefaultProvider,
                UpdatedAt = shop.UpdatedAt,
                Message = "Thông tin shop đã được cập nhật thành công"
            };

            return ServiceResult<UpdateShopInfoResponseDto>.Success(response, response.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<UpdateShopInfoResponseDto>.InternalServerError($"Error updating shop info: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UpdateShopStatusResponseDto>> UpdateShopStatusAsync(Guid shopId, UpdateShopStatusDto dto)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<UpdateShopStatusResponseDto>.NotFound("Shop not found");

            if (!Enum.TryParse<ShopStatus>(dto.NewStatus.ToUpper(), out var newStatus))
                return ServiceResult<UpdateShopStatusResponseDto>.BadRequest($"Invalid status value: {dto.NewStatus}");

            var previousStatus = shop.Status;
            shop.Status = newStatus;
            shop.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Shops.Update(shop);
            await _unitOfWork.SaveChangesAsync();

            var message = newStatus switch
            {
                ShopStatus.ACTIVE =>
                    "Shop đã được duyệt và kích hoạt thành công",
                ShopStatus.INACTIVE =>
                    "Shop đã được chuyển về trạng thái chờ duyệt",
                ShopStatus.SUSPENDED =>
                    "Shop đã bị tạm khóa",
                ShopStatus.BANNED =>
                    "Shop đã bị cấm vĩnh viễn",
                _ => "Trạng thái shop đã được cập nhật thành công"
            };

            return ServiceResult<UpdateShopStatusResponseDto>.Success(new UpdateShopStatusResponseDto
            {
                ShopId = shop.ShopId,
                Status = shop.Status.ToString(),
                UpdatedAt = shop.UpdatedAt,
                Message = message
            }, message);
        }
        catch (Exception ex)
        {
            return ServiceResult<UpdateShopStatusResponseDto>.InternalServerError($"Error updating shop status: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ShopDto>>> GetAllShopsAsync()
    {
        try
        {
            var shops = await _unitOfWork.Shops.GetActiveShopsAsync();
            return ServiceResult<IEnumerable<ShopDto>>.Success(shops.ToDto());
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ShopDto>>.InternalServerError($"Error retrieving shops: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ShopDto>>> GetAllShopsAdminAsync()
    {
        try
        {
            var shops = await _unitOfWork.Shops.GetAllShopsAsync();
            return ServiceResult<IEnumerable<ShopDto>>.Success(shops.ToDto());
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ShopDto>>.InternalServerError($"Error retrieving shops: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopDto>> GetShopByIdAsync(Guid shopId)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<ShopDto>.NotFound("Shop not found");

            return ServiceResult<ShopDto>.Success(shop.ToDto());
        }
        catch (Exception ex)
        {
            return ServiceResult<ShopDto>.InternalServerError($"Error retrieving shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopDto>> GetShopByOwnerIdAsync(Guid ownerId)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByOwnerIdAsync(ownerId);
            if (shop == null)
                return ServiceResult<ShopDto>.NotFound("Shop not found for this owner");

            return ServiceResult<ShopDto>.Success(shop.ToDto());
        }
        catch (Exception ex)
        {
            return ServiceResult<ShopDto>.InternalServerError($"Error retrieving shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopDto>> CreateShopAsync(CreateShopDto dto)
    {
        try
        {
            var shop = dto.ToModel();
            await _unitOfWork.Shops.AddAsync(shop);
            await _unitOfWork.SaveChangesAsync();

            // 🔔 Publish event qua RabbitMQ để thông báo các service khác
            if (_rabbitPublisher != null)
            {
                var shopCreatedEvent = new ShopCreatedEvent
                {
                    ShopId = shop.ShopId,
                    ShopName = shop.Name,
                    OwnerId = shop.OwnerAccountId,
                    CreatedAt = shop.CreatedAt
                };

                _rabbitPublisher.PublishToQueue("shop.created", shopCreatedEvent);
            }

            return ServiceResult<ShopDto>.Created(shop.ToDto(), "Shop created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ShopDto>.InternalServerError($"Error creating shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopDto>> UpdateShopAsync(Guid shopId, UpdateShopDto dto)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<ShopDto>.NotFound("Shop not found");

            shop.MapToUpdate(dto);
            _unitOfWork.Shops.Update(shop);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<ShopDto>.Success(shop.ToDto(), "Shop updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ShopDto>.InternalServerError($"Error updating shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteShopAsync(Guid shopId)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult.NotFound("Shop not found");

            shop.Status = ShopStatus.INACTIVE;
            shop.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Shops.Update(shop);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success("Shop deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting shop: {ex.Message}");
        }
    }
}

