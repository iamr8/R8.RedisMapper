using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static RedisValue GetRedisValue(this JsonElement value, IList<IRedisValueFormatter> valueFormatters, RedisValueWriterContext writerContext)
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
        public static RedisValue GetRedisValue<T>(T value, IList<IRedisValueFormatter> valueFormatters, RedisValueWriterContext writerContext)
        {
            if (value is null)
                return RedisValue.Null;

            switch (value)
            {
                case RedisValue valRedis:
                {
                    return valRedis;
                }

                case JsonElement jsonElement:
                {
                    return jsonElement.GetRedisValue(valueFormatters, writerContext);
                }

                default:
                {
                    var valueType = value.GetType();
                    var context = new RedisValueWriterContext(writerContext, valueType);
                    var formatter = valueFormatters.GetWriterFormatter(valueType);
                    if (formatter != null)
                        return formatter.Write(value, context);

                    return RedisValueFormatter.Write(value, context);
                }
            }
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
        internal static ReadOnlyMemory<HashEntry> ToHashEntries<T>(this IDictionary<string, T> dictionary, RedisFieldFormatter fieldFormatter, IList<IRedisValueFormatter> valueFormatters, RedisValueWriterContext serializationContext)
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
        /// <typeparam name="T">The type of the model to convert.</typeparam>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> of <see cref="HashEntry"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the value of a property is null or empty.</exception>
        internal static ReadOnlyMemory<HashEntry> GetHashEntries<T>(this T model, RedisFieldFormatter fieldFormatter, IList<IRedisValueFormatter> valueFormatters, RedisValueWriterContext writerContext)
        {
            var props = model.GetType().GetProperties(Array.Empty<string>(), fieldFormatter);
            Memory<HashEntry> memory = new HashEntry[props.Length];

            var lastIndex = -1;
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props.Span[i];
                var value = prop.GetValue(model, valueFormatters, writerContext);
                if (value.IsNullOrEmpty)
                    throw new InvalidOperationException($"The value of '{prop.FormattedName}' cannot be null or empty.");
                memory.Span[++lastIndex] = new HashEntry(prop.FormattedName, value);
            }

            memory = memory[..(lastIndex + 1)];
            return memory;
        }


        /// <summary>
        /// Returns the relevant formatter for the given type. If not found, returns the default formatter.
        /// </summary>
        /// <param name="formatters">The formatters to search in.</param>
        /// <param name="type">A type that representing the type of value.</param>
        /// <returns>A <see cref="IRedisValueFormatter"/> object.</returns>
        private static IRedisValueFormatter GetWriterFormatter(this IEnumerable<IRedisValueFormatter> formatters, Type type)
        {
            var formatter = formatters.FirstOrDefault(x => x.Type == type);
            if (formatter == null)
                return null;

            var constructor = formatter.GetType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Array.Empty<Type>(), modifiers: null);
            if (constructor == null)
                return null;

            var newInstance = (IRedisValueFormatter)constructor.Invoke(Array.Empty<object>());
            return newInstance;
        }

        /// <summary>
        /// Returns the relevant formatter for the given type. If not found, returns the default formatter.
        /// </summary>
        /// <param name="formatters">The formatters to search in.</param>
        /// <param name="type">A type that representing the type of value.</param>
        /// <returns>A <see cref="IRedisValueFormatter"/> object.</returns>
        public static IRedisValueFormatter GetReaderFormatter(this IEnumerable<IRedisValueFormatter> formatters, Type type)
        {
            var formatter = formatters.FirstOrDefault(x => x.Type == type);
            if (formatter == null)
                return null;

            var constructor = formatter.GetType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Array.Empty<Type>(), modifiers: null);
            if (constructor == null)
                return null;

            var newInstance = (IRedisValueFormatter)constructor.Invoke(Array.Empty<object>());
            return newInstance;
        }
    }
}