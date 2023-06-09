namespace R8.RedisHelper.Interfaces
{
    public interface IRedisOperation
    {
        RedisKey CacheKey { get; }
        string Command { get; }
        string[] Fields { get; }
        RedisValue[] Values { get; }
        string GetCommandText();
    }
}