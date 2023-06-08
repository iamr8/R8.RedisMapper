using R8.RedisHelper.Interfaces;
using R8.RedisHelper.Models;
using R8.RedisHelper.Utils;

namespace R8.RedisHelper.Handlers
{
    internal class RedisBatchWriter : IRedisWrite
    {
        private readonly IDatabaseAsync _database;
        private readonly CommandFlags _flags;

        public readonly List<IRedisWriter> Writers = new();

        public RedisBatchWriter(IDatabaseAsync database, CommandFlags flags)
        {
            _database = database;
            _flags = flags;
        }

        public void Set<TValue>(RedisCacheKey cacheKey, string field, TValue value, When when = When.Always)
        {
            var writer = _database.Set(cacheKey, field, value, when, _flags);
            Writers.Add(writer);
        }

        public void Set(RedisCacheKey cacheKey, object values)
        {
            var writer = _database.Set(cacheKey, values, _flags);
            Writers.Add(writer);
        }

        public void Set<T>(RedisCacheKey cacheKey, T model) where T : class, new()
        {
            var optimized = model.ToOptimizedDictionary();
            Set(cacheKey, optimized);
        }

        public void Delete(RedisCacheKey cacheKey)
        {
            var writer = _database.Delete(cacheKey, _flags);
            Writers.Add(writer);
        }

        public void Delete(RedisCacheKey cacheKey, string field)
        {
            var writer = _database.Delete(cacheKey, field, _flags);
            Writers.Add(writer);
        }

        public void Increment(RedisCacheKey cacheKey, string field, long value = 1L)
        {
            var writer = _database.Increment(cacheKey, field, value, _flags);
            Writers.Add(writer);
        }

        public void Increment(RedisCacheKey cacheKey, long value = 1L)
        {
            var writer = _database.Increment(cacheKey, value, _flags);
            Writers.Add(writer);
        }

        public void Expire(RedisCacheKey cacheKey, TimeSpan time)
        {
            var writer = _database.Expire(cacheKey, time, _flags);
            Writers.Add(writer);
        }
    }
}