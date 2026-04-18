namespace ShipmentService.Application.Shipping;

/// <summary>
/// Shipment status helpers. Transitions are permissive (any valid DB status) except leaving strict terminals.
/// Status values are uppercase and match DB check constraint CHK_Shipments_status.
/// </summary>
public static class ShipmentStatusTransitions
{
    /// <summary>Statuses that cannot change except idempotent (same value).</summary>
    private static readonly HashSet<string> StrictTerminal = new(StringComparer.OrdinalIgnoreCase)
    {
        "DELIVERED", "RETURNED", "CANCELLED"
    };

    /// <summary>Must match <c>CHK_Shipments_status</c> on Shipments.</summary>
    private static readonly HashSet<string> KnownStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "DRAFT", "PENDING", "PICKUP_SCHEDULED", "PICKED_UP", "IN_TRANSIT", "OUT_FOR_DELIVERY",
        "DELIVERED", "DELIVERY_FAILED", "RETURNING", "RETURNED", "CANCELLED"
    };

    /// <summary>Shipments in these states may be deleted (not in active pipeline).</summary>
    public static readonly HashSet<string> DeletableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "DRAFT", "CANCELLED", "RETURNED", "DELIVERY_FAILED"
    };

    public static string Normalize(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return string.Empty;
        return status.Trim().ToUpperInvariant();
    }

    public static bool IsStrictTerminal(string status) => StrictTerminal.Contains(Normalize(status));

    public static bool TryValidateTransition(string? currentRaw, string nextRaw, out string? error)
    {
        var current = Normalize(currentRaw);
        var next = Normalize(nextRaw);

        if (string.IsNullOrEmpty(next))
        {
            error = "Status is required.";
            return false;
        }

        if (!KnownStatuses.Contains(next))
        {
            error = $"Unknown shipment status '{next}'. Allowed values: {string.Join(", ", KnownStatuses.OrderBy(s => s))}.";
            return false;
        }

        if (current == next)
        {
            error = null;
            return true;
        }

        if (StrictTerminal.Contains(current))
        {
            error = $"Cannot change status from terminal state '{current}'.";
            return false;
        }

        error = null;
        return true;
    }
}
