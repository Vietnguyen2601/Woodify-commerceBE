namespace OrderService.Application.Helpers;

/// <summary>
/// Helper để tính tiền hoa hồng từ subtotal và commission rate
/// Tính toán được sử dụng khi tạo Order
/// </summary>
public static class CommissionCalculator
{
    /// <summary>
    /// Default commission rate nếu không được chỉ định: 6%
    /// </summary>
    public const decimal DEFAULT_COMMISSION_RATE = 0.06m;

    /// <summary>
    /// Tính tiền hoa hồng từ subtotal và commission rate
    /// Sử dụng FLOOR (làm tròn xuống) để tính toán chính xác
    /// 
    /// Example:
    ///   subtotalCents = 1,000,000 (10k VND)
    ///   commissionRate = 0.06 (6%)
    ///   Result = FLOOR(1,000,000 × 0.06) = FLOOR(60,000) = 60,000 cents
    /// </summary>
    /// <param name="subtotalCents">Tổng tiền sản phẩm (đơn vị: cents)</param>
    /// <param name="commissionRate">Tỷ lệ hoa hồng (decimal: 0.06 = 6%)</param>
    /// <returns>Tiền hoa hồng đã tính (cents)</returns>
    public static long CalculateCommissionCents(double subtotalCents, decimal commissionRate)
    {
        // Step 1: Validate inputs
        if (subtotalCents <= 0)
            return 0;

        // Step 2: Validate commission rate (0% to 100%)
        if (commissionRate < 0 || commissionRate > 1)
            return 0;

        // Step 3: Calculate commission
        // Convert to double for multiplication, then back to long
        double commissionAmount = subtotalCents * (double)commissionRate;

        // Step 4: Floor (làm tròn xuống) để đảm bảo không overcharge
        long commissionCents = (long)Math.Floor(commissionAmount);

        return commissionCents;
    }

    /// <summary>
    /// Tính tiền hoa hồng với default rate (6%)
    /// Convenience method khi không được cung cấp commission rate từ shop
    /// </summary>
    /// <param name="subtotalCents">Tổng tiền sản phẩm (cents)</param>
    /// <returns>Tiền hoa hồng đã tính (cents)</returns>
    public static long CalculateCommissionCentsWithDefault(double subtotalCents)
    {
        return CalculateCommissionCents(subtotalCents, DEFAULT_COMMISSION_RATE);
    }

    /// <summary>
    /// Validate commission rate có hợp lệ không
    /// Valid range: 0% to 100% (0.0 to 1.0)
    /// </summary>
    /// <param name="rate">Commission rate</param>
    /// <returns>True nếu hợp lệ, false nếu không</returns>
    public static bool IsValidCommissionRate(decimal rate)
    {
        return rate >= 0 && rate <= 1;
    }

    /// <summary>
    /// Trace calculation (for logging/debugging)
    /// Returns breakdown of commission calculation
    /// </summary>
    public static string GetCalculationBreakdown(double subtotalCents, decimal commissionRate)
    {
        if (subtotalCents <= 0) return $"Subtotal invalid: {subtotalCents}";
        if (!IsValidCommissionRate(commissionRate)) return $"Commission rate invalid: {commissionRate}";

        double commissionAmount = subtotalCents * (double)commissionRate;
        long commissionCents = (long)Math.Floor(commissionAmount);

        return $"Subtotal: {subtotalCents:N0}đ × Rate: {commissionRate:P} = {commissionAmount:N0}đ → " +
               $"FLOOR = {commissionCents:N0}đ";
    }
}
