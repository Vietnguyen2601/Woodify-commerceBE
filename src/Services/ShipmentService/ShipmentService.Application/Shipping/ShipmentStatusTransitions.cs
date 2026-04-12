namespace ShipmentService.Application.Shipping;

/// <summary>
/// Validates shipment lifecycle transitions (happy path + returns / failures).
/// Status values are uppercase and match DB check constraint.
/// </summary>
public static class ShipmentStatusTransitions
{
    /// <summary>Statuses that cannot change except idempotent (same value).</summary>
    private static readonly HashSet<string> StrictTerminal = new(StringComparer.OrdinalIgnoreCase)
    {
        "DELIVERED", "RETURNED", "CANCELLED"
    };

    /// <summary>Allowed targets from each non-terminal status.</summary>
    private static readonly Dictionary<string, HashSet<string>> Transitions = BuildTransitions();

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

        if (string.Equals(current, "DELIVERY_FAILED", StringComparison.OrdinalIgnoreCase))
        {
            if (next is "RETURNING" or "CANCELLED")
            {
                error = null;
                return true;
            }

            error = "From DELIVERY_FAILED only RETURNING or CANCELLED is allowed.";
            return false;
        }

        if (!Transitions.TryGetValue(current, out var allowed) || !allowed.Contains(next))
        {
            error = $"Invalid status transition '{current}' -> '{next}'. See shipment flow documentation for allowed next states.";
            return false;
        }

        error = null;
        return true;
    }

    private static Dictionary<string, HashSet<string>> BuildTransitions()
    {
        var d = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        void Add(string from, params string[] to)
        {
            if (!d.TryGetValue(from, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                d[from] = set;
            }

            foreach (var t in to)
                set.Add(t);
        }

        Add("DRAFT", "PENDING", "CANCELLED");
        Add("PENDING", "PICKUP_SCHEDULED", "PICKED_UP", "CANCELLED");
        Add("PICKUP_SCHEDULED", "PENDING", "PICKED_UP", "CANCELLED");
        Add("PICKED_UP", "IN_TRANSIT", "OUT_FOR_DELIVERY", "DELIVERED", "DELIVERY_FAILED", "CANCELLED");
        Add("IN_TRANSIT", "OUT_FOR_DELIVERY", "DELIVERED", "DELIVERY_FAILED", "RETURNING");
        Add("OUT_FOR_DELIVERY", "DELIVERED", "DELIVERY_FAILED", "RETURNING");
        Add("RETURNING", "RETURNED", "DELIVERY_FAILED");

        return d;
    }
}
