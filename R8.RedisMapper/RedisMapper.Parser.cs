using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        
        private static int FromDouble(double other) => (int)other;

        private static Func<double, int> di = FromDouble;


        public static TObject Parse<TObject>(this RedisValue[] redisValues, IList<IRedisValueSerializer> valueFormatters, RedisValueReaderContext readerContext) where TObject : class, IRedisCacheObject, new()
        {
            if (redisValues.Length == 0)
                return default;

            if (redisValues.All(x => x.IsNull))
                return default;

            var model = new TObject();
            if (!(model is IRedisCacheObjectAdapter<TObject> adapter))
                throw new InvalidOperationException($"It might the source generator did not generate the reflector for {model.GetType()}.");
            
            var anyValid = false;
            for (var i = 0; i < redisValues.Length; i++)
            {
                var cacheValid = adapter.SetValue(model, adapter.Properties[i], redisValues[i]);
                if (cacheValid)
                    anyValid = true;
            }

            if (!anyValid)
                return default;

            return model;
        }
    }
}