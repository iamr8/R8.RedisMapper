using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R8.RedisHelper.Utils.JsonConverters;

public class JsonRegionInfoToStringConverter : JsonConverter<RegionInfo>
{
    public override RegionInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var twoIso = reader.GetString();
        if (string.IsNullOrEmpty(twoIso))
            return null;

        return new RegionInfo(twoIso);
    }

    public override void Write(Utf8JsonWriter writer, RegionInfo value, JsonSerializerOptions options)
    {
        if (value == null)
            return;

        var culture = value.TwoLetterISORegionName;
        writer.WriteStringValue(culture);
    }
}