using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityService.APIService.Extensions;

/// <summary>
/// Custom DateTime converter that accepts multiple date formats
/// Supported formats: ISO 8601, MM-dd-yyyy, dd-MM-yyyy, yyyy-MM-dd
/// </summary>
public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] AcceptedFormats = new[]
    {
        "yyyy-MM-dd",           // ISO format: 2004-03-22
        "dd-MM-yyyy",           // European format: 22-03-2004
        "MM-dd-yyyy",           // US format: 03-22-2004
        "yyyy/MM/dd",           // Unix format: 2004/03/22
        "dd/MM/yyyy",           // European with slash: 22/03/2004
        "MM/dd/yyyy",           // US with slash: 03/22/2004
        "o",                    // ISO 8601 full: 2004-03-22T00:00:00.0000000
        "yyyy-MM-ddTHH:mm:ss",  // ISO with time: 2004-03-22T00:00:00
        "yyyy-MM-ddTHH:mm:ssZ", // ISO with time and Z: 2004-03-22T00:00:00Z
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return DateTime.MinValue;

        string? dateString = reader.GetString();
        if (string.IsNullOrWhiteSpace(dateString))
            return DateTime.MinValue;

        // Try parsing with each accepted format
        var selectedFormat = AcceptedFormats
            .Where(format => DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
            .FirstOrDefault();

        if (selectedFormat != null)
        {
            DateTime.TryParseExact(dateString, selectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result);
            // Ensure Kind is Utc for PostgreSQL compatibility
            return new DateTime(result.Ticks, DateTimeKind.Utc);
        }

        // Try using the default parser as fallback
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
        {
            return new DateTime(parsedDate.Ticks, DateTimeKind.Utc);
        }

        throw new JsonException($"Unable to convert \"{dateString}\" to DateTime. Accepted formats: {string.Join(", ", AcceptedFormats)}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Write as ISO 8601 format
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}

/// <summary>
/// Custom nullable DateTime converter
/// </summary>
public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly CustomDateTimeConverter _innerConverter = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return _innerConverter.Read(ref reader, typeof(DateTime), options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            _innerConverter.Write(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
