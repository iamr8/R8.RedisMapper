using System;

namespace R8.RedisMapper
{
    public class RedisMapperException : Exception
    {
        public RedisMapperException(string message) : base(message)
        {
        }

        public RedisMapperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}