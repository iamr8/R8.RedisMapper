using System.Collections.Generic;
using System.Text.Json;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    public static class Configuration
    {
        /// <summary>
        /// Gets or sets the Serializer Options.
        /// </summary>
        public static JsonSerializerOptions SerializerOptions { internal get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether to ignore default values when casting to <see cref="RedisValue"/>.
        /// </para>
        /// <para>
        /// Example: if the type is <see cref="int"/> and set to <c>0</c> or <c>null</c>, then no value will be stored in Redis for that key.
        /// </para>
        /// </summary>
        public static bool IgnoreDefaultValues { internal get; set; }

        /// <summary>
        /// Gets or sets the Redis Value Formatter. Default: <see cref="RedisFieldCamelCaseFormatter"/>.
        /// </summary>
        public static RedisFieldFormatter FieldFormatter { internal get; set; } = new RedisFieldCamelCaseFormatter();

        /// <summary>
        /// Gets or sets the Redis Value Formatter.
        /// </summary>
        public static IList<IRedisValueFormatter> ValueFormatters { get; } = new List<IRedisValueFormatter>();
    }
}