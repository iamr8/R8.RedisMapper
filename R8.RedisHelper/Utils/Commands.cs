using System.Collections.ObjectModel;
using System.ComponentModel;
using R8.RedisHelper.Interfaces;
using R8.RedisHelper.Models;

namespace R8.RedisHelper.Utils
{
    internal static class Commands
    {
        public static IRedisReader Get<T>(this IDatabaseAsync database, RedisCacheKey cacheKey, params string[] fields) where T : new()
        {
            if (cacheKey == null) throw new ArgumentNullException(nameof(cacheKey));

            var _modelType = typeof(T);
            var props = _modelType.GetPublicProperties();
            if (fields?.Any() == true)
                props = props
                    .Where(prop => fields.Any(_prop => _prop.Equals(prop.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

            var modelFields = props.ToDictionary(x => x.Name.ToCamelCase(), x => x);
            var redisKey = cacheKey.Value;

            RedisReaderTask<T> reader;
            if (modelFields.Count == 1)
            {
                var modelField = modelFields.Keys.First();

                reader = new RedisReaderTask<T>
                {
                    Command = "HGET",
                    CacheKey = cacheKey,
                    Fields = modelFields.Keys.ToArray(),
                    Action = () => database.HashGetAsync(redisKey, new RedisValue(modelField))
                };
            }
            else
            {
                reader = new RedisReaderTask<T>
                {
                    Command = "HMGET",
                    CacheKey = cacheKey,
                    Fields = modelFields.Keys.ToArray(),
                    ActionWithPluralReturnType = () => database.HashGetAsync(redisKey, modelFields.Keys.Select(key => new RedisValue(key)).ToArray())
                };
            }

            reader.PostAction = redisValues =>
            {
                if (redisValues.All(x => x.IsNull))
                    return RedisCache<T>.Null;

                var model = new T();
                var missingProps = new List<string>(redisValues.Length);
                for (int i = 0; i < redisValues.Length; i++)
                {
                    var prop = modelFields.ElementAt(i).Value;
                    var redisValue = redisValues[i];

                    try
                    {
                        var cacheValid = prop.TrySetFromRedisValue(model, redisValue);
                        if (!cacheValid)
                            missingProps.Add(prop.Name);
                    }
                    catch (Exception e)
                    {
                        // this._logger.LogError(e, e.Message);
                        if (!missingProps.Contains(prop.Name))
                            missingProps.Add(prop.Name);
                    }
                }

                var output = new RedisCache<T>(redisKey, model, missingProps);
                return output;
            };

            return reader;
        }

        public static IRedisReader Get(this IDatabaseAsync database, RedisCacheKey cacheKey, params string[] fields)
        {
            if (cacheKey == null)
                throw new ArgumentNullException(nameof(cacheKey));
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));
            if (fields.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(fields));

            var redisKey = cacheKey.Value;

            RedisReaderTask reader;
            if (fields.Length == 1)
            {
                reader = new RedisReaderTask
                {
                    Command = "HGET",
                    CacheKey = cacheKey,
                    Fields = fields,
                    Action = () => database.HashGetAsync(redisKey, new RedisValue(fields[0]))
                };
            }
            else
            {
                reader = new RedisReaderTask
                {
                    Command = "HMGET",
                    CacheKey = cacheKey,
                    Fields = fields,
                    ActionWithPluralReturnType = () => database.HashGetAsync(redisKey, fields.Select(key => new RedisValue(key)).ToArray())
                };
            }

            reader.PostAction = redisValues =>
            {
                if (redisValues.All(x => x.IsNull))
                    return RedisCache.Null;

                var dictionary = new Dictionary<string, RedisValue>();
                for (int i = 0; i < redisValues.Length; i++)
                {
                    var field = fields[i];
                    var redisValue = redisValues[i];
                    dictionary.Add(field, redisValue);
                }

                var output = new RedisCache(redisKey, dictionary);
                return output;
            };

            return reader;
        }

        public static IRedisWriter Set<TValue>(this IDatabaseAsync database, RedisCacheKey cacheKey, string field, TValue value, When when = When.Always, CommandFlags flags = CommandFlags.FireAndForget)
        {
            if (cacheKey == null)
                throw new ArgumentNullException(nameof(cacheKey));
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            field = field.ToCamelCase();
            var redisField = new RedisValue(field);
            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<bool>
            {
                CacheKey = cacheKey
            };

            if (!value.IsOptimizable())
            {
                writer.Command = "HDEL";
                writer.Fields = new[] {field};
                writer.ActionWithReturnType = () => database.HashDeleteAsync(redisKey, redisField, flags);
            }
            else
            {
                var redisValue = RedisValue.Unbox(value.ToOptimizedObject());

                writer.Command = when switch
                {
                    When.Always => "HSET",
                    When.Exists => "HSETNX",
                    When.NotExists => "HSETNX",
                };
                writer.Fields = new[] {field};
                writer.Values = new[] {redisValue};
                writer.ActionWithReturnType = () => database.HashSetAsync(redisKey, redisField, redisValue, when, flags);
            }

            return writer;
        }

        public static IRedisWriter Set(this IDatabaseAsync database, RedisCacheKey cacheKey, object values, CommandFlags flags = CommandFlags.FireAndForget)
        {
            var optimizedFields = new Dictionary<string, object>();
            if (values is ReadOnlyDictionary<string, object> readOnlyDictionary)
            {
                optimizedFields = readOnlyDictionary.ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    var name = descriptor.Name;
                    var value = descriptor.GetValue(values);
                    optimizedFields.Add(name, value);
                }
            }

            if (optimizedFields.Count == 0)
                throw new ArgumentException("No fields to set.", nameof(values));

            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<bool> {CacheKey = cacheKey};
            if (optimizedFields.Count == 1)
            {
                var (key, value) = optimizedFields.First();

                var redisValue = RedisValue.Unbox(value);

                writer.Command = "HSET";
                writer.Fields = new[] {key};
                writer.Values = new[] {redisValue};
                writer.ActionWithReturnType = () => database.HashSetAsync(redisKey, new RedisValue(key), redisValue, flags: flags);
            }
            else
            {
                var hashFields = optimizedFields
                    .Select(field => new HashEntry(new RedisValue(field.Key), RedisValue.Unbox(field.Value)))
                    .ToArray();

                writer.Command = "HMSET";
                writer.Fields = optimizedFields.Keys.ToArray();
                writer.Values = hashFields.Select(x => x.Value).ToArray();
                writer.Action = () => database.HashSetAsync(redisKey, hashFields, flags: flags);
            }

            return writer;
        }

