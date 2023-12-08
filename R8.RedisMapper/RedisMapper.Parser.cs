using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    internal static class Parser
    {
        public static Dictionary<string, RedisValue> Parse(this IEnumerable<RedisValue> redisValues, IEnumerable<string> fields)
        {
            var dict = redisValues
                .Zip(fields, (value, field) => new KeyValuePair<string, RedisValue>(field, value))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            return dict;
        }

        public static T Parse<T>(this IReadOnlyList<RedisValue> redisValues, IReadOnlyList<CachedPropertyInfo> props, IList<IRedisValueFormatter> valueFormatters, RedisValueReaderContext readerContext)
        {
            if (redisValues.Count == 0)
                return default;

            if (redisValues.All(x => x.IsNull))
                return default;

            var model = Activator.CreateInstance<T>();
            var anyValid = false;
            for (var i = 0; i < redisValues.Count; i++)
            {
                var prop = props[i];
                var redisValue = redisValues[i];

                var rc = new RedisValueReaderContext(readerContext, prop.Property.PropertyType);
                var cacheValid = prop.SetValue(model, redisValue, valueFormatters, rc);
                if (cacheValid)
                    anyValid = true;
            }

            if (!anyValid)
                return default;

            return model;
        }
    }
}