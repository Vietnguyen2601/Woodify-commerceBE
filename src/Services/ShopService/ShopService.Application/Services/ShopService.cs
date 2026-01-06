using Shared.Events;
using Shared.Messaging;
using Shared.Results;
using ShopService.Application.DTOs;
using ShopService.Application.Mappers;
using ShopService.Infrastructure.UnitOfWork;
using ShopService.Application.Interfaces;

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
                Console.WriteLine($"[RabbitMQ] Published shop.created event for ShopId: {shop.ShopId}");
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

            var updatedShop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            return ServiceResult<ShopDto>.Success(updatedShop!.ToDto(), "Shop updated successfully");
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

            shop.Status = global::ShopService.Domain.Enums.ShopStatus.INACTIVE;
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

