using System;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    public interface IRedisValueSerializer
    {
        Type Type { get; }
        T Deserialize<T>(RedisValue value);
        RedisValue Serialize<T>(T value);
    }

    public abstract class RedisValueSerializer<TType> : IRedisValueSerializer
    {
        Type IRedisValueSerializer.Type => typeof(TType);
        public abstract RedisValue Serialize(TType value);
        public abstract TType Deserialize(RedisValue value);
        
        RedisValue IRedisValueSerializer.Serialize<T>(T value)
        {
            if (value is TType type)
                return Serialize(type);
            throw new InvalidCastException();
        }

        T IRedisValueSerializer.Deserialize<T>(RedisValue value)
        {
            if (typeof(T) == typeof(TType))
                return (T) (object) Deserialize(value);
            throw new InvalidCastException();
        }
        
        public static IRedisValueSerializer Instance { get; }
    }
}