using System;
using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class EnumTests
    {
        public enum EDummyEnum
        {
            None = 0,
            Chosen = 1,
        }

        public enum EDummyEnum2 : long
        {
            None = 2,
            Chosen = 3,
        }

        public class DummyEnum
        {
            public EDummyEnum Value { get; set; }
            public EDummyEnum? ValueNullable { get; set; }
        }

        [Theory]
        [InlineData(nameof(DummyEnum.ValueNullable), EDummyEnum.Chosen)]
        [InlineData(nameof(DummyEnum.ValueNullable), null)]
        public void should_set_get_nullable(string propName, EDummyEnum? value)
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var redisValue = (RedisValue)(int?)value;

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.Property.GetValue(instance) as EDummyEnum?;
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyEnum.Value), EDummyEnum.None)]
        public void should_set_get(string propName, EDummyEnum value)
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var redisValue = (RedisValue)(int?)value;

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.Property.GetValue(instance) as EDummyEnum?;
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_by_common_method()
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var value = EDummyEnum.Chosen;
            var redisValue = (RedisValue)(int)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(redisValue);
        }

        [Fact]
        public void should_set_get_2_by_redis_value()
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.Property.GetValue(instance) as EDummyEnum?;
            v.Should().Be(EDummyEnum.None);
        }

        [Fact]
        public void should_set_get_nullable_2_by_redis_value()
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.Property.GetValue(instance) as EDummyEnum?;
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_by_redis_value_nonmatching_type()
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var redisValue = (RedisValue)"ghgh";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var value = EDummyEnum.Chosen;
            var redisValue = (RedisValue)(int)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.Property.GetValue(instance) as EDummyEnum?;
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyEnum);
            var instance = new DummyEnum();
            var value = EDummyEnum.Chosen;
            var redisValue = (RedisValue)(int)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.Property.GetValue(instance) as EDummyEnum?;
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(EDummyEnum.None)]
        [InlineData(EDummyEnum.Chosen)]
        public void should_return_value_from_enumint(EDummyEnum value)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as EDummyEnum?;
            parsed!.Should().Be(value);
        }

        [Theory]
        [InlineData(null, true, true)]
        [InlineData(EDummyEnum.None, true, true)]
        [InlineData(EDummyEnum.None, false, false)]
        [InlineData(EDummyEnum.Chosen, false, false)]
        public void should_return_value_nullable(EDummyEnum? value, bool ignoreDefaultValue, bool nullExpected)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var reader = new RedisValueReaderContext(writerContext, typeof(EDummyEnum?));
            var parsed = RedisValueFormatter.Read(actual, reader) as EDummyEnum?;
            if (nullExpected)
            {
                parsed.Should().BeNull();
            }
            else
            {
                parsed.Should().Be(value);
            }
        }
    }
}