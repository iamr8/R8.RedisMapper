namespace R8.RedisHelper.Models
{
    public readonly struct RedisCache
    {
        internal RedisCache(RedisKey key, Dictionary<string, RedisValue> value)
        {
            Key = new RedisCacheKey(key);
            Value = value;
        }

        public RedisCacheKey Key { get; }

        /// <summary>
        /// Gets the model from the cached values.
        /// </summary>
        public Dictionary<string, RedisValue> Value { get; }

        /// <summary>
        /// Gets whether the cache is null.
        /// </summary>
        public bool IsNull => Value == null;

        /// <summary>
        /// Initializes a new instance of <see cref="RedisCache{TModel}"/> with null values.
        /// </summary>
        public static RedisCache Null => new();
    }
}