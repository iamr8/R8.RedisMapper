using StackExchange.Redis;

namespace R8.RedisMapper.ConsoleSample;

public class DateTimeValueSerializer : RedisValueSerializer<DateTime>
{
    public override RedisValue Serialize(DateTime value)
    {
        throw new NotImplementedException();
    }

    public override DateTime Deserialize(RedisValue value)
    {
        throw new NotImplementedException();
    }
}