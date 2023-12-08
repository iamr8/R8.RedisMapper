using System;
using System.Text;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    public static class RedisKeyFactory
    {
        private const char Delimiter = ':';

        /// <summary>
        /// Creates a new instance of <see cref="RedisKey"/> with the specified prefix and key.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the prefix or key is null.</exception>
        public static RedisKey Create(string prefix, string key)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var prefixBytes = Encoding.UTF8.GetBytes(prefix);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Span<byte> spanPool = stackalloc byte[prefixBytes.Length + keyBytes.Length + 1];
            var cursor = 0;
            foreach (var p in prefixBytes)
                spanPool[cursor++] = p;
            spanPool[cursor++] = (byte)Delimiter;
            foreach (var k in keyBytes)
                spanPool[cursor++] = k;

            var bytes = spanPool.ToArray();
            var redisKey = (RedisKey)bytes;
            return redisKey;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RedisKey"/> with the specified prefix and key.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the prefix or key is null.</exception>
        public static RedisKey Create(FormattableString str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            return new RedisKey(str.ToString());
        }

        /// <summary>
        /// Appends the key:value to the previous key.
        /// </summary>
        /// <remarks>Don't prepend <c>:</c> to start of the prefix. It will be added automatically.</remarks>
        /// <exception cref="ArgumentNullException">When the prefix or key is null.</exception>
        public static RedisKey Append(this RedisKey redisKey, string prefix, string key)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Span<char> span = stackalloc char[1 + prefix.Length + 1 + key.Length];
            span[0] = Delimiter;
            prefix.AsSpan().CopyTo(span[1..]);
            span[prefix.Length + 1] = Delimiter;
            key.AsSpan().CopyTo(span[(prefix.Length + 2)..]);
            return redisKey.Append(new RedisKey(span.ToString()));
        }

        /// <summary>
        /// Returns any part of the key according to the index.
        /// </summary>
        /// <param name="redisKey">A <see cref="RedisKey"/> object.</param>
        /// <param name="index">The index of the part.</param>
        /// <param name="delimiter">The delimiter character.</param>
        /// <returns>The part of the key.</returns>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        public static string Get(this RedisKey redisKey, int index, char delimiter = Delimiter)
        {
            var keyData = (byte[])redisKey;
            var passedIndex = -1;

            Span<byte> spanPool = stackalloc byte[keyData.Length];
            var spanCursor = 0;
            for (var i = 0; i < keyData.Length; i++)
            {
                var key = keyData[i];
                if (key == (byte)delimiter)
                {
                    passedIndex++;
                    if (passedIndex == index)
                    {
                        return Encoding.UTF8.GetString(spanPool[..spanCursor]);
                    }

                    spanCursor = 0;
                    spanPool.Clear();
                }
                else if (i == keyData.Length - 1)
                {
                    spanPool[spanCursor++] = key;
                    if (spanCursor > 0)
                    {
                        var f = Encoding.UTF8.GetString(spanPool[..spanCursor]);
                        return f;
                    }

                    return string.Empty;
                }
                else
                {
                    spanPool[spanCursor++] = key;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}