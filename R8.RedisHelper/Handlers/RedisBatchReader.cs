using R8.RedisHelper.Interfaces;
using R8.RedisHelper.Models;
using R8.RedisHelper.Utils;

namespace R8.RedisHelper.Handlers
{
    internal class RedisBatchReader<T> : IRedisRead where T : new()
    {
        private readonly IDatabaseAsync _database;

        public readonly List<IRedisReader> Readers = new();

        public RedisBatchReader(IDatabaseAsync database)
        {
            _database = database;
        }

        public void Get(RedisKey redisKey, params string[] fields)
        {
            var reader = _database.Get<T>(redisKey, fields);
            Readers.Add(reader);
        }
    }
}