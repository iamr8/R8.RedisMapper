using System;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    public interface IRedisValueFormatter
    {
        Type Type { get; }
        RedisValue Write(object value, RedisValueWriterContext context);
        object? Read(RedisValue value, RedisValueReaderContext context);
    }
}