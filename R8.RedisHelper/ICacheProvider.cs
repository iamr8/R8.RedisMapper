using R8.RedisHelper.Models;

namespace R8.RedisHelper
{
    public interface ICacheProvider : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets the database id.
        /// </summary>
        int DatabaseId { get; }

        /// <summary>
        /// Flushes redis database.
        /// </summary>
        Task FlushAsync(bool fireAndForget = true);

        /// <summary>
        /// Returns all keys with the given pattern using SCAN.
        /// <para>https://redis.io/commands/scan/</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws if any of the given parameters is null or empty.</exception>
        RedisKey[] Scan(string pattern, int pageSize = 100);

        /// <summary>
        /// Executes a batch of commands to redis.
        /// </summary>
        Task BatchAsync(Action<IRedisWrite> action, bool fireAndForget = true);

        /// <summary>
        /// Executes a batch of commands to redis and returns the result.
        /// </summary>
        Task<IReadOnlyList<RedisCache<T>>> BatchAsync<T>(Action<IRedisRead> action) where T : new();

        /// <summary>
        /// Publishes a message to the given channel.
        /// </summary>
        Task PublishAsync(string channelName, string message, bool fireAndForget = true);

        /// <summary>
        /// Subscribes to to the given channel.
        /// </summary>
        Task SubscribeAsync(string channelName, Action<string> action);

        /// <summary>
        /// Checks if a key exists in Redis.
        /// <para>https://redis.io/commands/exists/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="redisKey"/> is null or empty.</exception>
        Task<bool> ExistsAsync(RedisKey redisKey);

        /// <summary>
        /// Executes the command using EXPIRE.
        /// <para>https://redis.io/commands/expire/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="time">The expiration time.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<bool>> ExpireAsync(RedisKey redisKey, TimeSpan time, bool fireAndForget = true);

        /// <summary>
        /// Executes the command using INCR.
        /// <para>https://redis.io/commands/incr/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="value">The increment value.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<long>> IncrementAsync(RedisKey redisKey, long value = 1, bool fireAndForget = false);

        /// <summary>
        /// Executes the command using HINCRBY.
        /// <para>https://redis.io/commands/hincrby/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="field">The field to increment.</param>
        /// <param name="value">The increment value.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<long>> IncrementAsync(RedisKey redisKey, string field, long value = 1, bool fireAndForget = true);

        /// <summary>
        /// Executes the command using DEL.
        /// <para>https://redis.io/commands/del/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<bool>> DeleteAsync(RedisKey redisKey, bool fireAndForget = true);

        /// <summary>
        /// Executes the command using HDEL.
        /// <para>https://redis.io/commands/hdel/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="field">The field to delete.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<bool>> DeleteAsync(RedisKey redisKey, string field, bool fireAndForget = true);

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
        /// <param name="redisKey">The cache key.</param>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="when">The condition to set the value.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<bool>> SetAsync<TValue>(RedisKey redisKey, string field, TValue value, When when = When.Always, bool fireAndForget = true);

        /// <summary>
        /// <inheritdoc cref="SetAsync"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="model">The model to set.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task SetAsync<T>(RedisKey redisKey, T model, bool fireAndForget = true);

        /// <summary>
        /// Execute the command using HMSET. If the number of fields is 1, it will be executed using HSET.
        /// <para>https://redis.io/commands/hmset/<br />https://redis.io/commands/hset/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="values">The values to set.</param>
        /// <param name="fireAndForget">If true, the command will be executed using FireAndForget.</param>
        /// <returns>A <see cref="RedisCache{T}"/> with the result. If <see cref="fireAndForget"/> is true, the underlying value will be default value.</returns>
        Task<RedisCache<bool>> SetAsync(RedisKey redisKey, object values, bool fireAndForget = true);

        /// <summary>
        /// Reads a hash from Redis and maps it to a model using HMGET. If number of fields is 1, HGET is used instead.
        /// <para>https://redis.io/commands/hmget/<br />https://redis.io/commands/hget/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="fields">The fields to read. If null or empty, all fields will be read.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="fields"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="fields"/> is empty.</exception>
        Task<RedisCache> GetAsync(RedisKey redisKey, params string[] fields);

        /// <summary>
        /// Reads a hash from Redis and maps it to a model using HMGET. If number of fields is 1, HGET is used instead.
        /// <para>https://redis.io/commands/hmget/<br />https://redis.io/commands/hget/</para>
        /// </summary>
        /// <param name="redisKey">The cache key.</param>
        /// <param name="fields">The fields to read. If null or empty, all fields will be read.</param>
        Task<RedisCache<T>> GetAsync<T>(RedisKey redisKey, params string[] fields) where T : new();
    }
}