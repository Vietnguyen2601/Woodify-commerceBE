namespace PaymentService.Application.Helpers;

/// <summary>
/// Helper class for payment operations
/// </summary>
public static class PaymentHelper
{
    /// <summary>
    /// Tính tổng tiền từ danh sách orders
    /// </summary>
    /// <param name="orders">Danh sách Order DTOs từ OrderService (có TotalAmountCents đã bao gồm shipping fee)</param>
    /// <returns>Tổng tiền (cents)</returns>
    public static long CalculateTotalAmount(List<dynamic> orders)
    {
        if (orders == null || !orders.Any())
            return 0;

        long total = 0;
        foreach (var order in orders)
        {
            // OrderDto có double TotalAmountCents
            total += (long)order.TotalAmountCents;
        }
        return total;
    }

    /// <summary>
    /// Tạo unique order code cho PayOS từ list OrderIds
    /// Formula: TimeStamp + Random
    /// </summary>
    public static long GenerateOrderCode(List<Guid> orderIds)
    {
        if (orderIds == null || !orderIds.Any())
            throw new ArgumentException("OrderIds cannot be empty");

        // Tạo unique code từ timestamp + random
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);

        // Combine: timestamp (10 digits) + random (4 digits)
        var orderCode = long.Parse($"{timestamp % 10000000000}{random:D4}");

        return orderCode;
    }

    /// <summary>
    /// Validate list of order IDs
    /// </summary>
    public static bool ValidateOrderIds(List<Guid> orderIds)
    {
        return orderIds != null && orderIds.Count > 0;
    }

    /// <summary>
    /// Validate payment amount
    /// </summary>
    public static bool ValidateAmount(long amountCents)
    {
        return amountCents > 0;
    }
}
