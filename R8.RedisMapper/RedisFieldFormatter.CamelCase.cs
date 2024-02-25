using System;
using System.Text;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    /// <summary>
    /// A formatter that converts the given string to camel case.
    /// </summary>
    public sealed class RedisFieldCamelCaseFormatter : RedisFieldFormatter
    {
        public override RedisValue GetFormatted(string field)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentNullException(nameof(field));

            if (char.IsLower(field[0]))
                return field;

            Span<char> characters = stackalloc char[field.Length];
            characters[0] = char.ToLowerInvariant(field[0]);
            for (var i = 1; i < field.Length; i++)
                characters[i] = field[i];
            
            var encoding = Encoding.UTF8;
            var maxBytes = encoding.GetMaxByteCount(characters.Length);
            Span<byte> buffers = stackalloc byte[maxBytes];
            var bytesWritten = encoding.GetBytes(characters, buffers);
            buffers = buffers[..bytesWritten];

            return (RedisValue)buffers.ToArray();
        }
    }
}