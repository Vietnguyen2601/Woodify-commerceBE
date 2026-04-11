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
    ///   SubtotalVnd = 1,000,000 (10k VND)
    ///   commissionRate = 0.06 (6%)
    ///   Result = FLOOR(1,000,000 × 0.06) = 60,000 VND
    /// </summary>
    /// <param name="subtotalVnd">Tổng tiền sản phẩm (VND)</param>
    /// <param name="commissionRate">Tỷ lệ hoa hồng (decimal: 0.06 = 6%)</param>
    /// <returns>Tiền hoa hồng đã tính (VND)</returns>
    public static long CalculateCommissionVnd(double subtotalVnd, decimal commissionRate)
    {
        if (subtotalVnd <= 0)
            return 0;

        if (commissionRate < 0 || commissionRate > 1)
            return 0;

        double commissionAmount = subtotalVnd * (double)commissionRate;
        return (long)Math.Floor(commissionAmount);
    }

    /// <summary>
    /// Tính tiền hoa hồng với default rate (6%)
    /// Convenience method khi không được cung cấp commission rate từ shop
    /// </summary>
    /// <param name="subtotalVnd">Tổng tiền sản phẩm (cents)</param>
    /// <returns>Tiền hoa hồng đã tính (VND)</returns>
    public static long CalculateCommissionVndWithDefault(double subtotalVnd)
    {
        return CalculateCommissionVnd(subtotalVnd, DEFAULT_COMMISSION_RATE);
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
    public static string GetCalculationBreakdown(double subtotalVnd, decimal commissionRate)
    {
        if (subtotalVnd <= 0) return $"Subtotal invalid: {subtotalVnd}";
        if (!IsValidCommissionRate(commissionRate)) return $"Commission rate invalid: {commissionRate}";

        double commissionAmount = subtotalVnd * (double)commissionRate;
        long commissionVnd = (long)Math.Floor(commissionAmount);

        return $"Subtotal: {subtotalVnd:N0}đ × Rate: {commissionRate:P} = {commissionAmount:N0}đ → " +
               $"FLOOR = {commissionVnd:N0}đ";
    }
}
