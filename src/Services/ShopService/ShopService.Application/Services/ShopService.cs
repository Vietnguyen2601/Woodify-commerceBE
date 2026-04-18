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
    private readonly IShopReadModelTotalsQuery _readModelTotals;

    public ShopService(
        IUnitOfWork unitOfWork,
        IShopReadModelTotalsQuery readModelTotals,
        RabbitMQPublisher? rabbitPublisher = null)
    {
        _unitOfWork = unitOfWork;
        _readModelTotals = readModelTotals;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task<ServiceResult<RegisterShopResponseDto>> RegisterShopAsync(RegisterShopDto dto)
    {
        try
        {
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
                Status = ShopStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Shops.AddAsync(shop);
            await _unitOfWork.SaveChangesAsync();

            // 🔔 Publish ShopCreated event
            if (_rabbitPublisher != null)
            {
                var shopCreatedEvent = new ShopCreatedEvent
                {
                    ShopId = shop.ShopId,
                    ShopName = shop.Name,
                    OwnerId = shop.OwnerAccountId,
                    DefaultPickupAddress = shop.DefaultPickupAddress,
                    DefaultProvider = shop.DefaultProvider,
                    DefaultProviderServiceCode = null,
                    CreatedAt = shop.CreatedAt
                };

                _rabbitPublisher.Publish("shop.events", "shop.created", shopCreatedEvent);
                Console.WriteLine($"[ShopService] Published ShopCreated event: ShopId={shop.ShopId}, ShopName={shop.Name}");
            }

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
        catch (ArgumentException ex)
        {
            return ServiceResult<RegisterShopResponseDto>.BadRequest(ex.Message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
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

            // 🔔 Publish ShopUpdated event
            if (_rabbitPublisher != null)
            {
                var shopUpdatedEvent = new ShopUpdatedEvent
                {
                    ShopId = shop.ShopId,
                    OwnerAccountId = shop.OwnerAccountId,
                    ShopName = shop.Name,
                    ShopPhone = null,
                    ShopEmail = null,
                    ShopAddress = null,
                    DefaultPickupAddress = shop.DefaultPickupAddress,
                    DefaultProvider = shop.DefaultProvider,
                    DefaultProviderServiceCode = null,
                    UpdatedAt = shop.UpdatedAt ?? DateTime.UtcNow
                };

                _rabbitPublisher.Publish("shop.events", "shop.updated", shopUpdatedEvent);
                Console.WriteLine($"[ShopService] Published ShopUpdated event: ShopId={shop.ShopId}, ShopName={shop.Name}");
            }

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
        catch (ArgumentException ex)
        {
            return ServiceResult<UpdateShopInfoResponseDto>.BadRequest(ex.Message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<UpdateShopStatusResponseDto>.InternalServerError($"Error updating shop status: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopBankAccountDto>> GetShopBankAccountAsync(Guid shopId)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<ShopBankAccountDto>.NotFound("Shop not found");

            return ServiceResult<ShopBankAccountDto>.Success(new ShopBankAccountDto
            {
                BankName = shop.BankName,
                BankAccountNumber = shop.BankAccountNumber,
                BankAccountName = shop.BankAccountName
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<ShopBankAccountDto>.InternalServerError($"Error retrieving bank account info: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopBankAccountDto>> UpdateShopBankAccountAsync(Guid shopId, UpdateShopBankAccountDto dto)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<ShopBankAccountDto>.NotFound("Shop not found");

            if (shop.Status is ShopStatus.SUSPENDED or ShopStatus.BANNED)
                return ServiceResult<ShopBankAccountDto>.BadRequest(
                    $"Cannot update bank account for shop with status '{shop.Status}'.");

            var bankName = dto.BankName?.Trim();
            var bankAccountNumber = dto.BankAccountNumber?.Trim();
            var bankAccountName = dto.BankAccountName?.Trim();

            if (string.IsNullOrWhiteSpace(bankName) ||
                string.IsNullOrWhiteSpace(bankAccountNumber) ||
                string.IsNullOrWhiteSpace(bankAccountName))
            {
                return ServiceResult<ShopBankAccountDto>.BadRequest(
                    "bankName, bankAccountNumber, bankAccountName are required.");
            }

            shop.BankName = bankName;
            shop.BankAccountNumber = bankAccountNumber;
            shop.BankAccountName = bankAccountName;
            shop.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Shops.Update(shop);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<ShopBankAccountDto>.Success(new ShopBankAccountDto
            {
                BankName = shop.BankName,
                BankAccountNumber = shop.BankAccountNumber,
                BankAccountName = shop.BankAccountName
            }, "Bank account info updated successfully");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<ShopBankAccountDto>.InternalServerError($"Error updating bank account info: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ShopPublicDto>>> GetAllShopsAsync()
    {
        try
        {
            var shops = (await _unitOfWork.Shops.GetActiveShopsAsync()).ToList();
            var totals = await _readModelTotals.GetTotalsByShopIdsAsync(shops.Select(s => s.ShopId).ToList());
            return ServiceResult<IEnumerable<ShopPublicDto>>.Success(shops.ToPublicDto(totals));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<IEnumerable<ShopPublicDto>>.InternalServerError($"Error retrieving shops: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ShopDto>>> GetAllShopsAdminAsync()
    {
        try
        {
            var shops = (await _unitOfWork.Shops.GetAllShopsAsync()).ToList();
            var totals = await _readModelTotals.GetTotalsByShopIdsAsync(shops.Select(s => s.ShopId).ToList());
            return ServiceResult<IEnumerable<ShopDto>>.Success(shops.ToDto(totals));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<IEnumerable<ShopDto>>.InternalServerError($"Error retrieving shops: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopPublicDto>> GetShopByIdAsync(Guid shopId)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId);
            if (shop == null)
                return ServiceResult<ShopPublicDto>.NotFound("Shop not found");

            var totals = await _readModelTotals.GetTotalsByShopIdsAsync(new List<Guid> { shopId });
            totals.TryGetValue(shopId, out var t);
            return ServiceResult<ShopPublicDto>.Success(shop.ToPublicDto(t.TotalProducts, t.TotalOrders));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<ShopPublicDto>.InternalServerError($"Error retrieving shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopDetailDto>> GetShopByOwnerIdAsync(Guid ownerId)
    {
        try
        {
            var shop = await _unitOfWork.Shops.GetByOwnerIdAsync(ownerId);
            if (shop == null)
                return ServiceResult<ShopDetailDto>.NotFound("Shop not found for this owner");

            var totals = await _readModelTotals.GetTotalsByShopIdsAsync(new List<Guid> { shop.ShopId });
            totals.TryGetValue(shop.ShopId, out var t);
            return ServiceResult<ShopDetailDto>.Success(shop.ToDetailDto(t.TotalProducts, t.TotalOrders));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult<ShopDetailDto>.InternalServerError($"Error retrieving shop: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShopDto>> CreateShopAsync(CreateShopDto dto)
    {
        try
        {
            var existingShop = await _unitOfWork.Shops.GetByOwnerIdAsync(dto.OwnerAccountId);
            if (existingShop != null)
                return ServiceResult<ShopDto>.BadRequest("This account already has a shop.");

            var shop = dto.ToModel();
            shop.Status = ShopStatus.ACTIVE;
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
                    DefaultPickupAddress = shop.DefaultPickupAddress,
                    DefaultProvider = shop.DefaultProvider,
                    DefaultProviderServiceCode = null,
                    CreatedAt = shop.CreatedAt
                };

                _rabbitPublisher.Publish("shop.events", "shop.created", shopCreatedEvent);
                Console.WriteLine($"[ShopService] Published ShopCreated event: ShopId={shop.ShopId}, ShopName={shop.Name}");
            }

            var totals = await _readModelTotals.GetTotalsByShopIdsAsync(new List<Guid> { shop.ShopId });
            totals.TryGetValue(shop.ShopId, out var t);
            return ServiceResult<ShopDto>.Created(shop.ToDto(t.TotalProducts, t.TotalOrders), "Shop created successfully");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
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

            // 🔔 Publish ShopUpdated event
            if (_rabbitPublisher != null)
            {
                var shopUpdatedEvent = new ShopUpdatedEvent
                {
                    ShopId = shop.ShopId,
                    OwnerAccountId = shop.OwnerAccountId,
                    ShopName = shop.Name,
                    ShopPhone = null,
                    ShopEmail = null,
                    ShopAddress = null,
                    DefaultPickupAddress = shop.DefaultPickupAddress,
                    DefaultProvider = shop.DefaultProvider,
                    DefaultProviderServiceCode = null,
                    UpdatedAt = shop.UpdatedAt ?? DateTime.UtcNow
                };

                _rabbitPublisher.Publish("shop.events", "shop.updated", shopUpdatedEvent);
                Console.WriteLine($"[ShopService] Published ShopUpdated event: ShopId={shop.ShopId}, ShopName={shop.Name}");
            }

            var totals = await _readModelTotals.GetTotalsByShopIdsAsync(new List<Guid> { shop.ShopId });
            totals.TryGetValue(shop.ShopId, out var t);
            return ServiceResult<ShopDto>.Success(shop.ToDto(t.TotalProducts, t.TotalOrders), "Shop updated successfully");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
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

            if (_rabbitPublisher != null)
            {
                var deleted = new ShopDeletedEvent
                {
                    ShopId = shopId,
                    DeletedAt = DateTime.UtcNow
                };
                _rabbitPublisher.Publish("shop.events", "shop.deleted", deleted);
                Console.WriteLine($"[ShopService] Published ShopDeleted event: ShopId={shopId}");
            }

            return ServiceResult.Success("Shop deleted successfully");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            return ServiceResult.InternalServerError($"Error deleting shop: {ex.Message}");
        }
    }
}

