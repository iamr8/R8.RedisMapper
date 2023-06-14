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

        public void Set<TValue>(RedisKey redisKey, string field, TValue value, When when = When.Always)
        {
            var writer = _database.Set(redisKey, field, value, when, _flags);
            Writers.Add(writer);
        }

        public void Set(RedisKey redisKey, object values)
        {
            var writer = _database.Set(redisKey, values, _flags);
            Writers.Add(writer);
        }

        public void Set<T>(RedisKey redisKey, T model) where T : class, new()
        {
            var optimized = model.ToOptimizedDictionary();
            Set(redisKey, optimized);
        }

        public void Delete(RedisKey redisKey)
        {
            var writer = _database.Delete(redisKey, _flags);
            Writers.Add(writer);
        }

        public void Delete(RedisKey redisKey, string field)
        {
            var writer = _database.Delete(redisKey, field, _flags);
            Writers.Add(writer);
        }

        public void Increment(RedisKey redisKey, string field, long value = 1L)
        {
            var writer = _database.Increment(redisKey, field, value, _flags);
            Writers.Add(writer);
        }

        public void Increment(RedisKey redisKey, long value = 1L)
        {
            var writer = _database.Increment(redisKey, value, _flags);
            Writers.Add(writer);
        }

        public void Decrement(RedisKey redisKey, string field, long value = 1L)
        {
            var writer = _database.Decrement(redisKey, field, value, _flags);
            Writers.Add(writer);
        }
    
        public void Decrement(RedisKey redisKey, long value = 1L, long min = long.MinValue)
        {
            var writer = _database.Decrement(redisKey, value, min, _flags);
            Writers.Add(writer);
        }
        
        public void Expire(RedisKey redisKey, TimeSpan time)
        {
            var writer = _database.Expire(redisKey, time, _flags);
            Writers.Add(writer);
        }
    }
}