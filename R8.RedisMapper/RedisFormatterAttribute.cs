using System;

namespace R8.RedisMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RedisFormatterAttribute : Attribute
    {
        public RedisFormatterAttribute(Type formatterType)
        {
        }
    }
}