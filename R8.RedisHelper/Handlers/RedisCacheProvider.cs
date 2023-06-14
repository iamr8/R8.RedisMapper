using Microsoft.Extensions.Logging;
using R8.RedisHelper.Interfaces;
using R8.RedisHelper.Models;
using R8.RedisHelper.Utils;

namespace R8.RedisHelper.Handlers
{
    internal class RedisCacheProvider : ICacheProvider
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly IServer _server;
        private readonly ISubscriber _subscriber;

        private readonly ILogger<RedisCacheProvider> _logger;

        public RedisCacheProvider(RedisHelperOptions options, ILogger<RedisCacheProvider> logger)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(options.Configurations);
            _connectionMultiplexer.ConnectionFailed += (sender, args) => { logger.LogError("Connection failed: Connection type: {ConnectionType}, Failure type: {FailureType}, Exception: {Exception}", args.ConnectionType.ToString(), args.FailureType.ToString(), args.Exception?.StackTrace); };
            _connectionMultiplexer.ConnectionRestored += (sender, args) => { logger.LogInformation("Connection Restored: Connection type: {ConnectionType}, Failure type: {FailureType}, Exception: {Exception}", args.ConnectionType.ToString(), args.FailureType.ToString(), args.Exception?.StackTrace); };
            _connectionMultiplexer.ErrorMessage += (sender, args) => { logger.LogError("Error: {Message}", args.Message); };
            _connectionMultiplexer.InternalError += (sender, args) => { logger.LogError("Internal errors: Connection type: {ConnectionType}, Exception: {Exception}", args.ConnectionType.ToString(), args.Exception?.StackTrace); };

            _logger = logger;

