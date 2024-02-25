using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    internal static class RedisValueExtensions
    {
        /// <summary>
        /// Converts a <see cref="JsonElement"/> to a <see cref="RedisValue"/> using the provided value formatters and writer context.
        /// </summary>
        /// <param name="value">The <see cref="JsonElement"/> to convert.</param>
        /// <param name="valueFormatters">The list of value formatters to use for conversion.</param>
        /// <param name="writerContext">The writer context to use for conversion.</param>
        /// <returns>The converted <see cref="RedisValue"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown if the <see cref="JsonElement"/>'s value kind is not supported.</exception>
        private static RedisValue GetRedisValue(this JsonElement value, IList<IRedisValueSerializer> valueFormatters, RedisValueWriterContext writerContext)
        {
            switch (value)
            {
                case { ValueKind: JsonValueKind.Number }:
                {
                    var val = value.GetDouble();
                    return GetRedisValue(val, valueFormatters, writerContext);
                }
                case { ValueKind: JsonValueKind.False }:
                case { ValueKind: JsonValueKind.True }:
                {
                    var val = value.GetBoolean();
                    return GetRedisValue(val, valueFormatters, writerContext);
                }
                case { ValueKind: JsonValueKind.Object }:
                {
                    var val = value.GetRawText();
                    return GetRedisValue(val, valueFormatters, writerContext);
                }
                case { ValueKind: JsonValueKind.Array }:
                {
                    var arr = value.EnumerateArray();
                    return GetRedisValue(arr, valueFormatters, writerContext);
                }
                default:
                    throw new NotSupportedException($"JsonElement with {value.ValueKind} is not supported.");
            }
        }
        
        /// <summary>
        /// Converts the specified value to a <see cref="RedisValue"/> using the provided <see cref="valueFormatters"/> and <see cref="writerContext"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="valueFormatters">The list of value formatters.</param>
        /// <param name="writerContext">The writer context.</param>
        /// <returns>The <see cref="RedisValue"/> representation of the value.</returns>
        public static RedisValue GetRedisValue<T>(T value, IList<IRedisValueSerializer> valueFormatters, RedisValueWriterContext writerContext)
        {
            if (value is null)
                return RedisValue.Null;
        
            var valueType = value.GetType();
            var context = new RedisValueWriterContext(writerContext, valueType);
            // switch (value)
            // {
            //     case RedisValue valRedis:
            //     return valRedis;
            //
            //     case JsonElement jsonElement:
            //     return jsonElement.GetRedisValue(valueFormatters, writerContext);
            //
            //     case int valueInt:
            //     {
            //         var intFormatter = (RedisValueFormatter<int>)valueFormatters.GetWriterFormatter(typeof(int));
            //         if (intFormatter != null)
            //             return intFormatter.Write(valueInt, context);
            //         break;
            //     }
            //     
            //     case long valueLong:
            //     {
            //         var longFormatter = (RedisValueFormatter<long>)valueFormatters.GetWriterFormatter(typeof(long));
            //         if (longFormatter != null)
            //             return longFormatter.Write(valueLong, context);
            //         break;
            //     }
            //     
            //     case double valueDouble:
            //     {
            //         var doubleFormatter = (RedisValueFormatter<double>)valueFormatters.GetWriterFormatter(typeof(double));
            //         if (doubleFormatter != null)
            //             return doubleFormatter.Write(valueDouble, context);
            //         break;
            //     }
            //     
            //     case float valueFloat:
            //     {
            //         var floatFormatter = (RedisValueFormatter<float>)valueFormatters.GetWriterFormatter(typeof(float));
            //         if (floatFormatter != null)
            //             return floatFormatter.Write(valueFloat, context);
            //         break;
            //     }
            //     
            //     case decimal valueDecimal:
            //     {
            //         var decimalFormatter = (RedisValueFormatter<decimal>)valueFormatters.GetWriterFormatter(typeof(decimal));
            //         if (decimalFormatter != null)
            //             return decimalFormatter.Write(valueDecimal, context);
            //         break;
            //     }
            //     
            //     case bool valueBool:
            //     {
            //         var boolFormatter = (RedisValueFormatter<bool>)valueFormatters.GetWriterFormatter(typeof(bool));
            //         if (boolFormatter != null)
            //             return boolFormatter.Write(valueBool, context);
            //         break;
            //     }
            //     
            //     case string valueString:
            //     {
            //         var stringFormatter = (RedisObjectFormatter<string>)valueFormatters.GetWriterFormatter(typeof(string));
            //         if (stringFormatter != null)
            //             return stringFormatter.Write(valueString, context);
            //         break;
            //     }
            //     
            //     case DateTime valueDateTime:
            //     {
            //         var dateTimeFormatter = (RedisValueFormatter<DateTime>)valueFormatters.GetWriterFormatter(typeof(DateTime));
            //         if (dateTimeFormatter != null)
            //             return dateTimeFormatter.Write(valueDateTime, context);
            //         break;
            //     }
            // }
            //
            // var formatter = valueFormatters.GetWriterFormatter(valueType);
            // if (formatter != null)
            // {
            //     if (valueType.IsValueType && formatter is RedisValueFormatterInternal valueFormatter)
            //     {
            //         var rv = valueFormatter.WriteCore(value, context);
            //     }
            //     else if (formatter is RedisObjectFormatter objectFormatter)
            //     {
            //         throw new NotImplementedException();
            //     }
            //     else
            //     {
            //         throw new NotSupportedException($"Formatter of type {formatter.GetType()} is not supported.");
            //     }
            // }
            //
            return RedisValueFormatter.Write(value, context);
        }

        /// <summary>
        /// Converts the specified dictionary into an array of <see cref="HashEntry"/> objects using the provided field formatter, value formatters, and serialization context.
        /// </summary>
        /// <typeparam name="T">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to convert.</param>
        /// <param name="fieldFormatter">The formatter used to format the dictionary keys into Redis field names.</param>
        /// <param name="valueFormatters">The list of formatters used to format the dictionary values into Redis values.</param>
        /// <param name="serializationContext">The context used for value serialization.</param>
        /// <returns>An array of <see cref="HashEntry"/> objects representing the key-value pairs in the dictionary.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a value in the dictionary is null or empty after formatting.</exception>
        internal static ReadOnlyMemory<HashEntry> ToHashEntries<T>(this IDictionary<string, T> dictionary, RedisFieldFormatter fieldFormatter, IList<IRedisValueSerializer> valueFormatters, RedisValueWriterContext serializationContext)
        {
            Memory<HashEntry> memory = new HashEntry[dictionary.Count];

            var lastIndex = -1;
            foreach (var key in dictionary.Keys)
            {
                var hashKey = fieldFormatter.GetFormatted(key);
                var value = dictionary[key];
                var hashValue = GetRedisValue(value, valueFormatters, serializationContext);
                if (hashValue.IsNullOrEmpty)
                    throw new InvalidOperationException($"The value of '{hashKey}' cannot be null or empty.");
                memory.Span[++lastIndex] = new HashEntry(hashKey, hashValue);
            }

            memory = memory[..(lastIndex + 1)];
            return memory;
        }

        /// <summary>
        /// Converts a model to <see cref="ReadOnlyMemory{T}"/> of <see cref="HashEntry"/>.
        /// </summary>
        /// <param name="model">A model to convert.</param>
        /// <param name="fieldFormatter">A formatter for the field names.</param>
        /// <param name="valueFormatters">A list of formatters for property values.</param>
        /// <param name="writerContext">A context for value serialization.</param>
        /// <typeparam name="TObject">The type of the model object.</typeparam>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> of <see cref="HashEntry"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the value of a property is null or empty.</exception>
        internal static ReadOnlyMemory<HashEntry> GetHashEntries<TObject>(this TObject model, RedisFieldFormatter fieldFormatter, IList<IRedisValueSerializer> valueFormatters, RedisValueWriterContext writerContext) where TObject : class, IRedisCacheObject
        {
            if (!(model is IRedisCacheObjectAdapter<TObject> adapter))
                throw new InvalidOperationException($"It might the source generator did not generate the reflector for {model.GetType()}.");
            
            Memory<HashEntry> memory = new HashEntry[adapter.Properties.Length];

            var lastIndex = -1;
            foreach (var propName in adapter.Properties)
            {
                var value = adapter.GetValue(model, propName);
                if (value.IsNullOrEmpty)
                    throw new InvalidOperationException($"The value of '{propName}' cannot be null or empty.");
                memory.Span[++lastIndex] = new HashEntry(fieldFormatter.GetFormatted(propName), value);
            }

            memory = memory[..(lastIndex + 1)];
            return memory;
        }


        /// <summary>
        /// Returns the relevant formatter for the given type. If not found, returns the default formatter.
        /// </summary>
        /// <param name="formatters">The formatters to search in.</param>
        /// <param name="type">A type that representing the type of value.</param>
        /// <returns>A <see cref="IRedisValueSerializer"/> object.</returns>
        private static IRedisValueSerializer GetWriterFormatter(this IEnumerable<IRedisValueSerializer> formatters, Type type)
        {
            var formatter = formatters.FirstOrDefault(x => x.Type == type);
            if (formatter == null)
                return null;

            var constructor = formatter.GetType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Array.Empty<Type>(), modifiers: null);
            if (constructor == null)
                return null;

            var newInstance = (IRedisValueSerializer)constructor.Invoke(Array.Empty<object>());
            return newInstance;
        }

        /// <summary>
        /// Returns the relevant formatter for the given type. If not found, returns the default formatter.
        /// </summary>
        /// <param name="formatters">The formatters to search in.</param>
        /// <param name="type">A type that representing the type of value.</param>
        /// <returns>A <see cref="IRedisValueSerializer"/> object.</returns>
        public static IRedisValueSerializer GetReaderFormatter(this IEnumerable<IRedisValueSerializer> formatters, Type type)
        {
            var formatter = formatters.FirstOrDefault(x => x.Type == type);
            if (formatter == null)
                return null;

            var constructor = formatter.GetType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Array.Empty<Type>(), modifiers: null);
            if (constructor == null)
                return null;

            var newInstance = (IRedisValueSerializer)constructor.Invoke(Array.Empty<object>());
            return newInstance;
        }
    }
}