using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using R8.RedisHelper.Utils.JsonConverters.Helpers;

namespace R8.RedisHelper.Utils.JsonConverters;

public class JsonIPAddressToStringConverter : JsonConverter<IPAddress>
{
    public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw ThrowHelper.GenerateJsonException_DeserializeUnableToConvertValue(typeof(IPAddress));

        Span<char> charData = stackalloc char[45];
        var count = Encoding.UTF8.GetChars(
            reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan,
            charData);
        return !IPAddress.TryParse(charData[..count], out IPAddress value)
            ? throw ThrowHelper.GenerateJsonException_DeserializeUnableToConvertValue(typeof(IPAddress))
            : value;
    }

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
    {
        var data = value.AddressFamily == AddressFamily.InterNetwork
            ? stackalloc char[15]
            : stackalloc char[45];
        if (!value.TryFormat(data, out var charsWritten))
            throw new JsonException($"IPAddress [{value}] could not be written to JSON.");

        var output = data[..charsWritten];
        writer.WriteStringValue(output);
    }
}