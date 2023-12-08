using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    internal static class TypeReflections
    {
        private static readonly ConcurrentDictionary<Type, CachedPropertyInfo[]> CachedTypes = new ConcurrentDictionary<Type, CachedPropertyInfo[]>();

        private static ReadOnlyMemory<CachedPropertyInfo> GetPropertiesFromCache(this CachedPropertyInfo[] cache, IReadOnlyCollection<string> propertyNames)
        {
            if (propertyNames.Count == 0)
                return cache;

            Memory<CachedPropertyInfo> filteredArray = new CachedPropertyInfo[propertyNames.Count];
            var lastIndex = -1;
            foreach (var propertyName in propertyNames)
            {
                if (!propertyNames.Contains(propertyName, StringComparer.Ordinal))
                    continue;

                var property = Array.Find(cache, x => string.Equals(x.FormattedName, propertyName, StringComparison.Ordinal));
                if (property.Equals(CachedPropertyInfo.Empty))
                    continue;

                filteredArray.Span[++lastIndex] = property;
            }

            filteredArray = filteredArray[..(lastIndex + 1)];
            return filteredArray;
        }

        private static ReadOnlyMemory<CachedPropertyInfo> GetPropertiesAndCache(this Type type, IReadOnlyCollection<string> propertyNames, RedisFieldFormatter fieldFormatter)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Memory<CachedPropertyInfo> memory = new CachedPropertyInfo[1024];
            var lastIndex = -1;

            foreach (var prop in props)
            {
                if (TryAddProperty(memory.Span, prop))
                {
                    memory.Span[++lastIndex] = new CachedPropertyInfo(fieldFormatter.GetFormatted(prop.Name)!, prop);
                }
            }

            if (type.IsInterface)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    var interfaceProps = interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in interfaceProps)
                    {
                        if (TryAddProperty(memory.Span, prop))
                            memory.Span[++lastIndex] = new CachedPropertyInfo(fieldFormatter.GetFormatted(prop.Name)!, prop);
                    }
                }
            }

            memory = memory[..(lastIndex + 1)];
            CachedTypes.TryAdd(type, memory.ToArray());

            if (propertyNames.Count > 0)
            {
                Memory<CachedPropertyInfo> filteredArray = new CachedPropertyInfo[propertyNames.Count];
                var lastIndex2 = -1;
                foreach (var propertyName in propertyNames)
                {
                    if (!propertyNames.Contains(propertyName, StringComparer.Ordinal))
                        continue;

                    var property = CachedPropertyInfo.Empty;
                    for (var i = 0; i < memory.Length; i++)
                    {
                        var memProp = memory.Span[i];
                        if (string.Equals(memProp.FormattedName, propertyName, StringComparison.Ordinal))
                            property = memProp;
                    }

                    if (property.Equals(CachedPropertyInfo.Empty))
                        continue;

                    filteredArray.Span[++lastIndex2] = property;
                }

                filteredArray = filteredArray[..(lastIndex2 + 1)];
                return filteredArray;
            }

            return memory;
        }

        /// <summary>
        /// Returns properties of a type.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> to get properties from.</param>
        /// <param name="propertyNames">An array of property names to filter.</param>
        /// <param name="fieldFormatter">A <see cref="RedisFieldFormatter"/> to format the property names.</param>
        /// <returns>An array of public <see cref="PropertyInfo"/>s.</returns>
        /// <exception cref="ArgumentNullException">When the type is null.</exception>
        internal static ReadOnlyMemory<CachedPropertyInfo> GetProperties(this Type type, string[] propertyNames, RedisFieldFormatter fieldFormatter)
        {
            if (CachedTypes.TryGetValue(type, out var cachedProps))
                return cachedProps.GetPropertiesFromCache(propertyNames);

            return type.GetPropertiesAndCache(propertyNames, fieldFormatter);
        }

        /// <summary>
        /// Checks if a property exists in a given <see cref="Span{T}"/> of <see cref="PropertyInfo"/>s.
        /// </summary>
        /// <param name="span">A <see cref="Span{T}"/> of <see cref="PropertyInfo"/>s.</param>
        /// <param name="prop">The <see cref="PropertyInfo"/> to check.</param>
        /// <returns>The index of the <see cref="PropertyInfo"/> if exists, otherwise -1.</returns>
        private static bool TryAddProperty(this ReadOnlySpan<CachedPropertyInfo> span, MemberInfo prop)
        {
            var index = -1;
            for (var i = 0; i < span.Length; i++)
            {
                var existingProp = span[i];
                if (existingProp.Equals(CachedPropertyInfo.Empty))
                    break;

                if (!string.Equals(existingProp.Property.Name, prop.Name, StringComparison.Ordinal))
                {
                    index = -1;
                    continue;
                }

                index = i;
                break;
            }

            return index == -1;
        }

        /// <summary>
        /// Sets the value of the property from <see cref="RedisValue"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">When the property does not have a setter.</exception>
        /// <exception cref="NotSupportedException">When the property type is not supported.</exception>
        /// <returns>A <see cref="bool"/> value that representing whether the value is set or not.</returns>
        public static bool SetValue<TModel>(this CachedPropertyInfo property, TModel container, RedisValue value, IEnumerable<IRedisValueFormatter> formatters, RedisValueReaderContext readerContext)
        {
            if (!value.HasValue || value.IsNullOrEmpty)
                return false;

            var formatter = formatters.GetReaderFormatter(property.PropertyType);
            if (formatter != null)
            {
                var obj = formatter.Read(value, readerContext);
                property.Property.SetValue(container, obj);
            }
            else
            {
                var obj = RedisValueFormatter.Read(value, readerContext);
                property.Property.SetValue(container, obj);
            }

            return true;
        }

        public static RedisValue GetValue<TModel>(this CachedPropertyInfo property, TModel container, IList<IRedisValueFormatter> valueFormatters, RedisValueWriterContext writerContext)
        {
            var val = property.Property.GetValue(container);
            if (val == null)
                return RedisValue.Null;

            return RedisValueExtensions.GetRedisValue(val, valueFormatters, writerContext);
        }
    }
}