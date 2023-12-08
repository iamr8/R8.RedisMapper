using System.Text.Json;

namespace R8.RedisMapper
{
    internal interface IRedisValueContext
    {
        /// <summary>
        /// Gets a value indicating whether to ignore default value.
        /// </summary>
        bool IgnoreDefaultValue { get; }

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> that is used when need to deserialize the value.
        /// </summary>
        JsonSerializerOptions SerializerOptions { get; }
    }
}