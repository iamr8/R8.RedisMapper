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
            Key = key;
            Value = value;
            _missedFieldsMemory = null;
            _missedFieldsCount = 0;
        }

        public RedisKey Key { get; }

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
                return Array.Empty<string>();

            var missedFields = new string[_missedFieldsCount];
            var span = _missedFieldsMemory.Span;

            var start = 0;
            var fieldIndex = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != '\0') 
                    continue;
                
                missedFields[fieldIndex++] = span.Slice(start, i - start).ToString();
                start = i + 1;
            }

            missedFields[fieldIndex] = span[start..].ToString();
            return missedFields;
        }
    }
}