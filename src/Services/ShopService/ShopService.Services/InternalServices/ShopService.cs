using Shared.Events;
using Shared.Messaging;
using ShopService.Common.DTOs;
using ShopService.Repositories.Mapper;
using ShopService.Repositories.UnitOfWork;
using ShopService.Services.Interfaces;

namespace ShopService.Services.InternalServices;

public class ShopService : IShopService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RabbitMQPublisher? _rabbitPublisher;

    public ShopService(IUnitOfWork unitOfWork, RabbitMQPublisher? rabbitPublisher = null)
    {
        _unitOfWork = unitOfWork;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task<IEnumerable<ShopDto>> GetAllShopsAsync()
    {
        var shops = await _unitOfWork.Shops.GetActiveShopsAsync();
        return shops.ToDto();
    }

    public async Task<ShopDto?> GetShopByIdAsync(Guid shopId)
    {
        var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
        return shop?.ToDto();
    }

    public async Task<ShopDto?> GetShopByOwnerIdAsync(Guid ownerId)
    {
        var shop = await _unitOfWork.Shops.GetByOwnerIdAsync(ownerId);
        return shop?.ToDto();
    }

    public async Task<ShopDto> CreateShopAsync(CreateShopDto dto)
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
                ShopName = shop.ShopName,
                OwnerId = shop.OwnerId,
                CreatedAt = shop.CreatedAt
            };
            
            _rabbitPublisher.PublishToQueue("shop.created", shopCreatedEvent);
            Console.WriteLine($"[RabbitMQ] Published shop.created event for ShopId: {shop.ShopId}");
        }

        return shop.ToDto();
    }

    public async Task<ShopDto?> UpdateShopAsync(Guid shopId, UpdateShopDto dto)
    {
        var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
        if (shop == null) return null;

        shop.MapToUpdate(dto);
        _unitOfWork.Shops.Update(shop);
        await _unitOfWork.SaveChangesAsync();
        return shop.ToDto();
    }

    public async Task<bool> DeleteShopAsync(Guid shopId)
    {
        var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
        if (shop == null) return false;

        shop.IsActive = false;
        shop.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Shops.Update(shop);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
