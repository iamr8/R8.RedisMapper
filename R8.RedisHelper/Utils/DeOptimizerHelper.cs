using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace R8.RedisHelper.Utils
{
    public static class DeOptimizerHelper
    {
        private static void SetDateTime<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue)
        {
            if (!cacheValue.TryParse(out long unix))
                throw new NotSupportedException();

            prop.SetStructValue<T, DateTime>(model, Helpers.FromUnixTimeSeconds(unix));
        }

        private static void SetString<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue)
        {
            prop.SetValue<T, string>(model, cacheValue.ToString());
        }

        private static void SetDouble<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue)
        {
            if (!cacheValue.TryParse(out double i))
                throw new NotSupportedException();

            prop.SetStructValue<T, double>(model, i);
        }

        private static void SetInt64<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue)
        {
            if (!cacheValue.TryParse(out long i))
                throw new NotSupportedException();

            prop.SetStructValue<T, long>(model, i);
        }

        private static void SetInt32<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue, Type underlyingType)
        {
            if (!cacheValue.TryParse(out int i))
                throw new NotSupportedException();

            if (underlyingType.IsEnum)
            {
                prop.SetValue(model, Enum.ToObject(underlyingType, i));
            }
            else
            {
                prop.SetStructValue<T, int>(model, i);
            }
        }

        private static void SetBoolean<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue)
        {
            if (!cacheValue.TryParse(out int i))
                throw new NotSupportedException();

            prop.SetStructValue<T, bool>(model, i == 1);
        }

        private static void SetListArray<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue, Type underlyingType)
        {
            var cacheString = cacheValue.ToString();

            var elementType = underlyingType.IsArray
                ? underlyingType.GetElementType()
                : underlyingType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType)!;

            var node = JsonNode.Parse(cacheString);
            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    var strVal = item.ToString();

                    if (Type.GetTypeCode(elementType) == TypeCode.Object)
                    {
                        var obj = JsonSerializer.Deserialize(strVal, elementType, Helpers.JsonSerializerDefaultOptions);
                        list.Add(obj);
                    }
                    else
                    {
                        var parsedVal = elementType.FromOptimizedValue(strVal);
                        list.Add(parsedVal);
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            if (underlyingType.IsArray)
            {
                var arr = Array.CreateInstance(elementType, list.Count);
                list.CopyTo(arr, 0);
                prop.SetValue(model, arr);
            }
            else
            {
                prop.SetValue(model, list);
            }
        }

        private static void SetDictionary<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue, Type underlyingType)
        {
            var cacheString = cacheValue.ToString();

            var genericTypes = underlyingType.GetGenericArguments();
            var dictType = typeof(Dictionary<,>).MakeGenericType(genericTypes);
            var dict = (IDictionary)Activator.CreateInstance(dictType)!;

            var node = JsonNode.Parse(cacheString);
            if (node is JsonObject obj)
            {
                foreach (var (key, val) in obj)
                {
                    var keyType = genericTypes[0];
                    var valType = genericTypes[1];

                    var strVal = val.ToString();

                    var parsedVal = valType.FromOptimizedValue(strVal);
                    var parsedKey = keyType.FromOptimizedValue(key);

                    dict.Add(parsedKey, parsedVal);
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            prop.SetValue(model, dict);
        }

        private static void SetObject<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue, Type underlyingType)
        {
            var json = cacheValue.ToString();

            var obj = JsonSerializer.Deserialize(json, underlyingType, Helpers.JsonSerializerDefaultOptions);

            prop.SetValue(model, obj);
        }

        private static void SetValue<TModel, TProp>(this PropertyInfo prop, TModel model, TProp value)
        {
            var setter = prop.SetMethod!.CreateDelegate<Action<TModel, TProp>>();
            setter(model, value);
        }

        private static void SetStructValue<TModel, TProp>(this PropertyInfo prop, TModel model, TProp value) where TProp : struct
        {
            if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
            {
                var setter = prop.SetMethod!.CreateDelegate<Action<TModel, TProp?>>();
                setter(model, value);
            }
            else
            {
                prop.SetValue<TModel, TProp>(model, value);
            }
        }

        /// <summary>
        /// Sets the value of the property from the cache.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static bool TrySetFromRedisValue<T>([DisallowNull] this PropertyInfo prop, [DisallowNull] T model, [DisallowNull] RedisValue cacheValue)
        {
            if (prop.GetSetMethod() == null)
                throw new InvalidOperationException("Property must have a setter.");

            if (!cacheValue.HasValue || cacheValue.IsNullOrEmpty)
            {
                // Data not cached yet.
                return false;
            }

            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.Boolean:
                {
                    prop.SetBoolean(model, cacheValue);
                    break;
                }
                case TypeCode.Int32:
                {
                    prop.SetInt32(model, cacheValue, underlyingType);
                    break;
                }
                case TypeCode.Int64:
                {
                    prop.SetInt64(model, cacheValue);
                    break;
                }
                case TypeCode.Double:
                {
                    prop.SetDouble(model, cacheValue);
                    break;
                }
                case TypeCode.String:
                {
                    prop.SetString(model, cacheValue);
                    break;
                }
                case TypeCode.DateTime:
                {
                    prop.SetDateTime(model, cacheValue);
                    break;
                }
                case TypeCode.Object:
                {
                    if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
                    {
                        prop.SetDictionary(model, cacheValue, underlyingType);
                    }
                    else if ((underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) || underlyingType.IsArray)
                    {
                        prop.SetListArray(model, cacheValue, underlyingType);
                    }
                    else
                    {
                        prop.SetObject(model, cacheValue, underlyingType);
                    }

                    break;
                }

                default:
                    throw new NotSupportedException();
            }

            return true;
        }

        private static object? FromOptimizedValue(this Type type, string str)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            str = str.Replace("\u0022", "\"");
            var valUnderlyingType = Nullable.GetUnderlyingType(type) ?? type;
            switch (Type.GetTypeCode(valUnderlyingType))
            {
                case TypeCode.Object:
                {
                    var obj = JsonSerializer.Deserialize(str, type, Helpers.JsonSerializerDefaultOptions);
                    return obj;
                }
                case TypeCode.Boolean:
                {
                    var valueParsed = int.TryParse(str, out var i);
                    if (valueParsed)
                        return i == 1 || (i == 0 ? false : throw new NotSupportedException());

                    valueParsed = bool.TryParse(str, out var b);
                    if (!valueParsed)
                        throw new NotSupportedException();

                    return b;
                }
                case TypeCode.Int32:
                {
                    var valueParsed = int.TryParse(str, out var i);
                    if (!valueParsed)
                        throw new NotSupportedException();

                    return i;
                }
                case TypeCode.Int64:
                {
                    var valueParsed = long.TryParse(str, out var l);
                    if (!valueParsed)
                        throw new NotSupportedException();

                    return l;
                }
                case TypeCode.Double:
                {
                    var valueParsed = double.TryParse(str, out var d);
                    if (!valueParsed)
                        throw new NotSupportedException();

                    return d;
                }
                case TypeCode.DateTime:
                {
                    var valueParsed = long.TryParse(str, out var dt);
                    if (!valueParsed)
                        throw new NotSupportedException();

                    return Helpers.FromUnixTimeSeconds(dt);
                }
                case TypeCode.String:
                {
                    return str;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}