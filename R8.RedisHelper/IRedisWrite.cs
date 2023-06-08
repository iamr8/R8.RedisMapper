using R8.RedisHelper.Models;

namespace R8.RedisHelper
{
    public interface IRedisWrite
    {
        /// <summary>
        /// Executes the command using HSET*. If the value is null, it will be deleted using HDEL.
        /// <para>
        ///       if <see cref="When"/> is set to <see cref="When.NotExists"/>, it will be executed using HSETNX.<br/>
        ///       If is set to <see cref="When.Exists"/>, it will be executed using HSETXX.<br/>
        ///       Otherwise, it will be executed using HSET.
        /// </para>
        /// <para>https://redis.io/commands/hset/<br />https://redis.io/commands/hsetnx/</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        void Set<TValue>(RedisCacheKey cacheKey, string field, TValue value, When when = When.Always);

        /// <summary>
        /// <inheritdoc cref="Set"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        void Set<T>(RedisCacheKey cacheKey, T model) where T : class, new();

        /// <summary>
        /// Execute the command using HMSET. If the number of fields is 1, it will be executed using HSET.
        /// <para>https://redis.io/commands/hmset/<br />https://redis.io/commands/hset/</para>
        /// </summary>
        void Set(RedisCacheKey cacheKey, object values);

        /// <summary>
        /// Executes the command using DEL.
        /// <para>https://redis.io/commands/del/</para>
        /// </summary>
        void Delete(RedisCacheKey cacheKey);

        /// <summary>
        /// Executes the command using HDEL.
        /// <para>https://redis.io/commands/hdel/</para>
        /// </summary>
        void Delete(RedisCacheKey cacheKey, string field);

        /// <summary>
        /// Executes the command using HINCRBY.
        /// <para>https://redis.io/commands/hincrby/</para>
        /// </summary>
        void Increment(RedisCacheKey cacheKey, string field, long value = 1L);

        /// <summary>
        /// Executes the command using INCR.
        /// <para>https://redis.io/commands/incr/</para>
        /// </summary>
        void Increment(RedisCacheKey cacheKey, long value = 1L);

        /// <summary>
        /// Executes the command using EXPIRE.
        /// <para>https://redis.io/commands/expire/</para>
        /// </summary>
        void Expire(RedisCacheKey cacheKey, TimeSpan time);
    }
}