using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StackExchange.Redis;

namespace R8.RedisMapper.ConsoleSample;

public class R8JsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

public class Int32ValueSerializer : RedisValueSerializer<int>
{
    public override RedisValue Serialize(int value)
    {
        return value + 1;
    }

    public override int Deserialize(RedisValue value)
    {
        return int.Parse(value) - 1;
    }
}