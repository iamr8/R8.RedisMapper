using R8.RedisHelper.Models;

namespace R8.RedisHelper
{
    public interface IRedisRead
    {
        /// <summary>
        /// Reads a hash from Redis and maps it to a model using HMGET. If number of fields is 1, HGET is used instead.
        /// <para>https://redis.io/commands/hmget/<br />https://redis.io/commands/hget/</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        void Get(RedisKey redisKey, params string[] fields);
    }
}