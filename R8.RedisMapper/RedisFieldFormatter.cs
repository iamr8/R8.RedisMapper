using StackExchange.Redis;

namespace R8.RedisMapper
{
    /// <summary>
    /// A base class for formatting the given field name.
    /// </summary>
    public abstract class RedisFieldFormatter
    {
        /// <summary>
        /// Applies the given formatter to the given field.
        /// </summary>
        /// <param name="field">The value to be formatted.</param>
        /// <returns>A field that is formatted.</returns>
        public abstract RedisValue GetFormatted(string field);
    }
}