using R8.RedisHelper.Models;

namespace R8.RedisHelper.Interfaces
{
    public interface IRedisOperation
    {
        RedisCacheKey CacheKey { get; }
        string Command { get; }
        string[] Fields { get; }
        RedisValue[] Values { get; }
        string GetCommandText();
    }
}