using System;
using System.Collections;
using System.Text.Json;

using StackExchange.Redis;

namespace R8.RedisMapper
{
    internal static class RedisValueFormatter
    {
        /// <summary>
        /// Converts a given value to <see cref="RedisValue"/>.
        /// </summary>
        /// <param name="value">A <see cref="RedisValue"/> that representing given value.</param>
        /// <param name="context">A <see cref="RedisValueWriterContext"/> that representing current context.</param>
        /// <typeparam name="T">A generic type to be serialized.</typeparam>
        /// <returns>A <see cref="RedisValue"/> that representing given value.</returns>
        public static RedisValue Write<T>(T value, RedisValueWriterContext context)
        {
            if (context.IgnoreDefaultValue && value is null)
                return RedisValue.Null;

            if (value is int valueInt)
            {
                if (context.IgnoreDefaultValue && (valueInt is int.MinValue || valueInt is 0))
                    return RedisValue.Null;

                return valueInt;
            }

            if (value is long valueLong)
            {
                if (context.IgnoreDefaultValue && (valueLong is long.MinValue || valueLong is 0))
                    return RedisValue.Null;

                return valueLong;
            }

            if (value is double valueDouble)
            {
                if (context.IgnoreDefaultValue && (valueDouble is double.NaN || valueDouble is double.MinValue || valueDouble is 0))
                    return RedisValue.Null;

                return valueDouble;
            }

            if (value is bool valueBool)
            {
                if (context.IgnoreDefaultValue && !valueBool)
                    return RedisValue.Null;

                return valueBool;
            }

            if (value is string valueString)
            {
                if (string.IsNullOrWhiteSpace(valueString))
                    return context.IgnoreDefaultValue ? RedisValue.Null : RedisValue.EmptyString;

                return valueString;
            }

            if (value is DateTime valueDateTime)
            {
                if (context.IgnoreDefaultValue && valueDateTime == DateTime.MinValue)
                    return RedisValue.Null;

                var ticks = valueDateTime.Ticks;
                return ticks;
            }

            if (context.DataType.IsEnum)
            {
                if (context.IgnoreDefaultValue && value!.Equals(default))
                    return RedisValue.Null;

                return (int)(object)value!;
            }

            var json = JsonSerializer.SerializeToUtf8Bytes(value, context.SerializerOptions);
            return json;
        }

        /// <summary>
        /// Converts a <see cref="RedisValue"/> to given type.
        /// </summary>
        /// <param name="value">A <see cref="RedisValue"/> that representing given value.</param>
        /// <param name="context">A <see cref="RedisValueReaderContext"/> that representing current context.</param>
        /// <returns>A <see cref="object"/> that representing given value.</returns>
        /// <exception cref="RedisMapperException">Thrown when given value is not a valid value.</exception>
        public static object Read(RedisValue value, RedisValueReaderContext context)
        {
            if (value.IsNullOrEmpty)
            {
                if (context.IsNullable)
                    return null;

                if (context.ReturnType.IsValueType)
                    return Activator.CreateInstance(context.ReturnType);

                return default;
            }

            if (context.ReturnType == typeof(int))
            {
                if (!value.IsInteger || !value.TryParse(out int i))
                    throw new RedisMapperException($"The given value '{value}' is not a valid integer value.");

                return i;
            }

            if (context.ReturnType == typeof(long))
            {
                if (!value.IsInteger || !value.TryParse(out long l))
                    throw new RedisMapperException($"The given value '{value}' is not a valid long value.");

                return l;
            }

            if (context.ReturnType == typeof(double))
            {
                if (!value.TryParse(out double d))
                    throw new RedisMapperException($"The given value '{value}' is not a valid double value.");

                return d;
            }

            if (context.ReturnType == typeof(bool))
            {
                return (bool)value;
            }

            if (context.ReturnType == typeof(string))
            {
                return (string)value!;
            }

            if (context.ReturnType == typeof(DateTime))
            {
                if (!value.IsInteger || !value.TryParse(out long i))
                    throw new RedisMapperException($"The given value '{value}' is not a valid long value.");

                if (i == 0)
                    return context.IsNullable ? (object) null : default(DateTime);

                return new DateTime(i);
            }

            if (context.ReturnType.IsEnum)
            {
                if (!value.IsInteger || !value.TryParse(out int i))
                    throw new RedisMapperException($"The given value '{value}' is not a valid integer value.");

                var defaultValue = Enum.ToObject(context.ReturnType, 0);
                var currentValue = Enum.ToObject(context.ReturnType, i);
                if (context.IgnoreDefaultValue && currentValue.Equals(defaultValue) && context.IsNullable)
                    return null;

                return currentValue;
            }

            var json = (byte[])value!;
            var result = JsonSerializer.Deserialize(json, context.ReturnType, context.SerializerOptions);
            if (context.IgnoreDefaultValue)
            {
                if (result is IDictionary { Count: 0 })
                    return null;

                if (result is IList { Count: 0 })
                    return null;
            }

            return result;
        }
    }
}