using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductService.APIService.Converters;

public class NullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            
            // Nếu string rỗng hoặc whitespace, trả về null
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            // Parse GUID
            if (Guid.TryParse(stringValue, out var guid))
            {
                return guid;
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to Guid.");
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
