using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    public static class Commands
    {
        private static void ExtractConfiguration<T>(Action<PipelineContext<T>> options,
            out CachedPropertyInfo[] props, out RedisValueReaderContext readerContext, out IList<IRedisValueFormatter> valueFormatters, out RedisFieldFormatter fieldFormatter)
        {
            if (options != null)
            {
                var opt = new PipelineContext<T>();
                options.Invoke(opt);

                readerContext = new RedisValueReaderContext(typeof(T))
                {
                    IgnoreDefaultValue = opt.IgnoreDefaultValue ?? Configuration.IgnoreDefaultValues,
                    SerializerOptions = opt.SerializerOptions ?? Configuration.SerializerOptions,
                };

                fieldFormatter = opt.FieldFormatter ?? Configuration.FieldFormatter;
                valueFormatters = opt.ValueFormatters ?? Configuration.ValueFormatters;

                if (opt.AllProperties)
                {
                    props = typeof(T).GetProperties(Array.Empty<string>(), Configuration.FieldFormatter).ToArray();
                }
                else
                {
                    if (opt.Properties.Count == 0)
                        throw new RedisMapperException("No properties has been selected");

                    Span<string> span = new string[opt.Properties.Count];
                    for (var i = 0; i < opt.Properties.Count; i++)
                        span[i] = opt.Properties[i];

                    props = typeof(T).GetProperties(span.ToArray(), Configuration.FieldFormatter).ToArray();
                }
            }
            else
            {
                props = typeof(T).GetProperties(Array.Empty<string>(), Configuration.FieldFormatter).ToArray();
                readerContext = new RedisValueReaderContext(typeof(T))
                {
                    IgnoreDefaultValue = Configuration.IgnoreDefaultValues,
                    SerializerOptions = Configuration.SerializerOptions,
                };
                fieldFormatter = Configuration.FieldFormatter;
                valueFormatters = Configuration.ValueFormatters;
            }
        }

        private static void ExtractConfiguration(Action<PipelineContext> options,
            out string[] fields, out RedisFieldFormatter fieldFormatter)
        {
            var opt = new PipelineContext();
            options.Invoke(opt);

            fieldFormatter = opt.FieldFormatter ?? Configuration.FieldFormatter;

            if (opt.Fields.Count == 0)
                throw new RedisMapperException("No fields has been selected");

            fields = opt.Fields.ToArray();
        }

        /// <summary>
        /// Retrieves the values associated with the specified fields in the hash stored at the specified cache key asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="database">The asynchronous <see cref="RedisDatabase"/> connection.</param>
        /// <param name="key">The key of the cache hash.</param>
        /// <param name="options">Optional. The action to perform on the <see cref="PipelineContext{T}"/>.</param>
        /// <param name="flags">Optional. The flags to use for this operation.</param>
        /// <returns>
        /// A Task representing the asynchronous operation that retrieves the value associated with the specified field
        /// in the hash stored at the specified cache key. The task result represents the retrieved value.
        /// </returns>
        public static async Task<T> HashGetAsync<T>(this IDatabaseAsync database, RedisKey key, Action<PipelineContext<T>> options = null, CommandFlags flags = CommandFlags.None)
        {
            ExtractConfiguration(options, out var props, out var readerContext, out var valueFormatters, out var fieldFormatter);

            var fields = props.Select(x => fieldFormatter.GetFormatted(x.Property.Name)).ToArray();
            var redisValues = await database.HashGetAsync(key, fields, flags).ConfigureAwait(false);
            var redisCache = redisValues.Parse<T>(props, valueFormatters, readerContext);
            return redisCache;
        }

        /// <summary>
        /// Retrieves the values associated with the specified fields in the hash stored at the specified cache key asynchronously.
        /// </summary>
        /// <param name="database">The asynchronous <see cref="RedisDatabase"/> connection.</param>
        /// <param name="key">The key of the cache hash.</param>
        /// <param name="options">Optional. The action to perform on the <see cref="PipelineContext"/>.</param>
        /// <param name="flags">Optional. The flags to use for this operation.</param>
        /// <returns>
        /// A Task representing the asynchronous operation that retrieves the value associated with the specified field
        /// in the hash stored at the specified cache key. The task result represents the retrieved value.
        /// </returns>
        public static async Task<IReadOnlyDictionary<string, RedisValue>> HashGetAsync(this IDatabaseAsync database, RedisKey key, Action<PipelineContext> options, CommandFlags flags = CommandFlags.None)
        {
            ExtractConfiguration(options, out var fields, out var fieldFormatter);

            var hashFields = fields.Select(x => fieldFormatter.GetFormatted(x)).ToArray();
            var redisValues = await database.HashGetAsync(key, hashFields, flags).ConfigureAwait(false);
            var redisCache = redisValues.Parse(fields);
            return redisCache;
        }


        
        
        /// <summary>
        /// Returns the deserialized model value of the specified key asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the model to retrieve.</typeparam>
        /// <param name="database">The Redis database connection.</param>
        /// <param name="key">The <see cref="RedisKey"/> of the cache.</param>
        /// <param name="flags">The flags to use for this operation.</param>
        /// <returns>The deserialized model value.</returns>
        public static async Task<T> GetAsync<T>(this IDatabaseAsync database, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var redisCache = await database.StringGetAsync(key, flags).ConfigureAwait(false);
            if (redisCache.IsNullOrEmpty)
                return default;

            var c = (string)redisCache!;
            var model = JsonSerializer.Deserialize<T>(c, Configuration.SerializerOptions);
            return model;
        }

        
        
        
        public static Task<bool> SetAsync<TValue>(this IDatabaseAsync database, RedisKey key, TValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var writerContext = new RedisValueWriterContext
            {
                IgnoreDefaultValue = Configuration.IgnoreDefaultValues,
                SerializerOptions = Configuration.SerializerOptions,
                DataType = typeof(TValue),
            };
            var val = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
            if (val.IsNullOrEmpty)
            {
                return database.KeyDeleteAsync(key, flags);
            }
            else
            {
                return database.StringSetAsync(key, val, expiry, when, flags);
            }
        }

        
        
        

        /// <summary>
        /// Sets the values of the specified keys in a hash stored at the specified key asynchronously.
        /// </summary>
        /// <param name="database">The Redis database instance.</param>
        /// <param name="key">The key of the hash.</param>
        /// <param name="model">The model object to store as hash entries.</param>
        /// <param name="flags">The flags to be applied to the command (optional).</param>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
        public static Task HashSetAsync<TModel>(this IDatabaseAsync database, RedisKey key, TModel model, CommandFlags flags = CommandFlags.None) where TModel : class
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var writerContext = new RedisValueWriterContext
            {
                IgnoreDefaultValue = Configuration.IgnoreDefaultValues,
                SerializerOptions = Configuration.SerializerOptions,
            };
            var hashEntries = model.GetHashEntries(Configuration.FieldFormatter, Configuration.ValueFormatters, writerContext);
            return database.HashSetAsync(key, hashEntries.ToArray(), flags: flags);
        }

        /// <summary>
        /// Asynchronously sets the values of multiple fields in a hash stored at the specified key in Redis.
        /// </summary>
        /// <typeparam name="TValue">The type of the values in the hash entries.</typeparam>
        /// <param name="database">The Redis database.</param>
        /// <param name="key">The key of the hash.</param>
        /// <param name="keys">A dictionary containing the fields and their corresponding values to be set in the hash.</param>
        /// <param name="flags">The command flags (optional).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keys"/> is null.</exception>
        public static Task HashSetAsync<TValue>(this IDatabaseAsync database, RedisKey key, IDictionary<string, TValue> keys, CommandFlags flags = CommandFlags.None)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var writerContext = new RedisValueWriterContext
            {
                IgnoreDefaultValue = Configuration.IgnoreDefaultValues,
                SerializerOptions = Configuration.SerializerOptions,
            };
            var hashEntries = keys.ToHashEntries(Configuration.FieldFormatter, Configuration.ValueFormatters, writerContext);
            return database.HashSetAsync(key, hashEntries.ToArray(), flags: flags);
        }
    }
}