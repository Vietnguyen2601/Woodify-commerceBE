using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipmentService.APIService.Extensions;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] AcceptedFormats =
    {
        "yyyy-MM-dd",
        "dd-MM-yyyy",
        "MM-dd-yyyy",
        "yyyy/MM/dd",
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "o",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return DateTime.MinValue;

        string? dateString = reader.GetString();
        if (string.IsNullOrWhiteSpace(dateString)) return DateTime.MinValue;

        var selectedFormat = AcceptedFormats
            .FirstOrDefault(f => DateTime.TryParseExact(dateString, f, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _));

        if (selectedFormat != null)
        {
            DateTime.TryParseExact(dateString, selectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result);
            return new DateTime(result.Ticks, DateTimeKind.Utc);
        }

        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            return new DateTime(parsed.Ticks, DateTimeKind.Utc);

        throw new JsonException($"Unable to convert \"{dateString}\" to DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
}

public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly CustomDateTimeConverter _inner = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        return _inner.Read(ref reader, typeof(DateTime), options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue) _inner.Write(writer, value.Value, options);
        else writer.WriteNullValue();
    }
}