        public static IRedisWriter Delete(this IDatabaseAsync database, RedisCacheKey cacheKey, CommandFlags flags = CommandFlags.FireAndForget)
        {
            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<bool>
            {
                Command = "DEL",
                CacheKey = cacheKey,
                Fields = Array.Empty<string>(),
                ActionWithReturnType = () => database.KeyDeleteAsync(redisKey, flags: flags)
            };

            return writer;
        }

        public static IRedisWriter Delete(this IDatabaseAsync database, RedisCacheKey cacheKey, string field, CommandFlags flags = CommandFlags.FireAndForget)
        {
            field = field.ToCamelCase();
            var redisField = new RedisValue(field);
            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<bool>
            {
                Command = "HDEL",
                CacheKey = cacheKey,
                Fields = new[] {field},
                ActionWithReturnType = () => database.HashDeleteAsync(redisKey, redisField, flags: flags)
            };

            return writer;
        }

        public static IRedisWriter Expire(this IDatabaseAsync database, RedisCacheKey cacheKey, TimeSpan time, CommandFlags flags = CommandFlags.FireAndForget)
        {
            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<bool>
            {
                Command = "EXPIRE",
                CacheKey = cacheKey,
                Fields = Array.Empty<string>(),
                Values = new[] {RedisValue.Unbox((int) time.TotalSeconds), },
                ActionWithReturnType = () => database.KeyExpireAsync(redisKey, time, flags: flags)
            };

            return writer;
        }

        public static IRedisWriter Increment(this IDatabaseAsync database, RedisCacheKey cacheKey, long value = 1L, CommandFlags flags = CommandFlags.FireAndForget)
        {
            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<long>
            {
                Command = "INCR",
                CacheKey = cacheKey,
                Fields = Array.Empty<string>(),
                ActionWithReturnType = () => database.StringIncrementAsync(redisKey, value, flags)
            };

            return writer;
        }

        public static IRedisWriter Increment(this IDatabaseAsync database, RedisCacheKey cacheKey, string field, long value = 1L, CommandFlags flags = CommandFlags.FireAndForget)
        {
            field = field.ToCamelCase();
            var redisField = new RedisValue(field);
            var redisKey = cacheKey.Value;

            var writer = new RedisWriterTask<long>
            {
                Command = "HINCRBY",
                CacheKey = cacheKey,
                Fields = new[] {field},
                ActionWithReturnType = () => database.HashIncrementAsync(redisKey, redisField, value: value, flags: flags)
            };

            return writer;
        }
    }
}