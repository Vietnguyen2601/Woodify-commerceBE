namespace Shared.Constants;

/// <summary>
/// Standardized shipping service codes and fee calculations
/// Đảm bảo consistency giữa OrderService và ShipmentService
/// </summary>
public static class ShippingServiceConstants
{
    // ─── SERVICE CODES ────────────────────────────────────────────────
    // Mã dịch vụ chuẩn (tương ứng với provider: GHN, Grab, v.v.)
    public const string SERVICE_EXPRESS = "EXPRESS";       // Service ID 1 - Super Express
    public const string SERVICE_FAST = "FAST";             // Service ID 2 - Fast
    public const string SERVICE_STANDARD = "STANDARD";     // Service ID 3 - Standard (Default)
    public const string SERVICE_ECONOMY = "ECONOMY";       // Service ID 4 - Economy

    // ─── BUCKET TYPES ────────────────────────────────────────────────
    // Phân loại kích thước đơn hàng
    public const string BUCKET_LETTER = "LETTER";          // Letter: 0-500g
    public const string BUCKET_STANDARD = "STANDARD";      // Standard: 500g-2kg
    public const string BUCKET_BULKY = "BULKY";           // Bulky: 2-5kg
    public const string BUCKET_OVERSIZED = "OVERSIZED";   // Oversized: >5kg

    // ─── WEIGHT THRESHOLDS (GRAMS) ───────────────────────────────────
    public const int WEIGHT_LETTER_MAX = 500;
    public const int WEIGHT_STANDARD_MAX = 2000;
    public const int WEIGHT_BULKY_MAX = 5000;
    public const int WEIGHT_SURCHARGE_UNIT = 500;          // Mỗi 500g tính 1 lần

    // ─── BASE FEES (VND) ──────────────────────────────────────────────
    public static long GetBaseFee(int serviceId, string bucketType = BUCKET_STANDARD)
    {
        return (serviceId, bucketType) switch
        {
            // EXPRESS: 35,000 - 60,000 VND
            (1, BUCKET_LETTER) => 20000,
            (1, BUCKET_STANDARD) => 35000,
            (1, BUCKET_BULKY) => 60000,
            (1, BUCKET_OVERSIZED) => 100000,

            // FAST: 28,000 - 50,000 VND
            (2, BUCKET_LETTER) => 18000,
            (2, BUCKET_STANDARD) => 28000,
            (2, BUCKET_BULKY) => 50000,
            (2, BUCKET_OVERSIZED) => 85000,

            // STANDARD: 20,000 - 35,000 VND
            (3, BUCKET_LETTER) => 12000,
            (3, BUCKET_STANDARD) => 20000,
            (3, BUCKET_BULKY) => 35000,
            (3, BUCKET_OVERSIZED) => 70000,

            // ECONOMY: 20,000 - 30,000 VND
            (4, BUCKET_LETTER) => 12000,
            (4, BUCKET_STANDARD) => 20000,
            (4, BUCKET_BULKY) => 30000,
            (4, BUCKET_OVERSIZED) => 60000,

            // Default: 25,000 VND
            _ => 25000
        };
    }

    // ─── WEIGHT SURCHARGE (VND per 500g block) ───────────────────────
    // Mỗi 500g thêm tính 1 phí bổ sung
    public const long WEIGHT_SURCHARGE_PER_UNIT = 2000;

    // ─── SERVICE ID CONVERSION ────────────────────────────────────────
    /// <summary>
    /// Convert service code (text) → service ID (int) cho calculator
    /// </summary>
    public static int GetServiceId(string? code)
    {
        return code?.ToUpperInvariant() switch
        {
            SERVICE_EXPRESS => 1,
            SERVICE_FAST => 2,
            SERVICE_STANDARD => 3,
            SERVICE_ECONOMY => 4,
            _ => 3  // Default: STANDARD
        };
    }

    /// <summary>
    /// Validate if service code is valid
    /// </summary>
    public static bool IsValidServiceCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var upperCode = code.ToUpperInvariant();
        return upperCode == SERVICE_EXPRESS ||
               upperCode == SERVICE_FAST ||
               upperCode == SERVICE_STANDARD ||
               upperCode == SERVICE_ECONOMY;
    }

    // ─── BUCKET TYPE DETERMINATION ────────────────────────────────────
    /// <summary>
    /// Xác định bucket type dựa trên cân nặng (grams)
    /// </summary>
    public static string GetBucketType(int weightGrams)
    {
        if (weightGrams <= WEIGHT_LETTER_MAX)
            return BUCKET_LETTER;
        else if (weightGrams <= WEIGHT_STANDARD_MAX)
            return BUCKET_STANDARD;
        else if (weightGrams <= WEIGHT_BULKY_MAX)
            return BUCKET_BULKY;
        else
            return BUCKET_OVERSIZED;
    }

    // ─── SHIPPING FEE CALCULATOR ─────────────────────────────────────
    /// <summary>
    /// Calculate total shipping fee
    /// Formula: BaseFee + (WeightGrams / 500) × WEIGHT_SURCHARGE_PER_UNIT
    /// </summary>
    public static long CalculateTotalFee(int serviceId, int weightGrams, string? bucketType = null)
    {
        // Determine bucket if not provided
        bucketType ??= GetBucketType(weightGrams);

        // Base fee
        long baseFee = GetBaseFee(serviceId, bucketType);

        // Weight surcharge: mỗi 500g thêm 2,000 VND
        long weightSurcharge = (weightGrams / WEIGHT_SURCHARGE_UNIT) * WEIGHT_SURCHARGE_PER_UNIT;

        return baseFee + weightSurcharge;
    }
}
