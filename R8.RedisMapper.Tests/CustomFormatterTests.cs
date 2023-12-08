using System;
using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class CustomFormatterTests
    {
        public enum DummyEnum
        {
            Value1,
            Value2
        }
        
        public class DummyEnumClass
        {
            public DummyEnum Value { get; set; }
        }

        public class DummyEnumValueFormatter : IRedisValueFormatter
        {
            public Type Type => typeof(DummyEnum);
            public RedisValue Write(object value, RedisValueWriterContext context)
            {
                if (value is DummyEnum dummyEnum)
                {
                    return dummyEnum.ToString();
                }

                return RedisValue.Null;
            }

            public object Read(RedisValue value, RedisValueReaderContext context)
            {
                if (value.HasValue)
                {
                    var str = value.ToString();
                    if (Enum.TryParse<DummyEnum>(str, out var result))
                        return result;
                }

                return null;
            }
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            Configuration.ValueFormatters.Add(new DummyEnumValueFormatter());
            
            var type = typeof(DummyEnumClass);
            var instance = new DummyEnumClass();
            var value = "Value2";
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnumClass.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(value);
        }
    }
}