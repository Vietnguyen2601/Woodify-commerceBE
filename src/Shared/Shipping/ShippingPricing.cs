using Shared.Constants;

namespace Shared.Shipping;

/// <summary>
/// Single shipping fee model for OrderService, ShipmentService, and checkout preview.
/// <para><b>Logic (VND integer amounts; names may say "Cents" elsewhere in the solution).</b></para>
/// <list type="number">
/// <item><b>Weight bucket</b> — from total grams: LETTER ≤500g, STANDARD ≤2kg, BULKY ≤5kg, OVERSIZED &gt;5kg.</item>
/// <item><b>Base fee</b> — table by (service tier × bucket). Tier: EXP=1, FAST=2, STD=3, ECO=4.</item>
/// <item><b>Weight line</b> — floor(weight/500) × 2_000 VND (extra handling per 500g).</item>
/// <item><b>Raw fee</b> — base + weight line, then min with <see cref="AbsoluteMaxShippingFeeVnd"/>.</item>
/// <item><b>Free shipping</b> — if order <b>subtotal</b> (goods only) ≥ <see cref="FreeShippingSubtotalThresholdVnd"/>, fee = 0.</item>
/// <item><b>Order total</b> — subtotal + final shipping (same rule used when creating an order).</item>
/// </list>
/// No second “bulky %” layer and no DB multiplier here — avoids fees stacking too high and matches payment total.
/// </summary>
public static class ShippingPricing
{
    /// <summary>Goods subtotal (VND) at or above this → shipping fee0.</summary>
    public const long FreeShippingSubtotalThresholdVnd = 500_000L;

    /// <summary>Hard cap so very heavy carts do not produce extreme shipping.</summary>
    public const long AbsoluteMaxShippingFeeVnd = 399_000L;

    public static readonly string[] CheckoutTierCodes =
    {
        ShippingServiceConstants.CODE_ECO,
        ShippingServiceConstants.CODE_STD,
        ShippingServiceConstants.CODE_EXP
    };

    public static string DisplayLabelForTier(string code) =>
        ShippingServiceConstants.CanonicalizeProviderServiceCode(code) switch
        {
            var c when c == ShippingServiceConstants.CODE_ECO => "Economy (ECO)",
            var c when c == ShippingServiceConstants.CODE_STD => "Standard (STD)",
            var c when c == ShippingServiceConstants.CODE_EXP => "Express (EXP)",
            _ => code
        };

    /// <summary>Shipping fee before free-ship rule.</summary>
    public static long ComputeShippingFeeVnd(string? providerServiceCode, int weightGrams)
    {
        int serviceId = ShippingServiceConstants.GetServiceId(providerServiceCode);
        long raw = ShippingServiceConstants.CalculateTotalFee(serviceId, Math.Max(0, weightGrams));
        return Math.Min(raw, AbsoluteMaxShippingFeeVnd);
    }

    /// <summary>Shipping after free-ship on goods subtotal.</summary>
    public static long FinalShippingFeeVnd(string? providerServiceCode, int weightGrams, double orderSubtotalVnd)
    {
        if (orderSubtotalVnd >= FreeShippingSubtotalThresholdVnd)
            return 0L;
        return ComputeShippingFeeVnd(providerServiceCode, weightGrams);
    }

    public static double GrandTotalVnd(double orderSubtotalVnd, long finalShippingFeeVnd) =>
        orderSubtotalVnd + finalShippingFeeVnd;

    /// <summary>ECO / STD / EXP quotes for checkout UI (same math as create order).</summary>
    public static IReadOnlyList<CheckoutShippingTierQuote> QuoteAllCheckoutTiers(int weightGrams, double orderSubtotalVnd)
    {
        var list = new List<CheckoutShippingTierQuote>(CheckoutTierCodes.Length);
        foreach (var code in CheckoutTierCodes)
        {
            var canon = ShippingServiceConstants.CanonicalizeProviderServiceCode(code);
            long fee = FinalShippingFeeVnd(canon, weightGrams, orderSubtotalVnd);
            bool free = orderSubtotalVnd >= FreeShippingSubtotalThresholdVnd;
            list.Add(new CheckoutShippingTierQuote(
                canon,
                DisplayLabelForTier(canon),
                GrandTotalVnd(orderSubtotalVnd, fee),
                free));
        }

        return list;
    }
}

/// <summary>One checkout tier row. <see cref="TotalAmountVnd"/> = subtotal + shipping (order payable total).</summary>
public sealed record CheckoutShippingTierQuote(
    string ProviderServiceCode,
    string DisplayLabel,
    double TotalAmountVnd,
    bool IsFreeShipping);
