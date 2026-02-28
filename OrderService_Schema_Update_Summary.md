# OrderService Schema Update Summary

## Date: February 28, 2026

This document summarizes the comprehensive updates made to the OrderService to align with the new database schema specifications.

## Schema Changes Overview

### 1. **Cart Entity**
**Changes:**
- ✅ Removed `UpdatedAt` field
- ✅ Kept: `CartId`, `AccountId`, `CreatedAt`
- ✅ Navigation property to CartItems maintained

### 2. **CartItem Entity**
**Changes:**
- ✅ Simplified to match new schema
- ✅ Changed `UnitPriceCents` (long) → `Price` (double)
- ✅ Removed fields: `CompareAtPriceCents`, `IsSelected`, `CustomizationNote`, `IsActive`, `AddedAt`, `UpdatedAt`
- ✅ Kept: `CartItemId`, `CartId`, `VersionId`, `ShopId`, `Quantity`, `Price`

### 3. **Order Entity**
**Changes:**
- ✅ Significantly simplified schema
- ✅ Made `AccountId` and `ShopId` required (non-nullable)
- ✅ Changed `SubtotalCents` and `TotalAmountCents` from `long` → `double`
- ✅ Renamed `VoucherApplied` → `VoucherId`
- ✅ Added `Payment` (Guid?) to reference payment entity
- ✅ Changed `DeliveryAddressId` from `Guid?` → `string?`
- ✅ Removed fields:
  - `OrderCode`
  - `CustomerName`, `CustomerPhone`, `CustomerEmail`
  - `ShopName`, `Currency`
  - `ShippingFeeCents`, `DiscountCents`, `TaxCents`
  - `PaymentMethod` enum, `PaymentStatus`, `PaymentTransactionId`
  - `CustomerNote`, `ShopNote`
  - `PlacedAt`, `ConfirmedAt`, `ShippedAt`, `DeliveredAt`, `CompletedAt`, `CancelledAt`
  - `CancelReason`, `CancelledBy`
- ✅ Kept: `OrderId`, `AccountId`, `ShopId`, `SubtotalCents`, `TotalAmountCents`, `VoucherId`, `Payment`, `Status`, `DeliveryAddressId`, `CreatedAt`, `UpdatedAt`
- ✅ Removed `PaymentMethod` and `PaymentStatus` enums from entity file

### 4. **OrderItem Entity**
**Changes:**
- ✅ Simplified schema
- ✅ Changed `TaxCents` from `long` → `double`
- ✅ Changed `LineTotalCents` from `long` → `double`
- ✅ Removed fields: `ProductName`, `VariantName`, `SellerSku`, `ReturnedQuantity`, `RefundedAmountCents`, `ShippingInfo`
- ✅ Kept: `OrderItemId`, `OrderId`, `VersionId`, `UnitPriceCents` (bigint), `Quantity`, `DiscountCents`, `TaxCents`, `ShipmentId`, `LineTotalCents`, `Status`, `CreatedAt`

### 5. **ProductVersionCache Entity**
**Changes:**
- ✅ Removed `PriceCents` and `BasePriceCents` fields
- ✅ Using `Price` (double) directly for pricing

## Updated Components

### Infrastructure Layer

#### ✅ **OrderDbContext.cs**
- Updated all entity configurations to match new schema
- Removed UpdatedAt configuration from Cart
- Simplified CartItem configuration (Price as double precision)
- Simplified Order configuration (removed all obsolete fields)
- Updated OrderItem configuration (TaxCents and LineTotalCents as double precision)

#### ✅ **IOrderRepository.cs & OrderRepository.cs**
- Removed `GetOrderByCodeAsync()` method
- Removed `GenerateOrderCodeAsync()` method
- Updated `GetOrdersByAccountIdAsync()` to use `CreatedAt` instead of `PlacedAt`

#### ✅ **OrderDbSeeder.cs**
- Updated Cart seeding (removed UpdatedAt)
- Updated CartItem seeding (using Price instead of UnitPriceCents)
- Updated Order seeding (simplified to match new schema)
- Updated OrderItem seeding (removed obsolete fields)
- ProductVersionCache seeding maintained (already correct)

### Application Layer

#### ✅ **DTOs**

**CartDtos.cs:**
- `AddToCartDto`: Removed `CustomizationNote`
- `UpdateCartItemDto`: Removed `IsSelected`, `CustomizationNote`
- `CartDto`: Removed `UpdatedAt`, changed `TotalPriceCents` → `TotalPrice` (double)
- `CartItemDto`: Simplified to match new schema, using `Price` (double)
- `CheckoutPreviewDto`: Changed `SubtotalCents` → `Subtotal` (double)
- `CheckoutItemDto`: Using `Price` (double)

**OrderDtos.cs:**
- `CreateOrderFromCartDto`: Simplified to include only `AccountId`, `ShopId`, `DeliveryAddressId`, `VoucherId`, `Payment`
- `OrderDto`: Simplified to match new Order entity schema
- `OrderItemDto`: Removed `ProductName`, `VariantName`, `SellerSku`, `ReturnedQuantity`, `RefundedAmountCents`, `ShippingInfo`

