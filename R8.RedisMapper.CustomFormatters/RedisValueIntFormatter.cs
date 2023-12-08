using StackExchange.Redis;

namespace R8.RedisMapper.CustomFormatters
{
    public class RedisValueIntFormatter : IRedisValueFormatter
    {
        public Type Type => typeof(int);

        public RedisValue Write(object value, RedisValueWriterContext context)
        {
            throw new NotImplementedException();
        }

        public object? Read(RedisValue value, RedisValueReaderContext context)
        {
            throw new NotImplementedException();
        }
    }
}