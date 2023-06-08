using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R8.RedisHelper.Utils.JsonConverters;

public class JsonCultureToStringConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var twoIso = reader.GetString();
        if (string.IsNullOrEmpty(twoIso))
            return null;

        return new CultureInfo(twoIso);
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
    {
        if (value == null)
            return;

        var culture = value.Name;
        writer.WriteStringValue(culture);
    }
}