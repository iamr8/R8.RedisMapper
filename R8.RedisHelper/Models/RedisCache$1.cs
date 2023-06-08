namespace R8.RedisHelper.Models
{
    public readonly struct RedisCache<TModel>
    {
        private readonly ReadOnlyMemory<char> _missedFieldsMemory;
        private readonly int _missedFieldsCount;

        internal RedisCache(RedisKey key, TModel value, IEnumerable<string> missedFields) : this(key, value)
        {
            var missedFieldsJoined = string.Join('\0', missedFields);
            _missedFieldsMemory = missedFieldsJoined.AsMemory();
            _missedFieldsCount = missedFields.Count();
        }

        internal RedisCache(RedisKey key, TModel value)
        {
            Key = new RedisCacheKey(key);
            Value = value;
            _missedFieldsMemory = null;
            _missedFieldsCount = 0;
        }

        public RedisCache(TModel value) : this()
        {
            Value = value;
        }

        public RedisCacheKey Key { get; }

        /// <summary>
        /// Gets the model from the cached values.
        /// </summary>
        public TModel Value { get; }

        /// <summary>
        /// Gets whether the cache is null.
        /// </summary>
        public bool IsNull => Value == null;

        /// <summary>
        /// Initializes a new instance of <see cref="RedisCache{TModel}"/> with null values.
        /// </summary>
        public static RedisCache<TModel> Null => new();

        /// <summary>
        /// Returns the fields that were missed when the cache was created.
        /// </summary>
        public string[] GetMissedFields()
        {
            if (_missedFieldsCount == 0)
            {
                return null;
            }

            var missedFields = new string[_missedFieldsCount];
            var span = _missedFieldsMemory.Span;

            int start = 0;
            int fieldIndex = 0;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == '\0')
                {
                    missedFields[fieldIndex++] = span.Slice(start, i - start).ToString();
                    start = i + 1;
                }
            }

            missedFields[fieldIndex] = span.Slice(start).ToString();

            return missedFields;
        }
    }
}