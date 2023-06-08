using System.Text;

namespace R8.RedisHelper.Models
{
    public readonly struct RedisCacheKey
    {
        private readonly RedisKey? _redisKey;

        internal RedisCacheKey(RedisKey redisKey)
        {
            _redisKey = redisKey;

            All = new Dictionary<string, string>();

            var _k = _redisKey.ToString();
            if (!_k.Contains(':'))
            {
                ((Dictionary<string, string>) All).Add(_k, null);
            }
            else
            {
                var key = _k.Split(':');
                for (var i = 0; i < key.Length; i++)
                {
                    if (i % 2 != 0)
                        continue;

                    ((Dictionary<string, string>) All).Add(key[i], key[i + 1]);
                    i++;
                }
            }
        }

        private bool IsNull => !_redisKey.HasValue;

        internal RedisKey Value => _redisKey.Value;

        /// <summary>
        /// Creates a new instance of <see cref="RedisCacheKey"/> with the specified prefix and key.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the prefix or key is null.</exception>
        public static RedisCacheKey Create(string prefix, string key)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Span<char> span = stackalloc char[prefix.Length + key.Length + 1];
            prefix.AsSpan().CopyTo(span);
            span[prefix.Length] = ':';
            key.AsSpan().CopyTo(span[(prefix.Length + 1)..]);
            return new RedisCacheKey(new RedisKey(span.ToString()));
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedisCacheKey"/> with the specified prefix and key.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the key is null.</exception>
        public static RedisCacheKey Create(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return new RedisCacheKey(new RedisKey(key));
        }

        /// <summary>
        /// Returns the specified part of the key.
        /// </summary>
        /// <param name="key">A key part.</param>
        /// <returns>A <see cref="RedisCacheKey"/> instance.</returns>
        /// <exception cref="ArgumentNullException">When the key is null.</exception>
        /// <exception cref="KeyNotFoundException">When the key is not found.</exception>
        public RedisCacheKey GetPart(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (!All.ContainsKey(key))
                throw new KeyNotFoundException($"The key '{key}' was not found.");

            var _key = All[key];
            return new RedisCacheKey(new RedisKey(_key));
        }

        /// <summary>
        /// Appends the key to the prefix.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the prefix or key is null.</exception>
        /// <exception cref="InvalidOperationException">When the main key is null.</exception>
        public RedisCacheKey Append(string prefix, string key)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (!_redisKey.HasValue)
                throw new InvalidOperationException("Cannot append to a null key.");

            var sb = new StringBuilder()
                .Append(':')
                .Append(prefix)
                .Append(':')
                .Append(key);
            return new RedisCacheKey(_redisKey.Value.Append(new RedisKey(sb.ToString())));
        }

        /// <summary>
        /// Returns the last part of the key as the specified type.
        /// </summary>
        /// <exception cref="NullReferenceException">When the key is null.</exception>
        /// <exception cref="InvalidOperationException">When the key has more than one part.</exception>
        public int AsInt()
        {
            if (IsNull)
                throw new NullReferenceException("Cannot parse a null key.");

            if (All.Count > 1)
                throw new InvalidOperationException("Cannot parse a key with more than one part.");

            var _key = All.First();
            var value = _key.Value;

            return int.Parse(value);
        }

        /// <summary>
        /// Returns the last part of the key as the specified type.
        /// </summary>
        /// <exception cref="NullReferenceException">When the key is null.</exception>
        /// <exception cref="InvalidOperationException">When the key has more than one part.</exception>
        public string AsString()
        {
            if (IsNull)
                throw new NullReferenceException("Cannot parse a null key.");

            if (All.Count > 1)
                throw new InvalidOperationException("Cannot parse a key with more than one part.");

            var _key = All.First();
            var value = _key.Value;
            return value;
        }

        /// <summary>
        /// Returns the parts of the key as a dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, string> All { get; }

        public string this[string key]
        {
            get
            {
                if (IsNull)
                    throw new InvalidOperationException("Cannot get a value from a null key.");

                if (All.TryGetValue(key, out var value))
                    return value;

                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }
        }

        public static implicit operator string?(RedisCacheKey key)
        {
            return key._redisKey.ToString();
        }

        public static implicit operator RedisCacheKey(string key)
        {
            return new RedisCacheKey(new RedisKey(key));
        }

        public bool Equals(RedisCacheKey other)
        {
            return Nullable.Equals(_redisKey, other._redisKey);
        }

        public override bool Equals(object? obj)
        {
            return obj is RedisCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _redisKey.GetHashCode();
        }

        private sealed class RedisKeyEqualityComparer : IEqualityComparer<RedisCacheKey>
        {
            public bool Equals(RedisCacheKey x, RedisCacheKey y)
            {
                return Nullable.Equals(x._redisKey, y._redisKey);
            }

            public int GetHashCode(RedisCacheKey obj)
            {
                return obj._redisKey.GetHashCode();
            }
        }

        public static IEqualityComparer<RedisCacheKey> RedisKeyComparer { get; } = new RedisKeyEqualityComparer();

        public override string? ToString()
        {
            return _redisKey.ToString();
        }

        public static bool operator ==(RedisCacheKey left, RedisCacheKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RedisCacheKey left, RedisCacheKey right)
        {
            return !(left == right);
        }
    }
}