            DatabaseId = options.DatabaseId;
            _database = _connectionMultiplexer.GetDatabase(DatabaseId);
            _server = _connectionMultiplexer.GetServer(options.Configurations.EndPoints.First());
            _subscriber = _connectionMultiplexer.GetSubscriber();
        }

        public int DatabaseId { get; }

        public Task FlushAsync(bool fireAndForget = true)
        {
            return _server.FlushDatabaseAsync(DatabaseId, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
        }

        public async Task<bool> ExistsAsync(RedisKey redisKey)
        {
            var exists = await _database.KeyExistsAsync(redisKey);
            _logger.LogDebug("EXISTS {CacheKey}", redisKey);

            return exists;
        }

        public RedisKey[] Scan(string pattern, int pageSize = 100)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            var patternKey = new RedisValue(pattern);

            long cursor = 0;
            int keysInPage;

            var keys = new List<RedisKey>();

            do
            {
                var _keys = _server.Keys(database: DatabaseId, pattern: patternKey, pageSize: pageSize, cursor: cursor).ToList();
                keysInPage = _keys.Count;
                if (keysInPage > 0)
                {
                    keys.AddRange(_keys);
                    _logger.LogDebug("SCAN {Cursor} MATCH {Pattern} COUNT {PageSize} ({Keys})", cursor, pattern, pageSize, keys);
                }

                cursor += pageSize;
            } while (keysInPage == pageSize);

            if (keys.Count == 0)
            {
                _logger.LogDebug("SCAN 0 MATCH {Pattern} COUNT {PageSize}", pattern, pageSize);
                return Array.Empty<RedisKey>();
            }

            var output = new RedisKey[keys.Count];
            for (int i = 0; i < keys.Count; i++)
                output[i] = keys[i];

            return output;
        }

        public async Task SubscribeAsync(string channelName, Action<string> action)
        {
            var channel = new RedisChannel(channelName, RedisChannel.PatternMode.Literal);

            var queue = await _subscriber.SubscribeAsync(channel);
            queue.OnMessage(message => action(message.Message));
        }

        public Task PublishAsync(string channelName, string message, bool fireAndForget = true)
        {
            var channel = new RedisChannel(channelName, RedisChannel.PatternMode.Literal);
            var redisValue = new RedisValue(message);

            return _subscriber.PublishAsync(channel, redisValue, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
        }

        public async Task BatchAsync(Action<IRedisWrite> action, bool fireAndForget = true)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var batch = _database.CreateBatch();
            var b = new RedisBatchWriter(batch, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);

            try
            {
                action(b);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return;
            }

            if (!b.Writers.Any())
                return;

            var tasks = new List<Task>();
            var operations = new List<IRedisOperation>();
            foreach (var writer in b.Writers)
            {
                operations.Add(writer);
                tasks.Add(writer.ExecuteAsync());
            }

            batch.Execute();

            try
            {
                await Task.WhenAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return;
            }

            operations.WriteToLog(_logger);
        }

        public async Task<IReadOnlyList<RedisCache<T>>> BatchAsync<T>(Action<IRedisRead> action)
            where T : new()
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var batch = _database.CreateBatch();
            var b = new RedisBatchReader<T>(batch);

            try
            {
                action(b);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Array.Empty<RedisCache<T>>();
            }

            if (!b.Readers.Any())
                return Array.Empty<RedisCache<T>>();

            var tasks = new List<Task>();
            var operations = new List<IRedisOperation>();
            foreach (var reader in b.Readers)
            {
                operations.Add(reader);
                tasks.Add(reader.ExecuteAsync());
            }

            batch.Execute();

            try
            {
                await Task.WhenAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Array.Empty<RedisCache<T>>();
            }

            var redisCaches = new List<RedisCache<T>>();
            foreach (RedisReaderTask<T> reader in b.Readers)
            {
                var redisCache = reader.PostAction.Invoke(reader.GetResult());
                if (redisCache.IsNull)
                    continue;

                redisCaches.Add(redisCache);
            }

            operations.WriteToLog(_logger);
            return redisCaches;
        }

        public async Task<RedisCache<T>> GetAsync<T>(RedisKey redisKey, params string[] fields) where T : new()
        {
            var reader = _database.Get<T>(redisKey, fields);
            await reader.ExecuteAsync();
            var result = reader.GetResult();
            reader.WriteToLog(_logger);

            var redisCache = ((RedisReaderTask<T>)reader).PostAction.Invoke(result);
            return redisCache;
        }

        public async Task<RedisCache> GetAsync(RedisKey redisKey, params string[] fields)
        {
            var reader = _database.Get(redisKey, fields);
            await reader.ExecuteAsync();
            var result = reader.GetResult();
            reader.WriteToLog(_logger);

            var redisCache = ((RedisReaderTask)reader).PostAction.Invoke(result);
            return redisCache;
        }

        public async Task<RedisCache<bool>> SetAsync<TValue>(RedisKey redisKey, string field, TValue value, When when = When.Always, bool fireAndForget = true)
        {
            var writer = _database.Set(redisKey, field, value, when, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<bool>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<bool>(redisKey, result);
            return c;
        }

        public Task SetAsync<T>(RedisKey redisKey, T model, bool fireAndForget = true)
        {
            var optimized = model.ToOptimizedDictionary();
            return SetAsync(redisKey, (object) optimized, fireAndForget);
        }

        public async Task<RedisCache<bool>> SetAsync(RedisKey redisKey, object values, bool fireAndForget = true)
        {
            var writer = _database.Set(redisKey, values, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<bool>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<bool>(redisKey, result);
            return c;
        }

        public async Task<RedisCache<bool>> DeleteAsync(RedisKey redisKey, bool fireAndForget = true)
        {
            var writer = _database.Delete(redisKey, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<bool>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<bool>(redisKey, result);
            return c;
        }

        public async Task<RedisCache<bool>> DeleteAsync(RedisKey redisKey, string field, bool fireAndForget = true)
        {
            var writer = _database.Delete(redisKey, field, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<bool>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<bool>(redisKey, result);
            return c;
        }

        public async Task<RedisCache<long>> IncrementAsync(RedisKey redisKey, string field, long value = 1, bool fireAndForget = true)
        {
            var writer = _database.Increment(redisKey, field, value, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<long>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<long>(redisKey, result);
            return c;
        }

        public async Task<RedisCache<long>> IncrementAsync(RedisKey redisKey, long value = 1, bool fireAndForget = false)
        {
            var writer = _database.Increment(redisKey, value, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<long>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<long>(redisKey, result);
            return c;
        }

        public async Task<RedisCache<long>> DecrementAsync(RedisKey redisKey, string field, long value = 1, bool fireAndForget = true)
        {
            var writer = _database.Decrement(redisKey, field, value, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<long>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<long>(redisKey, result);
            return c;
        }

        public async Task<RedisCache<long>> DecrementAsync(RedisKey redisKey, long value = 1, long min = long.MinValue, bool fireAndForget = false)
        {
            var writer = _database.Decrement(redisKey, value, min, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<long>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<long>(redisKey, result);
            return c;
        }
        
        public async Task<RedisCache<bool>> ExpireAsync(RedisKey redisKey, TimeSpan time, bool fireAndForget = true)
        {
            var writer = _database.Expire(redisKey, time, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
            await writer.ExecuteAsync();
            var result = writer.GetResult<bool>();
            writer.WriteToLog(_logger);

            var c = new RedisCache<bool>(redisKey, result);
            return c;
        }

        public void Dispose()
        {
            _connectionMultiplexer.Close(true);
            _connectionMultiplexer.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _connectionMultiplexer.CloseAsync(true);
            await _connectionMultiplexer.DisposeAsync();
        }
    }
}