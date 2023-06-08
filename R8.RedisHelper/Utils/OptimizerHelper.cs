using System.Collections;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace R8.RedisHelper.Utils
{
    public static class OptimizerHelper
    {
        private static object ToOptimizedValue<T>(this T value)
        {
            var type = value.GetType();
            if (type.IsPrimitive)
            {
                if (value is bool b)
                    return b ? 1 : 0;

                if (type != typeof(double) && type != typeof(int) && type != typeof(long))
                    throw new NotSupportedException();

                return value;
            }

            if (type.IsEnum)
                return (int)(object)value;

            if (value is string str)
                return value;

            if (value is DateTime dt)
                return dt.ToUnixTimeSeconds();

            var val = value.JsonSerialize();
            return val;
        }

        public static object ToOptimizedObject<T>(this T v)
        {
            switch (v)
            {
                case IList list:
                {
                    var json = v.JsonSerialize();
                    return json;
                }
                case IDictionary dic:
                {
                    var dict = new Dictionary<object, object>();
                    foreach (var key in dic.Keys)
                    {
                        var val = dic[key];
                        dict.Add(key, val.ToOptimizedValue());
                    }

                    var json = dict.JsonSerialize();
                    return json;
                }
                case JsonElement je:
                {
                    return je.GetRawText();
                }
                default:
                    return v.ToOptimizedValue();
            }
        }

        internal static bool IsOptimizable<TValue>(this TValue obj)
        {
            if (obj is null)
                return false;

            if (obj.Equals(default))
                return false;

            if (obj is string str && string.IsNullOrWhiteSpace(str))
                return false;

            if (obj is IList { Count: 0 } or IDictionary { Count: 0 } or JsonElement { ValueKind: JsonValueKind.Null })
                return false;

            return true;
        }

        internal static IReadOnlyDictionary<string, object> ToOptimizedDictionary<T>(this T model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var props = typeof(T).GetPublicProperties();

            var modelFields = new Dictionary<string, object>();
            foreach (var prop in props)
            {
                var value = prop.GetValue(model);
                if (value == null || value.Equals(default))
                    continue;

                modelFields.Add(prop.Name, value);
            }

            if (!modelFields.Any())
                return null;

            var optimized = modelFields.ToOptimizedDictionary();
            return new ReadOnlyDictionary<string, object>(optimized);
        }

        internal static Dictionary<string, TValue> ToOptimizedDictionary<TValue>(this Dictionary<string, TValue> values) where TValue : class
        {
            var dict = new Dictionary<string, TValue>();
            foreach (var (k, v) in values)
            {
                var key = k.ToCamelCase();
                if (!v.IsOptimizable())
                    continue;

                var val = v.ToOptimizedObject();
                dict.Add(key, (TValue)val);
            }

            return dict;
        }
    }
}