#### ✅ **Mappers**

**CartMapper.cs:**
- Updated to work with simplified CartItem schema
- Changed from `TotalPriceCents` (long) to `TotalPrice` (double)
- Removed mappings for obsolete fields

#### ✅ **Services**

**CartService.cs:**
- Updated to work with `Price` (double) instead of `UnitPriceCents`
- Removed all references to UpdatedAt, IsSelected, CustomizationNote, IsActive, AddedAt
- Updated validation logic to use `productCache.Price` instead of `productCache.PriceCents`
- Updated calculations to use double values

**OrderService.cs:**
- Completely restructured to work with simplified Order schema
- Removed order code generation logic
- Simplified order creation process
- Updated MapToOrderDto to match new schema
- Updated validation logic to use `Price` (double)
- Removed references to all obsolete Order fields

**IOrderService.cs:**
- Removed `GetOrderByCodeAsync()` method

#### ✅ **Consumers**

**ProductEventConsumer.cs:**
- Removed `PriceCents` and `BasePriceCents` initialization
- Using only `Price` (double) from product events

### API Layer

#### ✅ **Controllers**

**OrdersController.cs:**
- Removed `GetOrderByCode` endpoint
- Kept: `CreateOrderFromCart`, `GetOrder`, `GetOrdersByAccount`

**CartsController.cs:**
- No changes needed (already compatible with new schema)

## Database Migration

✅ **Migration Created**: `SimplifyOrderSchema`
- Location: `OrderService.Infrastructure/Migrations/`
- This migration will:
  - Remove UpdatedAt column from carts table
  - Change cart_items.unit_price_cents to cart_items.price (double precision)
  - Remove multiple columns from cart_items: compare_at_price_cents, is_selected, customization_note, is_active, added_at, updated_at
  - Simplify orders table (remove order_code, customer_name, customer_phone, customer_email, shop_name, currency, shipping_fee_cents, discount_cents, tax_cents, payment_method, payment_status, payment_transaction_id, customer_note, shop_note, placed_at, confirmed_at, shipped_at, delivered_at, completed_at, cancelled_at, cancel_reason, cancelled_by)
  - Change orders.subtotal_cents and total_amount_cents to double precision
  - Rename voucher_applied to voucher_id
  - Add payment column
  - Change delivery_address_id to nvarchar
  - Remove columns from order_items: product_name, variant_name, seller_sku, returned_quantity, refunded_amount_cents, shipping_info
  - Change order_items.tax_cents and line_total_cents to double precision
  - Remove price_cents and base_price_cents from product_version_cache

⚠️ **Note**: This migration includes data loss operations. Review carefully before applying to production.

## CI/CD Compliance

All code follows CI/CD best practices:

✅ **Clean Architecture**: Proper separation of concerns across Domain, Infrastructure, Application, and API layers
✅ **Consistent Naming**: All entities, DTOs, services follow naming conventions
✅ **Type Safety**: Strong typing throughout, using appropriate data types
✅ **Nullable Reference Types**: Proper use of nullable annotations
✅ **Repository Pattern**: Maintained abstraction for data access
✅ **Dependency Injection**: All dependencies properly registered
✅ **Error Handling**: Consistent use of ServiceResult pattern
✅ **Documentation**: XML comments maintained for public APIs
✅ **Build Success**: ✅ No compilation errors

## Testing Recommendations

Before deploying to production:

1. ✅ **Build Verification**: Completed - all projects build successfully
2. ⚠️ **Unit Tests**: Review and update unit tests for modified services
3. ⚠️ **Integration Tests**: Test cart and order workflows end-to-end
4. ⚠️ **Migration Testing**: Test migration on copy of production database
5. ⚠️ **API Testing**: Verify all API endpoints work with new schema
6. ⚠️ **Seeder Testing**: Verify seeder creates valid data

## Migration Instructions

To apply the migration:

```bash
# Navigate to Infrastructure project
cd src/Services/OrderService/OrderService.Infrastructure

# Apply migration to database
dotnet ef database update --startup-project ../OrderService.APIService/OrderService.APIService.csproj --context OrderDbContext
```

Or use the migration commands in your CI/CD pipeline.

## Rollback Plan

If issues arise after deployment:

1. Revert the migration using:
   ```bash
   dotnet ef database update [PreviousMigrationName] --startup-project ../OrderService.APIService/OrderService.APIService.csproj
   ```

2. Deploy the previous version of the service

3. Investigate issues and reapply after fixes

## Summary

✅ All entities updated to match new schema
✅ All DTOs updated and simplified
✅ All services refactored for new schema
✅ All controllers updated
✅ All mappers updated
✅ Seeder updated with new data format
✅ Repositories cleaned up
✅ Migration created and ready to apply
✅ Code is CI/CD ready with no build errors

The OrderService is now fully aligned with your new table specifications and ready for deployment.
